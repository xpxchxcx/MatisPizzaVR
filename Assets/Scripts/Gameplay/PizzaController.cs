using UnityEngine;
using System.Collections.Generic;
using System;

public class PizzaController : MonoBehaviour
{
    [Header("Order Info")]
    public string pizzaName;
    public OrderData orderData;

    [Header("Assembly State")]
    public AssemblyPhase assemblyPhase = AssemblyPhase.None;
    public BakeState bakeState = BakeState.Raw;

    [Header("Toppings")]
    public Dictionary<string, int> toppings = new();
    public bool isSauced = false;
    public bool isCooked = false;
    public bool isBurnt = false;
    public bool isServed = false;

    private float bakeTimer = 0f;


    public DoughController doughController;

    public GameObject flattenedDoughPrefab;


    void Start()
    {
        doughController.GetComponentInChildren<DoughController>();
    }

    private void OnEnable()
    {
        doughController.GetComponentInChildren<DoughController>();
        SauceSpreadRecognizer.OnSauceComplete += OnSauceCompleted;
        doughController.OnDoughFlattened += OnDoughFlattened;
        ToppingHandler.OnToppingsCompleted += OnToppingsCompleted;
    }

    private void OnDisable()
    {
        SauceSpreadRecognizer.OnSauceComplete -= OnSauceCompleted;
        doughController.OnDoughFlattened -= OnDoughFlattened;
        ToppingHandler.OnToppingsCompleted -= OnToppingsCompleted;

    }

    /// <summary>
    /// Initialize the pizza from a given order.
    /// </summary>
    public void InitializeFromOrder(OrderData order)
    {
        orderData = order;
        pizzaName = order.pizzaName;
        assemblyPhase = AssemblyPhase.Kneading;
        bakeState = BakeState.Raw;

        isSauced = false;
        isCooked = false;
        isBurnt = false;
        isServed = false;

        toppings.Clear();

        Debug.Log($"[PizzaController] Initialized pizza for order: {pizzaName}");
    }

    public void OnDoughFlattened(GameObject go)
    {
        AssemblyManager.Instance.AdvanceAssemblyPhase(go.GetComponent<PizzaController>(), AssemblyPhase.SauceStage);
        UpdateGameObjToFlattened();
        Debug.Log($"[PizzaController] {pizzaName} flattened, moving to Sauce Stage.");
    }

    public void OnSauceCompleted()
    {
        isSauced = true;
        assemblyPhase = AssemblyPhase.ToppingsStage;
        Debug.Log($"[PizzaController] {pizzaName} sauce completed, moving to Toppings Stage.");
    }

    /// <summary>
    /// Adds a topping to this pizza (tracks count).
    /// </summary>
    public void AddTopping(string toppingName)
    {
        if (assemblyPhase != AssemblyPhase.ToppingsStage)
        {
            Debug.LogWarning($"[PizzaController] Can't add topping yet! Current phase: {assemblyPhase}");
            return;
        }

        if (toppings.ContainsKey(toppingName))
            toppings[toppingName]++;
        else
            toppings[toppingName] = 1;

        Debug.Log($"[PizzaController] Added topping: {toppingName} (x{toppings[toppingName]})");
    }

    public void OnToppingsCompleted()
    {
        assemblyPhase = AssemblyPhase.ReadyForOven;
        Debug.Log($"[PizzaController] {pizzaName} ready for oven!");
    }

    public void OnBaked()
    {
        assemblyPhase = AssemblyPhase.Baked;
    }

    public void StartBaking()
    {
        if (assemblyPhase != AssemblyPhase.ReadyForOven)
        {
            Debug.LogWarning("[PizzaController] Pizza not ready for oven yet!");
            return;
        }

        assemblyPhase = AssemblyPhase.Baking;
        bakeState = BakeState.Baking;
        bakeTimer = 0f;

        Debug.Log($"[PizzaController] {pizzaName} started baking.");
    }

    public void UpdateBaking(float deltaTime, float bakeTime, float burnTime)
    {
        if (assemblyPhase != AssemblyPhase.Baking)
            return;

        bakeTimer += deltaTime;

        if (bakeTimer >= burnTime)
        {
            bakeState = BakeState.Burnt;
            isBurnt = true;
            isCooked = false;
            Debug.Log($"[PizzaController] {pizzaName} burnt!");
        }
        else if (bakeTimer >= bakeTime)
        {
            bakeState = BakeState.Cooked;
            isCooked = true;
            isBurnt = false;
            assemblyPhase = AssemblyPhase.Baked;
            Debug.Log($"[PizzaController] {pizzaName} cooked!");
        }
    }

    public void OnServed()
    {
        isServed = true;
        assemblyPhase = AssemblyPhase.Served;
        Debug.Log($"[PizzaController] {pizzaName} served!");
    }

    /// <summary>
    /// Checks whether pizza meets order topping requirements.
    /// </summary>
    public bool ValidateAgainstOrder()
    {
        if (orderData == null) return false;
        if (!isCooked || !isSauced) return false;
        if (isBurnt) return false;

        foreach (var req in orderData.requiredToppings)
        {
            if (!toppings.TryGetValue(req.toppingName, out int count))
            {
                Debug.LogWarning($"[PizzaController] Missing topping: {req.toppingName}");
                return false;
            }

            if (count < req.requiredCount)
            {
                Debug.LogWarning($"[PizzaController] Not enough {req.toppingName} (needed {req.requiredCount}, got {count})");
                return false;
            }
        }

        Debug.Log($"[PizzaController] {pizzaName} validated successfully!");
        return true;
    }

    public void UpdateGameObjToFlattened()
    {
        // Disable child
        Transform disableChild = transform.Find("Unflattened_Dough");
        if (disableChild != null)
            disableChild.gameObject.SetActive(false);
        SpawnFlattenedDough();


    }


    public void SpawnFlattenedDough()
    {
        // Save transforms
        Transform dough = doughController.transform;

        Transform parent = dough.parent;              // keep same parent
        Vector3 pos = dough.localPosition;            // keep dough’s position relative to parent
        Quaternion rot = dough.localRotation;         // keep dough’s rotation


        pos.y += 0.02f;

        // Instantiate new flattened dough under the SAME parent
        GameObject flat = Instantiate(flattenedDoughPrefab, parent);

        // Restore pos/rot relative to parent
        flat.transform.localPosition = pos;
        flat.transform.localRotation = rot;


        Debug.Log("[DoughController] Spawned flattened dough under same parent.");
    }
}
