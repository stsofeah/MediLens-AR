using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;


/// <summary>
/// Login using credentials saved via PlayerPrefs when the user signs up.
/// Assign TMP_InputFields and a TMP_Text for error messages in the Inspector.
/// </summary>
public class LoginManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_InputField emailField;
    public TMP_InputField passwordField;

    [Header("Custom Login Error Panel (optional)")]
    public GameObject loginErrorPanel;
    public TMP_Text loginErrorText;

    [Tooltip("Shows validation or login errors (can leave empty while testing).")]
    public TMP_Text errorMessageText;

    [Header("Scenes (must match names in File > Build Settings)")]
    [Tooltip("Scene asset name for USER after successful login.")]
    public string userHomeSceneName = "User Homepage";

    [Tooltip("Scene asset name for PHARMACIST after successful login.")]
    public string pharmacistHomeSceneName = "Pharmacist Homepage";

    [Tooltip("Scene asset name for registration.")]
    public string signUpSceneName = "SignUp Version 2";

    [Tooltip("Scene asset name for password recovery.")]
    public string forgotPasswordSceneName = "ForgotPassword";

    [Header("Hardcoded Pharmacist Account")]
    [Tooltip("Pharmacist username that bypasses sign up.")]
    public string pharmacistUsername = "pharmacist";

    [Tooltip("Pharmacist password that bypasses sign up.")]
    public string pharmacistPassword = "pharmacist123";

    [Header("Error Panel Animation")]
    [Tooltip("Total duration of the pop + shake (seconds).")]
    public float errorAnimDuration = 0.35f;

    [Tooltip("Starting scale when the panel appears.")]
    public float errorStartScale = 0.85f;

    [Tooltip("Overshoot scale during the bounce.")]
    public float errorOvershootScale = 1.08f;

    [Tooltip("How far (in UI units) the tiny shake moves horizontally.")]
    public float errorShakeDistance = 10f;

    private FirebaseAuth auth;
    private FirebaseFirestore db;

    private RectTransform _errorPanelRect;
    private Vector3 _errorPanelBaseScale = Vector3.one;
    private Vector2 _errorPanelBaseAnchoredPos;
    private Coroutine _errorAnimRoutine;

    private void Awake()
    {
        CacheErrorPanelDefaults();
    }

    private void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;

        HideLoginErrorPanel();
        ClearError();
    }

    /// <summary>Wire the Login button OnClick to this.</summary>
    public void Login()
    {
        Debug.Log("[LoginManager] Login() — button clicked.");
        AttemptLogin();
    }

    public void AttemptLogin()
    {
        ClearError();

        string email = emailField != null ? emailField.text.Trim() : string.Empty;
        string password = passwordField != null ? passwordField.text : string.Empty;

        if (emailField == null || passwordField == null)
        {
            ShowError("Assign Email and Password fields in the LoginManager Inspector.");
            Debug.LogError("[LoginManager] emailField or passwordField is not assigned in the Inspector.");
            return;
        }

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowError("Please enter your login credentials.");
            return;
        }

        // Pharmacist path: hardcoded account, no sign-up required.
        //if (username == pharmacistUsername && password == pharmacistPassword)
        //{
        //    PlayerPrefs.SetString("MediLens_CurrentUsername", username);
        //    PlayerPrefs.Save();

        //    Debug.Log("[LoginManager] Pharmacist login successful. Loading scene: " + pharmacistHomeSceneName);
        //    HideLoginErrorPanel();
        //    SceneManager.LoadScene(pharmacistHomeSceneName);
        //    return;
        //}

        auth.SignInWithEmailAndPasswordAsync(email, password)
    .ContinueWithOnMainThread(task =>
    {
        if (task.IsCanceled || task.IsFaulted)
        {
            ShowError("Invalid email or password");
            Debug.LogError(task.Exception);
            return;
        }

        FirebaseUser user = task.Result.User;

        db.Collection("users")
        .Document(user.UserId)
        .GetSnapshotAsync()
        .ContinueWithOnMainThread(docTask =>
        {
            if (docTask.IsFaulted || !docTask.Result.Exists)
            {
                ShowError("User data not found.");
                return;
            }

            DocumentSnapshot doc = docTask.Result;

            string role = doc.GetValue<string>("role");

            PlayerPrefs.SetString("MediLens_CurrentUserID", user.UserId);
            PlayerPrefs.Save();

            Debug.Log("Role = " + role);

            HideLoginErrorPanel();

            if (role == "pharmacist")
            {
                SceneManager.LoadScene(pharmacistHomeSceneName);
            }
            else
            {
                SceneManager.LoadScene(userHomeSceneName);
            }
        });
    });
    }

    /// <summary>Wire the Sign Up button OnClick to this (or use SceneController.LoadSignUpScene).</summary>
    public void GoToSignUp()
    {
        Debug.Log("[LoginManager] GoToSignUp() — button clicked. Loading scene: " + signUpSceneName);
        SceneManager.LoadScene(signUpSceneName);
    }

    /// <summary>Wire the Forgot Password button OnClick to this.</summary>
    public void GoToForgotPassword()
    {
        Debug.Log("[LoginManager] GoToForgotPassword() — button clicked. Loading scene: " + forgotPasswordSceneName);
        SceneManager.LoadScene(forgotPasswordSceneName);
    }

    private void ClearError()
    {
        if (loginErrorText != null)
            loginErrorText.text = string.Empty;
        if (errorMessageText != null)
            errorMessageText.text = string.Empty;
    }

    private void ShowError(string message)
    {
        if (loginErrorPanel != null)
            loginErrorPanel.SetActive(true);

        if (loginErrorText != null)
            loginErrorText.text = message;

        if (errorMessageText != null)
            errorMessageText.text = message;
        Debug.LogWarning("[LoginManager] " + message);

        RestartErrorPanelAnimation();
    }

    private void HideLoginErrorPanel()
    {
        StopErrorPanelAnimation(resetTransform: true);
        if (loginErrorPanel != null)
            loginErrorPanel.SetActive(false);
    }

    private void CacheErrorPanelDefaults()
    {
        if (loginErrorPanel == null)
            return;

        _errorPanelRect = loginErrorPanel.GetComponent<RectTransform>();
        if (_errorPanelRect == null)
            return;

        _errorPanelBaseScale = _errorPanelRect.localScale;
        _errorPanelBaseAnchoredPos = _errorPanelRect.anchoredPosition;
    }

    private void RestartErrorPanelAnimation()
    {
        if (loginErrorPanel == null)
            return;

        if (_errorPanelRect == null)
            CacheErrorPanelDefaults();

        if (_errorPanelRect == null)
            return;

        StopErrorPanelAnimation(resetTransform: true);
        _errorAnimRoutine = StartCoroutine(AnimateErrorPanel());
    }

    private void StopErrorPanelAnimation(bool resetTransform)
    {
        if (_errorAnimRoutine != null)
        {
            StopCoroutine(_errorAnimRoutine);
            _errorAnimRoutine = null;
        }

        if (!resetTransform || _errorPanelRect == null)
            return;

        _errorPanelRect.localScale = _errorPanelBaseScale;
        _errorPanelRect.anchoredPosition = _errorPanelBaseAnchoredPos;
    }

    private System.Collections.IEnumerator AnimateErrorPanel()
    {
        // Beginner-friendly: simple time-based lerps with SmoothStep.
        float total = Mathf.Max(0.05f, errorAnimDuration);

        float scalePart = total * 0.70f; // pop + bounce
        float shakePart = total - scalePart; // tiny shake afterwards

        Vector3 startScale = _errorPanelBaseScale * Mathf.Max(0.01f, errorStartScale);
        Vector3 overshootScale = _errorPanelBaseScale * Mathf.Max(0.01f, errorOvershootScale);
        Vector3 endScale = _errorPanelBaseScale;

        _errorPanelRect.localScale = startScale;
        _errorPanelRect.anchoredPosition = _errorPanelBaseAnchoredPos;

        // Scale: start -> overshoot
        yield return ScaleOverTime(startScale, overshootScale, scalePart * 0.55f);
        // Scale: overshoot -> normal
        yield return ScaleOverTime(overshootScale, endScale, scalePart * 0.45f);

        // Shake: left-right-left, then return to center
        float d = Mathf.Abs(errorShakeDistance);
        Vector2 basePos = _errorPanelBaseAnchoredPos;
        Vector2 left = basePos + Vector2.left * d;
        Vector2 right = basePos + Vector2.right * d;

        if (shakePart > 0.001f)
        {
            yield return MoveOverTime(basePos, left, shakePart * 0.25f);
            yield return MoveOverTime(left, right, shakePart * 0.35f);
            yield return MoveOverTime(right, left, shakePart * 0.25f);
            yield return MoveOverTime(left, basePos, shakePart * 0.15f);
        }

        _errorPanelRect.localScale = endScale;
        _errorPanelRect.anchoredPosition = basePos;
        _errorAnimRoutine = null;
    }

    private System.Collections.IEnumerator ScaleOverTime(Vector3 from, Vector3 to, float duration)
    {
        duration = Mathf.Max(0.01f, duration);
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime; // UI animations feel better unaffected by timescale
            float u = Mathf.Clamp01(t / duration);
            u = Mathf.SmoothStep(0f, 1f, u);
            _errorPanelRect.localScale = Vector3.LerpUnclamped(from, to, u);
            yield return null;
        }
        _errorPanelRect.localScale = to;
    }

    private System.Collections.IEnumerator MoveOverTime(Vector2 from, Vector2 to, float duration)
    {
        duration = Mathf.Max(0.01f, duration);
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float u = Mathf.Clamp01(t / duration);
            u = Mathf.SmoothStep(0f, 1f, u);
            _errorPanelRect.anchoredPosition = Vector2.LerpUnclamped(from, to, u);
            yield return null;
        }
        _errorPanelRect.anchoredPosition = to;
    }

    /// <summary>
    /// Same key scheme as SignUpManager so signup and login stay in sync.
    /// </summary>
    internal static string PasswordKeyForUser(string username)
    {
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(username.Trim());
        return "MediLens_Pwd_" + System.Convert.ToBase64String(bytes);
    }
}
