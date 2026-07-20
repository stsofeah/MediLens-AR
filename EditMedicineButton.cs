using UnityEngine;
using UnityEngine.SceneManagement;

public class EditMedicineButton : MonoBehaviour
{
    public string medicineID;

    public void OpenEditScene()
    {
        SelectedMedicine.medicineID =
            medicineID;

        SelectedMedicine.previousScene =
            SceneManager.GetActiveScene().name;

        Debug.Log(
            "Previous Scene = " +
            SelectedMedicine.previousScene);

        SceneManager.LoadScene("EditMed");
    }
}