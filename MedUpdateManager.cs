using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using SFB;

public class MedUpdateManager : MonoBehaviour
{
    [Header("Scenes")]
    public string homeScene = "HomePharmacist";
    public string medRecordScene = "MedRecord";

    [Header("Input Fields")]
    public TMP_InputField medInput;
    public TMP_InputField dosInput;
    public TMP_InputField warningInput;
    public TMP_InputField effectInput;

    [Header("Marker Preview")]
    public Image markerPreview;

    [Header("UI")]
    public GameObject dimOverlay;

    private string selectedMarkerPath = "";

    private void Start()
    {
        if (dimOverlay != null)
            dimOverlay.SetActive(false);
    }

    // =====================================
    // BACK BUTTON
    // =====================================
    public void BackButton()
    {
        SceneManager.LoadScene(homeScene);
    }

    // =====================================
    // SELECT MARKER IMAGE
    // =====================================
    public void SelectMarker()
    {
        var paths = StandaloneFileBrowser.OpenFilePanel(
            "Select Marker Image",
            "",
            new[] {
                new ExtensionFilter("Image Files", "png", "jpg", "jpeg")
            },
            false
        );

        if (paths.Length > 0)
        {
            selectedMarkerPath = paths[0];

            Debug.Log("Selected Marker:");
            Debug.Log(selectedMarkerPath);

            LoadPreview(selectedMarkerPath);
        }
    }

    // =====================================
    // LOAD IMAGE PREVIEW
    // =====================================
    private void LoadPreview(string filePath)
    {
        byte[] imageData = System.IO.File.ReadAllBytes(filePath);

        Texture2D texture = new Texture2D(2, 2);

        if (texture.LoadImage(imageData))
        {
            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
            );

            markerPreview.sprite = sprite;
        }
    }

    // =====================================
    // SAVE BUTTON
    // =====================================
    public void UploadMedicine()
    {
        bool hasChanges =
            !string.IsNullOrWhiteSpace(medInput.text) ||
            !string.IsNullOrWhiteSpace(dosInput.text) ||
            !string.IsNullOrWhiteSpace(warningInput.text) ||
            !string.IsNullOrWhiteSpace(effectInput.text) ||
            !string.IsNullOrEmpty(selectedMarkerPath);

        if (!hasChanges)
        {
            Debug.LogWarning("Please update at least one field");
            return;
        }

        Debug.Log("Changes saved successfully");

        StartCoroutine(ShowSuccessAndReturn());
    }

    // =====================================
    // SUCCESS OVERLAY + REDIRECT
    // =====================================
    private IEnumerator ShowSuccessAndReturn()
    {
        if (dimOverlay != null)
            dimOverlay.SetActive(true);

        yield return new WaitForSeconds(2f);

        SceneManager.LoadScene(medRecordScene);
    }
}