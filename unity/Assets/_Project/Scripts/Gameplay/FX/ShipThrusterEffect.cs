using UnityEngine;

namespace StarStrike.Gameplay.FX
{
    public class ShipThrusterEffect : MonoBehaviour
    {
        [Header("Configuration")]
        public Color normalThrustColor = new Color(0.2f, 0.85f, 1.0f, 0.85f); // Cyan
        public Color boostThrustColor = new Color(1.0f, 0.75f, 0.2f, 0.95f);  // Gold / Orange
        public Vector3 localOffset = new Vector3(-0.5f, 0f, 0f);

        private ParticleSystem exhaustParticles;
        private TrailRenderer trailRenderer;

        public bool IsThrusting { get; private set; }
        public bool IsBoosting { get; private set; }

        private void Awake()
        {
            EnsureInitialized();
        }

        public void EnsureInitialized()
        {
            if (trailRenderer == null) SetupTrail();
            if (exhaustParticles == null) SetupParticles();
        }

        private void SetupTrail()
        {
            GameObject trailObj = new GameObject("ThrusterTrail");
            trailObj.transform.SetParent(transform, false);
            trailObj.transform.localPosition = localOffset;

            trailRenderer = trailObj.AddComponent<TrailRenderer>();
            trailRenderer.material = new Material(Shader.Find("Sprites/Default"));
            trailRenderer.time = 0.3f;
            trailRenderer.startWidth = 0.35f;
            trailRenderer.endWidth = 0.02f;
            trailRenderer.autodestruct = false;
            trailRenderer.emitting = false;
            trailRenderer.sortingOrder = 4; // Below ship (sortingOrder 10)

            ApplyTrailGradient(normalThrustColor);
        }

        private void SetupParticles()
        {
            if (FXService.Instance != null)
            {
                GameObject fxObj = FXService.Instance.CreateThrustFX();
                fxObj.transform.SetParent(transform, false);
                fxObj.transform.localPosition = localOffset;
                exhaustParticles = fxObj.GetComponent<ParticleSystem>();
                if (exhaustParticles != null) exhaustParticles.Stop();
            }
        }

        private void ApplyTrailGradient(Color baseColor)
        {
            if (trailRenderer == null) return;

            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(baseColor, 0.0f),
                    new GradientColorKey(baseColor * 0.7f, 0.5f),
                    new GradientColorKey(Color.white, 1.0f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(baseColor.a, 0.0f),
                    new GradientAlphaKey(baseColor.a * 0.5f, 0.5f),
                    new GradientAlphaKey(0.0f, 1.0f)
                }
            );
            trailRenderer.colorGradient = gradient;
        }

        public void SetThrusting(bool thrusting, bool boosting)
        {
            EnsureInitialized();

            IsThrusting = thrusting;
            IsBoosting = boosting;

            Color targetColor = boosting ? boostThrustColor : normalThrustColor;

            if (trailRenderer != null)
            {
                trailRenderer.emitting = thrusting;
                if (thrusting)
                {
                    ApplyTrailGradient(targetColor);
                    trailRenderer.startWidth = boosting ? 0.5f : 0.35f;
                }
            }

            if (exhaustParticles != null)
            {
                if (thrusting)
                {
                    var main = exhaustParticles.main;
                    main.startColor = targetColor;
                    if (!exhaustParticles.isPlaying) exhaustParticles.Play();
                }
                else
                {
                    if (exhaustParticles.isPlaying) exhaustParticles.Stop();
                }
            }
        }
    }
}
