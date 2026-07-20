using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// AR scene UI manager:
/// - Back button (loads Home scene)
/// - Scan card: visible when no markers tracked, hidden when any marker is visible
/// </summary>
[DefaultExecutionOrder(-100)]
public sealed class ARUIManager : MonoBehaviour
{
    [Header("Scene Navigation")]
    [SerializeField] string homeSceneName = "Home";
    [SerializeField] string historySceneName = "ARHistory";

    [Header("UI References")]
    [SerializeField] GameObject scanCard;

    [Header("Debug")]
    [SerializeField] bool enableDebugLogs = true;

    int _markersVisibleCount;

    void Awake()
    {
        _markersVisibleCount = 0;
        SetScanCardVisible(true, "Awake");
    }

    void Start()
    {
        SetScanCardVisible(_markersVisibleCount <= 0, "Start");
    }

    /// <summary>
    /// Wire AR_UI/backButton OnClick to this method.
    /// </summary>
    public void OnBackPressed()
    {
        if (string.IsNullOrWhiteSpace(homeSceneName))
        {
            Debug.LogError("[ARUIManager] Home scene name is empty. Assign it in the Inspector.", this);
            return;
        }

        SceneManager.LoadScene(homeSceneName);
    }

    public void OnHistoryPressed()
    {
        if (string.IsNullOrWhiteSpace(historySceneName))
        {
            Debug.LogError("[ARUIManager] History scene name is empty.");
            return;
        }

        Debug.Log("[ARUIManager] Loading History scene: " + historySceneName);
        SceneManager.LoadScene(historySceneName);
    }

    /// <summary>
    /// Called by <see cref="MarkerTracker"/> when a marker becomes visible.
    /// </summary>
    public void NotifyMarkerFound()
    {
        _markersVisibleCount++;

        if (enableDebugLogs)
            Debug.Log("[ARUIManager] Marker Found. Count = " + _markersVisibleCount, this);

        // Fallback: hide immediately when any marker is found (do not wait on counter edge cases).
        HideScanCardImmediate();
    }

    /// <summary>
    /// Called by <see cref="MarkerTracker"/> when a marker becomes not visible.
    /// </summary>
    public void NotifyMarkerLost()
    {
        _markersVisibleCount = Mathf.Max(0, _markersVisibleCount - 1);

        if (enableDebugLogs)
            Debug.Log("[ARUIManager] Marker Lost. Count = " + _markersVisibleCount, this);

        // Fallback: show scan card only when no markers remain visible.
        if (_markersVisibleCount <= 0)
            SetScanCardVisible(true, "NotifyMarkerLost");
        else if (enableDebugLogs)
            Debug.Log("[ARUIManager] Scan card stays hidden (" + _markersVisibleCount + " marker(s) still visible).", this);
    }

    /// <summary>
    /// Immediate hide — safe to call from MarkerTracker as a fallback.
    /// </summary>
    public void HideScanCardImmediate()
    {
        if (enableDebugLogs)
            Debug.Log("[ARUIManager] HideScanCardImmediate()", this);

        SetScanCardVisible(false, "HideScanCardImmediate");
    }

    void SetScanCardVisible(bool isVisible, string reason)
    {
        if (scanCard == null)
        {
            Debug.LogError("[ARUIManager] scanCard reference is missing. Reason: " + reason, this);
            return;
        }

        scanCard.SetActive(isVisible);

        if (enableDebugLogs)
        {
            Debug.Log(
                "[ARUIManager] scanCard '" + scanCard.name + "' SetActive(" + isVisible + ") " +
                "| activeSelf=" + scanCard.activeSelf +
                " | activeInHierarchy=" + scanCard.activeInHierarchy +
                " | reason=" + reason,
                this);
        }
    }
}
