using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "OrderData", menuName = "Game/OrderData")]
public class OrderData : ScriptableObject
{
    [Header("Order Info")]
    public string orderId;
    public string pizzaName;
    public List<string> requiredToppings;
    public float timeLimit = 60f;  // seconds
    public int scoreValue = 100;

    [Header("Status")]
    public bool isInProgress;
    public bool isCompleted;
}
