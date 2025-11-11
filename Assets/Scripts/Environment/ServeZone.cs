using UnityEngine;

public class ServeZone : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("pizza"))
        {
            PizzaController pizzaController = other.GetComponent<PizzaController>();

            OrderManager.Instance.ValidateOrder(pizzaController);

        }
    }
}
