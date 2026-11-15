using System.Collections;
using UnityEngine;
using StarStrike.Core;
using StarStrike.Gameplay.Audio;
using StarStrike.Gameplay.FX;

namespace StarStrike.Gameplay
{
    public class SectorHazardController : MonoBehaviour
    {
        public static SectorHazardController Instance { get; private set; }

        public enum SectorBiome
        {
            AsteroidBelt = 1, // Calm baseline
            IonNebula = 2,    // Periodic inertia drift & radar scramble
            SolarFlare = 3    // Coronal mass ejection requiring asteroid shelter
        }

        [Header("Current Environment")]
        [SerializeField] private SectorBiome currentBiome = SectorBiome.AsteroidBelt;
        public SectorBiome CurrentBiome => currentBiome;

        [Header("Ion Storm Settings")]
        public float ionStormInterval = 11f;
        public float ionWarningDuration = 1.5f;
        public float ionSurgeDuration = 4.0f;
        public float ionRadarJitter = 14f;

        [Header("Solar Flare Settings")]
        public float solarFlareInterval = 18f;
        public float solarWarningDuration = 3.0f;
        public float solarDamage = 30f;
        public Vector2 flareDirection = new Vector2(0.707f, -0.707f);

        private float hazardTimer = 0f;
        private bool isWarningActive = false;
        private bool isHazardActive = false;

        private ShipMovement cachedPlayerMovement;
        private ShipHealth cachedPlayerHealth;
        private Transform playerTransform;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnEnable()
        {
            GameEvents.OnWaveStarted += HandleWaveStarted;
            GameEvents.OnGameStarted += HandleGameStarted;
        }

        private void OnDisable()
        {
            GameEvents.OnWaveStarted -= HandleWaveStarted;
            GameEvents.OnGameStarted -= HandleGameStarted;
            ResetAllHazards();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            ResetAllHazards();
        }

        private void HandleGameStarted()
        {
            SetSector(SectorBiome.AsteroidBelt);
        }

        private void HandleWaveStarted(int waveNumber, string waveName, float duration)
        {
            if (waveNumber == 1) SetSector(SectorBiome.AsteroidBelt);
            else if (waveNumber == 2) SetSector(SectorBiome.IonNebula);
            else SetSector(SectorBiome.SolarFlare);
        }

        public void SetSector(SectorBiome biome)
        {
            ResetAllHazards();
            currentBiome = biome;
            hazardTimer = 0f;
        }

        private void CachePlayerReferences()
        {
            if (playerTransform == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    playerTransform = player.transform;
                    cachedPlayerMovement = player.GetComponent<ShipMovement>();
                    cachedPlayerHealth = player.GetComponent<ShipHealth>();
                }
            }
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
            {
                return;
            }

            CachePlayerReferences();

            switch (currentBiome)
            {
                case SectorBiome.IonNebula:
                    UpdateIonStorm();
                    break;

                case SectorBiome.SolarFlare:
                    UpdateSolarFlare();
                    break;

                default:
                    // Baseline: no active hazards
                    break;
            }
        }

        #region Ion Storm Biome
        private void UpdateIonStorm()
        {
            hazardTimer += Time.deltaTime;

            if (!isWarningActive && !isHazardActive && hazardTimer >= (ionStormInterval - ionWarningDuration))
            {
                isWarningActive = true;
                GameEvents.TriggerHazardWarning("ION STORM SURGE DETECTED", ionWarningDuration);
                AudioService.Instance?.PlaySound("click", 1.2f);
            }

            if (!isHazardActive && hazardTimer >= ionStormInterval)
            {
                StartCoroutine(ExecuteIonSurge());
            }
        }

        private IEnumerator ExecuteIonSurge()
        {
            isHazardActive = true;
            isWarningActive = false;
            hazardTimer = 0f;

            GameEvents.TriggerHazardTriggered("ION STORM");

            // Disable autobraking on player ship
            if (cachedPlayerMovement != null)
            {
                cachedPlayerMovement.autoBrakeEnabled = false;
            }

            // Scramble Minimap
            GameEvents.TriggerRadarJitterChanged(ionRadarJitter);

            AudioService.Instance?.PlaySound("click", 0.6f);
            FXService.Instance?.CameraShake(0.12f, 0.2f);

            yield return new WaitForSeconds(ionSurgeDuration);

            // Restore normal flight damping & radar
            if (cachedPlayerMovement != null)
            {
                cachedPlayerMovement.autoBrakeEnabled = true;
            }

            GameEvents.TriggerRadarJitterChanged(0f);

            isHazardActive = false;
            GameEvents.TriggerHazardEnded("ION STORM");
        }
        #endregion

        #region Solar Flare Biome
        private void UpdateSolarFlare()
        {
            hazardTimer += Time.deltaTime;

            if (!isWarningActive && !isHazardActive && hazardTimer >= (solarFlareInterval - solarWarningDuration))
            {
                isWarningActive = true;
                GameEvents.TriggerHazardWarning("SOLAR FLARE DETECTED - TAKE COVER BEHIND ASTEROIDS!", solarWarningDuration);
                AudioService.Instance?.PlaySound("explosion", 0.5f);
            }

            if (!isHazardActive && hazardTimer >= solarFlareInterval)
            {
                StartCoroutine(ExecuteSolarFlareSweep());
            }
        }

        private IEnumerator ExecuteSolarFlareSweep()
        {
            isHazardActive = true;
            isWarningActive = false;
            hazardTimer = 0f;

            GameEvents.TriggerHazardTriggered("SOLAR FLARE");

            FXService.Instance?.CameraShake(0.35f, 0.45f);
            AudioService.Instance?.PlaySound("explosion", 1.1f);

            yield return new WaitForSeconds(0.4f);

            // Check if player is safely sheltered behind an asteroid
            if (playerTransform != null && cachedPlayerHealth != null)
            {
                Vector2 playerPos = playerTransform.position;
                Vector2 raySource = -flareDirection.normalized;

                // Raycast from player outward toward the incoming flare origin
                RaycastHit2D[] hits = Physics2D.RaycastAll(playerPos, raySource, 30f);
                bool isSheltered = false;

                for (int i = 0; i < hits.Length; i++)
                {
                    if (hits[i].collider != null && hits[i].collider.GetComponent<Asteroid>() != null)
                    {
                        isSheltered = true;
                        break;
                    }
                }

                if (isSheltered)
                {
                    GameEvents.TriggerHazardEnded("SHELTERED (0 DAMAGE)");
                }
                else
                {
                    cachedPlayerHealth.TakeDamage(solarDamage);
                    GameEvents.TriggerHazardEnded("SOLAR FLARE IMPACT");
                }
            }

            isHazardActive = false;
        }

        public bool IsPositionSheltered(Vector2 position)
        {
            Vector2 raySource = -flareDirection.normalized;
            RaycastHit2D[] hits = Physics2D.RaycastAll(position, raySource, 30f);
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].collider != null && hits[i].collider.GetComponent<Asteroid>() != null)
                {
                    return true;
                }
            }
            return false;
        }
        #endregion

        public void ResetAllHazards()
        {
            StopAllCoroutines();
            isWarningActive = false;
            isHazardActive = false;
            hazardTimer = 0f;

            if (cachedPlayerMovement != null)
            {
                cachedPlayerMovement.autoBrakeEnabled = true;
            }

            GameEvents.TriggerRadarJitterChanged(0f);
        }
    }
}
