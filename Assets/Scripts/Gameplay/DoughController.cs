using UnityEngine;
using UnityEngine.InputSystem;

public class DoughController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PrepSurface prepSurface;
    [SerializeField] private PizzaController pizzaController;

    [Header("Kneading Settings")]
    [SerializeField] private int kneadingRequired = 5;
    private int currentKneadingCount = 0;

    void Update()
    {
        // Debug input for testing kneading motion (M key)
        if (Keyboard.current != null && Keyboard.current[Key.M].wasPressedThisFrame)
        {
            SimulateKneadingMotion();
        }
    }

    // Simulate kneading motion (called by 'M' key for debugging, later by VR hand tracking)
    public void SimulateKneadingMotion()
    {
        // Check if dough is on the prep surface
        if (prepSurface != null && prepSurface.IsDoughOnSurface())
        {
            currentKneadingCount++;
            Debug.Log($"Kneading motion detected! Count: {currentKneadingCount}/{kneadingRequired}");

            // Check if we've reached the required number of kneads
            if (currentKneadingCount >= kneadingRequired)
            {
                OnKneadingComplete();
            }
        }
    }

    // Called when kneading is complete
    private void OnKneadingComplete()
    {
        Debug.Log("<color=green>Kneading complete! Dough is ready to be flattened!</color>");
        
        // Call PizzaController when ready
        if (pizzaController != null)
        {
            pizzaController.OnDoughFlattened();
        }
    }
}
