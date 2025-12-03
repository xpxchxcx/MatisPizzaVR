using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class OrderManagerUI : MonoBehaviour
{
    [Header("Started Orders UI Slots (3 max)")]
    [SerializeField] private List<TextMeshProUGUI> startedSlots;

    [Header("Active Orders UI Slots (3 max)")]
    [SerializeField] private List<TextMeshProUGUI> activeSlots;

    void Awake()
    {
        for (int i = 0; i < activeSlots.Count; i++)
        {

            startedSlots[i].gameObject.SetActive(false);

            activeSlots[i].gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        OrderManager.Instance.OnOrderSpawned += OnOrdersChanged;
        OrderManager.Instance.OnOrderCompleted += OnOrdersChanged;
        PrepSurface.OnDoughPlaced += OnDoughPlaced;
        ServeZone.OnPizzaServed += OnPizzaServed;
    }

    private void OnDisable()
    {
        OrderManager.Instance.OnOrderSpawned -= OnOrdersChanged;
        OrderManager.Instance.OnOrderCompleted -= OnOrdersChanged;
        ServeZone.OnPizzaServed -= OnPizzaServed;
    }

    void Start()
    {
        OrderManager.Instance.OnOrderSpawned += OnOrdersChanged;
        OrderManager.Instance.OnOrderCompleted += OnOrdersChanged;
        PrepSurface.OnDoughPlaced += OnDoughPlaced;
        ServeZone.OnPizzaServed += OnPizzaServed;
    }

    private void OnPizzaServed(PizzaController controller, bool arg2)
    {
        RefreshOrderManagerUI();
    }

    private void OnOrdersChanged(OrderData _)
    {
        RefreshOrderManagerUI();
    }

    private void OnDoughPlaced(GameObject go)
    {
        RefreshOrderManagerUI();
    }

    public void RefreshOrderManagerUI()
    {
        UpdateListUI(
            OrderManager.Instance.startedOrders,
            startedSlots
        );
        UpdateListUI(
            OrderManager.Instance.activeOrders,
            activeSlots
        );
    }

    private void UpdateListUI(List<OrderData> orderList, List<TextMeshProUGUI> uiSlots)
    {
        for (int i = 0; i < uiSlots.Count; i++)
        {
            if (i < orderList.Count)
            {
                OrderData order = orderList[i];

                uiSlots[i].text = $"{order.orderId}\n{order.pizzaName}";
                uiSlots[i].gameObject.SetActive(true);
            }
            else
            {
                uiSlots[i].gameObject.SetActive(false);
            }
        }
    }
}
