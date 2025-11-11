using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "OrderData", menuName = "Game/OrderData")]
public class OrderData : ScriptableObject
{
    [Header("Order Info")]
    public string orderId;
    public string pizzaName;

    [Header("Topping Requirements")]
    public List<ToppingRequirement> requiredToppings;  // each topping has name + required count

    public float timeLimit = 60f;  // seconds
    public int maxScoreValue = 100;

    [Header("Status")]
    public bool isInProgress;
    public bool isCompleted;
}

[System.Serializable]
public class ToppingRequirement
{
    public string toppingName;
    public int requiredCount;
}
