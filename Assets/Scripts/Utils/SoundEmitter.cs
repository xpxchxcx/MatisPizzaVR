using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

namespace AudioSystem
{
    [RequireComponent(typeof(AudioSource))]
    public class SoundEmitter : MonoBehaviour
    {
        public SoundData Data { get; private set; }
        public AudioSource audioSource;
        Coroutine playCoroutine;

        [SerializeField] private float min = -0.05f;

        [SerializeField] private float max = 0.05f;
        void Awake()
        {
            audioSource = gameObject.GetOrAdd<AudioSource>();
        }

        public void Play()
        {

            if (playCoroutine != null)
                StopCoroutine(playCoroutine);

            audioSource.Play();
            playCoroutine = StartCoroutine(WaitForSoundEnd());
        }

        IEnumerator WaitForSoundEnd()
        {
            yield return new WaitWhile(() => audioSource.isPlaying);
            SoundManager.Instance.ReleaseSoundEmitter(this);
        }

        public void Stop()
        {
            if (playCoroutine != null)
            {
                StopCoroutine(playCoroutine);
                playCoroutine = null;
            }

            audioSource.Stop();
            SoundManager.Instance.ReleaseSoundEmitter(this);
        }

        public void Initialize(SoundData soundData, bool is3D = false)
        {
            Data = soundData;
            audioSource.clip = soundData.clip;
            audioSource.outputAudioMixerGroup = soundData.mixerGroup;
            audioSource.loop = soundData.loop;
            audioSource.playOnAwake = soundData.playOnAwake;
            audioSource.spatialBlend = is3D ? 1f : 0f; // 0 = 2D, 1 = 3D
        }

        public void WithRandomPitch(float? minOverride = null, float? maxOverride = null)
        {
            float low = minOverride ?? min;
            float high = maxOverride ?? max;
            audioSource.pitch += UnityEngine.Random.Range(low, high);

        }
    }

}
