using UnityEngine;
using UnityEngine.SceneManagement;

public class WelcomeManager : MonoBehaviour
{
    public void ContinueToAR()
    {
        SceneManager.LoadScene("AR");
    }

    public void SkipTutorial()
    {
        SceneManager.LoadScene("AR");
    }
}