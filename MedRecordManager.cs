using UnityEngine;
using UnityEngine.SceneManagement;

public class MedRecordManager : MonoBehaviour
{
    [Header("Scene Names")]
    public string homePharmacistScene = "HomePharmacist";
    public string EditMedScene = "EditMed";

    [Header("Navigation")]
    public string nextMedRecordScene = "MedRecord[Next]";
    public string previousMedRecordScene = "MedRecord";

    // Back Button
    public void BackToHome()
    {
        SceneManager.LoadScene(homePharmacistScene);
    }

    // Edit Button
    public void OpenEditMed()
    {
        SceneManager.LoadScene(EditMedScene);
    }

    // Right Button
    public void GoToNextPage()
    {
        SceneManager.LoadScene(nextMedRecordScene);
    }

    // Left Button
    public void GoToPreviousPage()
    {
        SceneManager.LoadScene(previousMedRecordScene);
    }
}