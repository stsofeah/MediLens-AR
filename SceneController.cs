using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Simple scene navigation you can hook up from Button OnClick events.
/// Add this to any GameObject in your scene (often an empty "Managers" object).
/// </summary>
public class SceneController : MonoBehaviour
{
    [Header("Scenes (must match names in File > Build Settings)")]
    public string loginSceneName = "Login Version 2";
    public string signUpSceneName = "SignUp Version 2";
    public string homeSceneName = "Home";

    public void LoadLoginScene()
    {
        Debug.Log("[SceneController] LoadLoginScene() — loading: " + loginSceneName);
        SceneManager.LoadScene(loginSceneName);
    }

    public void LoadSignUpScene()
    {
        Debug.Log("[SceneController] LoadSignUpScene() — loading: " + signUpSceneName);
        SceneManager.LoadScene(signUpSceneName);
    }

    public void LoadHomeScene()
    {
        Debug.Log("[SceneController] LoadHomeScene() — loading: " + homeSceneName);
        SceneManager.LoadScene(homeSceneName);
    }
}
