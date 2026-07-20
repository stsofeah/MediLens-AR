using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MarkerLibrarySearch : MonoBehaviour
{
    //[Header("Search")]
    //public TMP_InputField searchInput;

    [Header("Medicine Cards")]
    public GameObject inhalerCard;
    public GameObject paracetamolCard;
    public GameObject ibuprofenCard;
    public GameObject cetirizineCard;
    public GameObject eyeDropCard;
    public GameObject nasalSprayCard;
    public GameObject amoxicillinCard;
    public GameObject insulinPenCard;

    [Header("Scene Names")]
    public string homePharmacistScene = "HomePharmacist";

    [Header("Navigation")]
    public string nextMarkerLibraryScene = "MarkerLibraryNext";
    public string previousMarkerLibraryScene = "MarkerLibrary";

    //private void Start()
    //{
    //    if (searchInput != null)
    //    {
    //        searchInput.onValueChanged.AddListener(SearchMedicine);
    //    }
    //}

    //void SearchMedicine(string searchText)
    //{
    //    searchText = searchText.ToLower();

    //    inhalerCard.SetActive(
    //        "salbutamol inhaler".Contains(searchText) ||
    //        "inhaler".Contains(searchText));

    //    paracetamolCard.SetActive(
    //        "paracetamol".Contains(searchText));

    //    ibuprofenCard.SetActive(
    //        "ibuprofen".Contains(searchText));

    //    cetirizineCard.SetActive(
    //        "cetirizine".Contains(searchText));
    //}

    public void BackToHome()
    {
        SceneManager.LoadScene(homePharmacistScene);
    }

    public void GoToNextPage()
    {
        SceneManager.LoadScene(nextMarkerLibraryScene);
    }

    public void GoToPreviousPage()
    {
        SceneManager.LoadScene(previousMarkerLibraryScene);
    }
}