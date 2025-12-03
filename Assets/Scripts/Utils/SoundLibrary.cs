using System;
using System.Collections.Generic;
using UnityEngine;
using AudioSystem;

[CreateAssetMenu(fileName = "SoundLibrary", menuName = "Scriptable Objects/Sound Library")]
public class SoundLibrarySO : ScriptableObject
{
    [SerializeField] private List<SoundEntry> entries = new();

    private Dictionary<string, SoundData> lookup;

    void OnEnable()
    {
        BuildLookup();
    }

    void BuildLookup()
    {
        lookup = new Dictionary<string, SoundData>(StringComparer.OrdinalIgnoreCase);

        foreach (var e in entries)
        {
            if (e == null || e.data == null || string.IsNullOrWhiteSpace(e.id))
                continue;

            if (lookup.ContainsKey(e.id))
            {
                Debug.LogWarning($"Duplicate sound id '{e.id}' in {name}");
                continue;
            }

            lookup.Add(e.id, e.data);
        }
    }

    public bool TryGet(string id, out SoundData data)
    {
        if (lookup == null || lookup.Count == 0)
            BuildLookup();

        if (string.IsNullOrWhiteSpace(id))
        {
            data = null;
            return false;
        }

        return lookup.TryGetValue(id, out data) && data != null;
    }

    //  expose entries read-only if you need them elsewhere
    public IReadOnlyList<SoundEntry> Entries => entries;
}