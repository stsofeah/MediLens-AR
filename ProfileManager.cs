using UnityEngine;
using UnityEngine.SceneManagement;

using TMPro;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;

/// <summary>
/// Navigation and logout for the Profile scene (MediLens AR).
/// Scene names must match File > Build Settings > Scenes In Build.
/// </summary>
public class ProfileManager : MonoBehaviour
{
    /// <summary>
    /// Written by LoginManager on successful login. Clearing this logs the user out
    /// without deleting saved accounts (password keys stay for next login).
    /// </summary>
    public const string CurrentUsernameKey = "MediLens_CurrentUsername";

    [Header("Profile Information")]
    public TMP_Text usernameText;

    public TMP_Text emailText;

    private FirebaseAuth auth;
    private FirebaseFirestore db;

    [Header("Scenes (must match Build Settings)")]
    public string homeSceneName = "Home";

    [Tooltip("Exact login scene name in your project")]
    public string loginSceneName = "Login Version 2";

    private void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;

        LoadProfile();
    }

    /// <summary>Wire the Back button OnClick to this.</summary>
    public void GoBackToHome()
    {
        Debug.Log("[ProfileManager] Back pressed — loading Home scene: " + homeSceneName);
        SceneManager.LoadScene(homeSceneName);
    }

    /// <summary>Wire the Logout button OnClick to this.</summary>
    public void Logout()
    {
        // Remove "who is logged in" so the app treats the user as signed out.
        PlayerPrefs.DeleteKey(CurrentUsernameKey);
        PlayerPrefs.Save();

        Debug.Log("[ProfileManager] Logout — cleared session key: " + CurrentUsernameKey + ". Loading Login scene: " + loginSceneName);
        SceneManager.LoadScene(loginSceneName);
    }

    private void LoadProfile()
    {
        if (auth.CurrentUser == null)
        {
            Debug.LogWarning("No user logged in.");
            return;
        }

        string uid = auth.CurrentUser.UserId;

        db.Collection("users")
          .Document(uid)
          .GetSnapshotAsync()
          .ContinueWithOnMainThread(task =>
          {
              if (task.IsCompleted)
              {
                  DocumentSnapshot snapshot = task.Result;

                  if (snapshot.Exists)
                  {
                      string username =
                          snapshot.GetValue<string>("username");

                      string email =
                          snapshot.GetValue<string>("email");

                      if (!string.IsNullOrEmpty(username))
                      {
                          username =
                              char.ToUpper(username[0]) +
                              username.Substring(1);
                      }

                      usernameText.text = username;
                      emailText.text = email;
                  }
              }
          });
    }
}
