using System;
using UnityEngine;

public class OvenSurface : MonoBehaviour
{
    public static event Action<GameObject> OnSaucedDoughPlaced;
    public static event Action<GameObject> OnSaucedDoughRemoved;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("sauced"))
        {
            GameObject go = other.gameObject;
            PizzaController currentPizzaController = go.transform.parent.GetComponent<PizzaController>();

            if (currentPizzaController != null && currentPizzaController.assemblyPhase == AssemblyPhase.ReadyForOven)
            {

                Debug.Log($"topped Dough entered topping prep surface: {other.gameObject.name}");
            }

        }
    }

}

