using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// MyReminders scene — loads saved reminders from PlayerPrefs and creates cards dynamically.
/// Supports edit (opens addReminder with pre-filled data) and delete per card.
/// </summary>
public class ReminderManager : MonoBehaviour
{
    [Header("Scenes (must match Build Settings)")]
    public string homeSceneName = "Home";

    [Tooltip("Exact name of your Add Reminder scene asset.")]
    public string addReminderSceneName = "addReminder";

    [Header("Card Prefab Setup (keep your UI identical)")]
    public GameObject reminderCardPrefab;

    [Tooltip("The RectTransform (Content) inside your Scroll View.")]
    public Transform savedRemindersContainer;

    [Tooltip("Optional: hide the template card in the scene so only dynamic cards show.")]
    public GameObject templateReminderCardToHide;

    [Header("ScrollView Auto Layout (keeps cards stacked, no overlap)")]
    [Tooltip("Enable to auto-configure Content + ScrollRect for vertical stacking.")]
    public bool autoConfigureScrollLayout = true;

    [Tooltip("Clean spacing between cards.")]
    public float cardSpacing = 24f;

    [Header("ReminderCard paths (do not rename in prefab)")]
    public string medTextPath = "medName";
    public string timeTextPath = "timeIcon/timeText";
    public string mealTextPath = "mealIcon/mealText";
    public string editButtonPath = "editButton";
    public string deleteButtonPath = "deleteButton";

    [Header("Optional Delete Confirmation Popup")]
    public GameObject deleteConfirmPanel;
    public TMP_Text deleteConfirmText;
    public Button confirmDeleteButton;
    public Button cancelDeleteButton;

    private const string SpawnedCardNamePrefix = "SavedReminder_";

    private int _pendingDeleteIndex = -1;
    private bool _useDeleteConfirmation;
    private RectTransform _contentRect;
    private ScrollRect _scrollRect;

    private void Start()
    {
        CacheScrollReferences();
        if (autoConfigureScrollLayout)
            EnsureScrollLayoutSetup();

        HideTemplateIfAssigned();
        SetupDeleteConfirmation();
        LoadSavedRemindersFromPrefs();
    }

    public void GoBackToHome()
    {
        Debug.Log("[ReminderManager] Back pressed — loading Home scene: " + homeSceneName);
        SceneManager.LoadScene(homeSceneName);
    }

    /// <summary>Wire Add button OnClick — opens addReminder for a NEW reminder.</summary>
    public void GoToAddReminder()
    {
        AddReminderManager.ReminderPrefs.ClearEditMode();
        Debug.Log("[ReminderManager] Add pressed — loading scene: " + addReminderSceneName);
        SceneManager.LoadScene(addReminderSceneName);
    }

    private void HideTemplateIfAssigned()
    {
        if (templateReminderCardToHide != null)
            templateReminderCardToHide.SetActive(false);
    }

    public void LoadSavedRemindersFromPrefs()
    {
        Debug.Log("[ReminderManager] LoadSavedRemindersFromPrefs() called.");

        if (savedRemindersContainer == null)
        {
            Debug.LogWarning("[ReminderManager] savedRemindersContainer is not assigned.");
            return;
        }

        if (reminderCardPrefab == null)
        {
            Debug.LogWarning("[ReminderManager] reminderCardPrefab is not assigned.");
            return;
        }

        RemovePreviouslySpawnedCards();

        int count = PlayerPrefs.GetInt(AddReminderManager.ReminderPrefs.CountKey, 0);
        Debug.Log("[ReminderManager] Loading " + count + " saved reminder(s) from PlayerPrefs.");

        for (int i = 0; i < count; i++)
        {
            string med = PlayerPrefs.GetString(AddReminderManager.ReminderPrefs.MedKey(i), string.Empty);
            string time = PlayerPrefs.GetString(AddReminderManager.ReminderPrefs.TimeKey(i), string.Empty);
            string meal = PlayerPrefs.GetString(AddReminderManager.ReminderPrefs.MealKey(i), string.Empty);

            GameObject instance = Instantiate(reminderCardPrefab, savedRemindersContainer);
            instance.name = SpawnedCardNamePrefix + i;
            instance.SetActive(true); // Important when source prefab/object was inactive.

            RectTransform rt = instance.GetComponent<RectTransform>();
            if (rt != null)
                rt.localScale = Vector3.one;

            Debug.Log("[ReminderManager] Spawned card " + instance.name +
                      " | parent=" + (instance.transform.parent != null ? instance.transform.parent.name : "null") +
                      " | med=" + med + " | time=" + time + " | meal=" + meal);

            BindReminderCard(instance.transform, med, time, meal);
        }

        ForceRefreshLayout();
    }

    private void RemovePreviouslySpawnedCards()
    {
        for (int i = savedRemindersContainer.childCount - 1; i >= 0; i--)
        {
            Transform child = savedRemindersContainer.GetChild(i);
            if (child != null && child.name != null && child.name.StartsWith(SpawnedCardNamePrefix))
                Destroy(child.gameObject);
        }
    }

    private void CacheScrollReferences()
    {
        _contentRect = savedRemindersContainer as RectTransform;
        _scrollRect = savedRemindersContainer != null ? savedRemindersContainer.GetComponentInParent<ScrollRect>() : null;
    }

    /// <summary>
    /// Ensures Content auto-stacks children vertically and resizes for scrolling.
    /// This does NOT alter card visuals (colors/shadows/fonts/icons).
    /// </summary>
    private void EnsureScrollLayoutSetup()
    {
        if (_contentRect == null)
        {
            Debug.LogWarning("[ReminderManager] Content is not a RectTransform. Cannot configure layout.");
            return;
        }

        VerticalLayoutGroup vlg = _contentRect.GetComponent<VerticalLayoutGroup>();
        if (vlg == null)
            vlg = _contentRect.gameObject.AddComponent<VerticalLayoutGroup>();

        // Keep cards centered, preserve prefab size, only stack vertically.
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.spacing = Mathf.Max(0f, cardSpacing);
        vlg.childControlWidth = false;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = false;
        vlg.childForceExpandHeight = false;

        ContentSizeFitter fitter = _contentRect.GetComponent<ContentSizeFitter>();
        if (fitter == null)
            fitter = _contentRect.gameObject.AddComponent<ContentSizeFitter>();

        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        if (_scrollRect != null)
        {
            _scrollRect.horizontal = false;
            _scrollRect.vertical = true;
            _scrollRect.movementType = ScrollRect.MovementType.Elastic;
            _scrollRect.inertia = true;

            // Make sure ScrollRect points at the same Content this script uses.
            if (_scrollRect.content != _contentRect)
                _scrollRect.content = _contentRect;
        }

        Debug.Log("[ReminderManager] Scroll layout configured. Spacing=" + vlg.spacing);
    }

    private void ForceRefreshLayout()
    {
        if (_contentRect == null)
            return;

        LayoutRebuilder.ForceRebuildLayoutImmediate(_contentRect);
        Canvas.ForceUpdateCanvases();
    }

    private void BindReminderCard(Transform cardRoot, string medicine, string time, string meal)
    {
        TMP_Text medTmp = FindTmpByPath(cardRoot, medTextPath);
        TMP_Text timeTmp = FindTmpByPath(cardRoot, timeTextPath);
        TMP_Text mealTmp = FindTmpByPath(cardRoot, mealTextPath);

        if (medTmp != null)
            medTmp.text = medicine;
        else
            Debug.LogWarning("[ReminderManager] Could not find TMP_Text at path: " + medTextPath);

        if (timeTmp != null)
            timeTmp.text = time;
        else
            Debug.LogWarning("[ReminderManager] Could not find TMP_Text at path: " + timeTextPath);

        if (mealTmp != null)
            mealTmp.text = meal;
        else
            Debug.LogWarning("[ReminderManager] Could not find TMP_Text at path: " + mealTextPath);

        Debug.Log("[ReminderManager] Bound card " + cardRoot.name +
                  " | medTmp=" + (medTmp != null) +
                  " | timeTmp=" + (timeTmp != null) +
                  " | mealTmp=" + (mealTmp != null));

        int index = ParseSpawnedCardIndex(cardRoot.gameObject.name);
        if (index < 0)
            return;

        BindEditButton(cardRoot, index);
        BindDeleteButton(cardRoot, index);
    }

    private void BindEditButton(Transform cardRoot, int reminderIndex)
    {
        Button editBtn = FindButtonByPath(cardRoot, editButtonPath);
        if (editBtn == null)
        {
            Debug.LogWarning("[ReminderManager] Could not find edit Button at path: " + editButtonPath);
            return;
        }

        editBtn.onClick.RemoveAllListeners();
        editBtn.onClick.AddListener(() => OnEditPressed(reminderIndex));
    }

    private void OnEditPressed(int reminderIndex)
    {
        Debug.Log("[ReminderManager] Edit pressed for reminder index: " + reminderIndex);

        AddReminderManager.ReminderPrefs.SetEditIndex(reminderIndex);
        SceneManager.LoadScene(addReminderSceneName);
    }

    private void BindDeleteButton(Transform cardRoot, int reminderIndex)
    {
        Button deleteBtn = FindButtonByPath(cardRoot, deleteButtonPath);
        if (deleteBtn == null)
        {
            Debug.LogWarning("[ReminderManager] Could not find delete Button at path: " + deleteButtonPath);
            return;
        }

        deleteBtn.onClick.RemoveAllListeners();
        deleteBtn.onClick.AddListener(() => OnDeletePressed(reminderIndex));
    }

    private void OnDeletePressed(int reminderIndex)
    {
        Debug.Log("[ReminderManager] Delete pressed for reminder index: " + reminderIndex);

        if (_useDeleteConfirmation)
        {
            _pendingDeleteIndex = reminderIndex;
            ShowDeleteConfirmation();
            return;
        }

        DeleteReminderAt(reminderIndex);
    }

    private void SetupDeleteConfirmation()
    {
        _useDeleteConfirmation = deleteConfirmPanel != null && confirmDeleteButton != null && cancelDeleteButton != null;

        if (!_useDeleteConfirmation)
        {
            if (deleteConfirmPanel != null)
            {
                // If panel exists but buttons are missing, keep it hidden and fallback to immediate delete.
                deleteConfirmPanel.SetActive(false);
                Debug.LogWarning("[ReminderManager] Delete confirmation is partially assigned. Delete will run immediately.");
            }
            return;
        }

        deleteConfirmPanel.SetActive(false);

        confirmDeleteButton.onClick.RemoveListener(ConfirmDeleteClicked);
        cancelDeleteButton.onClick.RemoveListener(CancelDeleteClicked);
        confirmDeleteButton.onClick.AddListener(ConfirmDeleteClicked);
        cancelDeleteButton.onClick.AddListener(CancelDeleteClicked);
    }

    private void ShowDeleteConfirmation()
    {
        if (!_useDeleteConfirmation)
            return;

        if (deleteConfirmText != null)
            deleteConfirmText.text = "Delete this reminder?";

        deleteConfirmPanel.SetActive(true);
    }

    private void ConfirmDeleteClicked()
    {
        Debug.Log("[ReminderManager] Confirm delete for index: " + _pendingDeleteIndex);

        if (deleteConfirmPanel != null)
            deleteConfirmPanel.SetActive(false);

        DeleteReminderAt(_pendingDeleteIndex);
        _pendingDeleteIndex = -1;
    }

    private void CancelDeleteClicked()
    {
        Debug.Log("[ReminderManager] Delete cancelled.");
        if (deleteConfirmPanel != null)
            deleteConfirmPanel.SetActive(false);

        _pendingDeleteIndex = -1;
    }

    private void DeleteReminderAt(int indexToDelete)
    {
        int count = PlayerPrefs.GetInt(AddReminderManager.ReminderPrefs.CountKey, 0);
        if (indexToDelete < 0 || indexToDelete >= count)
        {
            Debug.LogWarning("[ReminderManager] Delete failed. Index out of range: " + indexToDelete);
            return;
        }

        Debug.Log("[ReminderManager] Deleting reminder at index: " + indexToDelete);

        for (int i = indexToDelete; i < count - 1; i++)
        {
            PlayerPrefs.SetString(AddReminderManager.ReminderPrefs.MedKey(i),
                PlayerPrefs.GetString(AddReminderManager.ReminderPrefs.MedKey(i + 1), string.Empty));
            PlayerPrefs.SetString(AddReminderManager.ReminderPrefs.TimeKey(i),
                PlayerPrefs.GetString(AddReminderManager.ReminderPrefs.TimeKey(i + 1), string.Empty));
            PlayerPrefs.SetString(AddReminderManager.ReminderPrefs.MealKey(i),
                PlayerPrefs.GetString(AddReminderManager.ReminderPrefs.MealKey(i + 1), string.Empty));
        }

        int lastIndex = count - 1;
        PlayerPrefs.DeleteKey(AddReminderManager.ReminderPrefs.MedKey(lastIndex));
        PlayerPrefs.DeleteKey(AddReminderManager.ReminderPrefs.TimeKey(lastIndex));
        PlayerPrefs.DeleteKey(AddReminderManager.ReminderPrefs.MealKey(lastIndex));

        PlayerPrefs.SetInt(AddReminderManager.ReminderPrefs.CountKey, count - 1);
        PlayerPrefs.Save();

        LoadSavedRemindersFromPrefs();
    }

    private static int ParseSpawnedCardIndex(string cardName)
    {
        if (string.IsNullOrEmpty(cardName) || !cardName.StartsWith(SpawnedCardNamePrefix))
            return -1;

        string number = cardName.Substring(SpawnedCardNamePrefix.Length);
        return int.TryParse(number, out int index) ? index : -1;
    }

    private static TMP_Text FindTmpByPath(Transform root, string path)
    {
        if (root == null || string.IsNullOrEmpty(path))
            return null;

        Transform t = root.Find(path);
        if (t != null)
        {
            TMP_Text direct = t.GetComponent<TMP_Text>();
            if (direct != null)
                return direct;
        }

        // Fallback: if path fails because of slightly different nesting, try by last object name.
        string[] split = path.Split('/');
        string leafName = split[split.Length - 1];
        foreach (TMP_Text tmp in root.GetComponentsInChildren<TMP_Text>(true))
        {
            if (tmp.gameObject.name == leafName)
                return tmp;
        }

        return null;
    }

    private static Button FindButtonByPath(Transform root, string path)
    {
        if (root == null || string.IsNullOrEmpty(path))
            return null;

        Transform t = root.Find(path);
        return t != null ? t.GetComponent<Button>() : null;
    }
}
