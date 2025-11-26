using System.Collections;
using UnityEngine;

public class OvenController : Singleton<OvenController>
{
    [Header("Timing Settings")]
    [Tooltip("Seconds to bake before pizza is cooked.")]
    public float bakeTime = 20f;

    [Tooltip("Seconds after baking before pizza burns if not collected.")]
    public float burnTime = 10f;

    public bool isSomethingCooking = false;

    public Transform ovenPoint;
    [Header("Oven Cooking Settings")]
    [Tooltip("Radius around ovenPoint where pizza continues cooking.")]
    public float cookRadius = 0.35f;

    private void OnDrawGizmos()
    {
        if (ovenPoint == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(ovenPoint.position, cookRadius);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("sauced"))
        {
            PizzaController pizza = other.GetComponentInParent<PizzaController>();


            if (isSomethingCooking || pizza.assemblyPhase < AssemblyPhase.ReadyForOven || pizza.assemblyPhase > AssemblyPhase.Baked)
            {
                Debug.Log("[Oven] Wrong phase for baking. Or something is baking already");
                return;
            }
            Debug.Log("[Oven]calling enter oven.");

            pizza.EnterOven(ovenPoint);
            Debug.Log("[Oven] Pizza entered oven surface.");
        }
    }



    void Update()
    {
        if (isSomethingCooking)
        {
            transform.Find("CookingCollider").gameObject.SetActive(true);
        }
        else
        {
            transform.Find("CookingCollider").gameObject.SetActive(false);
        }
    }
}
