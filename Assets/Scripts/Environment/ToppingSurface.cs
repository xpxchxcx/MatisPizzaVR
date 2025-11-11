using System;
using UnityEngine;

public class ToppingSurface : MonoBehaviour
{
    public static event Action<GameObject> OnSaucedDoughPlaced;
    public static event Action<GameObject> OnSaucedDoughRemoved;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("dough"))
        {
            GameObject go = other.gameObject;
            PizzaController currentPizzaController = go.GetComponent<PizzaController>();

            if (currentPizzaController != null && currentPizzaController.assemblyPhase == AssemblyPhase.ToppingsStage)
            {
                go.GetComponent<ToppingHandler>().SetToppingAllowed(true);
                Debug.Log($"sauced Dough entered topping prep surface: {other.gameObject.name}");
            }

        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("dough"))
        {
            GameObject go = other.gameObject;
            PizzaController currentPizzaController = go.GetComponent<PizzaController>();

            if (currentPizzaController != null && currentPizzaController.assemblyPhase == AssemblyPhase.ToppingsStage)
            {
                go.GetComponent<ToppingHandler>().SetToppingAllowed(false);
                Debug.Log($"flattened Dough exited topping prep surface: {other.gameObject.name}");

            }

        }
    }
}

