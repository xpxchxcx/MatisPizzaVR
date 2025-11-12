using UnityEngine;
using System;

public class ServeZone : MonoBehaviour
{
    // Move scoring to ScoreManager
    public static event Action<PizzaController, bool> OnPizzaServed;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("pizza")) return;

        PizzaController pizza = other.GetComponent<PizzaController>();
        if (pizza == null)
        {
            Debug.LogWarning("[ServeZone] Object entered but no PizzaController found!");
            return;
        }

        // Mark as served
        pizza.OnServed();

        // Validate against the active order
        bool success = OrderManager.Instance.ValidatePizzaAndCompleteOrder(pizza);

        // Fire pizza served event. Currently listened to by ScoreManager.
        OnPizzaServed?.Invoke(pizza, success);

        // Cleanup
        AssemblyManager.Instance.CompletePizza(pizza);
    }
}
