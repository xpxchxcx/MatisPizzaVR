using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class DoughController : MonoBehaviour
{

    [SerializeField] private PizzaController pizzaController;

    [Header("Kneading Settings")]
    [SerializeField] private int kneadingRequired = 5;
    private int currentKneadingCount = 0;

    private bool isDoughOnSurface = false;

    public static event Action OnDoughFlattened;


    void Start()
    {
        pizzaController = GetComponent<PizzaController>();
    }

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
        if (isDoughOnSurface)
        {
            currentKneadingCount++;
            Debug.Log($"Kneading motion detected! Count: {currentKneadingCount}/{kneadingRequired}");

            // Check if we've reached the required number of kneads
            if (currentKneadingCount >= kneadingRequired)
            {
                OnDoughFlattened?.Invoke();
            }
        }
    }

    // Register kneading motion (to be called by VR hand tracking system)
    public void RegisterKnead()
    {
        // Check if dough is on the prep surface
        if (isDoughOnSurface)
        {
            currentKneadingCount++;
            Debug.Log($"Kneading motion detected! Count: {currentKneadingCount}/{kneadingRequired}");

            // Check if we've reached the required number of kneads
            if (currentKneadingCount >= kneadingRequired)
            {
                OnDoughFlattened?.Invoke();
            }
        }
    }



    public void setDoughSurfaceTrue()
    {
        isDoughOnSurface = true;
    }
    public void setDoughSurfaceFalse()
    {
        isDoughOnSurface = false;
    }



    public PizzaController GetPizzaController()
    {
        return pizzaController;
    }
}
