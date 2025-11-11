using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class SauceSpreadRecognizer : MonoBehaviour
{
    public static event Action OnSauceComplete;
    [SerializeField] private float sauceVolumeRequired = 100f;
    private float currentSauceVolume = 0f;
    private bool canBeSauced = false;

    void Update()
    {
        // Debug: S key to simulate sauce spreading
        if (Keyboard.current != null && Keyboard.current[Key.S].wasPressedThisFrame)
        {
            simulateSauceMotion();
        }

        CheckSauceMotion();
    }

    //TODO liquid simulation for sauce spreading

    //just a function to simulate sauce adding for now

    public void simulateSauceMotion()
    {
        currentSauceVolume += 20f;
        Debug.Log($"Sauce spreading! Volume: {currentSauceVolume}/{sauceVolumeRequired}");

    }

    // hook up with actual sauce spreading detection later
    public void registerSauceAmount(float amount)
    {
        if (canBeSauced)
        {
            currentSauceVolume += amount;
            Debug.Log($"Sauce spreading! Volume: {currentSauceVolume}/{sauceVolumeRequired}");
        }
    }

    public void CheckSauceMotion()
    {
        // put sauce motion detection logic here (e.g., hand tracking data)
        if (currentSauceVolume >= sauceVolumeRequired)
        {
            Debug.Log("<color=green>Sauce spreading complete!</color>");
            OnSauceComplete?.Invoke();
        }
    }

    public void setCanBeSauced(bool val)
    {
        canBeSauced = val;
    }
}
