using System;
using UnityEngine;

public class PrepSurface : MonoBehaviour
{
    public static event Action<GameObject> OnDoughPlaced;
    public static event Action<GameObject> OnDoughRemoved;

    // Helper method for tests to trigger the event
    public static void TriggerOnDoughPlaced(GameObject dough)
    {
        OnDoughPlaced?.Invoke(dough);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("dough"))
        {
            GameObject go = other.gameObject;
            DoughController currentDoughController = go.GetComponent<DoughController>();

            currentDoughController.setDoughSurfaceTrue();


            Debug.Log($"Dough entered prep surface: {other.gameObject.name}");
            OnDoughPlaced?.Invoke(go);

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("dough"))
        {
            GameObject go = other.gameObject;

            DoughController currentDoughController = go.GetComponent<DoughController>();

            currentDoughController.setDoughSurfaceFalse();

            OnDoughRemoved?.Invoke(go);
            Debug.Log($"Dough exited prep surface: {other.gameObject.name}");
        }
    }
}

