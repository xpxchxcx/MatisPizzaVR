using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OrderCard : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text pizzaNameText;
    public TMP_Text toppingsListText;
    public TMP_Text timerText;

    private OrderData orderData;

    public void Setup(OrderData order)
    {
        orderData = order;

        if (pizzaNameText != null)
            pizzaNameText.text = order.pizzaName;

        if (toppingsListText != null && order.requiredToppings != null)
            toppingsListText.text = string.Join(", ", order.requiredToppings);

        if (timerText != null)
            timerText.text = $"{order.timeLimit:F0}s";
    }
}
