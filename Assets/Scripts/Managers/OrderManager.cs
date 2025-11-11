using UnityEngine;
using System.Collections.Generic;
using System;

public class OrderManager : Singleton<OrderManager>
{
    [Header("References")]
    [Tooltip("Reference to the OrderLibrary ScriptableObject that contains all possible pizza order templates.")]
    public OrderLibrary orderLibrary;

    [Header("Runtime Lists")]
    public List<OrderData> activeOrders = new();      // currently active orders in progress
    public List<OrderData> completedOrders = new();   // finished or delivered orders

    [Header("Order Timing Settings")]
    [Tooltip("Minimum time before next order spawns (seconds).")]
    public float minSpawnDelay = 20f;

    [Tooltip("Maximum time before next order spawns (seconds).")]
    public float maxSpawnDelay = 50f;

    [Tooltip("Maximum number of active orders allowed at a time.")]
    public int maxActiveOrders = 3;


    public event Action<OrderData> OnOrderSpawned;
    public event Action<OrderData> OnOrderCompleted;


    private float nextSpawnTime;

    void Start()
    {
        ScheduleNextOrder();
    }

    void Update()
    {
        // Wait for the randomized next spawn time
        if (Time.time >= nextSpawnTime && activeOrders.Count < maxActiveOrders)
        {
            SpawnRandomOrder();
            ScheduleNextOrder(); // new random delay for the next one
        }
    }

    /// <summary>
    /// Picks a new random delay and sets the next spawn time.
    /// </summary>
    private void ScheduleNextOrder()
    {
        float delay = UnityEngine.Random.Range(minSpawnDelay, maxSpawnDelay);
        nextSpawnTime = Time.time + delay;
    }

    /// <summary>
    /// Spawns a random order from the OrderLibrary and adds it to active orders.
    /// </summary>
    public void SpawnRandomOrder()
    {
        if (orderLibrary == null)
        {
            Debug.LogError("OrderManager: Missing OrderLibrary reference!");
            return;
        }

        OrderData template = orderLibrary.GetRandomOrder();
        if (template == null) return;

        // Clone the template so we don't modify the original ScriptableObject
        OrderData newOrder = OrderLibrary.Instantiate(template);
        newOrder.isInProgress = true;
        newOrder.isCompleted = false;

        activeOrders.Add(newOrder);
        Debug.Log($"[OrderManager] Spawned new order: {newOrder.pizzaName}");

        // Trigger event
        OnOrderSpawned?.Invoke(newOrder);
    }

    /// <summary>
    /// Marks an order as completed, moves it to completed list, and removes it from active.
    /// </summary>
    public void MarkOrderCompleted(OrderData order)
    {
        if (order == null || !activeOrders.Contains(order)) return;

        order.isInProgress = false;
        order.isCompleted = true;
        activeOrders.Remove(order);
        completedOrders.Add(order);

        Debug.Log($"[OrderManager] Completed order: {order.pizzaName}");

        // Trigger event
        OnOrderCompleted?.Invoke(order);
    }

    /// <summary>
    /// Returns the next active order (e.g. first in queue).
    /// </summary>
    public OrderData GetNextPendingOrder()
    {
        if (activeOrders.Count == 0) return null;
        return activeOrders[0];
    }

    /// <summary>
    /// Clears all orders — useful for restarting a round or debugging.
    /// </summary>
    public void ClearAllOrders()
    {
        activeOrders.Clear();
        completedOrders.Clear();
        Debug.Log("[OrderManager] Cleared all orders.");
    }

    internal void ValidateOrder(PizzaController pizza)
    {
        if (pizza == null)
        {
            Debug.LogError("[OrderManager] Tried to validate a null PizzaController!");
            return;
        }

        // Check if there are any active orders
        if (activeOrders == null || activeOrders.Count == 0)
        {
            Debug.LogWarning("[OrderManager] No active orders to validate against!");
            return;
        }

        // Try to find an order that matches this pizza's name
        OrderData matchedOrder = activeOrders.Find(o => o.pizzaName == pizza.pizzaName);

        if (matchedOrder == null)
        {
            Debug.LogWarning($"[OrderManager] No matching active order found for pizza: {pizza.pizzaName}");
            return;
        }

        // Ask PizzaController if it satisfies the recipe
        bool isValid = pizza.ValidateAgainstOrder();

        if (isValid)
        {
            // Mark order as completed
            MarkOrderCompleted(matchedOrder);
            Debug.Log($"[OrderManager] Pizza '{pizza.pizzaName}' validated and completed successfully!");
        }
        else
        {
            Debug.LogWarning($"[OrderManager] Pizza '{pizza.pizzaName}' failed validation (wrong toppings, burnt, or incomplete).");
        }
    }
}
