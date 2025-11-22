using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PizzaBakedPrefabs", menuName = "Game/ PizzaBakedPrefabs")]
public class PizzaBakedPrefabs : ScriptableObject
{
    [System.Serializable]
    public struct CookedEntry
    {
        public string pizzaName;
        public GameObject cookedPrefab;
    }

    [Header("Cooked Pizza Prefabs")]
    public List<CookedEntry> cookedList = new List<CookedEntry>();

    [Header("Burnt Pizza Prefab (Shared)")]
    public GameObject burntPrefab;

    /// <summary>
    /// Looks up cooked prefab for given pizza name.
    /// </summary>
    public GameObject GetCookedPrefab(string name)
    {
        foreach (var e in cookedList)
        {
            if (e.pizzaName.Equals(name, System.StringComparison.OrdinalIgnoreCase))
                return e.cookedPrefab;
        }

        Debug.LogWarning($"[PizzaBakeData] No cooked prefab found for '{name}'");
        return null;
    }

    public GameObject GetBurntPrefab()
    {
        return burntPrefab;
    }
}
