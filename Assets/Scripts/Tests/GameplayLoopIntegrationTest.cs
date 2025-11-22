using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections.Generic;

public class GameplayLoopIntegrationTest
{
    private GameObject orderManagerGO;
    private GameObject assemblyManagerGO;
    private GameObject scoreManagerGO;
    private OrderManager orderManager;
    private AssemblyManager assemblyManager;
    private ScoreManager scoreManager;

    private GameObject pizzaGO;
    private PizzaController pizzaController;
    private DoughController doughController;
    private ToppingHandler toppingHandler;
    private SauceSpreadRecognizer sauceRecognizer;

    private OrderData testOrderData;
    private OrderLibrary testOrderLibrary;

    [SetUp]
    public void SetUp()
    {
        // Create manager GameObjects
        orderManagerGO = new GameObject("OrderManager");
        orderManager = orderManagerGO.AddComponent<OrderManager>();

        assemblyManagerGO = new GameObject("AssemblyManager");
        assemblyManager = assemblyManagerGO.AddComponent<AssemblyManager>();

        scoreManagerGO = new GameObject("ScoreManager");
        scoreManager = scoreManagerGO.AddComponent<ScoreManager>();

        // Create test OrderData
        testOrderData = ScriptableObject.CreateInstance<OrderData>();
        testOrderData.pizzaName = "Test Pizza";
        testOrderData.orderId = "ORD-TEST-001";
        testOrderData.requiredToppings = new List<ToppingRequirement>
        {
            new ToppingRequirement { toppingName = "pepperoni", requiredCount = 3 },
            new ToppingRequirement { toppingName = "cheese", requiredCount = 2 }
        };
        testOrderData.isInProgress = false;
        testOrderData.isCompleted = false;

        // Create test OrderLibrary
        testOrderLibrary = ScriptableObject.CreateInstance<OrderLibrary>();
        testOrderLibrary.possibleOrders = new List<OrderData> { testOrderData };

        // Assign OrderLibrary to OrderManager
        orderManager.orderLibrary = testOrderLibrary;

        // Create pizza GameObject with all required components
        pizzaGO = new GameObject("TestPizza");
        pizzaController = pizzaGO.AddComponent<PizzaController>();
        doughController = pizzaGO.AddComponent<DoughController>();
        toppingHandler = pizzaGO.AddComponent<ToppingHandler>();
        sauceRecognizer = pizzaGO.AddComponent<SauceSpreadRecognizer>();

        // Add collider (tag not needed since we call TriggerOnDoughPlaced directly in tests)
        BoxCollider collider = pizzaGO.AddComponent<BoxCollider>();

        // Reset ScoreManager
        scoreManager.ResetScore();

        // Clear all orders
        orderManager.ClearAllOrders();
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up ScriptableObjects
        if (testOrderData != null)
            Object.DestroyImmediate(testOrderData);
        if (testOrderLibrary != null)
            Object.DestroyImmediate(testOrderLibrary);

        // Destroy GameObjects
        if (pizzaGO != null)
            Object.DestroyImmediate(pizzaGO);
        if (orderManagerGO != null)
            Object.DestroyImmediate(orderManagerGO);
        if (assemblyManagerGO != null)
            Object.DestroyImmediate(assemblyManagerGO);
        if (scoreManagerGO != null)
            Object.DestroyImmediate(scoreManagerGO);
    }

    [UnityTest]
    public IEnumerator TestFullGameplayLoop_SuccessfulPizza()
    {
        // 1. Spawn order
        orderManager.SpawnRandomOrder();
        Assert.AreEqual(1, orderManager.activeOrders.Count, "Order should be in activeOrders");
        Assert.AreEqual(0, orderManager.startedOrders.Count, "No orders should be started yet");
        OrderData spawnedOrder = orderManager.activeOrders[0];

        yield return null;

        // 2. Place dough - triggers PrepSurface.OnDoughPlaced
        PrepSurface.TriggerOnDoughPlaced(pizzaGO);
        yield return null; // Wait for event handlers

        // Verify order moved to startedOrders and pizza linked
        Assert.AreEqual(0, orderManager.activeOrders.Count, "Order should be removed from activeOrders");
        Assert.AreEqual(1, orderManager.startedOrders.Count, "Order should be in startedOrders");
        Assert.AreEqual(spawnedOrder.orderId, pizzaController.orderData.orderId, "Pizza should be linked to order via orderId");
        Assert.AreEqual(AssemblyPhase.Kneading, pizzaController.assemblyPhase, "Pizza should be in Kneading phase");

        // 3. Knead dough (use actual required count from DoughController)
        // Set dough on surface first (required for SimulateKneadingMotion to work)
        doughController.setDoughSurfaceTrue();
        int kneadingRequired = doughController.KneadingRequired;
        for (int i = 0; i < kneadingRequired; i++)
        {
            doughController.SimulateKneadingMotion();
            yield return null;
        }

        // Verify phase progression
        Assert.AreEqual(AssemblyPhase.SauceStage, pizzaController.assemblyPhase, "Pizza should be in SauceStage after kneading");

        // 4. Complete sauce (use actual required volume from SauceSpreadRecognizer)
        sauceRecognizer.setCanBeSauced(true);
        sauceRecognizer.registerSauceAmount(); // Trigger completion
        yield return null; // Wait for Update() to call CheckSauceMotion and fire event
        yield return null; // Extra frame to ensure event handlers process

        // Verify sauce stage complete
        Assert.AreEqual(AssemblyPhase.ToppingsStage, pizzaController.assemblyPhase, "Pizza should be in ToppingsStage after sauce");
        Assert.IsTrue(pizzaController.isSauced, "Pizza should be marked as sauced");

        // 5. Add required toppings
        // Add pepperoni (3 required)
        pizzaController.AddTopping("pepperoni");
        pizzaController.AddTopping("pepperoni");
        pizzaController.AddTopping("pepperoni");

        // Add cheese (2 required)
        pizzaController.AddTopping("cheese");
        pizzaController.AddTopping("cheese");

        // Trigger toppings completed event
        ToppingHandler.TriggerOnToppingsCompleted();
        yield return null;

        // Verify ready for oven
        Assert.AreEqual(AssemblyPhase.ReadyForOven, pizzaController.assemblyPhase, "Pizza should be ready for oven");

        // 6. Start baking
        //pizzaController.StartBaking();
        Assert.AreEqual(AssemblyPhase.Baking, pizzaController.assemblyPhase, "Pizza should be in Baking phase");

        // 7. Complete baking (use actual values from OvenController)
        GameObject ovenGO = new GameObject("TestOven");
        OvenController ovenController = ovenGO.AddComponent<OvenController>();
        float bakeTime = ovenController.bakeTime;
        float burnTime = ovenController.bakeTime + ovenController.burnTime;
        float elapsed = 0f;
        float fixedDelta = 0.1f; // Use fixed delta for predictable timing

        while (pizzaController.assemblyPhase == AssemblyPhase.Baking && elapsed < bakeTime + 1f)
        {
            //pizzaController.UpdateBaking(fixedDelta, bakeTime, burnTime);
            elapsed += fixedDelta;
            yield return null;
        }

        // Cleanup
        Object.DestroyImmediate(ovenGO);

        // Verify pizza is cooked
        Assert.AreEqual(AssemblyPhase.Baked, pizzaController.assemblyPhase, "Pizza should be baked");
        Assert.IsTrue(pizzaController.isCooked, "Pizza should be marked as cooked");
        Assert.IsFalse(pizzaController.isBurnt, "Pizza should not be burnt");

        // 8. Serve pizza
        int initialScore = scoreManager.GetScore();
        bool orderCompletedFired = false;
        OrderData completedOrder = null;

        orderManager.OnOrderCompleted += (order) =>
        {
            orderCompletedFired = true;
            completedOrder = order;
        };

        // Simulate serving - trigger ServeZone.OnPizzaServed
        bool validationSuccess = orderManager.ValidatePizzaAndCompleteOrder(pizzaController);
        ServeZone.TriggerOnPizzaServed(pizzaController, validationSuccess);
        yield return null;

        // Verify order completion
        Assert.IsTrue(validationSuccess, "Pizza validation should succeed");
        Assert.AreEqual(0, orderManager.startedOrders.Count, "Order should be removed from startedOrders");
        Assert.AreEqual(1, orderManager.completedOrders.Count, "Order should be in completedOrders");
        Assert.IsTrue(orderCompletedFired, "OnOrderCompleted event should fire");
        Assert.AreEqual(spawnedOrder.orderId, completedOrder.orderId, "Completed order should match spawned order");

        // Verify scoring
        int finalScore = scoreManager.GetScore();
        Assert.AreEqual(initialScore + 100, finalScore, "Score should increase by 100 (baseScore)");
    }

    [UnityTest]
    public IEnumerator TestFullGameplayLoop_BurntPizza()
    {
        // Spawn order and place dough
        orderManager.SpawnRandomOrder();
        PrepSurface.TriggerOnDoughPlaced(pizzaGO);
        yield return null;

        // Complete assembly steps (kneading, sauce, toppings)
        doughController.setDoughSurfaceTrue();
        int kneadingRequired = doughController.KneadingRequired;
        for (int i = 0; i < kneadingRequired; i++)
        {
            doughController.SimulateKneadingMotion();
        }
        yield return null;

        sauceRecognizer.setCanBeSauced(true);

        sauceRecognizer.registerSauceAmount();
        yield return null; // Wait for Update() to process
        yield return null; // Extra frame for event handlers

        pizzaController.AddTopping("pepperoni");
        pizzaController.AddTopping("pepperoni");
        pizzaController.AddTopping("pepperoni");
        pizzaController.AddTopping("cheese");
        pizzaController.AddTopping("cheese");
        ToppingHandler.TriggerOnToppingsCompleted();
        yield return null;

        // Start baking
        //pizzaController.StartBaking();

        // Over-bake pizza (beyond burn time - use actual values from OvenController)
        GameObject ovenGO = new GameObject("TestOven");
        OvenController ovenController = ovenGO.AddComponent<OvenController>();
        float bakeTime = ovenController.bakeTime;
        float burnTime = ovenController.bakeTime + ovenController.burnTime;
        float elapsed = 0f;
        float fixedDelta = 0.1f; // Use fixed delta for more predictable timing

        // Bake until burnt (need to go past burnTime)
        // Use bakeTime > burnTime so it burns before it can be considered "cooked"
        // But we'll use actual values, so we need to keep phase as Baking
        while (!pizzaController.isBurnt && elapsed < burnTime + 2f)
        {
            // Keep pizza in Baking phase so UpdateBaking continues to work
            if (pizzaController.assemblyPhase != AssemblyPhase.Baking)
            {
                pizzaController.assemblyPhase = AssemblyPhase.Baking;
            }
            // Use reversed values: burnTime < bakeTime so burn check happens first
            //pizzaController.UpdateBaking(fixedDelta, burnTime + 1f, burnTime);
            elapsed += fixedDelta;
            yield return null;
        }

        // Cleanup
        Object.DestroyImmediate(ovenGO);

        // Verify pizza is burnt
        Assert.IsTrue(pizzaController.isBurnt, "Pizza should be burnt");
        Assert.IsFalse(pizzaController.isCooked, "Pizza should not be cooked when burnt");

        // Serve pizza
        int initialScore = scoreManager.GetScore();
        bool validationSuccess = orderManager.ValidatePizzaAndCompleteOrder(pizzaController);
        ServeZone.TriggerOnPizzaServed(pizzaController, validationSuccess);
        yield return null;

        // Verify validation fails
        Assert.IsFalse(validationSuccess, "Burnt pizza validation should fail");

        // Verify order remains in startedOrders (not completed)
        Assert.AreEqual(1, orderManager.startedOrders.Count, "Order should remain in startedOrders");
        Assert.AreEqual(0, orderManager.completedOrders.Count, "Order should not be in completedOrders");

        // Verify penalty applied
        int finalScore = scoreManager.GetScore();
        Assert.AreEqual(initialScore - 50, finalScore, "Score should decrease by 50 (penaltyBurnt)");
    }

    [UnityTest]
    public IEnumerator TestFullGameplayLoop_WrongToppings()
    {
        // Spawn order and place dough
        orderManager.SpawnRandomOrder();
        PrepSurface.TriggerOnDoughPlaced(pizzaGO);
        yield return null;

        // Complete kneading and sauce
        doughController.setDoughSurfaceTrue();
        int kneadingRequired = doughController.KneadingRequired;
        for (int i = 0; i < kneadingRequired; i++)
        {
            doughController.SimulateKneadingMotion();
        }
        yield return null;

        sauceRecognizer.setCanBeSauced(true);

        sauceRecognizer.registerSauceAmount();
        yield return null; // Wait for Update() to process
        yield return null; // Extra frame for event handlers

        // Add wrong toppings (mushrooms instead of pepperoni/cheese)
        pizzaController.AddTopping("mushroom");
        pizzaController.AddTopping("mushroom");
        pizzaController.AddTopping("mushroom");
        ToppingHandler.TriggerOnToppingsCompleted();
        yield return null;

        // Complete baking (use actual values from OvenController)
        //pizzaController.StartBaking();
        GameObject ovenGO = new GameObject("TestOven");
        OvenController ovenController = ovenGO.AddComponent<OvenController>();
        float bakeTime = ovenController.bakeTime;
        float burnTime = ovenController.bakeTime + ovenController.burnTime;
        float elapsed = 0f;
        float fixedDelta = 0.1f; // Use fixed delta for predictable timing

        while (pizzaController.assemblyPhase == AssemblyPhase.Baking && elapsed < bakeTime + 1f)
        {
            //pizzaController.UpdateBaking(fixedDelta, bakeTime, burnTime);
            elapsed += fixedDelta;
            yield return null;
        }

        // Cleanup
        Object.DestroyImmediate(ovenGO);

        // Verify pizza is cooked but has wrong toppings
        Assert.IsTrue(pizzaController.isCooked, "Pizza should be cooked");
        Assert.IsFalse(pizzaController.ValidateAgainstOrder(), "Pizza with wrong toppings should fail validation");

        // Serve pizza
        int initialScore = scoreManager.GetScore();
        bool validationSuccess = orderManager.ValidatePizzaAndCompleteOrder(pizzaController);
        ServeZone.TriggerOnPizzaServed(pizzaController, validationSuccess);
        yield return null;

        // Verify validation fails
        Assert.IsFalse(validationSuccess, "Pizza with wrong toppings should fail validation");

        // Verify order remains in startedOrders
        Assert.AreEqual(1, orderManager.startedOrders.Count, "Order should remain in startedOrders");
        Assert.AreEqual(0, orderManager.completedOrders.Count, "Order should not be in completedOrders");

        // Verify penalty applied
        int finalScore = scoreManager.GetScore();
        Assert.AreEqual(initialScore - 25, finalScore, "Score should decrease by 25 (penaltyWrongToppings)");
    }

    [UnityTest]
    public IEnumerator TestOrderAssignment_PreventDuplicateAssignments()
    {
        // Spawn 2 orders
        orderManager.SpawnRandomOrder();
        orderManager.SpawnRandomOrder();

        Assert.AreEqual(2, orderManager.activeOrders.Count, "Should have 2 orders in activeOrders");
        Assert.AreEqual(0, orderManager.startedOrders.Count, "No orders should be started yet");

        OrderData firstOrder = orderManager.activeOrders[0];
        OrderData secondOrder = orderManager.activeOrders[1];

        yield return null;

        // Place first dough - should assign first order
        PrepSurface.TriggerOnDoughPlaced(pizzaGO);
        yield return null;

        Assert.AreEqual(1, orderManager.activeOrders.Count, "One order should remain in activeOrders");
        Assert.AreEqual(1, orderManager.startedOrders.Count, "One order should be in startedOrders");
        Assert.AreEqual(firstOrder.orderId, orderManager.startedOrders[0].orderId, "First order should be started");
        Assert.AreEqual(firstOrder.orderId, pizzaController.orderData.orderId, "Pizza should be linked to first order");

        // Create second pizza GameObject
        GameObject pizzaGO2 = new GameObject("TestPizza2");
        PizzaController pizzaController2 = pizzaGO2.AddComponent<PizzaController>();
        DoughController doughController2 = pizzaGO2.AddComponent<DoughController>();
        pizzaGO2.AddComponent<ToppingHandler>();
        pizzaGO2.AddComponent<SauceSpreadRecognizer>();
        BoxCollider collider2 = pizzaGO2.AddComponent<BoxCollider>();

        yield return null;

        // Place second dough - should assign second order
        PrepSurface.TriggerOnDoughPlaced(pizzaGO2);
        yield return null;

        Assert.AreEqual(0, orderManager.activeOrders.Count, "No orders should remain in activeOrders");
        Assert.AreEqual(2, orderManager.startedOrders.Count, "Both orders should be in startedOrders");
        Assert.AreEqual(secondOrder.orderId, pizzaController2.orderData.orderId, "Second pizza should be linked to second order");

        // Verify GetNextPendingOrder returns null when activeOrders is empty
        OrderData nextOrder = orderManager.GetNextPendingOrder();
        Assert.IsNull(nextOrder, "GetNextPendingOrder should return null when activeOrders is empty");

        // Cleanup
        Object.DestroyImmediate(pizzaGO2);
    }
}

