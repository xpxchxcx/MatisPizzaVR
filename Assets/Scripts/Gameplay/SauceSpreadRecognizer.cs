using UnityEngine;
using UnityEngine.InputSystem;

public class SauceSpreadRecognizer : MonoBehaviour
{
    [SerializeField] private PizzaController pizzaController;
    [SerializeField] private float sauceVolumeRequired = 100f;
    private float currentSauceVolume = 0f;

    void Update()
    {
        // Debug: S key to simulate sauce spreading
        if (Keyboard.current != null && Keyboard.current[Key.S].wasPressedThisFrame)
        {
            CheckSauceMotion();
        }
    }

    public void CheckSauceMotion()
    {
        currentSauceVolume += 20f;
        Debug.Log($"Sauce spreading! Volume: {currentSauceVolume}/{sauceVolumeRequired}");

        if (currentSauceVolume >= sauceVolumeRequired)
        {
            OnSauceComplete();
        }
    }

    private void OnSauceComplete()
    {
        Debug.Log("<color=green>Sauce spreading complete!</color>");

        if (pizzaController != null)
        {
            // TODO: create on Complete sauce stage script function

            //pizzaController.CompleteSauceStage();
        }
    }
}
