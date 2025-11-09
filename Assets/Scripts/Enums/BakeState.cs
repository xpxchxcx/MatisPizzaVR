using UnityEngine;

/// <summary>
/// Tracks the pizza’s baking condition while in the oven.
/// </summary>
public enum BakeState
{
    Raw,
    DoughPlaced,
    Baking,
    Cooked,
    Burnt
}
