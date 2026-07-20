using UnityEngine;
using TMPro;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;
using System;
using System.Collections.Generic;

public class ARHistoryLoader : MonoBehaviour
{
    [Header("UI")]
    public Transform contentParent;
    public GameObject historyCardPrefab;

    FirebaseFirestore db;

    private void Start()
    {
        db = FirebaseFirestore.DefaultInstance;

        LoadHistory();
    }

    void LoadHistory()
    {
        FirebaseUser user =
            FirebaseAuth.DefaultInstance.CurrentUser;

        if (user == null)
        {
            Debug.LogError("No user logged in.");
            return;
        }

        db.Collection("users")
          .Document(user.UserId)
          .Collection("scanHistory")
          .OrderByDescending("scanDate")
          .GetSnapshotAsync()
          .ContinueWithOnMainThread(task =>
          {
              if (!task.IsCompleted)
                  return;

              QuerySnapshot snapshot = task.Result;

              foreach (DocumentSnapshot doc in snapshot.Documents)
              {
                  CreateHistoryCard(doc);
              }
          });
    }

    void CreateHistoryCard(DocumentSnapshot doc)
    {
        GameObject card =
            Instantiate(historyCardPrefab,
                        contentParent);

        string medicine =
            doc.GetValue<string>("medicineName");

        Timestamp timestamp =
            doc.GetValue<Timestamp>("scanDate");

        DateTime date =
            timestamp.ToDateTime();

        string formattedDate =
            date.ToString("dd MMM yyyy");

        TMP_Text[] texts =
            card.GetComponentsInChildren<TMP_Text>();

        foreach (TMP_Text t in texts)
        {
            if (t.name == "Title")
                t.text = medicine;

            if (t.name == "Value")
                t.text = formattedDate;
        }
    }
}