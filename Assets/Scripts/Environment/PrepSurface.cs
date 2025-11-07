using UnityEngine;

public class PrepSurface : MonoBehaviour
{
    [SerializeField] private bool isDoughOnSurface = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("dough"))
        {
            isDoughOnSurface = true;
            Debug.Log($"Dough entered prep surface: {other.gameObject.name}");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("dough"))
        {
            isDoughOnSurface = false;
            Debug.Log($"Dough left prep surface: {other.gameObject.name}");
        }
    }

    public bool IsDoughOnSurface()
    {
        return isDoughOnSurface;
    }
}

