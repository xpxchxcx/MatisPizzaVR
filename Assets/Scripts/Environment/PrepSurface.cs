using System;
using UnityEngine;

public class PrepSurface : MonoBehaviour
{
    public static event Action<DoughController> OnDoughPlaced;
    public static event Action<DoughController> OnDoughRemoved;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("dough"))
        {
            DoughController currentDoughController = other.GetComponent<DoughController>();
            if (currentDoughController != null)
            {
                currentDoughController.setDoughSurfaceTrue();
            }

            Debug.Log($"Dough entered prep surface: {other.gameObject.name}");
            OnDoughPlaced?.Invoke(currentDoughController);

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("dough"))
        {
            DoughController currentDoughController = other.GetComponent<DoughController>();
            if (currentDoughController != null)
            {
                currentDoughController.setDoughSurfaceFalse();
            }
            OnDoughRemoved?.Invoke(currentDoughController);
            Debug.Log($"Dough exited prep surface: {other.gameObject.name}");
        }
    }
}

