using System.Collections.Generic;
using UnityEngine;
using StarStrike.Core;

namespace StarStrike.Gameplay.Audio
{
    public class AudioService : MonoBehaviour
    {
        public static AudioService Instance { get; private set; }

        public bool isMuted = false;

        private AudioSource audioSource;
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

            GenerateClips();
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
        }

        public void PlaySound(string id, float volume = 1f)
        {
            if (isMuted) return;
            if (proceduralClips.TryGetValue(id, out AudioClip clip))
            {
                audioSource.PlayOneShot(clip, volume);
            }
        }

        // --- Event Handlers ---
        private void OnAsteroidDestroyed(Vector2 pos, string size)
        {
            PlaySound("explosion", size == "Large" ? 0.8f : (size == "Medium" ? 0.6f : 0.4f));
        }

        private void OnGemCollected(int amount, int total)
        {
            PlaySound("gem", 0.4f);
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
