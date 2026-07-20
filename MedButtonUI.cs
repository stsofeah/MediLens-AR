using UnityEngine;

public class MedButtonUI : MonoBehaviour
{
    public GameObject dosagePanel;
    public GameObject warningPanel;

    public void ShowDosagePanel()
    {
        dosagePanel.SetActive(true);
        warningPanel.SetActive(false);
    }

    public void ShowWarningPanel()
    {
        warningPanel.SetActive(true);
        dosagePanel.SetActive(false);
    }

    public void CloseAllPanels()
    {
        dosagePanel.SetActive(false);
        warningPanel.SetActive(false);
    }
}