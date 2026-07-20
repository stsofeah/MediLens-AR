using UnityEngine;

public class MedicineDataManager : MonoBehaviour
{
    public static MedicineDataManager Instance;

    public string medicineName;
    public string shortDescription;
    public string purpose;
    public string dosage;
    public string warning;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}