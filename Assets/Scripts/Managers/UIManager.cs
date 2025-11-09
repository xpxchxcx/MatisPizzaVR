using UnityEngine;
using System.Collections.Generic;

public class UIManager : Singleton<UIManager>
{
    [Header("UI References")]
    [Tooltip("Prefab for a single order card UI element.")]
    public GameObject orderCardPrefab;

    [Tooltip("Parent transform where active order cards will be spawned.")]
    public Transform ordersContainer;

    // Keep track of active order cards 
    private Dictionary<OrderData, GameObject> activeOrderCards = new();

    private void OnEnable()
    {
        // Subscribe to OrderManager events
        OrderManager.Instance.OnOrderSpawned += HandleOrderSpawned;
        OrderManager.Instance.OnOrderCompleted += HandleOrderCompleted;
    }

    private void OnDisable()
    {
        // Unsubscribe (to prevent null refs on scene unload)
        if (OrderManager.Instance != null)
        {
            OrderManager.Instance.OnOrderSpawned -= HandleOrderSpawned;
            OrderManager.Instance.OnOrderCompleted -= HandleOrderCompleted;
        }
    }

    /// <summary>
    /// Called when a new order is spawned.
    /// Creates a new UI card and displays the order info.
    /// </summary>
    private void HandleOrderSpawned(OrderData order)
    {
        if (orderCardPrefab == null || ordersContainer == null)
        {
            Debug.LogError("[UIManager] Missing orderCardPrefab or ordersContainer reference!");
            return;
        }

        GameObject card = Instantiate(orderCardPrefab, ordersContainer);
        card.name = $"OrderCard_{order.pizzaName}";

        // Optionally populate the UI card
        OrderCard cardComponent = card.GetComponent<OrderCard>();
        if (cardComponent != null)
        {
            cardComponent.Setup(order);
        }

        activeOrderCards[order] = card;
        Debug.Log($"[UIManager] Created order card for {order.pizzaName}");
    }

    /// <summary>
    /// Called when an order is completed.
    /// Removes its card from the UI.
    /// </summary>
    private void HandleOrderCompleted(OrderData order)
    {
        if (activeOrderCards.TryGetValue(order, out GameObject card))
        {
            Destroy(card);
            activeOrderCards.Remove(order);
            Debug.Log($"[UIManager] Removed order card for {order.pizzaName}");
        }
    }

    /// <summary>
    /// Clears all order cards: useful when restarting a round.
    /// </summary>
    public void ClearAllOrderCards()
    {
        foreach (var card in activeOrderCards.Values)
        {
            Destroy(card);
        }

        activeOrderCards.Clear();
        Debug.Log("[UIManager] Cleared all order cards.");
    }
}
