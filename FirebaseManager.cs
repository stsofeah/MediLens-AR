using UnityEngine;
using Firebase;
using Firebase.Extensions;

public class FirebaseManager : MonoBehaviour
{
    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            DependencyStatus status = task.Result;

            if (status == DependencyStatus.Available)
            {
                Debug.Log("Firebase Ready");
            }
            else
            {
                Debug.LogError("Firebase Error: " + status);
            }
        });
    }
}