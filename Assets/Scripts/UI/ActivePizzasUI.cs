using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ActivePizzasUI : MonoBehaviour
{
    [Header("Panel Prefab & Container")]
    [SerializeField] private GameObject pizzaPanelPrefab; // The panel prefab with all UI elements
    [SerializeField] private Transform panelContainer; // Parent for dynamically instantiated panels

    [Header("Optional: Max Panels")]
    [SerializeField] private int maxPanels = 3;

    // Internal dictionary to keep track of panels for each pizza
    private Dictionary<PizzaController, PizzaPanelUI> pizzaPanels = new();

    private void OnEnable()
    {
        AssemblyManager.Instance.OnPizzaRegistered += AddPizzaPanel;
        AssemblyManager.Instance.OnPizzaUnregistered += RemovePizzaPanel;
        PrepSurface.OnDoughPlaced += OnDoughPlaced;
        ServeZone.OnPizzaServed += OnPizzaServed;
    }

    void Start()
    {
        AssemblyManager.Instance.OnPizzaRegistered += AddPizzaPanel;
        AssemblyManager.Instance.OnPizzaUnregistered += RemovePizzaPanel;
        ServeZone.OnPizzaServed += OnPizzaServed;
    }

    private void OnDisable()
    {
        if (AssemblyManager.Instance == null) return;
        AssemblyManager.Instance.OnPizzaRegistered -= AddPizzaPanel;
        AssemblyManager.Instance.OnPizzaUnregistered -= RemovePizzaPanel;
        ServeZone.OnPizzaServed -= OnPizzaServed;
    }
    private void OnPizzaServed(PizzaController pizza, bool success)
    {
        RemovePizzaPanel(pizza); // Remove the panel when pizza is served
    }

    private void Update()
    {
        // Update all active pizza panels
        foreach (var kvp in pizzaPanels)
        {
            PizzaController pizza = kvp.Key;
            PizzaPanelUI panelUI = kvp.Value;

            if (pizza == null) continue;

            panelUI.PizzaName.text = pizza.pizzaName;
            panelUI.OrderNumber.text = $"Order: {pizza.orderData?.orderId}";
            panelUI.Phase.text = pizza.assemblyPhase.ToString();

            // Map progress from assemblyPhase
            float progress = GetProgressFromPhase(pizza.assemblyPhase);
            panelUI.Progress.value = progress;

            panelUI.StatusLabel.text = GetStatusText(pizza);
        }
    }

    private void AddPizzaPanel(PizzaController pizza)
    {
        if (pizzaPanels.ContainsKey(pizza)) return;

        if (pizzaPanels.Count >= maxPanels)
        {
            Debug.LogWarning("[ActivePizzasUI] Maximum panels reached!");
            return;
        }

        GameObject panelGO = Instantiate(pizzaPanelPrefab, panelContainer);
        PizzaPanelUI panelUI = panelGO.GetComponent<PizzaPanelUI>();

        if (panelUI == null)
        {
            Debug.LogError("[ActivePizzasUI] PizzaPanelPrefab missing PizzaPanelUI script!");
            Destroy(panelGO);
            return;
        }

        pizzaPanels[pizza] = panelUI;
    }

    private void OnDoughPlaced(GameObject dough)
    {
        PizzaController pizza = dough.GetComponentInParent<PizzaController>();
        if (pizzaPanels.ContainsKey(pizza)) return;

        if (pizzaPanels.Count >= maxPanels)
        {
            Debug.LogWarning("[ActivePizzasUI] Maximum panels reached!");
            return;
        }

        GameObject panelGO = Instantiate(pizzaPanelPrefab, panelContainer);
        PizzaPanelUI panelUI = panelGO.GetComponent<PizzaPanelUI>();

        if (panelUI == null)
        {
            Debug.LogError("[ActivePizzasUI] PizzaPanelPrefab missing PizzaPanelUI script!");
            Destroy(panelGO);
            return;
        }

        pizzaPanels[pizza] = panelUI;
    }

    private void RemovePizzaPanel(PizzaController pizza)
    {
        if (pizzaPanels.TryGetValue(pizza, out PizzaPanelUI panelUI))
        {
            Destroy(panelUI.gameObject);
            pizzaPanels.Remove(pizza);
        }
    }

    private float GetProgressFromPhase(AssemblyPhase phase)
    {
        return phase switch
        {
            AssemblyPhase.Kneading => 0f,
            AssemblyPhase.SauceStage => 0.2f,
            AssemblyPhase.ToppingsStage => 0.4f,
            AssemblyPhase.ReadyForOven => 0.6f,
            AssemblyPhase.Baking => 0.8f,
            AssemblyPhase.Baked => 1f,
            AssemblyPhase.Served => 1f,
            _ => 0f
        };
    }

    private string GetStatusText(PizzaController pizza)
    {
        if (pizza.isBurnt) return "Burnt!";
        if (pizza.isCooked) return "Cooked";
        if (pizza.isInsideOven) return "Baking";
        if (pizza.isSauced) return "Sauced";
        return "In Progress";
    }
}
