using Firebase.Auth;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.Notifications.Android;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// addReminder scene — medicine, time picker, meal choice, save to PlayerPrefs.
/// Supports creating a new reminder OR editing an existing one (see ReminderPrefs.EditIndexKey).
/// </summary>
public class AddReminderManager : MonoBehaviour
{
    /// <summary>Shared keys so ReminderManager can read/write the same data.</summary>
    public static class ReminderPrefs
    {
        private static string UID =>
            PlayerPrefs.GetString("MediLens_CurrentUserID", "guest");

        public static string CountKey =>
            "MediLens_" + UID + "_ReminderCount";

        public const string EditIndexKey = "MediLens_ReminderEditIndex";

        public static string MedKey(int index) =>
            "MediLens_" + UID + "_R_" + index + "_med";

        public static string TimeKey(int index) =>
            "MediLens_" + UID + "_R_" + index + "_time";

        public static string MealKey(int index) =>
            "MediLens_" + UID + "_R_" + index + "_meal";

        public static int GetEditIndex()
        {
            return PlayerPrefs.GetInt(EditIndexKey, -1);
        }

        public static void SetEditIndex(int index)
        {
            PlayerPrefs.SetInt(EditIndexKey, index);
            PlayerPrefs.Save();
        }

        public static void ClearEditMode()
        {
            PlayerPrefs.SetInt(EditIndexKey, -1);
            PlayerPrefs.Save();
        }

        public static bool IsEditMode()
        {
            return GetEditIndex() >= 0;
        }
    }

    [Header("Scenes (must match Build Settings)")]
    public string myRemindersSceneName = "MyReminders";

    [Header("Medicine")]
    public TMP_InputField medInput;

    [Header("Time")]
    public TMP_InputField timeInput;
    public GameObject timePickerPanel;
    public TMP_Dropdown dropdownHour;
    public TMP_Dropdown dropdownMinute;
    public TMP_Dropdown dropdownAmpm;

    [Header("Meal Selection (Before/After)")]
    [Tooltip("Assign the Before Meal button (must have Button + Image).")]
    public Button beforeMealButton;

    [Tooltip("Assign the After Meal button (must have Button + Image).")]
    public Button afterMealButton;

    [Tooltip("Drag your MealSelectionManager component here (it handles visuals + toggle behavior).")]
    public MealSelectionManager mealSelectionManager;

    private int _mealSelection = -1; // fallback (0=Before, 1=After)
    private const string MealBeforeLabel = "Before Meal";
    private const string MealAfterLabel = "After Meal";

    private void Awake()
    {
        SetupTimeInputClickOpensPicker();

        if (mealSelectionManager != null && beforeMealButton != null && afterMealButton != null)
            mealSelectionManager.Initialize(beforeMealButton, afterMealButton);
        else if (mealSelectionManager != null)
            Debug.LogWarning("[AddReminderManager] MealSelectionManager is assigned but Before/After buttons are not.");
    }

    private void Start()
    {

        var channel = new AndroidNotificationChannel()
        {
            Id = "medicine_channel",
            Name = "Medicine Reminder",
            Importance = Importance.High,
            Description = "Medicine Reminder Notifications"
        };

        AndroidNotificationCenter.RegisterNotificationChannel(channel);
        if (timePickerPanel != null)
            timePickerPanel.SetActive(false);

        PopulateTimeDropdowns();
        HookDropdownListeners();

        if (timeInput != null)
            timeInput.readOnly = true;

        _mealSelection = -1;

        if (ReminderPrefs.IsEditMode())
            LoadReminderForEdit(ReminderPrefs.GetEditIndex());
        else
        {
            if (timeInput != null)
                timeInput.text = string.Empty;
        }
    }

    private void OnDestroy()
    {
        RemoveDropdownListeners();
    }

    /// <summary>Wire BackButton OnClick to this.</summary>
    public void GoBackToMyReminders()
    {
        ReminderPrefs.ClearEditMode();
        Debug.Log("[AddReminderManager] Back — loading MyReminders: " + myRemindersSceneName);
        SceneManager.LoadScene(myRemindersSceneName);
    }

    public void OpenTimePicker()
    {
        if (timePickerPanel != null)
            timePickerPanel.SetActive(true);

        UpdateTimeFromDropdowns();
        Debug.Log("[AddReminderManager] Time picker opened.");
    }

    public void UpdateTimeFromDropdowns()
    {
        if (timeInput == null || dropdownHour == null || dropdownMinute == null || dropdownAmpm == null)
            return;

        int hour12 = dropdownHour.value + 1;
        int minute = dropdownMinute.value;
        string ampm = dropdownAmpm.options[dropdownAmpm.value].text;

        timeInput.text = hour12.ToString("00") + ":" + minute.ToString("00") + " " + ampm;
    }

    public void SelectBeforeMeal()
    {
        _mealSelection = 0;
        Debug.Log("[AddReminderManager] Meal selected: " + MealBeforeLabel);

        if (mealSelectionManager != null)
            mealSelectionManager.SelectBeforeMeal();
    }

    public void SelectAfterMeal()
    {
        _mealSelection = 1;
        Debug.Log("[AddReminderManager] Meal selected: " + MealAfterLabel);

        if (mealSelectionManager != null)
            mealSelectionManager.SelectAfterMeal();
    }

    /// <summary>Wire saveButton OnClick to this.</summary>
    public void SaveReminder()
    {
        string medicine = medInput != null ? medInput.text.Trim() : string.Empty;
        string timeText = timeInput != null ? timeInput.text.Trim() : string.Empty;
        int countBefore = PlayerPrefs.GetInt(ReminderPrefs.CountKey, 0);
        int editIndexBefore = ReminderPrefs.GetEditIndex();
        Debug.Log("[AddReminderManager] SaveReminder() start | countBefore=" + countBefore + " | editIndex=" + editIndexBefore);

        if (string.IsNullOrEmpty(medicine))
        {
            Debug.LogWarning("[AddReminderManager] Save failed: medicine name is empty.");
            return;
        }

        if (string.IsNullOrEmpty(timeText))
        {
            Debug.LogWarning("[AddReminderManager] Save failed: please tap Time Input and choose a time.");
            return;
        }

        string meal = string.Empty;
        if (mealSelectionManager != null && mealSelectionManager.HasSelection)
            meal = mealSelectionManager.SelectedMealValue;
        else
            meal = _mealSelection == 0 ? MealBeforeLabel : (_mealSelection == 1 ? MealAfterLabel : string.Empty);

        if (string.IsNullOrEmpty(meal))
        {
            Debug.LogWarning("[AddReminderManager] Save failed: choose Before Meal or After Meal.");
            return;
        }

        int editIndex = ReminderPrefs.GetEditIndex();
        if (editIndex >= 0)
        {
            int count = PlayerPrefs.GetInt(ReminderPrefs.CountKey, 0);
            if (editIndex >= count)
            {
                Debug.LogWarning("[AddReminderManager] Edit failed: index out of range. Saving as new instead.");
                editIndex = -1;
            }
            else
            {
                PlayerPrefs.SetString(ReminderPrefs.MedKey(editIndex), medicine);
                PlayerPrefs.SetString(ReminderPrefs.TimeKey(editIndex), timeText);
                PlayerPrefs.SetString(ReminderPrefs.MealKey(editIndex), meal);
                PlayerPrefs.Save();

                ScheduleMedicineNotification(medicine, timeText);

                Debug.Log("[AddReminderManager] Reminder updated (#" + editIndex + "): " + medicine + " | " + timeText + " | " + meal);
                Debug.Log("[AddReminderManager] Verify updated values => med=" + PlayerPrefs.GetString(ReminderPrefs.MedKey(editIndex), "<missing>") +
                          " | time=" + PlayerPrefs.GetString(ReminderPrefs.TimeKey(editIndex), "<missing>") +
                          " | meal=" + PlayerPrefs.GetString(ReminderPrefs.MealKey(editIndex), "<missing>"));
                ReminderPrefs.ClearEditMode();
                SceneManager.LoadScene(myRemindersSceneName);
                return;
            }
        }

        // Add new reminder (unchanged behavior).
        int newIndex = PlayerPrefs.GetInt(ReminderPrefs.CountKey, 0);

        PlayerPrefs.SetString(ReminderPrefs.MedKey(newIndex), medicine);
        PlayerPrefs.SetString(ReminderPrefs.TimeKey(newIndex), timeText);
        PlayerPrefs.SetString(ReminderPrefs.MealKey(newIndex), meal);
        PlayerPrefs.SetInt(ReminderPrefs.CountKey, newIndex + 1);

        PlayerPrefs.Save();

        ScheduleMedicineNotification(medicine, timeText);

        Debug.Log("[AddReminderManager] Reminder saved (#" + newIndex + "): " + medicine + " | " + timeText + " | " + meal);

        SceneManager.LoadScene(myRemindersSceneName);
    }

    private void LoadReminderForEdit(int index)
    {
        string medicine = PlayerPrefs.GetString(ReminderPrefs.MedKey(index), string.Empty);
        string timeText = PlayerPrefs.GetString(ReminderPrefs.TimeKey(index), string.Empty);
        string meal = PlayerPrefs.GetString(ReminderPrefs.MealKey(index), string.Empty);

        Debug.Log("[AddReminderManager] Edit mode — loading reminder #" + index + ": " + medicine);

        if (medInput != null)
            medInput.text = medicine;

        if (timeInput != null)
            timeInput.text = timeText;

        ApplyTimeToDropdowns(timeText);
        ApplyMealSelection(meal);
    }

    /// <summary>Parses time like "08:30 AM" and sets hour/minute/AM-PM dropdowns.</summary>
    private void ApplyTimeToDropdowns(string timeText)
    {
        if (string.IsNullOrEmpty(timeText))
            return;

        if (dropdownHour == null || dropdownMinute == null || dropdownAmpm == null)
            return;

        string[] parts = timeText.Trim().Split(' ');
        if (parts.Length < 2)
            return;

        string[] timeParts = parts[0].Split(':');
        if (timeParts.Length < 2)
            return;

        if (!int.TryParse(timeParts[0], out int hour12))
            return;
        if (!int.TryParse(timeParts[1], out int minute))
            return;

        string ampm = parts[1].ToUpperInvariant();

        dropdownHour.value = Mathf.Clamp(hour12 - 1, 0, dropdownHour.options.Count - 1);
        dropdownMinute.value = Mathf.Clamp(minute, 0, dropdownMinute.options.Count - 1);

        int ampmIndex = ampm == "PM" ? 1 : 0;
        dropdownAmpm.value = Mathf.Clamp(ampmIndex, 0, dropdownAmpm.options.Count - 1);

        UpdateTimeFromDropdowns();
    }

    private void ApplyMealSelection(string meal)
    {
        if (string.IsNullOrEmpty(meal))
            return;

        string normalized = meal.Trim().ToLowerInvariant();

        if (normalized.Contains("before"))
        {
            _mealSelection = 0;
            if (mealSelectionManager != null)
                mealSelectionManager.SelectBeforeMeal();
        }
        else if (normalized.Contains("after"))
        {
            _mealSelection = 1;
            if (mealSelectionManager != null)
                mealSelectionManager.SelectAfterMeal();
        }
    }

    private void PopulateTimeDropdowns()
    {
        if (dropdownHour != null)
        {
            dropdownHour.ClearOptions();
            var hours = new List<string>(12);
            for (int h = 1; h <= 12; h++)
                hours.Add(h.ToString("00"));
            dropdownHour.AddOptions(hours);
        }

        if (dropdownMinute != null)
        {
            dropdownMinute.ClearOptions();
            var mins = new List<string>(60);
            for (int m = 0; m < 60; m++)
                mins.Add(m.ToString("00"));
            dropdownMinute.AddOptions(mins);
        }

        if (dropdownAmpm != null)
        {
            dropdownAmpm.ClearOptions();
            dropdownAmpm.AddOptions(new List<string> { "AM", "PM" });
        }
    }

    private void HookDropdownListeners()
    {
        if (dropdownHour != null)
            dropdownHour.onValueChanged.AddListener(OnDropdownChanged);
        if (dropdownMinute != null)
            dropdownMinute.onValueChanged.AddListener(OnDropdownChanged);
        if (dropdownAmpm != null)
            dropdownAmpm.onValueChanged.AddListener(OnDropdownChanged);
    }

    private void RemoveDropdownListeners()
    {
        if (dropdownHour != null)
            dropdownHour.onValueChanged.RemoveListener(OnDropdownChanged);
        if (dropdownMinute != null)
            dropdownMinute.onValueChanged.RemoveListener(OnDropdownChanged);
        if (dropdownAmpm != null)
            dropdownAmpm.onValueChanged.RemoveListener(OnDropdownChanged);
    }

    private void OnDropdownChanged(int _)
    {
        UpdateTimeFromDropdowns();
    }

    private void SetupTimeInputClickOpensPicker()
    {
        if (timeInput == null)
            return;

        GameObject go = timeInput.gameObject;
        EventTrigger trigger = go.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = go.AddComponent<EventTrigger>();

        if (trigger.triggers == null)
            trigger.triggers = new List<EventTrigger.Entry>();

        foreach (var e in trigger.triggers)
        {
            if (e.eventID == EventTriggerType.PointerClick)
                return;
        }

        var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
        entry.callback.AddListener(_ => OpenTimePicker());
        trigger.triggers.Add(entry);
    }
    private void ScheduleMedicineNotification(string medicineName, string timeText)
    {
        try
        {
            string[] parts = timeText.Split(' ');

            string[] hm = parts[0].Split(':');

            int hour = int.Parse(hm[0]);
            int minute = int.Parse(hm[1]);

            string ampm = parts[1];

            if (ampm == "PM" && hour != 12)
                hour += 12;

            if (ampm == "AM" && hour == 12)
                hour = 0;

            DateTime now = DateTime.Now;

            DateTime fireTime = new DateTime(
                now.Year,
                now.Month,
                now.Day,
                hour,
                minute,
                0
            );

            if (fireTime <= now)
                fireTime = fireTime.AddDays(1);

            var notification = new AndroidNotification();

            notification.Title = "Medicine Reminder";
            notification.Text = "Time to take " + medicineName;
            notification.FireTime = fireTime;

            AndroidNotificationCenter.SendNotification(
                notification,
                "medicine_channel"
            );

            Debug.Log("Notification scheduled for: " + fireTime);
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }
}
