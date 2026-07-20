using UnityEngine;
using UnityEngine.Video;

public class VideoTutorialManager : MonoBehaviour
{
    [Header("Video")]
    public GameObject videoBackground;
    public VideoPlayer videoPlayer;

    [Header("AR Objects")]
    public GameObject medicineModel;
    public GameObject medCard;

    [Header("Panels")]
    public GameObject purposePanel;
    public GameObject dosagePanel;
    public GameObject warningPanel;

    private void Start()
    {
        // Video popup hidden when app starts
        videoBackground.SetActive(false);

        // Make sure video doesn't auto play
        videoPlayer.playOnAwake = false;
        videoPlayer.Stop();
    }

    public void OpenVideo()
    {
        // Hide AR contents
        medicineModel.SetActive(false);
        medCard.SetActive(false);

        if (purposePanel != null)
            purposePanel.SetActive(false);

        if (dosagePanel != null)
            dosagePanel.SetActive(false);

        if (warningPanel != null)
            warningPanel.SetActive(false);

        // Show video popup
        videoBackground.SetActive(true);

        // Restart video from beginning
        videoPlayer.Stop();
        videoPlayer.time = 0;
        videoPlayer.Play();
    }

    public void CloseVideo()
    {
        // Stop video completely
        videoPlayer.Stop();

        // Hide video popup
        videoBackground.SetActive(false);

        // Show AR contents again
        medicineModel.SetActive(true);
        medCard.SetActive(true);
    }
}