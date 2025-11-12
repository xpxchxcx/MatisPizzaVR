using UnityEngine;
using System.Collections.Generic;
using System;

public class ToppingHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PizzaController pizzaController;

    private Dictionary<string, int> currentToppings = new();
    private bool toppingPhaseAllowed = false;

    public void SetToppingAllowed(bool val) => toppingPhaseAllowed = val;

    public static event Action OnToppingsCompleted;

    // Helper method for tests to trigger the event
    public static void TriggerOnToppingsCompleted()
    {
        OnToppingsCompleted?.Invoke();
    }


    void Start()
    {
        pizzaController = GetComponentInParent<PizzaController>();
        Debug.Log($"[ToppingHandler] Ready for toppings on: {pizzaController.pizzaName}");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!toppingPhaseAllowed) return;

        string toppingTag = other.tag.ToLower();

        // Validate topping against recipe
        bool validTopping = false;
        foreach (var req in pizzaController.orderData.requiredToppings)
        {
            if (req.toppingName.ToLower() == toppingTag)
            {
                validTopping = true;
                break;
            }
        }

        if (!validTopping)
        {
            Debug.Log($"[ToppingHandler] Ignored invalid topping: {toppingTag}");
            return;
        }

        // Record topping
        if (!currentToppings.ContainsKey(toppingTag))
            currentToppings[toppingTag] = 0;

        currentToppings[toppingTag]++;

        Debug.Log($"[ToppingHandler] Added {toppingTag} (x{currentToppings[toppingTag]})");

        CheckRecipeCompletion();
    }

    private void CheckRecipeCompletion()
    {
        var order = pizzaController.orderData;
        if (order == null) return;

        foreach (var req in order.requiredToppings)
        {
            string toppingName = req.toppingName.ToLower();
            int requiredCount = req.requiredCount;

            if (!currentToppings.ContainsKey(toppingName) || currentToppings[toppingName] < requiredCount)
                return; // not complete yet
        }

        // All toppings satisfied
        OnRecipeComplete();
    }

    private void OnRecipeComplete()
    {
        OnToppingsCompleted?.Invoke();
        Debug.Log($"<color=green>[ToppingHandler] All toppings complete for {pizzaController.pizzaName}!</color>");
    }

    /// <summary>
    /// Used by UI or Oven to validate final pizza.
    /// </summary>
    public bool ValidateAgainstOrder()
    {
        var order = pizzaController.orderData;
        if (order == null) return false;

        foreach (var req in order.requiredToppings)
        {
            string name = req.toppingName.ToLower();
            int required = req.requiredCount;
            if (!currentToppings.TryGetValue(name, out int have) || have < required)
                return false;
        }
        return true;
    }
}
