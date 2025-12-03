using System;
using UnityEngine;
using UnityEngine.Audio;

namespace AudioSystem
{
    public class SoundBuilder
    {
        readonly SoundManager soundManager;
        public SoundData soundData;
        public Vector3 position = Vector3.zero;
        public bool randomPitch;

        public SoundBuilder(SoundManager soundManager)
        {
            this.soundManager = soundManager;
        }

        public SoundBuilder WithSoundData(SoundData soundData)
        {
            this.soundData = soundData;
            return this;
        }

        public SoundBuilder AtPosition(Vector3 position)
        {
            this.position = position;
            return this;
        }

        public void Play()
        {
            if (!soundManager.CanPlaySound(soundData))
            {
                Debug.LogWarning("Max sound instances reached for sound: " + soundData.clip.name);
                return;
            }
            SoundEmitter emitter = soundManager.GetSoundEmitter();
            emitter.Initialize(soundData);
            emitter.transform.position = position;
            emitter.transform.SetParent(soundManager.transform);

            if (randomPitch)
            {
                emitter.WithRandomPitch();
            }

            soundManager.Counts[soundData] = soundManager.Counts.TryGetValue(soundData, out int count) ? count + 1 : 1;

            emitter.Play();
        }
    }


}