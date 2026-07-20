using UnityEngine;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Collections.Generic;

public class FirebaseHistoryManager : MonoBehaviour
{
    public static FirebaseHistoryManager Instance;

    FirebaseFirestore db;

    private void Awake()
    {
        Instance = this;
        db = FirebaseFirestore.DefaultInstance;
    }

    public void SaveHistory(string medicineName)
    {
        FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;

        if (user == null)
        {
            Debug.LogError("No user logged in.");
            return;
        }

        Dictionary<string, object> historyData =
            new Dictionary<string, object>()
        {
            { "medicineName", medicineName },
            { "scanDate", Timestamp.GetCurrentTimestamp() }
        };

        db.Collection("users")
          .Document(user.UserId)
          .Collection("scanHistory")
          .AddAsync(historyData)
          .ContinueWithOnMainThread(task =>
          {
              if (task.IsCompleted)
              {
                  Debug.Log("History Saved: " + medicineName);
              }
          });
    }
}