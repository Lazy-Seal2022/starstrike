using System.Collections.Generic;
using UnityEngine;
using StarStrike.Core;
using StarStrike.Platform.Storage;

namespace StarStrike.Gameplay.Audio
{
    public class AudioService : MonoBehaviour
    {
        public static AudioService Instance { get; private set; }

        public bool isMuted = false;
        public float masterVolume = 1.0f;
        public float sfxVolume = 0.8f;
        public float musicVolume = 0.7f;

        private AudioSource audioSource;
        private AudioSource musicSource;
        private Dictionary<string, AudioClip> proceduralClips = new Dictionary<string, AudioClip>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;

            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;

            LoadSettings();
            GenerateClips();
            StartBackgroundMusic();
        }

        public void LoadSettings()
        {
            isMuted = StorageService.GetAudioMuted();
            masterVolume = StorageService.GetMasterVolume();
            sfxVolume = StorageService.GetSfxVolume();
            musicVolume = StorageService.GetMusicVolume();
            UpdateMusicVolume();
        }

        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            StorageService.SaveMasterVolume(masterVolume);
            UpdateMusicVolume();
        }

        public void SetSfxVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            StorageService.SaveSfxVolume(sfxVolume);
        }

        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            StorageService.SaveMusicVolume(musicVolume);
            UpdateMusicVolume();
        }

        public void SetMuted(bool muted)
        {
            isMuted = muted;
            StorageService.SaveAudioMuted(isMuted);
            UpdateMusicVolume();
        }

        public void UpdateMusicVolume()
        {
            if (musicSource != null)
            {
                musicSource.volume = isMuted ? 0f : Mathf.Clamp01(musicVolume * masterVolume * 0.5f);
            }
        }

        private void StartBackgroundMusic()
        {
            if (musicSource == null) return;
            if (!proceduralClips.ContainsKey("ambient_music"))
            {
                proceduralClips["ambient_music"] = GenerateAmbientMusic();
            }
            musicSource.clip = proceduralClips["ambient_music"];
            UpdateMusicVolume();
            if (!musicSource.isPlaying)
            {
                musicSource.Play();
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

        private void GenerateClips()
        {
            proceduralClips["laser"] = GenerateTone(44100, 0.1f, 880f, 440f, 0.5f, WaveType.Square);
            proceduralClips["enemy_laser"] = GenerateTone(44100, 0.15f, 440f, 220f, 0.5f, WaveType.Sawtooth);
            proceduralClips["gem"] = GenerateTone(44100, 0.1f, 1200f, 1600f, 0.3f, WaveType.Sine);
            proceduralClips["explosion"] = GenerateNoise(44100, 0.4f, 0.8f);
            proceduralClips["upgrade"] = GenerateTone(44100, 0.3f, 440f, 880f, 0.6f, WaveType.Sine);
            proceduralClips["click"] = GenerateTone(44100, 0.05f, 600f, 900f, 0.4f, WaveType.Sine);
        }

        public void PlaySound(string id, float volume = 1f)
        {
            if (isMuted) return;
            float effectiveVolume = Mathf.Clamp01(volume * sfxVolume * masterVolume);
            if (effectiveVolume <= 0.001f) return;
            if (audioSource == null) return;
            if (proceduralClips.TryGetValue(id, out AudioClip clip))
            {
                audioSource.PlayOneShot(clip, effectiveVolume);
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
            PlaySound("explosion", size == "Large" ? 0.8f : (size == "Medium" ? 0.6f : 0.4f));
        }

        private void OnGemCollected(int amount, int total)
        {
            if (amount > 0)
            {
                PlaySound("gem", 0.4f);
            }
        }

        private void OnPlayerEvolved(string shipId)
        {
            PlaySound("upgrade", 0.8f);
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
