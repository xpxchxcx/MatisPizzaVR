using UnityEngine;

/// <summary>
/// Tracks which stage of pizza assembly the player is currently on.
/// </summary>
public enum AssemblyPhase
{
    None,           // not yet started
    Kneading,       // dough being kneaded
    SauceStage,     // sauce spreading
    ToppingsStage,  // adding toppings
    ReadyForOven,   // assembled, ready to bake
    Baking,         // inside oven
    Baked,          // fully baked
    Served          // delivered to customer
}
