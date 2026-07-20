using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Firebase.Auth;
using Firebase.Extensions;

/// <summary>
/// Forgot Password UI for MediLens AR — validates email locally (no backend yet).
/// </summary>
public class ForgotPasswordManager : MonoBehaviour
{
    [Header("Input")]
    public TMP_InputField emailInput;

    [Header("Buttons")]
    public Button sendResetButton;

    [Header("Feedback Panels (Success animation)")]
    [Tooltip("Add a CanvasGroup component to your DimOverlay, then drag DimOverlay here.")]
    public CanvasGroup dimOverlayCanvasGroup;

    [Tooltip("The RectTransform of your Success Panel (the popup that scales).")]
    public RectTransform successPanelRect;

    public GameObject errorPanel;

    [Header("Feedback Text")]
    public TMP_Text successText;
    public TMP_Text errorText;

    [Header("Timing")]
    [Tooltip("How long to show the success message before loading the Login scene.")]
    public float successDisplaySeconds = 1.5f;

    [Tooltip("How long the DimOverlay fade and SuccessPanel pop take.")]
    public float successPopupAnimDuration = 0.35f;

    [Header("Scenes (must match names in File > Build Settings)")]
    public string loginSceneName = "Login Version 2";

    private Coroutine _successAnimCoroutine;
    private FirebaseAuth auth;

    private void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        ResetSuccessVisuals();

        if (errorPanel != null)
            errorPanel.SetActive(false);
        if (successText != null)
            successText.text = string.Empty;
        if (errorText != null)
            errorText.text = string.Empty;

        // In case this screen is shown again in the same session, keep the button usable.
        if (sendResetButton != null)
            sendResetButton.interactable = true;
    }

    /// <summary>Wire your Email / Send reset button OnClick to this.</summary>
    public void SendResetLink()
    {
        if (emailInput == null)
        {
            Debug.LogError("[ForgotPasswordManager] emailInput is not assigned. Drag your Email Input Field into the ForgotPasswordManager in the Inspector.");
            return;
        }

        string email = emailInput.text.Trim();

        if (string.IsNullOrEmpty(email))
        {
            ShowError("Please enter your email.");
            return;
        }

        // Simple check: must contain both @ and . (UI-only validation for now).
        if (!email.Contains("@") || !email.Contains("."))
        {
            ShowError("Please enter a valid email address.");
            return;
        }

        auth.SendPasswordResetEmailAsync(email)
     .ContinueWithOnMainThread(task =>
     {
         if (task.IsCompleted)
         {
             ShowSuccess("Password reset email sent!");

             if (sendResetButton != null)
                 sendResetButton.interactable = false;

             StartCoroutine(LoadLoginAfterSuccessDelay());
         }
         else
         {
             Debug.LogError(task.Exception);
             ShowError("Email not found or reset failed.");
         }
     });
    }

    /// <summary>Wire Back button and Login text OnClick to this.</summary>
    public void BackToLogin()
    {
        Debug.Log("[ForgotPasswordManager] BackToLogin() — loading scene: " + loginSceneName);
        SceneManager.LoadScene(loginSceneName);
    }

    private IEnumerator LoadLoginAfterSuccessDelay()
    {
        yield return new WaitForSeconds(successDisplaySeconds);
        SceneManager.LoadScene(loginSceneName);
    }

    private void ShowError(string message)
    {
        if (_successAnimCoroutine != null)
        {
            StopCoroutine(_successAnimCoroutine);
            _successAnimCoroutine = null;
        }

        ResetSuccessVisuals();

        if (errorText != null)
            errorText.text = message;
        if (errorPanel != null)
            errorPanel.SetActive(true);
    }

    private void ShowSuccess(string message)
    {
        if (errorPanel != null)
            errorPanel.SetActive(false);

        if (successText != null)
            successText.text = message;

        // Starting pose for the animation (shown on first frame before the coroutine runs).
        if (dimOverlayCanvasGroup != null)
        {
            dimOverlayCanvasGroup.alpha = 0f;
            dimOverlayCanvasGroup.blocksRaycasts = true;
            dimOverlayCanvasGroup.interactable = false;
            dimOverlayCanvasGroup.gameObject.SetActive(true);
        }

        if (successPanelRect != null)
        {
            successPanelRect.localScale = Vector3.one * 0.85f;
            successPanelRect.gameObject.SetActive(true);
        }

        if (_successAnimCoroutine != null)
            StopCoroutine(_successAnimCoroutine);

        _successAnimCoroutine = StartCoroutine(AnimateSuccessPopup());
    }

    /// <summary>
    /// Fades the dim using CanvasGroup alpha, and scales the panel: 0.85 → 1.08 → 1.0.
    /// Uses unscaled time so it still runs if the game is paused (timeScale = 0).
    /// </summary>
    private IEnumerator AnimateSuccessPopup()
    {
        float duration = Mathf.Max(0.0001f, successPopupAnimDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Pop: first half grows past final size, second half settles to 1.0.
            float scaleValue;
            if (t < 0.5f)
                scaleValue = Mathf.Lerp(0.85f, 1.08f, t / 0.5f);
            else
                scaleValue = Mathf.Lerp(1.08f, 1.0f, (t - 0.5f) / 0.5f);

            if (successPanelRect != null)
                successPanelRect.localScale = Vector3.one * scaleValue;

            if (dimOverlayCanvasGroup != null)
                dimOverlayCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t);

            yield return null;
        }

        if (successPanelRect != null)
            successPanelRect.localScale = Vector3.one;
        if (dimOverlayCanvasGroup != null)
            dimOverlayCanvasGroup.alpha = 1f;

        _successAnimCoroutine = null;
    }

    /// <summary>Puts success UI back to a clean hidden state for next time.</summary>
    private void ResetSuccessVisuals()
    {
        if (dimOverlayCanvasGroup != null)
        {
            dimOverlayCanvasGroup.alpha = 0f;
            dimOverlayCanvasGroup.blocksRaycasts = false;
            dimOverlayCanvasGroup.gameObject.SetActive(false);
        }

        if (successPanelRect != null)
        {
            successPanelRect.localScale = Vector3.one;
            successPanelRect.gameObject.SetActive(false);
        }
    }
}
