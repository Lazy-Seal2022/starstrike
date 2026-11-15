using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using StarStrike.Core;
using StarStrike.Platform.Storage;

namespace StarStrike.Gameplay.Audio
{
    public class AudioService : MonoBehaviour
    {
        private static AudioService instance;
        public static AudioService Instance
        {
            get
            {
                if (instance == null) return null;
                return instance;
            }
            private set => instance = value;
        }

        public static void ResetStaticInstance()
        {
            instance = null;
        }

        public bool isMuted = false;
        public float masterVolume = 1.0f;
        public float sfxVolume = 0.8f;
        public float musicVolume = 0.7f;

        [Header("Audio Configuration")]
        public AudioConfig audioConfig;

        [Header("Audio Mixer Routing")]
        public AudioMixer audioMixer;
        public AudioMixerGroup masterGroup;
        public AudioMixerGroup musicGroup;
        public AudioMixerGroup sfxGroup;
        public AudioMixerGroup uiGroup;

        public const int SFX_POOL_SIZE = 16;
        private AudioSource audioSource;
        private AudioSource musicSource;
        private AudioSource uiSource;
        private AudioSource[] sfxPool;
        private int sfxPoolIndex = 0;
        private Coroutine musicDuckingCoroutine;
        private Dictionary<string, AudioClip> proceduralClips = new Dictionary<string, AudioClip>();

        private void Awake()
        {
            Initialize();
        }

        public void Initialize()
        {
            if (Instance != null && Instance != this)
            {
                if (Application.isPlaying) Destroy(gameObject);
                else DestroyImmediate(gameObject);
                return;
            }
            Instance = this;

            if (audioSource == null)
            {
                audioSource = gameObject.GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }
                audioSource.playOnAwake = false;
            }

            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.playOnAwake = false;
                musicSource.loop = true;
            }

            if (uiSource == null)
            {
                uiSource = gameObject.AddComponent<AudioSource>();
                uiSource.playOnAwake = false;
            }

            EnsureSfxPool();

            if (audioConfig == null)
            {
                audioConfig = Resources.Load<AudioConfig>("Data/AudioConfig");
            }

            ApplyMixerRouting();
            LoadSettings();
            LoadAudioClips();
            StartBackgroundMusic();
        }

        public void EnsureSfxPool()
        {
            if (sfxPool == null || sfxPool.Length == 0)
            {
                sfxPool = new AudioSource[SFX_POOL_SIZE];
                for (int i = 0; i < SFX_POOL_SIZE; i++)
                {
                    var sfxChild = new GameObject($"SfxSource_{i}");
                    sfxChild.transform.SetParent(transform, false);
                    var src = sfxChild.AddComponent<AudioSource>();
                    src.playOnAwake = false;
                    src.loop = false;
                    sfxPool[i] = src;
                }
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void ApplyMixerRouting()
        {
            if (audioConfig != null && audioConfig.masterMixer != null)
            {
                audioMixer = audioConfig.masterMixer;
                masterGroup = audioConfig.masterGroup;
                musicGroup = audioConfig.musicGroup;
                sfxGroup = audioConfig.sfxGroup;
                uiGroup = audioConfig.uiGroup;
            }
            else if (audioMixer == null)
            {
                audioMixer = Resources.Load<AudioMixer>("Audio/StarStrikeMasterMixer");
            }

            if (audioMixer != null)
            {
                if (masterGroup == null)
                {
                    var groups = audioMixer.FindMatchingGroups("Master");
                    if (groups != null && groups.Length > 0) masterGroup = groups[0];
                }
                if (musicGroup == null)
                {
                    var groups = audioMixer.FindMatchingGroups("Music");
                    if (groups != null && groups.Length > 0) musicGroup = groups[0];
                }
                if (sfxGroup == null)
                {
                    var groups = audioMixer.FindMatchingGroups("SFX");
                    if (groups != null && groups.Length > 0) sfxGroup = groups[0];
                }
                if (uiGroup == null)
                {
                    var groups = audioMixer.FindMatchingGroups("UI");
                    if (groups != null && groups.Length > 0) uiGroup = groups[0];
                }

                if (musicSource != null && musicGroup != null) musicSource.outputAudioMixerGroup = musicGroup;
                if (audioSource != null && sfxGroup != null) audioSource.outputAudioMixerGroup = sfxGroup;
                if (uiSource != null && uiGroup != null) uiSource.outputAudioMixerGroup = uiGroup;

                if (sfxPool != null && sfxGroup != null)
                {
                    for (int i = 0; i < sfxPool.Length; i++)
                    {
                        if (sfxPool[i] != null) sfxPool[i].outputAudioMixerGroup = sfxGroup;
                    }
                }
            }
        }

        public void LoadSettings()
        {
            isMuted = StorageService.GetAudioMuted();
            masterVolume = StorageService.GetMasterVolume();
            sfxVolume = StorageService.GetSfxVolume();
            musicVolume = StorageService.GetMusicVolume();
            ApplyMixerVolumes();
            UpdateMusicVolume();
        }

        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            StorageService.SaveMasterVolume(masterVolume);
            ApplyMixerVolumes();
            UpdateMusicVolume();
        }

        public void SetSfxVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            StorageService.SaveSfxVolume(sfxVolume);
            ApplyMixerVolumes();
        }

        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            StorageService.SaveMusicVolume(musicVolume);
            ApplyMixerVolumes();
            UpdateMusicVolume();
        }

        public void SetMuted(bool muted)
        {
            isMuted = muted;
            StorageService.SaveAudioMuted(isMuted);
            ApplyMixerVolumes();
            UpdateMusicVolume();
        }

        public static float LinearToDecibels(float linear)
        {
            return linear > 0.0001f ? Mathf.Log10(linear) * 20f : -80f;
        }

        public void ApplyMixerVolumes()
        {
            if (audioMixer != null)
            {
                audioMixer.SetFloat("MasterVolume", isMuted ? -80f : LinearToDecibels(masterVolume));
                audioMixer.SetFloat("MusicVolume", LinearToDecibels(musicVolume));
                audioMixer.SetFloat("SFXVolume", LinearToDecibels(sfxVolume));
                audioMixer.SetFloat("UIVolume", LinearToDecibels(sfxVolume));
            }
        }

        public void UpdateMusicVolume()
        {
            if (musicSource != null)
            {
                musicSource.volume = isMuted ? 0f : Mathf.Clamp01(musicVolume * masterVolume * 0.5f);
            }
        }

        public void StartBackgroundMusic()
        {
            if (musicSource == null) return;
            if (!proceduralClips.ContainsKey("ambient_music"))
            {
                var musicClip = audioConfig != null ? audioConfig.ambientMusic : null;
                if (musicClip == null)
                {
                    musicClip = Resources.Load<AudioClip>("Audio/ambient_music");
                }
                if (musicClip != null)
                {
                    proceduralClips["ambient_music"] = musicClip;
                }
                else
                {
                    proceduralClips["ambient_music"] = GenerateAmbientMusic();
                }
            }
            musicSource.clip = proceduralClips["ambient_music"];
            UpdateMusicVolume();
            if (!musicSource.isPlaying)
            {
                musicSource.Play();
            }
        }

        public void StopBackgroundMusic()
        {
            if (musicSource != null && musicSource.isPlaying)
            {
                musicSource.Stop();
            }
        }

        private void OnEnable()
        {
            // Subscribe to events that should trigger sounds
            GameEvents.OnAsteroidDestroyed += OnAsteroidDestroyed;
            GameEvents.OnGemCollected += OnGemCollected;
            GameEvents.OnPlayerEvolved += OnPlayerEvolved;
        }

        private void OnDisable()
        {
            GameEvents.OnAsteroidDestroyed -= OnAsteroidDestroyed;
            GameEvents.OnGemCollected -= OnGemCollected;
            GameEvents.OnPlayerEvolved -= OnPlayerEvolved;
        }

        private void LoadAudioClips()
        {
            LoadClipWithFallback("laser", () => GenerateTone(44100, 0.1f, 880f, 440f, 0.5f, WaveType.Square));
            LoadClipWithFallback("enemy_laser", () => GenerateTone(44100, 0.15f, 440f, 220f, 0.5f, WaveType.Sawtooth));
            LoadClipWithFallback("gem", () => GenerateTone(44100, 0.1f, 1200f, 1600f, 0.3f, WaveType.Sine));
            LoadClipWithFallback("explosion", () => GenerateNoise(44100, 0.4f, 0.8f));
            LoadClipWithFallback("upgrade", () => GenerateTone(44100, 0.3f, 440f, 880f, 0.6f, WaveType.Sine));
            LoadClipWithFallback("click", () => GenerateTone(44100, 0.05f, 600f, 900f, 0.4f, WaveType.Sine));

            // Register compatibility aliases
            if (proceduralClips.TryGetValue("laser", out var laserClip))
            {
                proceduralClips["laser_fire"] = laserClip;
            }
            if (proceduralClips.TryGetValue("upgrade", out var upgradeClip))
            {
                proceduralClips["upgrade_acquired"] = upgradeClip;
            }
        }

        private void LoadClipWithFallback(string id, System.Func<AudioClip> fallbackGenerator)
        {
            AudioClip clip = audioConfig != null ? audioConfig.GetClip(id) : null;
            if (clip == null)
            {
                clip = Resources.Load<AudioClip>("Audio/" + id);
            }
            if (clip != null)
            {
                proceduralClips[id] = clip;
            }
            else if (fallbackGenerator != null)
            {
                proceduralClips[id] = fallbackGenerator();
            }
        }

        public bool HasClip(string id)
        {
            return proceduralClips.ContainsKey(id);
        }

        public AudioClip GetClip(string id)
        {
            if (proceduralClips.TryGetValue(id, out AudioClip clip))
            {
                return clip;
            }
            if (audioConfig != null)
            {
                clip = audioConfig.GetClip(id);
            }
            if (clip == null)
            {
                clip = Resources.Load<AudioClip>("Audio/" + id);
            }
            if (clip != null)
            {
                proceduralClips[id] = clip;
            }
            return clip;
        }

        public AudioSource GetNextSfxSource()
        {
            EnsureSfxPool();
            if (sfxPool != null && sfxPool.Length > 0)
            {
                AudioSource src = sfxPool[sfxPoolIndex];
                sfxPoolIndex = (sfxPoolIndex + 1) % sfxPool.Length;
                if (src != null) return src;
            }
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
            }
            return audioSource;
        }

        public void PlaySound(string id, float volume = 1f)
        {
            PlaySound(id, volume, 0.05f);
        }

        public void PlaySound(string id, float volume, float pitchJitter)
        {
            if (isMuted) return;
            float effectiveVolume = Mathf.Clamp01(volume * sfxVolume * masterVolume);
            if (effectiveVolume <= 0.001f) return;

            AudioClip clip = GetClip(id);
            if (clip == null) return;

            AudioSource targetSource = GetNextSfxSource();
            if (targetSource == null) return;

            targetSource.pitch = 1f + (pitchJitter > 0f ? Random.Range(-pitchJitter, pitchJitter) : 0f);
            targetSource.PlayOneShot(clip, effectiveVolume);
        }

        public void DuckMusic(float duckFactor = 0.35f, float duration = 0.6f)
        {
            if (!isActiveAndEnabled || musicSource == null) return;
            if (musicDuckingCoroutine != null) StopCoroutine(musicDuckingCoroutine);
            musicDuckingCoroutine = StartCoroutine(DuckMusicRoutine(duckFactor, duration));
        }

        private System.Collections.IEnumerator DuckMusicRoutine(float duckFactor, float duration)
        {
            float baseVol = isMuted ? 0f : Mathf.Clamp01(musicVolume * masterVolume * 0.5f);
            float duckedVol = baseVol * Mathf.Clamp01(duckFactor);
            musicSource.volume = duckedVol;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                musicSource.volume = Mathf.Lerp(duckedVol, baseVol, t);
                yield return null;
            }
            musicSource.volume = baseVol;
            musicDuckingCoroutine = null;
        }

        public void PlayUISound(string id, float volume = 1f)
        {
            if (isMuted) return;
            float effectiveVolume = Mathf.Clamp01(volume * sfxVolume * masterVolume);
            if (effectiveVolume <= 0.001f) return;
            AudioSource targetSource = uiSource != null ? uiSource : audioSource;
            if (targetSource == null) return;

            AudioClip clip = GetClip(id);
            if (clip != null)
            {
                targetSource.pitch = 1.0f;
                targetSource.PlayOneShot(clip, effectiveVolume);
            }
        }

        private AudioClip GenerateAmbientMusic()
        {
            int sampleRate = 44100;
            float duration = 4.0f;
            int sampleCount = (int)(sampleRate * duration);
            float[] data = new float[sampleCount];

            float[] freqs = new float[] { 110f, 164.81f, 220f, 329.63f }; // A2, E3, A3, E4 chord

            for (int i = 0; i < sampleCount; i++)
            {
                float timeSec = (float)i / sampleRate;

                // Slow, atmospheric pulsation
                float lfo = 0.75f + 0.25f * Mathf.Sin(2f * Mathf.PI * 0.5f * timeSec);

                float sample = 0f;
                for (int f = 0; f < freqs.Length; f++)
                {
                    sample += Mathf.Sin(2f * Mathf.PI * freqs[f] * timeSec) * (0.22f / (f + 1));
                }

                // Seamless loop fade edges
                float edgeFade = 1f;
                int fadeSamples = 2200;
                if (i < fadeSamples) edgeFade = (float)i / fadeSamples;
                else if (i > sampleCount - fadeSamples) edgeFade = (float)(sampleCount - i) / fadeSamples;

                data[i] = sample * lfo * edgeFade * 0.35f;
            }

            AudioClip clip = AudioClip.Create("ambient_music", sampleCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        // --- Event Handlers ---
        private void OnAsteroidDestroyed(Vector2 pos, string size)
        {
            PlaySound("explosion", size == "Large" ? 0.85f : (size == "Medium" ? 0.6f : 0.4f), 0.08f);
            if (size == "Large")
            {
                DuckMusic(0.3f, 0.7f);
            }
        }

        private void OnGemCollected(int amount, int total)
        {
            if (amount > 0)
            {
                PlaySound("gem", 0.4f, 0.06f);
            }
        }

        private void OnPlayerEvolved(string shipId)
        {
            PlaySound("upgrade", 0.85f, 0.02f);
            DuckMusic(0.5f, 0.4f);
        }

        // --- Procedural Generation Methods ---
        private enum WaveType { Sine, Square, Sawtooth }

        private AudioClip GenerateTone(int sampleRate, float duration, float startFreq, float endFreq, float volume, WaveType type)
        {
            int sampleCount = (int)(sampleRate * duration);
            float[] data = new float[sampleCount];
            float phase = 0f;

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleCount;
                float currentFreq = Mathf.Lerp(startFreq, endFreq, t);

                float phaseIncrement = currentFreq / sampleRate;
                phase += phaseIncrement;
                if (phase > 1f) phase -= 1f;

                float sample = 0f;
                switch (type)
                {
                    case WaveType.Sine:
                        sample = Mathf.Sin(phase * 2f * Mathf.PI);
                        break;
                    case WaveType.Square:
                        sample = phase < 0.5f ? 1f : -1f;
                        break;
                    case WaveType.Sawtooth:
                        sample = (phase * 2f) - 1f;
                        break;
                }

                // Envelope (fade out)
                float envelope = 1f - t;
                data[i] = sample * volume * envelope;
            }

            AudioClip clip = AudioClip.Create("tone", sampleCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip GenerateNoise(int sampleRate, float duration, float volume)
        {
            int sampleCount = (int)(sampleRate * duration);
            float[] data = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleCount;
                float envelope = 1f - t;
                data[i] = Random.Range(-1f, 1f) * volume * envelope;
            }

            AudioClip clip = AudioClip.Create("noise", sampleCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
