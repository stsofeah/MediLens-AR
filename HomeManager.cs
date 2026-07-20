using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;

/// <summary>
/// Handles navigation from the Home scene (MediLens AR).
/// Scene names must match exactly what you see in File > Build Settings > Scenes In Build.
/// </summary>
public class HomeManager : MonoBehaviour
{
    [Header("Welcome Text")]
    public TMP_Text hiText;

    private FirebaseAuth auth;
    private FirebaseFirestore db;

    [Header("Scene names (must match File > Build Settings)")]
    [Tooltip("Exact name of your Profile scene asset.")]
    public string profileSceneName = "Profile";

    [Tooltip("Exact name of your AR / scan scene asset.")]
    public string arSceneName = "AR";

    [Tooltip("Exact name of your My Reminders scene asset.")]
    public string myRemindersSceneName = "MyReminders";

    private void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;

        LoadUsername();
    }

    /// <summary>Wire the Profile button OnClick to this.</summary>
    public void GoToProfile()
    {
        Debug.Log("[HomeManager] Profile clicked — loading scene: " + profileSceneName);
        SceneManager.LoadScene(profileSceneName);
    }

    /// <summary>Wire the Scan Card button OnClick to this.</summary>
    public void GoToAR()
    {
        Debug.Log("[HomeManager] Scan Card clicked — loading scene: " + arSceneName);
        SceneManager.LoadScene(arSceneName);
    }

    /// <summary>Wire the Reminder Card button OnClick to this.</summary>
    public void GoToMyReminders()
    {
        Debug.Log("[HomeManager] Reminder Card clicked — loading scene: " + myRemindersSceneName);
        SceneManager.LoadScene(myRemindersSceneName);
    }

    private void LoadUsername()
    {
        if (auth.CurrentUser == null)
            return;

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

                      hiText.text = "Hi, " + username;
                  }
              }
          });
    }
}
