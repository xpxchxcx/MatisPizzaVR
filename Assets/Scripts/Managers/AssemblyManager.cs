using UnityEngine;
using System.Collections.Generic;

public class AssemblyManager : Singleton<AssemblyManager>
{
    [Header("Active Pizzas")]
    public List<PizzaController> activePizzas = new();

    private void OnEnable()
    {
        // Subscribe to PrepSurface or DoughController events if you want auto-registration
        PrepSurface.OnDoughPlaced += HandleDoughPlaced;
        PrepSurface.OnDoughRemoved += HandleDoughRemoved;
    }

    private void OnDisable()
    {
        // Always unsubscribe to prevent leaks
        PrepSurface.OnDoughPlaced -= HandleDoughPlaced;
        PrepSurface.OnDoughRemoved -= HandleDoughRemoved;
    }

    /// <summary>
    /// Register an existing pizza (with its PizzaController) into the assembly workflow.
    /// </summary>
    public void RegisterPizza(PizzaController pizza, OrderData order)
    {

        if (activePizzas.Contains(pizza))
        {
            Debug.LogWarning($"[AssemblyManager] Pizza {pizza.name} already registered!");
            return;
        }

        // Mark order as actively being worked on once dough placed on prep surface
        order.isInProgress = true;

        pizza.InitializeFromOrder(order);
        activePizzas.Add(pizza);

        Debug.Log($"[AssemblyManager] Registered pizza '{pizza.pizzaName}' for order: {order.pizzaName}");
    }

    /// <summary>
    /// Unregisters a pizza when removed, failed, or destroyed.
    /// </summary>
    public void UnregisterPizza(PizzaController pizza)
    {
        if (pizza != null && activePizzas.Contains(pizza))
        {
            activePizzas.Remove(pizza);
            Debug.Log($"[AssemblyManager] Unregistered pizza: {pizza.pizzaName}");
        }
    }

    /// <summary>
    /// Called when player finishes a pizza and serves it.
    /// </summary>
    public void CompletePizza(PizzaController pizza)
    {
        if (pizza == null) return;

        if (activePizzas.Contains(pizza))
            activePizzas.Remove(pizza);

        // Validate match against active order
        OrderData matchedOrder = OrderManager.Instance.activeOrders.Find(o => o.pizzaName == pizza.pizzaName);

        if (matchedOrder != null)
        {
            OrderManager.Instance.MarkOrderCompleted(matchedOrder);
            Debug.Log($"[AssemblyManager] Pizza served successfully: {pizza.pizzaName}");
        }
        else
        {
            Debug.LogWarning($"[AssemblyManager] Served pizza not matching any order: {pizza.pizzaName}");
        }
    }

    /// <summary>
    /// Progresses a pizza through its assembly stages.
    /// </summary>
    public void AdvanceAssemblyPhase(PizzaController pizza)
    {
        if (pizza == null) return;

        switch (pizza.assemblyPhase)
        {
            case AssemblyPhase.None:
                pizza.assemblyPhase = AssemblyPhase.Kneading;
                break;
            case AssemblyPhase.Kneading:
                pizza.assemblyPhase = AssemblyPhase.SauceStage;
                break;
            case AssemblyPhase.SauceStage:
                pizza.assemblyPhase = AssemblyPhase.ToppingsStage;
                break;
            case AssemblyPhase.ToppingsStage:
                pizza.assemblyPhase = AssemblyPhase.ReadyForOven;
                break;
            case AssemblyPhase.ReadyForOven:
                pizza.assemblyPhase = AssemblyPhase.Baking;
                break;
            case AssemblyPhase.Baking:
                pizza.assemblyPhase = AssemblyPhase.Baked;
                break;
            case AssemblyPhase.Baked:
                pizza.assemblyPhase = AssemblyPhase.Served;
                CompletePizza(pizza);
                break;
        }

        Debug.Log($"[AssemblyManager] {pizza.pizzaName} advanced to phase: {pizza.assemblyPhase}");
    }

    // Optional event hooks if you're using PrepSurface events
    private void HandleDoughPlaced(GameObject go)
    {
        PizzaController pizza = go.GetComponent<PizzaController>();

        // basically just auto-assign the next order to this pizza (simplicity sake)
        OrderData nextOrder = OrderManager.Instance.GetNextPendingOrder();
        if (nextOrder != null)
            RegisterPizza(pizza, nextOrder);
        else
            Debug.Log("[AssemblyManager] No pending order to assign.");
    }

    private void HandleDoughRemoved(GameObject go)
    {
        // do nothing for now
    }
}
