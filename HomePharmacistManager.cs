using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class HomePharmacistManager : MonoBehaviour
{
    [Header("Scene Names")]
    public string medRecordScene = "MedRecord";
    public string markerLibraryScene = "MarkerLibrary";
    public string loginScene = "Login Version 2";

    [Header("Last Updated")]
    public TMP_Text dateText;
    public TMP_Text timeText;

    void Start()
    {
        if (dateText != null)
        {
            dateText.text =
                PlayerPrefs.GetString(
                    "LastUpdatedDate",
                    "-- --- ----");
        }

        if (timeText != null)
        {
            timeText.text =
                PlayerPrefs.GetString(
                    "LastUpdatedTime",
                    "--:--");
        }
    }

    public void OpenMedicineRecords()
    {
        SceneManager.LoadScene(medRecordScene);
    }

    public void OpenMarkerLibrary()
    {
        SceneManager.LoadScene(markerLibraryScene);
    }

    public void Logout()
    {
        SceneManager.LoadScene(loginScene);
    }
}