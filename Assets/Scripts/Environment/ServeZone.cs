using UnityEngine;
using System;
using AudioSystem;

public class ServeZone : MonoBehaviour
{
    // Move scoring to ScoreManager
    public static event Action<PizzaController, bool> OnPizzaServed;

    // Helper method for tests to trigger the event
    public static void TriggerOnPizzaServed(PizzaController pizza, bool success)
    {
        OnPizzaServed?.Invoke(pizza, success);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("cooked"))
        {
            SoundManager.Instance.PlaySFX("Place");
            PizzaController pizza = other.GetComponentInParent<PizzaController>();
            if (pizza == null)
            {
                Debug.LogWarning("[ServeZone] Object entered but no PizzaController found!");
                return;
            }

            if (pizza.assemblyPhase == AssemblyPhase.Baked)
            {
                // Mark as served

                // Validate against the active order
                bool success = OrderManager.Instance.ValidatePizzaAndCompleteOrder(pizza);
                ///OrderManager.Instance.MarkOrderCompleted(pizza.orderData);
                // Fire pizza served event. Currently listened to by ScoreManager.
                OnPizzaServed?.Invoke(pizza, success);

                // Cleanup
                AssemblyManager.Instance.CompletePizza(pizza);

                pizza.OnServed();
            }



        }







    }
}
