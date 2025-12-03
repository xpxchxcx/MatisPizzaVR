using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

using TMPro;
using UnityEngine;
using UnityEngine.Pool;

namespace AudioSystem
{
    public class SoundManager : Singleton<SoundManager>
    {
        [SerializeField] private SoundLibrarySO library;

        IObjectPool<SoundEmitter> soundEmitterPool;
        readonly List<SoundEmitter> activeSoundEmitters = new List<SoundEmitter>();
        public readonly Dictionary<SoundData, int> Counts = new Dictionary<SoundData, int>();

        [SerializeField] SoundEmitter soundEmitterPrefab;
        [SerializeField] bool collectionCheck = true;
        [SerializeField] int defaultCapacity = 10;
        [SerializeField] int maxPoolSize = 100;
        [SerializeField] int maxSoundInstances = 30;

        // BGM Management
        [SerializeField] private AudioSource bgmSourceA;
        [SerializeField] private AudioSource bgmSourceB;

        private AudioSource activeBgm;
        private AudioSource inactiveBgm;

        private Coroutine bgmFadeRoutine;

        // Ambience Management (max 3 layers)
        [SerializeField] private AudioSource[] ambienceSources = new AudioSource[3];
        private SoundData[] ambienceSlots = new SoundData[3];
        private Coroutine[] ambienceFadeRoutines = new Coroutine[3];   // used for fade-IN only

        //Loop SFX stuff like for spell drawing

        Dictionary<string, AudioSource> loopChannels;
        private Coroutine loopFadeRoutine;

        private void OnEnable()
        {
            // if (GestureRecognizer.Instance != null)
            // {
            //     GestureRecognizer.Instance.OnDrawStart.AddListener(HandleDrawStart);
            //     GestureRecognizer.Instance.OnDrawEnd.AddListener(HandleDrawEnd);
            //     GestureRecognizer.Instance.OnGestureRecognized.AddListener(HandleGestureRecognized);
            //     GestureRecognizer.Instance.OnGestureNotRecognized.AddListener(HandleGestureNotRecognized);
            // }

            //Slow.. need change
            // var spells = FindFirstObjectByType<Spells>();
            // if (spells != null)
            // {
            //     spells.validSpellServe.AddListener(HandleValidSpell);
            //     spells.invalidSpellServe.AddListener(HandleInvalidSpell);
            // }
        }

        private void OnDisable()
        {
            // if (GestureRecognizer.Instance != null)
            // {
            //     GestureRecognizer.Instance.OnDrawStart.RemoveListener(HandleDrawStart);
            //     GestureRecognizer.Instance.OnDrawEnd.RemoveListener(HandleDrawEnd);
            //     GestureRecognizer.Instance.OnGestureRecognized.RemoveListener(HandleGestureRecognized);
            //     GestureRecognizer.Instance.OnGestureNotRecognized.RemoveListener(HandleGestureNotRecognized);
            // }

            // var spells = FindFirstObjectByType<Spells>();
            // if (spells != null)
            // {
            //     spells.validSpellServe.RemoveListener(HandleValidSpell);
            //     spells.invalidSpellServe.RemoveListener(HandleInvalidSpell);
            // }
        }

        void Start()
        {
            InitializePool();
            activeBgm = bgmSourceA;
            inactiveBgm = bgmSourceB;
            loopChannels = new Dictionary<string, AudioSource>();
            // GestureRecognizer.Instance.OnDrawStart.AddListener(HandleDrawStart);
            // GestureRecognizer.Instance.OnDrawEnd.AddListener(HandleDrawEnd);
            // GestureRecognizer.Instance.OnGestureRecognized.AddListener(HandleGestureRecognized);
            // GestureRecognizer.Instance.OnGestureNotRecognized.AddListener(HandleGestureNotRecognized);

            // var spells = FindFirstObjectByType<Spells>();
            // spells.validSpellServe.AddListener(HandleValidSpell);
            // spells.invalidSpellServe.AddListener(HandleInvalidSpell);
        }



        public SoundBuilder CreateSound() => new SoundBuilder(this);

        public bool CanPlaySound(SoundData soundData)
        {
            return !Counts.TryGetValue(soundData, out int count) || count < maxSoundInstances;
        }

        public SoundEmitter GetSoundEmitter()
        {
            return soundEmitterPool.Get();
        }

        public void ReleaseSoundEmitter(SoundEmitter emitter)
        {
            soundEmitterPool.Release(emitter);
        }

        private void InitializePool()
        {
            soundEmitterPool = new ObjectPool<SoundEmitter>(
                CreateSoundEmitter,
                OnTakeFromPool,
                OnReturnedToPool,
                OnDestroyPoolObject,
                collectionCheck,
                defaultCapacity,
                maxPoolSize
            );
        }

        private void OnDestroyPoolObject(SoundEmitter emitter)
        {
            Destroy(emitter.gameObject);
        }

        private void OnReturnedToPool(SoundEmitter emitter)
        {
            if (Counts.TryGetValue(emitter.Data, out var count))
            {
                Counts[emitter.Data] -= count > 0 ? 1 : 0;
            }
            emitter.gameObject.SetActive(false);
            activeSoundEmitters.Remove(emitter);
        }

        private void OnTakeFromPool(SoundEmitter emitter)
        {
            activeSoundEmitters.Add(emitter);
            emitter.gameObject.SetActive(true);
        }

        private SoundEmitter CreateSoundEmitter()
        {
            var soundEmitter = Instantiate(soundEmitterPrefab);
            soundEmitter.gameObject.SetActive(false);
            return soundEmitter;
        }

        // --- SFX / one-shot playback ---
        // to play sound: SoundManager.Instance.Play("SFX_Player_Run");
        // to play sound: SoundManager.Instance.Play(SoundIDs.SFX.Player.Dash);
        public void PlaySFX(string soundId, Vector3? position = null, bool randomPitch = false, bool is3D = false)
        {
            if (library == null)
            {
                Debug.LogError("No SoundLibrary assigned to SoundManager.");
                return;
            }

            if (!library.TryGet(soundId, out var soundData))
            {
                Debug.LogWarning($"Sound ID '{soundId}' not found in library.");
                return;
            }

            var builder = CreateSound()
                .WithSoundData(soundData)
                .AtPosition(position ?? Vector3.zero);

            builder.randomPitch = randomPitch;

            builder.Play();
        }


        // --- Random SFX with same id names ---
        // to play sound: SoundManager.Instance.Play(SoundIDs.SFX.Player.Run);
        public void PlayRandomSFX(string id, bool randomPitch = true)
        {
            // Collect all entries that match the exact same ID
            List<SoundData> matches = new List<SoundData>();

            foreach (var entry in library.Entries)
            {
                if (entry.id.StartsWith(id, StringComparison.OrdinalIgnoreCase))
                    matches.Add(entry.data);
            }

            if (matches.Count == 0)
            {
                Debug.LogWarning($"No SFX found for ID '{id}'");
                return;
            }

            // Pick random clip
            SoundData chosen = matches[UnityEngine.Random.Range(0, matches.Count)];
            var builder = CreateSound()
                           .WithSoundData(chosen)
                           .AtPosition(Vector3.zero);

            builder.randomPitch = randomPitch;


            builder.Play();
        }

        //Easy method to call footstep by animator event
        public void PlayFootstepSFX()
        {
            PlayRandomSFX(SoundIDs.SFX.Player.Run);
        }


        // --- BGM (crossfade between two dedicated AudioSources) ---
        private string bgmSoundId;
        public void PlayBGM(string soundId, float fadeTime = 1.5f)
        {
            if (bgmSoundId == soundId) return;//Dont cut the BGM if its the same
            if (!library.TryGet(soundId, out var soundData))
            {
                Debug.LogWarning($"BGM '{soundId}' not found in SoundLibrary");
                return;
            }
            if (activeBgm == null) activeBgm = bgmSourceA;
            if (activeBgm != null && activeBgm.clip != null && activeBgm.clip.name == soundData.clip.name)
            {
                return;
            }
            if (inactiveBgm == null) inactiveBgm = bgmSourceB;
            inactiveBgm.clip = soundData.clip;
            inactiveBgm.outputAudioMixerGroup = soundData.mixerGroup;
            inactiveBgm.loop = true;
            inactiveBgm.volume = 0f;
            inactiveBgm.Play();

            if (bgmFadeRoutine != null)
                StopCoroutine(bgmFadeRoutine);

            bgmFadeRoutine = StartCoroutine(FadeBGM(activeBgm, inactiveBgm, fadeTime));

            var temp = activeBgm;
            activeBgm = inactiveBgm;
            inactiveBgm = temp;
            bgmSoundId = soundId;
        }

        private IEnumerator FadeBGM(AudioSource from, AudioSource to, float duration)
        {
            float t = 0f;

            while (t < duration)
            {
                t += Time.deltaTime;
                float k = t / duration;

                from.volume = Mathf.Lerp(1f, 0f, k);
                to.volume = Mathf.Lerp(0f, 1f, k);

                yield return null;
            }

            from.Stop();
        }

        // --- Ambience: up to 3 layered loops ---
        // Example:
        // SoundManager.Instance.PlayAmbience(Abyss hum);
        // SoundManager.Instance.PlayAmbience(Abyss hum, Sanctivar streets);
        public void PlayAmbience(string layer0 = null, string layer1 = null, string layer2 = null, float fadeTime = 1.5f)
        {
            string[] ids = new string[3] { layer0, layer1, layer2 };
            ApplyAmbienceLayers(ids, fadeTime);
        }

        private void ApplyAmbienceLayers(string[] ids, float fadeTime)
        {
            // Fade out any layers that are no longer requested
            for (int i = 0; i < 3; i++)
            {
                bool hasNew = i < ids.Length && !string.IsNullOrEmpty(ids[i]);

                if (!hasNew && ambienceSlots[i] != null)
                {
                    // stop any fade-in currently running
                    if (ambienceFadeRoutines[i] != null)
                    {
                        StopCoroutine(ambienceFadeRoutines[i]);
                        ambienceFadeRoutines[i] = null;
                    }

                    // fade out whatever was in this slot
                    StartCoroutine(FadeOutAndStop(ambienceSources[i], fadeTime, ambienceSlots[i]));
                    ambienceSlots[i] = null;
                }
            }

            // Activate (or change) requested layers
            for (int i = 0; i < 3; i++)
            {
                if (i >= ids.Length || string.IsNullOrEmpty(ids[i]))
                    continue;

                if (!library.TryGet(ids[i], out var data))
                {
                    Debug.LogWarning($"Ambience '{ids[i]}' not found.");
                    continue;
                }

                StartAmbienceLayer(i, data, fadeTime, targetVolume: 1f);
            }
        }

        private void StartAmbienceLayer(int index, SoundData newData, float fadeTime, float targetVolume)
        {
            Debug.Log($"[AMB] Layer {index}: Playing {newData.clip.name}");
            AudioSource src = ambienceSources[index];

            // Stop any fade-IN running on this layer
            if (ambienceFadeRoutines[index] != null)
            {
                StopCoroutine(ambienceFadeRoutines[index]);
                ambienceFadeRoutines[index] = null;
            }

            // If something was already playing here, fade it out safely
            if (ambienceSlots[index] != null)
            {
                StartCoroutine(FadeOutAndStop(src, fadeTime, ambienceSlots[index]));
            }

            // Assign new ambience
            ambienceSlots[index] = newData;

            src.clip = newData.clip;
            src.outputAudioMixerGroup = newData.mixerGroup;
            src.loop = true;
            src.volume = 0f;
            src.Play();

            // Start fade-in for this new clip
            ambienceFadeRoutines[index] = StartCoroutine(FadeIn(src, targetVolume, fadeTime));
        }

        // --- Utils: fades ---
        private IEnumerator FadeIn(AudioSource source, float target, float time)
        {
            float t = 0;
            while (t < time)
            {
                t += Time.deltaTime;
                source.volume = Mathf.Lerp(0f, target, t / time);
                yield return null;
            }
            source.volume = target;
        }

        /// <summary>
        /// Safe fade-out: only continues while this source is still playing oldData.clip.
        /// If a new clip is assigned mid-fade, it aborts.
        /// </summary>
        private IEnumerator FadeOutAndStop(AudioSource source, float time, SoundData oldData)
        {
            float start = source.volume;
            float t = 0;

            while (t < time)
            {
                // If this source has been repurposed for another clip, abort the fade
                if (source.clip != oldData.clip)
                    yield break;

                t += Time.deltaTime;
                source.volume = Mathf.Lerp(start, 0f, t / time);
                yield return null;
            }

            if (source.clip == oldData.clip)
            {
                Debug.Log($"[AMB] Layer stopping: {oldData.clip.name}");
                source.Stop();
            }
        }

        //  Loop SFX management

        private AudioSource CreateLoopChannel(string key)
        {
            GameObject go = new GameObject($"LoopSFX_{key}");
            go.transform.SetParent(this.transform);

            AudioSource src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = true;
            src.volume = 0f;
            src.spatialBlend = 0f;

            loopChannels[key] = src;
            return src;
        }

        //Usage
        // SoundManager.Instance.PlayLoopSFX(SoundIDs.SFX.Spell.DrawHum);
        // SoundManager.Instance.StopLoopSFX(SoundIDs.SFX.Spell.DrawHum);


        public void PlayLoopSFX(string soundId, float fadeTime = 0.5f)
        {
            // If channel exists, reuse it
            if (!loopChannels.TryGetValue(soundId, out AudioSource src))
                src = CreateLoopChannel(soundId);

            if (!library.TryGet(soundId, out var data))
            {
                Debug.LogWarning($"Loop SFX '{soundId}' not found in library.");
                return;
            }

            src.clip = data.clip;
            src.outputAudioMixerGroup = data.mixerGroup;
            src.loop = true;

            // Fade in
            StartCoroutine(FadeAudioIn(src, fadeTime));
        }
        public void StopLoopSFX(string soundId, float fadeTime = 0.4f)
        {
            if (!loopChannels.TryGetValue(soundId, out var src))
                return;

            StartCoroutine(FadeAudioOutThenStop(src, fadeTime));
        }

        private IEnumerator FadeAudioIn(AudioSource src, float duration)
        {
            if (src == null) yield break;

            src.volume = 0f;
            src.Play();

            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                src.volume = Mathf.Lerp(0f, 1f, t / duration);
                yield return null;
            }

            src.volume = 1f;
        }

        private IEnumerator FadeAudioOutThenStop(AudioSource src, float duration)
        {
            if (src == null) yield break;

            float start = src.volume;
            float t = 0f;

            while (t < duration)
            {
                t += Time.deltaTime;
                src.volume = Mathf.Lerp(start, 0f, t / duration);
                yield return null;
            }

            src.Stop();
            src.volume = 1f; // reset for next play
        }

        public void HandleDrawStart()
        {
            PlayLoopSFX(SoundIDs.SFX.Spell.Draw, 0.3f);
        }

        public void HandleDrawEnd()
        {
            StopLoopSFX(SoundIDs.SFX.Spell.Draw, 0.3f);
        }

        public void HandleGestureRecognized(String spell, float conf)
        {
            Debug.Log("valid rune drawn sfx");

            PlaySFX(SoundIDs.SFX.Spell.ValidRuneDrawn, randomPitch: true);
        }

        public void HandleGestureNotRecognized()
        {
            Debug.Log("invalid rune drawn sfx");

            PlaySFX(SoundIDs.SFX.Spell.InvalidRuneDrawn, randomPitch: true);

        }
        public void HandleValidSpell()
        {
            Debug.Log("valid spell served sfx");
            PlaySFX(SoundIDs.SFX.Spell.ValidCast, randomPitch: true);

        }
        public void HandleInvalidSpell()
        {
            PlaySFX(SoundIDs.SFX.Spell.InvalidCast, randomPitch: true);

        }

        public void StopAllSounds(float fadeTime = 0.5f)
        {
            // BGM
            if (bgmFadeRoutine != null)
                StopCoroutine(bgmFadeRoutine);

            if (activeBgm != null)
                StartCoroutine(FadeOutAndStopBGM(activeBgm, fadeTime));

            if (inactiveBgm != null)
                inactiveBgm.Stop();

            bgmSoundId = null;

            // Ambience
            for (int i = 0; i < ambienceSources.Length; i++)
            {
                if (ambienceFadeRoutines[i] != null)
                {
                    StopCoroutine(ambienceFadeRoutines[i]);
                    ambienceFadeRoutines[i] = null;
                }

                if (ambienceSlots[i] != null)
                {
                    StartCoroutine(FadeOutAndStop(ambienceSources[i], fadeTime, ambienceSlots[i]));
                    ambienceSlots[i] = null;
                }
            }

            // Loop SFX
            foreach (var kvp in loopChannels)
            {
                StartCoroutine(FadeAudioOutThenStop(kvp.Value, fadeTime));
            }

            // One-shot SFX
            for (int i = activeSoundEmitters.Count - 1; i >= 0; i--)
            {
                var emitter = activeSoundEmitters[i];
                if (emitter != null)
                    ReleaseSoundEmitter(emitter);
            }

            activeSoundEmitters.Clear();
            Counts.Clear();
        }
        private IEnumerator FadeOutAndStopBGM(AudioSource src, float time)
        {
            float start = src.volume;
            float t = 0f;

            while (t < time)
            {
                t += Time.deltaTime;
                src.volume = Mathf.Lerp(start, 0f, t / time);
                yield return null;
            }

            src.Stop();
            src.volume = 1f;
        }



    }


}
