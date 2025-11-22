using UnityEngine;
using System.Collections.Generic;
using TMPro;

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

    public PizzaBakedPrefabs pizzaBakePrefabs;
    private GameObject finalPizzaInstance; // store the currently spawned cooked/burnt pizza

    public DoughController doughController;

    public GameObject flattenedDoughPrefab;
    public GameObject SaucedflattenedDoughPrefab;
    public GameObject sauceSpreadRecognizerPrefab;

    public GameObject flattenedDoughInstance;
    public GameObject saucedDoughInstance;

    public TextMeshPro tempDebug;

    void Start()
    {
        bakeTime = OvenController.Instance.bakeTime;
        burnTime = OvenController.Instance.burnTime;
    }

    private void OnEnable()
    {
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

    void Update()
    {
        tempDebug.text = $"Is inside oven: {isInsideOven} bakeTimer:{bakeTimer}/{bakeTime} \n State: {bakeState} is not cooked  and {bakeTimer} >= {bakeTime} is {bakeTimer >= bakeTime && bakeState != BakeState.Cooked} \n assemblyphase: {assemblyPhase}";

        if (isInsideOven)
        {
            bakeTimer += Time.deltaTime;

            float cookedThreshold = bakeTime;
            float burntThreshold = bakeTime + burnTime;

            if (bakeTimer >= burntThreshold && bakeState != BakeState.Burnt)
            {
                bakeState = BakeState.Burnt;
                isCooked = false;
                isBurnt = true;
                SpawnFinalPizza(true);
                Debug.Log("[Pizza] Burnt inside oven.");
            }
            else if (bakeTimer >= cookedThreshold && bakeState != BakeState.Cooked)
            {
                bakeState = BakeState.Cooked;
                isCooked = true;
                isBurnt = false;
                if (assemblyPhase != AssemblyPhase.Baked)
                {
                    OnBaked();
                }
                SpawnFinalPizza(false);
                Debug.Log("[Pizza] Finished cooking inside oven.");
            }
        }
    }

    // --- Oven interaction methods ---
    public void EnterOven(Transform ovenPoint)
    {

        //saucedDoughInstance.transform.position = ovenPoint.position;
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

    public void ExitOven()
    {
        isInsideOven = false;
        Debug.Log($"[Pizza] ExitOven called: {isInsideOven}");

        FinalizeBaking();
        Debug.Log("[Pizza] Exited oven.");
    }

    private void FinalizeBaking()
    {
        float cookedThreshold = bakeTime;
        float burntThreshold = bakeTime + burnTime;

        if (bakeTimer < cookedThreshold)
        {
            bakeState = BakeState.Raw;
            isCooked = false;
            isBurnt = false;
            Debug.Log("[Pizza] Removed early → RAW.");
        }
        else if (bakeTimer < burntThreshold)
        {
            bakeState = BakeState.Cooked;
            isCooked = true;
            isBurnt = false;
            if (finalPizzaInstance == null) SpawnFinalPizza(false);
            Debug.Log("[Pizza] Cooked correctly when removed.");
        }
        else
        {
            bakeState = BakeState.Burnt;
            isCooked = false;
            isBurnt = true;
            if (finalPizzaInstance == null) SpawnFinalPizza(true);
            Debug.Log("[Pizza] Burnt when removed.");
        }
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
    }

    public void OnSauceCompleted(GameObject go)
    {
        isSauced = true;
        AssemblyManager.Instance.AdvanceAssemblyPhase(go.GetComponent<PizzaController>(), AssemblyPhase.ToppingsStage);
        UpdateGameObjToSauced();
        Debug.Log($"[PizzaController] {pizzaName} sauce completed, moving to Toppings Stage.");
    }
    public void OnBaked()
    {

        AssemblyManager.Instance.AdvanceAssemblyPhase(GetComponent<PizzaController>(), AssemblyPhase.Baked);

        Debug.Log($"[PizzaController] {pizzaName} is baked, please serve!.");
    }

    public void OnBaking()
    {

        AssemblyManager.Instance.AdvanceAssemblyPhase(GetComponent<PizzaController>(), AssemblyPhase.Baking);

        Debug.Log($"[PizzaController] {pizzaName} is baking, please wait!.");
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
    }

    public void OnServed()
    {
        isServed = true;
        assemblyPhase = AssemblyPhase.Served;
        Debug.Log($"[PizzaController] {pizzaName} served!");
    }

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
        flattenedDoughInstance = flat;
        flat.transform.localPosition = pos;
        flat.transform.localRotation = rot;

        SpawnSauceSpreadRecognizerComponent(flat);
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
        sauced.transform.localPosition = pos;
        sauced.transform.localRotation = rot;

        HookToppingUI(saucedDoughInstance);
    }

    public void SpawnSauceSpreadRecognizerComponent(GameObject flattenedDough)
    {
        if (sauceSpreadRecognizerPrefab == null)
        {
            Debug.LogError("[PizzaController] No SauceSpreadRecognizer prefab assigned!");
            return;
        }

        GameObject recognizer = Instantiate(sauceSpreadRecognizerPrefab, flattenedDough.transform);
        recognizer.transform.localPosition = Vector3.zero;
        recognizer.transform.localRotation = Quaternion.identity;

        SauceSpreadRecognizer sauceSpreadRecognizer = recognizer.GetComponent<SauceSpreadRecognizer>();
        sauceSpreadRecognizer.sauceStatusText =
            flattenedDough.transform.Find("UI/SauceStatusText")?.GetComponent<TextMeshPro>();
        sauceSpreadRecognizer.sauceProgressText =
            flattenedDough.transform.Find("UI/SauceProgressText")?.GetComponent<TextMeshPro>();
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

        GameObject prefabToSpawn = burned
            ? pizzaBakePrefabs.GetBurntPrefab()
            : pizzaBakePrefabs.GetCookedPrefab(pizzaName);

        if (prefabToSpawn == null)
        {
            Debug.LogError($"[PizzaController] No prefab found for '{pizzaName}', burned={burned}");
            return;
        }

        finalPizzaInstance = Instantiate(prefabToSpawn, spawnPos, spawnRot, transform);
        Debug.Log($"[Pizza] Spawned {(burned ? "burnt" : "cooked")} pizza at correct position.");
    }
}
