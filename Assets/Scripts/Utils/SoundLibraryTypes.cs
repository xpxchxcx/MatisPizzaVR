using System;
using System.Collections.Generic;
using UnityEngine;
namespace AudioSystem
{
    public enum SoundType
    {
        Music,
        Ambience,
        SFX,
        UI,
        Voice
    }

    public enum SfxCategory
    {
        Player,
        UI,
        Environment,
        Impact,
        Special
    }



    [Serializable]
    public class SoundEntry
    {
        [Header("Identity")]

        public SoundType type;      // Music / Ambience / SFX / UI / Voice
        public SfxCategory sfxCategory;

        public string id;           // e.g. "SFX__PLAYER__GLYPH_CAST__01"
        public SoundData data;
    }

}