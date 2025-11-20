using UnityEngine;

public class SauceContainer : MonoBehaviour
{

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ladle"))
        {
            Transform grandParent = other.transform.parent.parent;
            Ladle ladle = grandParent.GetComponent<Ladle>();
            ladle.SetSoupTrue();
        }
    }
}
