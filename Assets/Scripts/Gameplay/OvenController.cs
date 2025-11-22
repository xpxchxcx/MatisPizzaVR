using System.Collections;
using UnityEngine;

public class OvenController : Singleton<OvenController>
{
    [Header("Timing Settings")]
    [Tooltip("Seconds to bake before pizza is cooked.")]
    public float bakeTime = 20f;

    [Tooltip("Seconds after baking before pizza burns if not collected.")]
    public float burnTime = 10f;

    public Transform ovenPoint;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("sauced"))
        {
            PizzaController pizza = other.GetComponentInParent<PizzaController>();


            if (pizza.assemblyPhase < AssemblyPhase.ReadyForOven)
            {
                Debug.Log("[Oven] Wrong phase for baking.");
                return;
            }
            Debug.Log("[Oven]calling enter oven.");

            pizza.EnterOven(ovenPoint);
            Debug.Log("[Oven] Pizza entered oven surface.");
        }
    }

    private void OnTriggerExit(Collider other)
    {

        var pizza = other.GetComponentInParent<PizzaController>();
        if (pizza == null) return;

        pizza.ExitOven();
        Debug.Log("[Oven] Pizza removed from oven.");
    }
}
