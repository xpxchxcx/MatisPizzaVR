using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class PizzaShopDashboard : MonoBehaviour
{


    [Header("Hand Tracking")]
    public GameObject dashboardCanvas;
    public Transform leftHandTransform;
    public Vector3 offset = new Vector3(0.15f, 0.10f, 0.10f);

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

    }

    private void AddOrderRow(OrderData order, string state)
    {

    }

    // ---------------- Pizzas ----------------
    private void RefreshPizzasUI()
    {

    }

    private void AddPizzaRow(PizzaController pizza)
    {

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
