using UnityEngine;
using System.Collections.Generic;
using System;
using TMPro;

public class ToppingHandler : MonoBehaviour
{
    public event Action OnToppingsCompleted;
    [Header("References")]
    [SerializeField] private PizzaController pizzaController;
    [Header("Debug UI")]


    [Header("UI")]
    public TextMeshPro toppingsHeader;
    public TextMeshPro toppingsProgress;
    public GameObject UI;


    private Dictionary<string, int> currentToppings = new();
    [SerializeField] private List<GameObject> nonGrabbableToppingPrefabs;
    private bool toppingPhaseAllowed = false;

    public float pizzaRadius = 0.5f;
    public float yOffset = 0.02f;


    public void SetToppingAllowed(bool val)
    {
        toppingPhaseAllowed = val;
        Debug.Log($"Topping phase allowed: {toppingPhaseAllowed}");
        UpdateDebugText();
    }




    // Helper method for tests to trigger the event
    //public static void TriggerOnToppingsCompleted()
    //{
    //     OnToppingsCompleted.Invoke();
    // }


    void Start()
    {

        pizzaController = GetComponentInParent<PizzaController>();
        UpdateDebugText();

        Debug.Log($"[ToppingHandler] Ready for toppings on: {pizzaController.pizzaName}");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!toppingPhaseAllowed) return;

        string toppingTag = other.tag.ToLower();
        Debug.Log($"[ToppingHandler] Got unverified topping tag: {toppingTag}");

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

        AttachTopping(other.gameObject);



        Debug.Log($"[ToppingHandler] Added {toppingTag} (x{currentToppings[toppingTag]})");

        UpdateDebugText();
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
        toppingsHeader.text = "";
        toppingsProgress.text = "";
        UI.SetActive(false);
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



    private void UpdateDebugText()
    {
        if (toppingsHeader == null || toppingsProgress == null || pizzaController.orderData == null)
            return;

        toppingsHeader.text = toppingPhaseAllowed
            ? "Toppings: ACTIVE"
            : "Toppings: Inactive";

        string progress = "";
        progress = pizzaController.pizzaName + "\n";


        foreach (var req in pizzaController.orderData.requiredToppings)
        {
            string name = req.toppingName.ToLower();
            int required = req.requiredCount;
            int added = currentToppings.TryGetValue(name, out int have) ? have : 0;

            progress += $"{name}: {added}/{required}\n";
        }

        toppingsProgress.text = progress;
    }

    private void AttachTopping(GameObject topping)
    {
        Debug.Log($"[ToppingHandler] attaching topping");
        string toppingTag = topping.tag.ToLower();
        Debug.Log($"[ToppingHandler] topping tag {toppingTag}");

        GameObject prefab = nonGrabbableToppingPrefabs.Find(p => p.tag.ToLower() == toppingTag);
        if (prefab == null)
        {
            Debug.LogWarning($"[ToppingHandler] No non-grabbable prefab found for topping {toppingTag}");
            return;
        }

        // 1. Instantiate as a child of the pizza
        GameObject newTopping = Instantiate(prefab, pizzaController.saucedDoughInstance.transform);

        // 2. Reset local position and rotation
        newTopping.transform.localPosition = Vector3.zero; // Start at pizza center
        newTopping.transform.localRotation = Quaternion.identity;
        newTopping.transform.localScale = prefab.transform.localScale;

        // 3. Random offset slightly above the pizza
        Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * pizzaRadius;
        newTopping.transform.localPosition = new Vector3(randomOffset.x, yOffset, randomOffset.y);

        // 4. Destroy original grabbable topping
        Destroy(topping);

        Debug.Log($"[ToppingHandler] Replaced {toppingTag} with non-grabbable topping on pizza.");

        // 5. Record topping in dictionary
        if (!currentToppings.ContainsKey(toppingTag))
            currentToppings[toppingTag] = 0;

        currentToppings[toppingTag]++;
        UpdateDebugText();
    }
}
