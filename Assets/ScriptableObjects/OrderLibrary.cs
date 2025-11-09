using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "OrderLibrary", menuName = "Game/Order Library", order = 0)]
public class OrderLibrary : ScriptableObject
{
    [Header("List of all possible pizza orders")]
    public List<OrderData> possibleOrders = new();

    /// <summary>
    /// Returns a random order template from the library.
    /// </summary>
    public OrderData GetRandomOrder()
    {
        if (possibleOrders == null || possibleOrders.Count == 0)
        {
            Debug.LogWarning("OrderLibrary: No orders found in the library!");
            return null;
        }

        int index = Random.Range(0, possibleOrders.Count);
        return possibleOrders[index];
    }

    /// <summary>
    /// Returns an order by name (if you want to fetch a specific pizza).
    /// </summary>
    public OrderData GetOrderByName(string pizzaName)
    {
        foreach (var order in possibleOrders)
        {
            if (order.pizzaName == pizzaName)
                return order;
        }

        Debug.LogWarning($"OrderLibrary: No order found with name '{pizzaName}'!");
        return null;
    }

    /// <summary>
    /// Returns an order by static ID (if you assigned IDs like '001', '002').
    /// </summary>
    public OrderData GetOrderById(string orderId)
    {
        foreach (var order in possibleOrders)
        {
            if (order.orderId == orderId)
                return order;
        }

        Debug.LogWarning($"OrderLibrary: No order found with ID '{orderId}'!");
        return null;
    }
}
