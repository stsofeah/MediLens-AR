using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EditMedicineManager : MonoBehaviour
{
    public TMP_InputField medInput;
    public TMP_InputField shortDescInput;
    public TMP_InputField purposeInput;
    public TMP_InputField dosageInput;
    public TMP_InputField warningInput;

    [Header("Popup")]
    public GameObject dimOverlay;

    MedicineData currentMedicine;

    void Start()
    {
        if (dimOverlay != null)
            dimOverlay.SetActive(false);

        currentMedicine =
            MedicineDatabase.medicines.Find(
                x => x.id == SelectedMedicine.medicineID);

        if (currentMedicine == null)
            return;

        medInput.text = currentMedicine.medicineName;
        shortDescInput.text = currentMedicine.shortDescription;
        purposeInput.text = currentMedicine.purpose;
        dosageInput.text = currentMedicine.dosage;
        warningInput.text = currentMedicine.warning;
    }

    public void SaveChanges()
    {
        currentMedicine.medicineName = medInput.text;
        currentMedicine.shortDescription = shortDescInput.text;
        currentMedicine.purpose = purposeInput.text;
        currentMedicine.dosage = dosageInput.text;
        currentMedicine.warning = warningInput.text;

        PlayerPrefs.SetString(
            "LastUpdatedDate",
            System.DateTime.Now.ToString(
                "dd MMM yyyy"));

        PlayerPrefs.SetString(
            "LastUpdatedTime",
            System.DateTime.Now.ToString(
                "hh:mm tt"));

        PlayerPrefs.Save();

        Debug.Log("Medicine Updated Successfully");

        if (dimOverlay != null)
            dimOverlay.SetActive(true);
    }

    public void BackToMedicineRecords()
    {
        if (!string.IsNullOrEmpty(
            SelectedMedicine.previousScene))
        {
            SceneManager.LoadScene(
                SelectedMedicine.previousScene);
        }
        else
        {
            SceneManager.LoadScene("MedRecord");
        }
    }

    public void GoToHomePharmacist()
    {
        SceneManager.LoadScene("HomePharmacist");
    }
}