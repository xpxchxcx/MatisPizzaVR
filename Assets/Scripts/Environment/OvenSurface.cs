using System;
using AudioSystem;
using UnityEngine;

public class OvenSurface : MonoBehaviour
{
    public static event Action<GameObject> OnSaucedDoughPlaced;
    public static event Action<GameObject> OnSaucedDoughRemoved;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("sauced"))
        {
            SoundManager.Instance.PlaySFX("Place");
            GameObject go = other.gameObject;
            PizzaController currentPizzaController = go.transform.parent.GetComponent<PizzaController>();

            if (currentPizzaController != null && currentPizzaController.assemblyPhase == AssemblyPhase.ReadyForOven)
            {

                Debug.Log($"topped Dough entered topping prep surface: {other.gameObject.name}");
            }

        }
    }

}

