using UnityEngine;
using UnityEngine.SceneManagement;

public class ARHistoryManager : MonoBehaviour
{
    public string homeSceneName = "Home";
    public string arSceneName = "AR";

    public void GoToHome()
    {
        SceneManager.LoadScene(homeSceneName);
    }

    public void GoToAR()
    {
        SceneManager.LoadScene(arSceneName);
    }
}