using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "OrderLibrary", menuName = "Game/OrderLibrary")]
public class OrderLibrary : ScriptableObject
{
    public List<OrderData> possibleOrders;
}
