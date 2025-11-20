using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class SauceSpreadRecognizer : MonoBehaviour
{
    public static event Action OnSauceComplete;
    [SerializeField] private int sauceVolumeRequired = 3;
    public float SauceVolumeRequired => sauceVolumeRequired;
    private int currentSauceVolume = 0;

    private bool canBeSauced = false;
    private bool sauceCompleted = false;
    void Update()
    {

        CheckSauceMotion();
    }


    public void registerSauceAmount()
    {
        if (canBeSauced)
        {
            currentSauceVolume += 1;
            Debug.Log($"Sauce spreading! Volume: {currentSauceVolume}/{sauceVolumeRequired}");

        }
    }

    public void CheckSauceMotion()
    {
        // put sauce motion detection logic here (e.g., hand tracking data)
        if (!sauceCompleted && currentSauceVolume >= sauceVolumeRequired)
        {
            sauceCompleted = true;
            Debug.Log("<color=green>Sauce spreading complete!</color>");
            OnSauceComplete?.Invoke();
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
            Transform grandParent = other.transform.parent.parent;
            Ladle ladle = grandParent.gameObject.GetComponent<Ladle>();
            if (ladle.hasSauce)
            {
                registerSauceAmount();
                ladle.ToggleSoup();
            }

        }
    }
}
