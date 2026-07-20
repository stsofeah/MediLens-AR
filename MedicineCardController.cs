using UnityEngine;

/// <summary>
/// Panel visibility for one medicine marker.
/// Attach to MedCanvas_* (keep MedCanvas ACTIVE; hide panels via SetActive).
/// Does not disable the Image Target or MedCanvas root.
/// </summary>
public sealed class MedicineCardController : MonoBehaviour
{
    [Header("Panels (hide/show only these children)")]
    [SerializeField] GameObject purposePanel;
    [SerializeField] GameObject dosagePanel;
    [SerializeField] GameObject warningPanel;

    void Awake()
    {
        HideAllPanels();
    }

    void Start()
    {
        HideAllPanels();
    }

    /// <summary>Called when marker is found — panels stay hidden until buttons are pressed.</summary>
    public void OnMarkerFound()
    {
        HideAllPanels();
    }

    /// <summary>Called when marker is lost — reset all panels.</summary>
    public void OnMarkerLost()
    {
        HideAllPanels();
    }

    public void ShowPurposePanel()
    {
        SetActiveSafe(purposePanel, true, nameof(purposePanel));
    }

    public void ShowDosagePanel()
    {
        SetActiveSafe(dosagePanel, true, nameof(dosagePanel));
    }

    public void ShowWarningPanel()
    {
        SetActiveSafe(warningPanel, true, nameof(warningPanel));
    }

    public void ClosePurpose()
    {
        SetActiveSafe(purposePanel, false, nameof(purposePanel));
    }

    public void CloseDosage()
    {
        SetActiveSafe(dosagePanel, false, nameof(dosagePanel));
    }

    public void CloseWarning()
    {
        SetActiveSafe(warningPanel, false, nameof(warningPanel));
    }

    void HideAllPanels()
    {
        SetActiveSafe(purposePanel, false, nameof(purposePanel));
        SetActiveSafe(dosagePanel, false, nameof(dosagePanel));
        SetActiveSafe(warningPanel, false, nameof(warningPanel));
    }

    static void SetActiveSafe(GameObject go, bool active, string fieldName)
    {
        if (go == null)
        {
            Debug.LogError("[MedicineCardController] Missing reference: " + fieldName);
            return;
        }

        go.SetActive(active);
    }
}
