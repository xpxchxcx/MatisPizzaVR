using UnityEngine;
using System.Collections.Generic;

public class PizzaController : MonoBehaviour
{
    [Header("Order Info")]
    public string pizzaName;
    public OrderData orderData;

    [Header("Assembly State")]
    public AssemblyPhase assemblyPhase = AssemblyPhase.None;
    public BakeState bakeState = BakeState.Raw;

    [Header("Toppings")]
    public List<string> toppingsAdded = new();
    public bool isSauced = false;
    public bool isCooked = false;
    public bool isBurnt = false;
    public bool isServed = false;

    private float bakeTimer = 0f;

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

        toppingsAdded.Clear();

        Debug.Log($"[PizzaController] Initialized pizza for order: {pizzaName}");
    }

    /// <summary>
    /// Called when kneading stage completes (player finishes motion).
    /// </summary>
    public void OnDoughFlattened()
    {
        assemblyPhase = AssemblyPhase.SauceStage;
        Debug.Log($"[PizzaController] {pizzaName} flattened, moving to Sauce Stage.");
    }

    /// <summary>
    /// Called when sauce spreading completes.
    /// </summary>
    public void OnSauceCompleted()
    {
        isSauced = true;
        assemblyPhase = AssemblyPhase.ToppingsStage;
        Debug.Log($"[PizzaController] {pizzaName} sauce completed, moving to Toppings Stage.");
    }

    /// <summary>
    /// Adds a topping to this pizza.
    /// </summary>
    public void AddTopping(string toppingName)
    {
        if (assemblyPhase != AssemblyPhase.ToppingsStage)
        {
            Debug.LogWarning($"[PizzaController] Can't add topping yet! Current phase: {assemblyPhase}");
            return;
        }

        toppingsAdded.Add(toppingName);
        Debug.Log($"[PizzaController] Added topping: {toppingName}");
    }

    /// <summary>
    /// Called when all toppings are added and pizza is ready to bake.
    /// </summary>
    public void OnToppingsCompleted()
    {
        assemblyPhase = AssemblyPhase.ReadyForOven;
        Debug.Log($"[PizzaController] {pizzaName} ready for oven!");
    }

    /// <summary>
    /// Begins baking process.
    /// </summary>
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

    /// <summary>
    /// Simulate baking over time (can be called from OvenController).
    /// </summary>
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

    /// <summary>
    /// Called when pizza is removed from oven or placed in ServeZone.
    /// </summary>
    public void OnServed()
    {
        isServed = true;
        assemblyPhase = AssemblyPhase.Served;
        Debug.Log($"[PizzaController] {pizzaName} served!");
    }

    /// <summary>
    /// Checks whether pizza meets order requirements.
    /// </summary>
    public bool ValidateAgainstOrder()
    {
        if (orderData == null) return false;
        if (!isCooked || !isSauced) return false;

        var required = new HashSet<string>(orderData.requiredToppings);
        var added = new HashSet<string>(toppingsAdded);

        bool toppingsMatch = required.SetEquals(added);

        return toppingsMatch && !isBurnt;
    }
}
