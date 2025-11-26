using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PizzaBakeUI : MonoBehaviour
{
    public Slider bakeProgressBar;      // shows raw → cooked
    public Slider burntProgressBar;     // shows cooked → burnt
    public TextMeshProUGUI timerText;   // countdown
    public TextMeshProUGUI bakeStatusText; // shows Raw / Baking / Cooked / Burnt
    public GameObject anchor;

    private PizzaController pizza;

    private void Start()
    {
        pizza = GetComponent<PizzaController>();
        if (pizza == null)
        {
            Debug.LogError("PizzaBakeUI can't find PizzaController!");
        }
        else
        {
            Debug.Log("PizzaBakeUI found PizzaController");
        }
    }

    private void Update()
    {
        if (pizza == null)
            return;

        // Hide UI if pizza is served, burnt, or has no active GameObject
        if (!pizza.IsCurrentlyBaking || pizza.assemblyPhase <= AssemblyPhase.ReadyForOven || pizza.isServed || pizza.bakeState == BakeState.Burnt ||
            (pizza.saucedDoughInstance == null && pizza.currentBakedPizzaInstance == null))
        {
            bakeProgressBar.gameObject.SetActive(false);
            burntProgressBar.gameObject.SetActive(false);
            timerText.gameObject.SetActive(false);
            bakeStatusText.gameObject.SetActive(false);
            return;
        }

        // Show anchor following pizza
        GameObject target = pizza.saucedDoughInstance ?? pizza.currentBakedPizzaInstance;
        if (target != null && anchor != null)
        {
            var follower = anchor.GetComponentInChildren<ParentFollower>();
            if (follower != null)
                follower.Follow(target.transform);
        }

        // Update UI depending on bake state
        switch (pizza.bakeState)
        {
            case BakeState.Raw:
            case BakeState.Baking:
                bakeProgressBar.gameObject.SetActive(true);
                burntProgressBar.gameObject.SetActive(false);
                timerText.gameObject.SetActive(true);
                bakeStatusText.gameObject.SetActive(true);

                bakeProgressBar.value = Mathf.Clamp01(pizza.bakeTimer / pizza.bakeTime);
                timerText.text = Mathf.Max(0f, pizza.bakeTime - pizza.bakeTimer).ToString("F1") + "s";
                bakeStatusText.text = "Baking...";
                break;

            case BakeState.Cooked:
                bakeProgressBar.gameObject.SetActive(false);
                burntProgressBar.gameObject.SetActive(true);
                timerText.gameObject.SetActive(true);
                bakeStatusText.gameObject.SetActive(true);

                float burnProgress = Mathf.Clamp01((pizza.bakeTimer - pizza.bakeTime) / pizza.burnTime);
                burntProgressBar.value = burnProgress;
                timerText.text = Mathf.Max(0f, (pizza.bakeTime + pizza.burnTime - pizza.bakeTimer)).ToString("F1") + "s";
                bakeStatusText.text = "Cooked!";
                break;
        }
    }
}
