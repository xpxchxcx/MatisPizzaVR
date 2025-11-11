using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class SauceSpreadRecognizer : MonoBehaviour
{
    public static event Action OnSauceComplete;
    [SerializeField] private float sauceVolumeRequired = 100f;
    private float currentSauceVolume = 0f;

    void Update()
    {
        // Debug: S key to simulate sauce spreading
        if (Keyboard.current != null && Keyboard.current[Key.S].wasPressedThisFrame)
        {
            simulateSauceMotion();
        }

        CheckSauceMotion();
    }

    public void simulateSauceMotion()
    {
        currentSauceVolume += 20f;
        Debug.Log($"Sauce spreading! Volume: {currentSauceVolume}/{sauceVolumeRequired}");

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


}
