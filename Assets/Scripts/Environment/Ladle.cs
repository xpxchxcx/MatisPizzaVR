using UnityEngine;

public class Ladle : MonoBehaviour
{
    public GameObject sauce;
    public bool hasSauce;

    void Start()
    {
        sauce = transform.Find("Soup").gameObject;
    }

    void Update()
    {
        hasSauce = sauce.activeInHierarchy;
    }

    public void ToggleSoup()
    {
        if (sauce.activeInHierarchy)
        {
            sauce.SetActive(false);
        }
        else
        {
            sauce.SetActive(true);
        }
    }

    public void SetSoupTrue()
    {
        sauce.SetActive(true);
    }

    public void SetSoupFalse()
    {
        sauce.SetActive(false);
    }
}
