using System;
using UnityEngine;

public class SauceSurface : MonoBehaviour
{
    public static event Action<GameObject> OnFlattenedDoughPlaced;
    public static event Action<GameObject> OnFlattenedDoughRemoved;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("dough"))
        {
            GameObject go = other.gameObject;
            PizzaController currentPizzaController = go.GetComponent<PizzaController>();

            if (currentPizzaController != null && currentPizzaController.assemblyPhase == AssemblyPhase.SauceStage)
            {
                go.GetComponent<SauceSpreadRecognizer>().setCanBeSauced(true);
                Debug.Log($"flattened Dough entered saucing prep surface: {other.gameObject.name}");

            }

        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("dough"))
        {
            GameObject go = other.gameObject;
            PizzaController currentPizzaController = go.GetComponent<PizzaController>();


            go.GetComponent<SauceSpreadRecognizer>().setCanBeSauced(false);
            Debug.Log($"flattened Dough exited saucing prep surface: {other.gameObject.name}");



        }
    }
}

