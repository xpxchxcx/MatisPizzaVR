using System;
using UnityEngine;
using TMPro;

public class DoughController : MonoBehaviour
{
    [SerializeField] private PizzaController pizzaController;

    [Header("Kneading Settings")]
    [SerializeField] private int kneadingRequired = 5;
    public int KneadingRequired => kneadingRequired; // keep public getter for tests
    private int currentKneadingCount = 0;

    public bool isDoughOnSurface = false;
    private bool isRightKneading = false;
    private bool isLeftKneading = false;
    public int CurrentKneadCount => currentKneadingCount;
    public bool IsRightKneading => isRightKneading;
    public bool IsLeftKneading => isLeftKneading;

    [Header("Knead Zone")]
    public KneadZone kneadZone;
    private bool isInKneadZone = false;
    public bool IsInKneadZone => isInKneadZone;

    /*     [Header("Debug Info")]
        public TextMeshPro tmp; // assign in inspector */

    public event Action<GameObject> OnDoughFlattened;
    private DoughDebugManager doughDebugManager;

    void Start()
    {
        AssemblyManager.StartKneadRightGestureEvent += StartRightKneadGesture;
        AssemblyManager.StartKneadLeftGestureEvent += StartLeftKneadGesture;
        AssemblyManager.EndKneadRightGestureEvent += EndRightKneadGesture;
        AssemblyManager.EndKneadLeftGestureEvent += EndLeftKneadGesture;

        pizzaController = GetComponentInParent<PizzaController>();
        kneadZone = GetComponent<KneadZone>();
        doughDebugManager = FindFirstObjectByType<DoughDebugManager>();


    }
    void OnEnable()
    {
        AssemblyManager.StartKneadRightGestureEvent += StartRightKneadGesture;
        AssemblyManager.StartKneadLeftGestureEvent += StartLeftKneadGesture;
        AssemblyManager.EndKneadRightGestureEvent += EndRightKneadGesture;
        AssemblyManager.EndKneadLeftGestureEvent += EndLeftKneadGesture;
    }

    void OnDestroy()
    {
        doughDebugManager = FindFirstObjectByType<DoughDebugManager>();
        if (doughDebugManager != null)
            doughDebugManager.UnregisterDough(this);
    }


    void OnDisable()
    {
        AssemblyManager.StartKneadRightGestureEvent -= StartRightKneadGesture;
        AssemblyManager.StartKneadLeftGestureEvent -= StartLeftKneadGesture;
        AssemblyManager.EndKneadRightGestureEvent -= EndRightKneadGesture;
        AssemblyManager.EndKneadLeftGestureEvent -= EndLeftKneadGesture;
    }


    void Update()
    {
        /* UpdateDebugText(); */
        RegisterKnead();
    }

    /* void UpdateDebugText()
    {
        if (tmp != null)
        {
            tmp.text = $"Dough Info:\n" +
                       $"- On Surface: {isDoughOnSurface}\n" +
                       $"- Right Hand Kneading: {isRightKneading}\n" +
                       $"- Left Hand Kneading: {isLeftKneading}\n" +
                       $"- In Knead Zone: {isInKneadZone}\n" +
                       $"- Knead Count: {currentKneadingCount}/{kneadingRequired}";
        }
    } */

    #region Right Hand Knead
    public void StartRightKneadGesture()
    {
        if (!isRightKneading)
        {
            isRightKneading = true;
            Debug.Log("[DoughController] Right Knead gesture started");
        }
    }

    public void EndRightKneadGesture()
    {
        if (isRightKneading)
        {
            isRightKneading = false;
            Debug.Log("[DoughController] Right Knead gesture ended");
        }
    }
    #endregion

    #region Left Hand Knead
    public void StartLeftKneadGesture()
    {
        if (!isLeftKneading)
        {
            isLeftKneading = true;
            Debug.Log("[DoughController] Left Knead gesture started");
        }
    }

    public void EndLeftKneadGesture()
    {
        if (isLeftKneading)
        {
            isLeftKneading = false;
            Debug.Log("[DoughController] Left Knead gesture ended");
        }
    }
    #endregion

    // Register kneading motion 
    public void RegisterKnead()
    {
        if (IsValidKnead())
        {
            currentKneadingCount++;
            Debug.Log($"Kneading motion detected! Count: {currentKneadingCount}/{kneadingRequired}");

            if (currentKneadingCount >= kneadingRequired)
            {
                OnDoughFlattened?.Invoke(PrepSurface.GetParent(this.gameObject));
                this.tag = "flattened";
                isDoughOnSurface = false;
            }

            isInKneadZone = false;

        }
    }

    // Keep for testing: simulate a kneading motion manually
    public void SimulateKneadingMotion()
    {
        if (isDoughOnSurface)
        {
            currentKneadingCount++;
            Debug.Log($"Simulated kneading motion! Count: {currentKneadingCount}/{kneadingRequired}");

            if (currentKneadingCount >= kneadingRequired)
                OnDoughFlattened?.Invoke(PrepSurface.GetParent(this.gameObject));
        }
    }

    public bool IsValidKnead()
    {
        // Must be on surface, in zone, and both hands kneading
        return isDoughOnSurface && isRightKneading && isLeftKneading && isInKneadZone;
    }

    #region Dough Surface
    public void setDoughSurfaceTrue()
    {
        isDoughOnSurface = true;
        doughDebugManager.RegisterDough(this);
    }

    public void setDoughSurfaceFalse()
    {
        isDoughOnSurface = false;
        doughDebugManager.UnregisterDough(this);

    }

    public void setDoughSurface(bool b)
    {
        isDoughOnSurface = b;
    }
    #endregion

    internal void SetHandInKneadZone(bool inZone)
    {
        isInKneadZone = inZone;
    }

    public PizzaController GetPizzaController()
    {
        return pizzaController;
    }


}
