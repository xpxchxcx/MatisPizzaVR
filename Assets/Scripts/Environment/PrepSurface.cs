using System;
using UnityEngine;
using UnityEngine.Events;

public class PrepSurface : MonoBehaviour
{
    public static event Action<GameObject> OnDoughPlaced;
    public static event Action<GameObject> OnDoughRemoved;
    public UnityEvent OnDebug;

    // Helper method for tests to trigger the event
    public static void TriggerOnDoughPlaced(GameObject dough)
    {
        OnDoughPlaced?.Invoke(dough);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("dough"))
        {
            GameObject doughGo = other.gameObject;
            DoughController currentDoughController = doughGo.GetComponentInChildren<DoughController>();

            currentDoughController.setDoughSurfaceTrue();


            Debug.Log($"Dough entered prep surface: {other.gameObject.name}");
            OnDoughPlaced?.Invoke(GetParent(doughGo));
            OnDebug.Invoke();

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("dough"))
        {
            GameObject doughGo = other.gameObject;

            DoughController currentDoughController = doughGo.GetComponentInChildren<DoughController>();

            currentDoughController.setDoughSurfaceFalse();

            OnDoughRemoved?.Invoke(GetParent(doughGo));
            Debug.Log($"Dough exited prep surface: {other.gameObject.name}");
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Collider col = GetComponent<Collider>();
        if (col is BoxCollider box)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(box.center, box.size);
        }
    }

    public static GameObject GetParent(GameObject go)
    {
        Transform parentTransform = go.transform.parent;
        return parentTransform.gameObject;
    }
}

