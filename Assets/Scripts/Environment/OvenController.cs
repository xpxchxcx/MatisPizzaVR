using System.Collections;
using UnityEngine;

public class OvenController : MonoBehaviour
{
    [Header("Timing Settings")]
    [Tooltip("Seconds to bake before pizza is cooked.")]
    public float bakeTime = 15f;

    [Tooltip("Seconds after baking before pizza burns if not collected.")]
    public float burnGraceTime = 10f;

    [Header("Output Settings")]
    // to set in environment or tag a game obj
    public Transform outputPoint;

    [Tooltip("Optional: debug key to simulate VR grab/collect.")]
    public KeyCode debugCollectKey = KeyCode.Space;

    private GameObject currentPizza;
    private bool isCollectable = false;
    private Coroutine ovenRoutine;

    // VR hook: callable from XR event or hand trigger
    // could u open palm gesture to get it -> hook up this func
    public void CollectPizzaVR()
    {
        if (currentPizza == null || !isCollectable) return;

        var pizza = currentPizza.GetComponent<PizzaController>();
        if (pizza != null)
            CollectPizza(pizza);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("dough")) return;

        var pizza = other.GetComponent<PizzaController>();

        if (pizza.assemblyPhase != AssemblyPhase.ReadyForOven)
        {
            Debug.Log($"[OvenController] {pizza.pizzaName} not ready for oven: destroying!");
            Destroy(other.gameObject);
            return;
        }

        currentPizza = pizza.gameObject;
        ovenRoutine = StartCoroutine(BakePizzaRoutine(pizza));
    }

    private IEnumerator BakePizzaRoutine(PizzaController pizza)
    {
        Debug.Log($"[OvenController] {pizza.pizzaName} entered oven belt.");

        pizza.StartBaking();
        currentPizza.SetActive(false); // simulate inside oven

        yield return new WaitForSeconds(bakeTime);

        pizza.UpdateBaking(bakeTime, bakeTime, bakeTime + burnGraceTime);
        pizza.OnBaked();
        currentPizza.transform.position = outputPoint.position;
        currentPizza.SetActive(true);
        isCollectable = true;

        Debug.Log($"[OvenController] {pizza.pizzaName} is ready to collect! (VR or Space key)");

        float burnTimer = 0f;
        while (burnTimer < burnGraceTime && isCollectable)
        {
            // Debug fallback (non-VR)
            if (Input.GetKeyDown(debugCollectKey))
            {
                CollectPizza(pizza);
                yield break;
            }

            burnTimer += Time.deltaTime;
            yield return null;
        }

        // Burn if uncollected
        pizza.UpdateBaking(bakeTime + burnGraceTime, bakeTime, bakeTime + burnGraceTime);
        isCollectable = false;
        Debug.Log($"[OvenController] {pizza.pizzaName} burned due to late collection!");
    }

    private void CollectPizza(PizzaController pizza)
    {
        isCollectable = false;
        pizza.isCooked = true;
        pizza.bakeState = BakeState.Cooked;

        Debug.Log($"[OvenController] {pizza.pizzaName} collected successfully!");
        // TODO: trigger VR animation / score UI / haptic feedback here
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == currentPizza)
        {
            currentPizza = null;
            isCollectable = false;
            if (ovenRoutine != null) StopCoroutine(ovenRoutine);
        }
    }
}
