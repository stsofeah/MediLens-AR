using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Collections.Generic;

/// <summary>
/// Handles sign up validation, local save with PlayerPrefs, and success popup flow.
/// </summary>
public class SignUpManager : MonoBehaviour
{
    [Header("Input Fields")]
    public TMP_InputField usernameField;
    public TMP_InputField emailField;
    public TMP_InputField passwordField;
    public TMP_InputField confirmPasswordField;

    [Header("Inline Error Panels")]
    public GameObject emailErrorPanel;
    public TMP_Text emailErrorText;
    public GameObject passwordErrorPanel;
    public TMP_Text passwordErrorText;
    public GameObject confirmErrorPanel;
    public TMP_Text confirmErrorText;

    [Header("Success UI")]
    public GameObject dimOverlay;
    public GameObject signUpSuccessPanel;
    public TMP_Text successText;

    [Header("Timing")]
    [Tooltip("How long to keep success popup visible before loading Login scene.")]
    public float successDisplaySeconds = 1.5f;

    [Header("Success Popup Animation")]
    [Tooltip("Animation duration for pop + bounce.")]
    public float successAnimDuration = 0.35f;
    public float successStartScale = 0.85f;
    public float successOvershootScale = 1.08f;

    [Header("Scenes (must match names in File > Build Settings)")]
    [Tooltip("Login scene asset name to return to after sign up or Back.")]
    public string loginSceneName = "Login Version 2";

    private FirebaseAuth auth;
    private FirebaseFirestore db;
    private RectTransform _successPanelRect;
    private Vector3 _successBaseScale = Vector3.one;
    private Coroutine _successFlowRoutine;

    private static readonly Regex EmailRegex = new Regex(
        @"^[^\s@]+@[^\s@]+\.[^\s@]+$",
        RegexOptions.Compiled);

    private static readonly Regex PasswordRegex = new Regex(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,}$",
        RegexOptions.Compiled);

    private void Awake()
    {
        if (signUpSuccessPanel != null)
        {
            _successPanelRect = signUpSuccessPanel.GetComponent<RectTransform>();
            if (_successPanelRect != null)
                _successBaseScale = _successPanelRect.localScale;
        }
    }

    private void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;

        HideAllErrorPanels();
        HideSuccessUI();
    }

    /// <summary>Wire the Create Account / Sign Up submit button OnClick to this.</summary>
    public void SignUp()
    {
        Debug.Log("[SignUpManager] SignUp() - button clicked.");
        AttemptSignUp();
    }

    public void AttemptSignUp()
    {
        StopSuccessFlowIfRunning();
        HideAllErrorPanels();
        HideSuccessUI();

        if (usernameField == null || emailField == null || passwordField == null || confirmPasswordField == null)
        {
            Debug.LogError("[SignUpManager] One or more TMP_InputFields are not assigned in the Inspector.");
            ShowPasswordError("Please assign all input fields in SignUpManager.");
            return;
        }

        string username = usernameField.text.Trim();
        string email = emailField.text.Trim();
        string password = passwordField.text;
        string confirm = confirmPasswordField.text;
        bool hasError = false;

        if (string.IsNullOrEmpty(username))
        {
            ShowPasswordError("Please enter a username.");
            hasError = true;
        }

        if (string.IsNullOrEmpty(email))
        {
            ShowEmailError("Please enter your email.");
        }
        else if (!EmailRegex.IsMatch(email))
        {
            ShowEmailError("Please enter a valid email address.");
            hasError = true;
        }

        if (string.IsNullOrEmpty(password))
        {
            ShowPasswordError("Please enter a password.");
        }
        else if (!PasswordRegex.IsMatch(password))
        {
            ShowPasswordError("Password must be 8+ chars with upper, lower, number, and special.");
            hasError = true;
        }

        if (string.IsNullOrEmpty(confirm))
        {
            ShowConfirmError("Please confirm your password.");
        }
        else if (password != confirm)
        {
            ShowConfirmError("Password and confirm password do not match.");
            hasError = true;
        }

        if (hasError)
        {
            return;
        }

        auth.CreateUserWithEmailAndPasswordAsync(email, password)
    .ContinueWithOnMainThread(task =>
    {
        if (task.IsCanceled || task.IsFaulted)
        {
            ShowEmailError("Account creation failed.");
            Debug.LogError(task.Exception);
            return;
        }

        FirebaseUser user = task.Result.User;

        Dictionary<string, object> userData = new Dictionary<string, object>()
        {
            { "username", username },
            { "email", email },
            { "role", "user" }
        };

        db.Collection("users")
          .Document(user.UserId)
          .SetAsync(userData)
          .ContinueWithOnMainThread(saveTask =>
          {
              if (saveTask.IsCompleted)
              {
                  Debug.Log("User saved to Firestore");

                  _successFlowRoutine =
                      StartCoroutine(ShowSuccessAndLoadLogin());
              }
          });
    });
    }

    /// <summary>Wire the Back to Login button OnClick to this.</summary>
    public void BackToLogin()
    {
        Debug.Log("[SignUpManager] BackToLogin() - button clicked. Loading scene: " + loginSceneName);
        SceneManager.LoadScene(loginSceneName);
    }

    private IEnumerator ShowSuccessAndLoadLogin()
    {
        ShowSuccessUI();
        yield return StartCoroutine(AnimateSuccessPopup());
        yield return new WaitForSecondsRealtime(Mathf.Max(0f, successDisplaySeconds));

        Debug.Log("[SignUpManager] Loading login scene after sign up: " + loginSceneName);
        SceneManager.LoadScene(loginSceneName);
    }

    private IEnumerator AnimateSuccessPopup()
    {
        if (_successPanelRect == null)
            yield break;

        float total = Mathf.Max(0.05f, successAnimDuration);
        float firstPart = total * 0.55f;
        float secondPart = total * 0.45f;

        Vector3 start = _successBaseScale * Mathf.Max(0.01f, successStartScale);
        Vector3 overshoot = _successBaseScale * Mathf.Max(0.01f, successOvershootScale);
        Vector3 end = _successBaseScale;

        _successPanelRect.localScale = start;

        yield return StartCoroutine(LerpScale(_successPanelRect, start, overshoot, firstPart));
        yield return StartCoroutine(LerpScale(_successPanelRect, overshoot, end, secondPart));

        _successPanelRect.localScale = end;
    }

    private IEnumerator LerpScale(RectTransform target, Vector3 from, Vector3 to, float duration)
    {
        duration = Mathf.Max(0.01f, duration);
        float t = 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float u = Mathf.Clamp01(t / duration);
            u = Mathf.SmoothStep(0f, 1f, u);
            target.localScale = Vector3.LerpUnclamped(from, to, u);
            yield return null;
        }

        target.localScale = to;
    }

    private void StopSuccessFlowIfRunning()
    {
        if (_successFlowRoutine != null)
        {
            StopCoroutine(_successFlowRoutine);
            _successFlowRoutine = null;
        }
    }

    private void HideAllErrorPanels()
    {
        SetPanelVisible(emailErrorPanel, false);
        SetPanelVisible(passwordErrorPanel, false);
        SetPanelVisible(confirmErrorPanel, false);
    }

    private void ShowEmailError(string message)
    {
        SetPanelVisible(emailErrorPanel, true);
        if (emailErrorText != null)
            emailErrorText.text = message;
        Debug.LogWarning("[SignUpManager] " + message);
    }

    private void ShowPasswordError(string message)
    {
        SetPanelVisible(passwordErrorPanel, true);
        if (passwordErrorText != null)
            passwordErrorText.text = message;
        Debug.LogWarning("[SignUpManager] " + message);
    }

    private void ShowConfirmError(string message)
    {
        SetPanelVisible(confirmErrorPanel, true);
        if (confirmErrorText != null)
            confirmErrorText.text = message;
        Debug.LogWarning("[SignUpManager] " + message);
    }

    private void ShowSuccessUI()
    {
        if (dimOverlay != null)
            dimOverlay.SetActive(true);

        if (signUpSuccessPanel != null)
            signUpSuccessPanel.SetActive(true);

        if (successText != null)
            successText.text = "Account created successfully!";

        Debug.Log("[SignUpManager] Showing success popup.");
    }

    private void HideSuccessUI()
    {
        if (dimOverlay != null)
            dimOverlay.SetActive(false);

        if (signUpSuccessPanel != null)
            signUpSuccessPanel.SetActive(false);

        if (_successPanelRect != null)
            _successPanelRect.localScale = _successBaseScale;
    }

    private void SetPanelVisible(GameObject panel, bool isVisible)
    {
        if (panel != null)
            panel.SetActive(isVisible);
    }
}
