using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System;
using UnityEngine.UI;
using AudioSystem;

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



    [Header("Baking")]
    public bool isInsideOven = false;
    public float bakeTimer = 0f;
    public float bakeTime;
    public float burnTime;
    private bool isCurrentlyBaking = false;
    public bool IsCurrentlyBaking => isCurrentlyBaking;

    public static event Action<GameObject> InBaking;
    public event Action PizzaDead;

    public PizzaBakedPrefabs pizzaBakePrefabs;
    private GameObject finalPizzaInstance; // store the currently spawned cooked/burnt pizza

    public DoughController doughController;
    public GameObject flattenedDoughPrefab;
    public GameObject SaucedflattenedDoughPrefab;
    public GameObject sauceSpreadRecognizerPrefab;

    // Instances for reference
    public GameObject unflattenedDoughInstance;

    public GameObject flattenedDoughInstance;
    public GameObject saucedDoughInstance;
    public GameObject currentBakedPizzaInstance;



    public GameObject pizzaLabelPrefab;
    private PizzaLabelUI pizzaLabelUI;
    public PizzaLabelUI PizzaLabelUI => pizzaLabelUI;


    void Start()
    {

        bakeTime = OvenController.Instance.bakeTime;
        burnTime = OvenController.Instance.burnTime;
    }

    private void OnEnable()
    {
        doughController.OnDoughFlattened += OnDoughFlattened;
    }

    private void OnDisable()
    {
        doughController.OnDoughFlattened -= OnDoughFlattened;
    }



    void Update()
    {
        if (OvenController.Instance == null || OvenController.Instance.ovenPoint == null)
            return;

        // Nothing to bake or burn
        if (saucedDoughInstance == null && currentBakedPizzaInstance == null)
            return;

        // Determine which GameObject represents the current state
        GameObject bakingGO = bakeState == BakeState.Raw ? saucedDoughInstance : currentBakedPizzaInstance;
        if (bakingGO == null)
            return;

        // Distance check in XZ-plane
        Vector3 posXZ = new Vector3(bakingGO.transform.position.x, 0f, bakingGO.transform.position.z);
        Vector3 ovenXZ = new Vector3(OvenController.Instance.ovenPoint.position.x, 0f, OvenController.Instance.ovenPoint.position.z);
        float distanceToOven = Vector3.Distance(posXZ, ovenXZ);

        bool insideOvenNow = distanceToOven <= OvenController.Instance.cookRadius;

        // Fire event only when entering the oven
        if (insideOvenNow && !isCurrentlyBaking)
        {
            SoundManager.Instance.PlayLoopSFX("Pizza_Cooking");
            isCurrentlyBaking = true;
            InBaking?.Invoke(bakingGO);
            Debug.Log("[Pizza] Entered oven, baking started");
        }

        // Fire event only when leaving the oven
        if (!insideOvenNow && isCurrentlyBaking)
        {
            SoundManager.Instance.StopLoopSFX("Pizza_Cooking");

            isCurrentlyBaking = false;
            OvenController.Instance.isSomethingCooking = false;

            InBaking?.Invoke(null); // or use a separate OnBakingStopped event
            Debug.Log("[Pizza] Left oven, baking stopped");
        }

        isInsideOven = insideOvenNow;

        // Only cook if inside oven
        if (!isInsideOven)
            return;

        bakeTimer += Time.deltaTime;

        float cookedThreshold = bakeTime;
        float burntThreshold = bakeTime + burnTime;

        // --- Handle Burnt first ---
        if (bakeTimer >= burntThreshold && bakeState != BakeState.Burnt)
        {
            bakeState = BakeState.Burnt;
            isCooked = false;
            isBurnt = true;

            SpawnFinalPizza(true); // replaces baking GameObject with burnt version
            Debug.Log("[Pizza] Burnt inside oven");
            return;
        }

        // --- Handle Cooked ---
        if (bakeTimer >= cookedThreshold && bakeState == BakeState.Raw)
        {
            bakeState = BakeState.Cooked;
            SoundManager.Instance.StopLoopSFX("Pizza_Timer_Done");

            isCooked = true;
            isBurnt = false;

            if (assemblyPhase != AssemblyPhase.Baked)
                OnBaked();

            SpawnFinalPizza(false); // replaces baking GameObject with cooked version
            Debug.Log("[Pizza] Finished cooking inside oven");
            return;
        }


    }
    // --- Oven interaction methods ---
    public void EnterOven(Transform ovenPoint)
    {
        OvenController.Instance.isSomethingCooking = true;
        saucedDoughInstance.transform.position = ovenPoint.position;
        if (assemblyPhase != AssemblyPhase.ReadyForOven)
        {
            Debug.LogWarning("[Pizza] Not ready for oven.");
            return;
        }

        isInsideOven = true;
        Debug.Log($"[Pizza] EnterOven called: {isInsideOven}");
        bakeTimer = 0f;
        bakeState = BakeState.Raw;
        isCooked = false;
        isBurnt = false;

        OnBaking();
        Debug.Log("[Pizza] Entered oven, baking started.");
    }



    // --- Existing methods for dough, sauce, toppings ---
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
        UpdateLabelProgress();
    }

    public void OnSauceCompleted(GameObject go)
    {
        isSauced = true;
        AssemblyManager.Instance.AdvanceAssemblyPhase(go.GetComponent<PizzaController>(), AssemblyPhase.ToppingsStage);
        UpdateGameObjToSauced();
        Debug.Log($"[PizzaController] {pizzaName} sauce completed, moving to Toppings Stage.");
        UpdateLabelProgress();
    }
    public void OnBaked()
    {

        AssemblyManager.Instance.AdvanceAssemblyPhase(GetComponent<PizzaController>(), AssemblyPhase.Baked);

        Debug.Log($"[PizzaController] {pizzaName} is baked, please serve!.");
        UpdateLabelProgress();
    }

    public void OnBaking()
    {

        AssemblyManager.Instance.AdvanceAssemblyPhase(GetComponent<PizzaController>(), AssemblyPhase.Baking);

        Debug.Log($"[PizzaController] {pizzaName} is baking, please wait!.");
        UpdateLabelProgress();
    }

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
        AssemblyManager.Instance.AdvanceAssemblyPhase(GetComponent<PizzaController>(), AssemblyPhase.ReadyForOven);
        Debug.Log($"[PizzaController] {pizzaName} ready for oven!");
        UpdateLabelProgress();
    }

    public void OnServed()
    {
        assemblyPhase = AssemblyPhase.Served;
        isServed = true;
        UpdateLabelProgress();

        Destroy(PizzaLabelUI, 1f);
        Debug.Log($"[PizzaController] {pizzaName} served!");
        PizzaDead?.Invoke();
        Destroy(this.gameObject, 2f);
        // put somme vfx and sound

    }

    public bool ValidateAgainstOrder()
    {
        Debug.Log("===== VALIDATING PIZZA =====");

        // Show all toppings on pizza
        Debug.Log("TOPPINGS ON PIZZA:");
        foreach (var kvp in toppings)
            Debug.Log($" - {kvp.Key}: {kvp.Value}");

        // Show required toppings
        Debug.Log("REQUIRED TOPPINGS:");
        foreach (var req in orderData.requiredToppings)
            Debug.Log($" - {req.toppingName}: {req.requiredCount}");

        Debug.Log($"Cooked: {isCooked}, Sauced: {isSauced}, Burnt: {isBurnt}");


        if (orderData == null) return false;
        if (!isCooked || !isSauced) return false;
        if (isBurnt) return false;

        foreach (var req in orderData.requiredToppings)
        {
            if (!toppings.TryGetValue(req.toppingName, out int count))
            {
                Debug.LogWarning($"[PizzaController] Missing topping: {req.toppingName}");
                return true;
                //return false;
            }

            if (count < req.requiredCount)
            {
                Debug.LogWarning($"[PizzaController] Not enough {req.toppingName} (needed {req.requiredCount}, got {count})");
                return true;
                //return false;
            }
        }

        Debug.Log($"[PizzaController] {pizzaName} validated successfully!");
        return true;
    }

    public void UpdateGameObjToFlattened()
    {
        Transform disableChild = transform.Find("Unflattened_Dough");
        if (disableChild != null)
            disableChild.gameObject.SetActive(false);
        SpawnFlattenedDough();
    }

    public void UpdateGameObjToSauced()
    {
        if (flattenedDoughInstance != null)
            flattenedDoughInstance.SetActive(false);
        SpawnSaucedFlattenedDough();
    }

    public void UpdateGameObjToCooked()
    {
        if (saucedDoughInstance != null)
            saucedDoughInstance.SetActive(false);
    }

    public void SpawnFlattenedDough()
    {
        Transform dough = doughController.transform;
        Transform parent = dough.parent;
        Vector3 pos = dough.localPosition;
        Quaternion rot = dough.localRotation;
        pos.y += 0.02f;

        GameObject flat = Instantiate(flattenedDoughPrefab, parent);
        Debug.Log($"Instantiated {"flattened prefab for order {orderId}: {pizza.name}"}");
        flattenedDoughInstance = flat;
        flat.transform.localPosition = pos;
        flat.transform.localRotation = rot;
        UpdateLabelTarget();
        SpawnSauceSpreadRecognizerComponent(flattenedDoughInstance);


    }

    public void SpawnSaucedFlattenedDough()
    {
        if (flattenedDoughInstance == null)
        {
            Debug.LogError("[PizzaController] Tried to spawn sauced dough but no flattened dough exists!");
            return;
        }

        Transform flat = flattenedDoughInstance.transform;
        Transform parent = flat.parent;
        Vector3 pos = flat.localPosition;
        Quaternion rot = flat.localRotation;

        GameObject sauced = Instantiate(SaucedflattenedDoughPrefab, parent);
        saucedDoughInstance = sauced;
        saucedDoughInstance.GetComponentInChildren<ToppingHandler>().OnToppingsCompleted += OnToppingsCompleted;
        sauced.transform.localPosition = pos;
        sauced.transform.localRotation = rot;
        UpdateLabelTarget();
        HookToppingUI(saucedDoughInstance);
    }

    public void SpawnSauceSpreadRecognizerComponent(GameObject flattenedDough)
    {
        if (sauceSpreadRecognizerPrefab == null)
        {
            Debug.Log("[PizzaController] No SauceSpreadRecognizer prefab assigned!");
            return;
        }


        GameObject recognizer = Instantiate(sauceSpreadRecognizerPrefab, flattenedDough.transform);
        Debug.Log($"Instantiated SauceSpreadRecognizer for order {orderData.orderId}: {orderData.name}");
        recognizer.transform.localPosition = Vector3.zero;
        recognizer.transform.localRotation = Quaternion.identity;

        SauceSpreadRecognizer sauceSpreadRecognizer = recognizer.GetComponent<SauceSpreadRecognizer>();
        sauceSpreadRecognizer.sauceStatusText =
            flattenedDough.transform.Find("UI/SauceStatusText")?.GetComponent<TextMeshPro>();
        sauceSpreadRecognizer.sauceProgressText =
            flattenedDough.transform.Find("UI/SauceProgressText")?.GetComponent<TextMeshPro>();


        sauceSpreadRecognizer.OnSauceComplete += OnSauceCompleted;
    }

    private void HookToppingUI(GameObject sauceDough)
    {
        ToppingHandler handler = sauceDough.GetComponentInChildren<ToppingHandler>();
        if (handler == null) return;

        handler.toppingsHeader =
            sauceDough.transform.Find("UI/ToppingsHeaderText")?.GetComponent<TextMeshPro>();
        handler.toppingsProgress =
            sauceDough.transform.Find("UI/ToppingsProgressText")?.GetComponent<TextMeshPro>();
    }

    public void SpawnFinalPizza(bool burned)
    {
        Vector3 spawnPos;
        Quaternion spawnRot;

        if (finalPizzaInstance != null)
        {
            spawnPos = finalPizzaInstance.transform.position;
            spawnRot = finalPizzaInstance.transform.rotation;
            Destroy(finalPizzaInstance);
        }
        else if (saucedDoughInstance != null)
        {
            spawnPos = saucedDoughInstance.transform.position;
            spawnRot = saucedDoughInstance.transform.rotation;
            Destroy(saucedDoughInstance);
        }
        else
        {
            spawnPos = transform.position;
            spawnRot = transform.rotation;
        }

        spawnPos.y += 0.03f;

        GameObject prefabToSpawn = burned
            ? pizzaBakePrefabs.GetBurntPrefab()
            : pizzaBakePrefabs.GetCookedPrefab(pizzaName);

        if (prefabToSpawn == null)
        {
            Debug.LogError($"[PizzaController] No prefab found for '{pizzaName}', burned={burned}");
            return;
        }

        finalPizzaInstance = Instantiate(prefabToSpawn, spawnPos, spawnRot, transform);

        currentBakedPizzaInstance = finalPizzaInstance;
        Transform parent = transform.parent;
        ParentFollower parentFollower = parent.GetComponentInChildren<ParentFollower>();
        parentFollower.Follow(currentBakedPizzaInstance.transform);
        UpdateLabelTarget();
        Debug.Log($"[Pizza] Spawned {(burned ? "burnt" : "cooked")} pizza at correct position.");
    }


    public void UpdateLabelTarget()
    {
        if (pizzaLabelUI == null)
        {
            Debug.Log("label pizza ui is null"); return;
        }


        // Pick the current "active" instance
        GameObject currentGO = null;

        if (currentBakedPizzaInstance != null)
            currentGO = currentBakedPizzaInstance;
        else if (saucedDoughInstance != null)
            currentGO = saucedDoughInstance;
        else if (flattenedDoughInstance != null)
            currentGO = flattenedDoughInstance;
        else
            currentGO = unflattenedDoughInstance;

        if (currentGO != null)

            pizzaLabelUI.SetTarget(currentGO.transform);
    }

    public void InitLabel()
    {
        GameObject labelObj = Instantiate(pizzaLabelPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
        pizzaLabelUI = labelObj.GetComponent<PizzaLabelUI>();
        Transform xrCam = GameObject.Find("Main Camera XR").transform;

        TextMeshProUGUI orderText = labelObj.transform.Find("Order Number")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI pizzaText = labelObj.transform.Find("Pizza Type")?.GetComponent<TextMeshProUGUI>();
        Slider slider = labelObj.transform.Find("AssemblySlider")?.GetComponent<Slider>();

        // Assign UI elements
        pizzaLabelUI.SetUIElements(orderText, pizzaText, slider);
        pizzaLabelUI.Initialize(this.gameObject, unflattenedDoughInstance.transform, orderData.orderId, pizzaName, 6, xrCam);
        UpdateLabelProgress();
    }


    public void UpdateLabelProgress()
    {
        if (pizzaLabelUI == null) return;

        float progress = 0f;

        switch (assemblyPhase)
        {
            case AssemblyPhase.Kneading: progress = 0f; break;
            case AssemblyPhase.SauceStage: progress = 1f; break;
            case AssemblyPhase.ToppingsStage: progress = 2f; break;
            case AssemblyPhase.ReadyForOven: progress = 3f; break;
            case AssemblyPhase.Baking: progress = 4f; break;
            case AssemblyPhase.Baked: progress = 5f; break;
            case AssemblyPhase.Served: progress = 6f; break;
        }

        pizzaLabelUI.UpdateProgress(progress);
    }
}
