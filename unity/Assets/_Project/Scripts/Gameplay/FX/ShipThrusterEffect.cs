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

        public ParticleSystem ExhaustParticles => exhaustParticles;

        public bool IsThrusting { get; private set; }
        public bool IsBoosting { get; private set; }

        private void Awake()
        {
            EnsureInitialized();
        }

        public void EnsureInitialized()
        {
            if (exhaustParticles == null) SetupParticles();
        }

        private void SetupParticles()
        {
            if (exhaustParticles != null) return;

            GameObject fxObj = FXService.CreateThrustFX();
            if (fxObj != null)
            {
                fxObj.transform.SetParent(transform, false);
                fxObj.transform.localPosition = localOffset;
                exhaustParticles = fxObj.GetComponent<ParticleSystem>();
                if (exhaustParticles != null) exhaustParticles.Stop();
            }
        }

        public void SetThrusting(bool thrusting, bool boosting)
        {
            EnsureInitialized();

            IsThrusting = thrusting;
            IsBoosting = boosting;

            Color targetColor = boosting ? boostThrustColor : normalThrustColor;

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
