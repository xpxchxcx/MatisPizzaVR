using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PizzaBakeUI : MonoBehaviour
{
    public Slider progressBar;          // visual bar
    public TextMeshProUGUI timerText;   // countdown
    public TextMeshProUGUI bakeStatusText; // shows Raw / Baking / Cooked / Burnt
    public GameObject anchor;
    private PizzaController pizza;
    float remaining;

    private void Start()
    {
        pizza = GetComponent<PizzaController>();
        if (pizza == null)
        {
            Debug.Log("PizzaBakeUI cant find pizza controller");
        }
        else
        {
            Debug.Log("PizzaBakeUI found pizza controller");

        }
    }

    private void Update()
    {
        if (pizza == null)
        {
            progressBar.gameObject.SetActive(false);
            timerText.gameObject.SetActive(false);
            bakeStatusText.gameObject.SetActive(false);
            return;
        }

        // Only show UI during baking or after baked
        bool showUI = pizza.assemblyPhase == AssemblyPhase.Baking
                      || pizza.assemblyPhase == AssemblyPhase.Baked;

        progressBar.gameObject.SetActive(showUI);
        timerText.gameObject.SetActive(showUI);
        bakeStatusText.gameObject.SetActive(showUI);

        if (!showUI) return;

        // Make anchor follow sauced dough if it exists
        if (pizza.saucedDoughInstance != null && anchor != null)
        {
            var follower = anchor.GetComponentInChildren<ParentFollower>();
            if (follower != null)
                follower.Follow(pizza.saucedDoughInstance.transform);
        }

        // Update progress bar: relative only to baking time
        progressBar.value = Mathf.Clamp01(pizza.bakeTimer / pizza.bakeTime);

        // Update remaining time based on bake state
        float remaining;
        switch (pizza.bakeState)
        {
            case BakeState.Raw:
            case BakeState.Baking:
                remaining = Mathf.Max(0f, pizza.bakeTime - pizza.bakeTimer);
                break;
            case BakeState.Cooked:
                remaining = Mathf.Max(0f, (pizza.bakeTime + pizza.burnTime) - pizza.bakeTimer);
                break;
            case BakeState.Burnt:
                remaining = 0f;
                break;
            default:
                remaining = 0f;
                break;
        }
        timerText.text = remaining.ToString("F1") + "s";

        // Update bake status text
        switch (pizza.bakeState)
        {
            case BakeState.Raw:
                bakeStatusText.text = "Raw";
                break;
            case BakeState.Baking:
                bakeStatusText.text = "Baking...";
                break;
            case BakeState.Cooked:
                bakeStatusText.text = "Cooked!";
                break;
            case BakeState.Burnt:
                bakeStatusText.text = "Burnt!";
                break;
        }
    }
}
