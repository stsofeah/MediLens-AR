using UnityEngine;
using Firebase.Auth;
using Firebase.Firestore;

/// <summary>
/// Per-marker Vuforia handler.
/// IMPORTANT: Never disable the Image Target root (this GameObject).
/// Only hide/show child content via SetActive on modelRoot, medCard, and panels.
/// </summary>
public sealed class MarkerTracker : MonoBehaviour
{
    [Header("Medicine")]
    [SerializeField] string medicineName;

    [Header("Managers")]
    [SerializeField] ARUIManager aruiManager;

    [Header("Marker Content (children only — NOT the Image Target root)")]
    [Tooltip("3D model (e.g., marker-inhaler/inhaler).")]
    [SerializeField] GameObject modelRoot;

    [Tooltip("MedCard UI root.")]
    [SerializeField] GameObject medCard;

    [Tooltip("Optional. Assign MedCanvas_* here to force it ACTIVE (Vuforia/detection stays on parent).")]
    [SerializeField] GameObject medCanvasRoot;

    [Header("Card Controller")]
    [SerializeField] MedicineCardController cardController;

    [Header("Debug")]
    [SerializeField] bool enableDebugLogs = true;

    bool _isVisible;

    void Awake()
    {
        EnsureImageTargetRootActive();
        EnsureMedCanvasActive();
        HideContent( notifyManager: false);
    }

    void Start()
    {
        EnsureImageTargetRootActive();
        EnsureMedCanvasActive();
        HideContent(notifyManager: false);

        if (enableDebugLogs)
        {
            Debug.Log(
                "[MarkerTracker] Start on '" + gameObject.name +
                "' | ImageTarget active=" + gameObject.activeInHierarchy +
                " | aruiManager=" + (aruiManager != null),
                this);
        }
    }

    /// <summary>Vuforia → On Target Found.</summary>
    public void OnTargetFound()
    {
        Debug.Log("[MarkerTracker] Target Found: " + gameObject.name, this);

        if (_isVisible)
        {
            if (enableDebugLogs)
                Debug.Log("[MarkerTracker] Target Found ignored (already visible): " + gameObject.name, this);
            return;
        }

        ShowContent(notifyManager: true);

        FirebaseHistoryManager.Instance.SaveHistory(medicineName);
    }

    /// <summary>Vuforia → On Target Lost.</summary>
    public void OnTargetLost()
    {
        Debug.Log("[MarkerTracker] Target Lost: " + gameObject.name, this);

        if (!_isVisible)
        {
            if (enableDebugLogs)
                Debug.Log("[MarkerTracker] Target Lost ignored (already hidden): " + gameObject.name, this);
            return;
        }

        HideContent(notifyManager: true);
    }

    /// <summary>
    /// Hides model + MedCard + panels. Does NOT disable this Image Target GameObject.
    /// </summary>
    void HideContent(bool notifyManager)
    {
        _isVisible = false;

        EnsureImageTargetRootActive();
        EnsureMedCanvasActive();

        SetChildActive(modelRoot, false);
        SetChildActive(medCard, false);

        if (cardController != null)
            cardController.OnMarkerLost();
        else
            Debug.LogError("[MarkerTracker] cardController missing on " + gameObject.name, this);

        if (notifyManager)
            NotifyARUIManager(markerFound: false);
    }

    /// <summary>
    /// Shows model + MedCard; panels stay hidden until buttons are pressed.
    /// </summary>
    void ShowContent(bool notifyManager)
    {
        _isVisible = true;

        EnsureImageTargetRootActive();
        EnsureMedCanvasActive();

        SetChildActive(modelRoot, true);
        SetChildActive(medCard, true);

        if (cardController != null)
            cardController.OnMarkerFound();
        else
            Debug.LogError("[MarkerTracker] cardController missing on " + gameObject.name, this);

        if (notifyManager)
            NotifyARUIManager(markerFound: true);
    }

    void EnsureImageTargetRootActive()
    {
        // Never disable the Vuforia Image Target root from code.
        if (!gameObject.activeSelf)
        {
            Debug.LogWarning(
                "[MarkerTracker] Image Target '" + gameObject.name +
                "' is disabled. Enable it in the Hierarchy so Vuforia can detect it.",
                this);
        }
    }

    void EnsureMedCanvasActive()
    {
        if (medCanvasRoot == null)
            return;

        if (!medCanvasRoot.activeSelf)
            medCanvasRoot.SetActive(true);
    }

    static void SetChildActive(GameObject go, bool active)
    {
        if (go == null)
            return;

        go.SetActive(active);
    }

    void NotifyARUIManager(bool markerFound)
    {
        if (aruiManager == null)
        {
            Debug.LogError(
                "[MarkerTracker] aruiManager not assigned on '" + gameObject.name +
                "'. scanCard will not update.",
                this);
            return;
        }

        if (enableDebugLogs)
        {
            Debug.Log(
                "[MarkerTracker] → ARUIManager." + (markerFound ? "NotifyMarkerFound" : "NotifyMarkerLost") +
                "() from '" + gameObject.name + "'",
                this);
        }

        if (markerFound)
        {
            aruiManager.NotifyMarkerFound();
            aruiManager.HideScanCardImmediate();
        }
        else
        {
            aruiManager.NotifyMarkerLost();
        }
    }
}
