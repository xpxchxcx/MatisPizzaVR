using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DoughDebugManager : MonoBehaviour
{
    public TextMeshPro debugOutput;

    private readonly List<DoughController> allDoughs = new();

    public void RegisterDough(DoughController dough)
    {
        if (!allDoughs.Contains(dough))
            allDoughs.Add(dough);
    }

    public void UnregisterDough(DoughController dough)
    {
        if (allDoughs.Contains(dough))
            allDoughs.Remove(dough);
    }

    void Update()
    {
        if (debugOutput == null) return;

        string output = "";

        foreach (var dough in allDoughs)
        {
            output += $"{dough.name}:\n" +
                      $"- On Surface: {dough.isDoughOnSurface}\n" +
                      $"- Right Knead: {dough.IsRightKneading}\n" +
                      $"- Left Knead: {dough.IsLeftKneading}\n" +
                      $"- In Knead Zone: {dough.IsInKneadZone}\n" +
                      $"- Knead Count: {dough.CurrentKneadCount}/{dough.KneadingRequired}\n\n";
        }

        debugOutput.text = output;
    }
}
