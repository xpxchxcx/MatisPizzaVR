using UnityEngine;

public class PrepSurface : MonoBehaviour
{

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

            Debug.Log($"Dough exited prep surface: {other.gameObject.name}");
        }
    }
}

