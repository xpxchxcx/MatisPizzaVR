using UnityEngine;

public class UIKiller : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("DeadLabel"))
        {
            Destroy(collision.gameObject, 2f);
        }
    }
}
