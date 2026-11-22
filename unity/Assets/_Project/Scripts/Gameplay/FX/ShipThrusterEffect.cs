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
        private SpriteRenderer plumeRenderer;
        private float animTimer = 0f;

        private static Sprite[] cruiseFrames;
        private static Sprite[] boostFrames;

        public static Sprite[] CruiseFrames => cruiseFrames;
        public static Sprite[] BoostFrames => boostFrames;
        public SpriteRenderer PlumeRenderer => plumeRenderer;

        public bool IsThrusting { get; private set; }
        public bool IsBoosting { get; private set; }

        public static void EnsureFramesLoaded()
        {
            if (cruiseFrames == null || cruiseFrames.Length == 0)
            {
                cruiseFrames = new Sprite[6];
                bool allFound = true;
                for (int i = 0; i < 6; i++)
                {
#if UNITY_EDITOR
                    var sp = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/_Project/Art/Sprites/VFX/ThrusterPlume/ThrusterCruise_frame_{i}.png");
                    if (sp != null) cruiseFrames[i] = sp;
                    else allFound = false;
#endif
                }
                if (!allFound)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        if (cruiseFrames[i] == null)
                            cruiseFrames[i] = ProceduralSpriteHelper.GenerateLaserSprite(false);
                    }
                }
            }

            if (boostFrames == null || boostFrames.Length == 0)
            {
                boostFrames = new Sprite[6];
                bool allFound = true;
                for (int i = 0; i < 6; i++)
                {
#if UNITY_EDITOR
                    var sp = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/_Project/Art/Sprites/VFX/ThrusterPlume/ThrusterBoost_frame_{i}.png");
                    if (sp != null) boostFrames[i] = sp;
                    else allFound = false;
#endif
                }
                if (!allFound)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        if (boostFrames[i] == null)
                            boostFrames[i] = ProceduralSpriteHelper.GenerateLaserSprite(true);
                    }
                }
            }
        }

        private void Awake()
        {
            EnsureInitialized();
        }

        public void EnsureInitialized()
        {
            EnsureFramesLoaded();

            if (plumeRenderer == null) SetupPlume();
            if (trailRenderer == null) SetupTrail();
            if (exhaustParticles == null) SetupParticles();
        }

        private void SetupPlume()
        {
            GameObject plumeObj = new GameObject("ThrusterPlume");
            plumeObj.transform.SetParent(transform, false);
            plumeObj.transform.localPosition = localOffset;
            // Point exhaust plume backwards (-X) relative to ship forward (+X)
            plumeObj.transform.localRotation = Quaternion.Euler(0f, 0f, 180f);

            plumeRenderer = plumeObj.AddComponent<SpriteRenderer>();
            var shader = Shader.Find("Sprites/Default");
            if (shader != null) plumeRenderer.sharedMaterial = new Material(shader);
            plumeRenderer.sortingOrder = 9; // Directly beneath ship hull (10)
            if (cruiseFrames != null && cruiseFrames.Length > 0)
            {
                plumeRenderer.sprite = cruiseFrames[0];
            }
            plumeRenderer.enabled = false;
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

        private void Update()
        {
            if (IsThrusting)
            {
                animTimer += Time.deltaTime;
                int frameIdx = ((int)(animTimer * 18f)) % 6;
                var frames = IsBoosting ? boostFrames : cruiseFrames;
                if (plumeRenderer != null)
                {
                    if (frames != null && frames.Length > 0 && frames[frameIdx] != null)
                    {
                        plumeRenderer.sprite = frames[frameIdx];
                    }
                    plumeRenderer.enabled = true;
                    // Slightly larger plume when boosting
                    float scale = IsBoosting ? 1.35f : 1.0f;
                    plumeRenderer.transform.localScale = new Vector3(scale, scale, 1f);
                }
            }
            else
            {
                if (plumeRenderer != null)
                {
                    plumeRenderer.enabled = false;
                }
                animTimer = 0f;
            }
        }

        public void SetThrusting(bool thrusting, bool boosting)
        {
            EnsureInitialized();

            IsThrusting = thrusting;
            IsBoosting = boosting;

            Color targetColor = boosting ? boostThrustColor : normalThrustColor;

            if (plumeRenderer != null)
            {
                plumeRenderer.enabled = thrusting;
            }

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
