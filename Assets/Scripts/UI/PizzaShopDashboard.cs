using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class PizzaShopDashboard : MonoBehaviour
{


    [Header("Hand Tracking")]
    public GameObject dashboardCanvas;

    private bool isDashboardGesture = false;

    void OnEnable()
    {
        // Dashboard gestures
        AssemblyManager.StartDashboardLeftGestureEvent += ShowDashboard;
        AssemblyManager.EndDashboardLeftGestureEvent += HideDashboard;


    }

    void OnDisable()
    {
        AssemblyManager.StartDashboardLeftGestureEvent -= ShowDashboard;
        AssemblyManager.EndDashboardLeftGestureEvent -= HideDashboard;


    }

    void Update()
    {
        if (!dashboardCanvas.activeSelf) return;

    }

    private void ShowDashboard()
    {
        if (isDashboardGesture) return;

        isDashboardGesture = true;
        dashboardCanvas.SetActive(true);


    }

    private void HideDashboard()
    {
        if (!isDashboardGesture) return;

        isDashboardGesture = false;
        dashboardCanvas.SetActive(false);
    }


}
