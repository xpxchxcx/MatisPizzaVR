using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class SauceSpreadRecognizer : MonoBehaviour
{
    public static event Action<GameObject> OnSauceComplete;
    [SerializeField] private int sauceVolumeRequired = 3;
    public float SauceVolumeRequired => sauceVolumeRequired;
    private int currentSauceVolume = 0;

    private bool canBeSauced = false;
    private bool sauceCompleted = false;


    [Header("UI")]
    public TextMeshPro sauceStatusText;
    public TextMeshPro sauceProgressText;
    void Update()
    {

        CheckSauceMotion();
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (sauceStatusText != null)
            sauceStatusText.text = canBeSauced ? "Ready for Sauce" : "Not Ready";

        if (sauceProgressText != null)
            sauceProgressText.text =
                $"Sauce: {currentSauceVolume}/{sauceVolumeRequired}";
    }

    public void registerSauceAmount()
    {
        if (canBeSauced)
        {
            currentSauceVolume += 1;
            Debug.Log($"Sauce spreading! Volume: {currentSauceVolume}/{sauceVolumeRequired}");
            UpdateUI();
        }
    }

    public void CheckSauceMotion()
    {
        // put sauce motion detection logic here (e.g., hand tracking data)
        if (!sauceCompleted && currentSauceVolume >= sauceVolumeRequired)
        {
            sauceCompleted = true;
            Debug.Log("<color=green>Sauce spreading complete!</color>");
            OnSauceComplete?.Invoke(transform.parent.parent.gameObject);
        }
    }

    public void setCanBeSauced(bool val)
    {
        canBeSauced = val;

    }


    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ladle"))
        {
            Debug.Log("Ladle sauced on flattened pizza ");
            Transform grandParent = other.transform.parent.parent;
            Ladle ladle = grandParent.gameObject.GetComponent<Ladle>();
            if (ladle.hasSauce)
            {
                registerSauceAmount();
                ladle.ToggleSoup();
            }

        }
    }

    public int GetRemainingSauceNeeded()
    {
        return Mathf.Max(sauceVolumeRequired - currentSauceVolume, 0);
    }
}
