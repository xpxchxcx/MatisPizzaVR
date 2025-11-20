using System;
using TMPro;
using UnityEngine;

public class SauceSurface : MonoBehaviour
{
    public static event Action<GameObject> OnFlattenedDoughPlaced;
    public static event Action<GameObject> OnFlattenedDoughRemoved;

    [Header("Debug UI")]
    public TextMeshPro debugTMP;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("flattened"))
        {
            GameObject go = other.gameObject;
            PizzaController currentPizzaController = go.transform.parent.GetComponent<PizzaController>();


            if (currentPizzaController != null && currentPizzaController.assemblyPhase == AssemblyPhase.SauceStage)
            {
                SauceSpreadRecognizer sauceSpreadRecognizer = go.transform.parent.Find("SauceSpreadRecognizer").GetComponent<SauceSpreadRecognizer>();
                sauceSpreadRecognizer.setCanBeSauced(true);
                Debug.Log($"flattened Dough entered saucing prep surface: {other.gameObject.name}");
                UpdateDebugUI(currentPizzaController, "entered");

            }

        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("flattened"))
        {
            GameObject go = other.gameObject;
            SauceSpreadRecognizer sauceSpreadRecognizer = go.transform.parent.Find("SauceSpreadRecognizer").GetComponent<SauceSpreadRecognizer>();

            sauceSpreadRecognizer.setCanBeSauced(false);
            PizzaController currentPizzaController = go.transform.parent.GetComponent<PizzaController>();

            Debug.Log($"flattened Dough exited saucing prep surface: {other.gameObject.name}");
            UpdateDebugUI(currentPizzaController, "exited");




        }
    }

    private void UpdateDebugUI(PizzaController pizza, string state)
    {
        if (debugTMP == null || pizza == null) return;

        string toppingsRequired = "";
        if (pizza.orderData != null)
        {
            foreach (var t in pizza.orderData.requiredToppings)
                toppingsRequired += $"{t.toppingName}: {t.requiredCount}\n";
        }

        debugTMP.text = $"Pizza: {pizza.pizzaName}\n" +
                        $"State: {state}\n" +
                        $"Assembly Phase: {pizza.assemblyPhase}\n" +
                        $"Toppings Required:\n{toppingsRequired}";
    }
}

