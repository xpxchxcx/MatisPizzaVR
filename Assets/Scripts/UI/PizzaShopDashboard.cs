using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class PizzaShopDashboard : MonoBehaviour
{
    [Header("UI Prefabs")]
    public GameObject orderRowPrefab;
    public GameObject pizzaRowPrefab;

    [Header("UI Containers")]
    public Transform orderListParent;
    public Transform pizzaListParent;

    [Header("Hand Tracking")]
    public GameObject dashboardCanvas;
    public Transform leftHandTransform;
    public Vector3 offset = new Vector3(0.15f, 0.10f, 0.10f);

    private readonly List<GameObject> orderRows = new();
    private readonly List<GameObject> pizzaRows = new();
    private bool isDashboardGesture = false;

    void OnEnable()
    {
        // Dashboard gestures
        AssemblyManager.StartDashboardLeftGestureEvent += ShowDashboard;
        AssemblyManager.EndDashboardLeftGestureEvent += HideDashboard;

        // Subscribe to order events
        OrderManager.Instance.OnOrderSpawned += OnOrdersChanged;
        OrderManager.Instance.OnOrderCompleted += OnOrdersChanged;

        // Subscribe to pizza events
        AssemblyManager.Instance.OnPizzaRegistered += OnPizzasChanged;
        AssemblyManager.Instance.OnPizzaUnregistered += OnPizzasChanged;
    }

    void OnDisable()
    {
        AssemblyManager.StartDashboardLeftGestureEvent -= ShowDashboard;
        AssemblyManager.EndDashboardLeftGestureEvent -= HideDashboard;

        if (OrderManager.Instance != null)
        {
            OrderManager.Instance.OnOrderSpawned -= OnOrdersChanged;
            OrderManager.Instance.OnOrderCompleted -= OnOrdersChanged;
        }

        if (AssemblyManager.Instance != null)
        {
            AssemblyManager.Instance.OnPizzaRegistered -= OnPizzasChanged;
            AssemblyManager.Instance.OnPizzaUnregistered -= OnPizzasChanged;
        }
    }

    void Update()
    {
        if (!dashboardCanvas.activeSelf) return;
        FollowHand();
    }

    private void FollowHand()
    {
        if (leftHandTransform == null) return;

        // Position the dashboard slightly in front of the hand in the direction it's pointing
        Vector3 forwardOffset = leftHandTransform.forward * offset.z +
                                leftHandTransform.up * offset.y +
                                leftHandTransform.right * offset.x;

        dashboardCanvas.transform.position = leftHandTransform.position + forwardOffset;

        // Rotate to face the same direction as the hand (so it "points" where the hand points)
        dashboardCanvas.transform.rotation = Quaternion.LookRotation(leftHandTransform.forward, Vector3.up);
    }

    private void ShowDashboard()
    {
        if (isDashboardGesture) return;

        isDashboardGesture = true;
        dashboardCanvas.SetActive(true);

        // Refresh immediately when dashboard opens
        RefreshOrdersUI();
        RefreshPizzasUI();
    }

    private void HideDashboard()
    {
        if (!isDashboardGesture) return;

        isDashboardGesture = false;
        dashboardCanvas.SetActive(false);
    }

    private void OnOrdersChanged(OrderData _)
    {
        if (!dashboardCanvas.activeSelf) return;
        RefreshOrdersUI();
    }

    private void OnPizzasChanged(PizzaController _)
    {
        if (!dashboardCanvas.activeSelf) return;
        RefreshPizzasUI();
    }

    // ---------------- Orders ----------------
    private void RefreshOrdersUI()
    {
        foreach (var row in orderRows) Destroy(row);
        orderRows.Clear();

        var orders = OrderManager.Instance;

        foreach (var order in orders.activeOrders)
            AddOrderRow(order, "Not Started");

        foreach (var order in orders.startedOrders)
            AddOrderRow(order, "In Progress");

        foreach (var order in orders.completedOrders)
            AddOrderRow(order, "COMPLETED");
    }

    private void AddOrderRow(OrderData order, string state)
    {
        GameObject row = Instantiate(orderRowPrefab, orderListParent);

        TextMeshProUGUI orderIdTMP = row.transform.Find("Canvas/OrderID")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI pizzaNameTMP = row.transform.Find("Canvas/PizzaName")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI statusTMP = row.transform.Find("Canvas/Status")?.GetComponent<TextMeshProUGUI>();

        if (orderIdTMP != null) orderIdTMP.text = order.orderId;
        if (pizzaNameTMP != null) pizzaNameTMP.text = order.pizzaName;
        if (statusTMP != null) statusTMP.text = state;

        // Space each row vertically
        RectTransform rt = row.GetComponent<RectTransform>();
        if (rt != null)
            rt.anchoredPosition = new Vector2(0, -110 * orderRows.Count);

        orderRows.Add(row);
    }

    // ---------------- Pizzas ----------------
    private void RefreshPizzasUI()
    {
        foreach (var row in pizzaRows) Destroy(row);
        pizzaRows.Clear();

        foreach (var pizza in AssemblyManager.Instance.activePizzas)
            AddPizzaRow(pizza);
    }

    private void AddPizzaRow(PizzaController pizza)
    {
        GameObject row = Instantiate(pizzaRowPrefab, pizzaListParent);

        TextMeshProUGUI orderIdTMP = row.transform.Find("Canvas/OrderID")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI pizzaNameTMP = row.transform.Find("Canvas/PizzaName")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI phaseTMP = row.transform.Find("Canvas/Phase")?.GetComponent<TextMeshProUGUI>();

        if (orderIdTMP != null) orderIdTMP.text = pizza.orderData.orderId;
        if (pizzaNameTMP != null) pizzaNameTMP.text = pizza.pizzaName;
        if (phaseTMP != null) phaseTMP.text = pizza.assemblyPhase.ToString();

        RectTransform rt = row.GetComponent<RectTransform>();
        if (rt != null)
            rt.anchoredPosition = new Vector2(0, -110 * pizzaRows.Count);

        pizzaRows.Add(row);
    }

    private int PhaseToProgress(AssemblyPhase phase)
    {
        return phase switch
        {
            AssemblyPhase.Kneading => 0,
            AssemblyPhase.SauceStage => 1,
            AssemblyPhase.ToppingsStage => 2,
            AssemblyPhase.ReadyForOven => 3,
            AssemblyPhase.Baking => 4,
            AssemblyPhase.Baked => 5,
            AssemblyPhase.Served => 6,
            _ => 0
        };
    }
}
