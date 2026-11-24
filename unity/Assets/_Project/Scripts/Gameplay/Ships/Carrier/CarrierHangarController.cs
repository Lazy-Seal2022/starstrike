using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using StarStrike.Core;
using StarStrike.Gameplay;

namespace StarStrike.Gameplay.Carrier
{
    public enum DroneCommandStance
    {
        DefensiveEscort = 0,
        OffensiveStrike = 1,
        DockAndRepair = 2
    }

    /// <summary>
    /// Carrier ship controller managing the automated drone foundry, escort squadron deployments,
    /// flight bay operations, tactical command stances, and automated Point Defense Cannons (PDC).
    /// </summary>
    public class CarrierHangarController : MonoBehaviour
    {
        [Header("Squadron Configuration")]
        public int droneCapacity = 4;
        public float droneBuildTime = 4.0f;
        public float launchCooldown = 0.5f;

        [Header("Point Defense Cannon (PDC)")]
        public bool enableAutomatedPDC = true;
        public float pointDefenseRange = 6.5f;
        public float pointDefenseFireRate = 0.22f;
        public float pointDefenseDamage = 12f;

        [Header("Flight Bay Hardpoints")]
        public Vector2[] launchBayOffsets = new Vector2[]
        {
            new Vector2(-0.40f, 1.50f),  // Port Flight Deck
            new Vector2(-0.40f, -1.50f)  // Starboard Flight Deck
        };

        [Header("Tactical State")]
        [SerializeField] private DroneCommandStance currentStance = DroneCommandStance.DefensiveEscort;
        private readonly List<CarrierEscortDrone> activeDrones = new List<CarrierEscortDrone>();
        private float currentBuildTimer = 0f;
        private float lastLaunchTime = -999f;
        private float lastPdcFireTime = -999f;
        private int nextLaunchBayIndex = 0;

        public DroneCommandStance CurrentStance => currentStance;
        public IReadOnlyList<CarrierEscortDrone> ActiveDrones => activeDrones;
        public int ActiveDroneCount => activeDrones.Count;
        public int DroneCapacity => droneCapacity;
        public float BuildProgress => Mathf.Clamp01(currentBuildTimer / Mathf.Max(0.1f, droneBuildTime));
        public bool IsBuilding => activeDrones.Count < droneCapacity;

        public event Action<CarrierEscortDrone> OnDroneLaunched;
        public event Action<CarrierEscortDrone> OnDroneDestroyed;
        public event Action<DroneCommandStance> OnStanceChanged;

        public Vector2 CurrentAimPosition
        {
            get
            {
                if (Camera.main != null && Mouse.current != null)
                {
                    Vector2 screenPos = Mouse.current.position.ReadValue();
                    float camDist = Mathf.Abs(Camera.main.transform.position.z);
                    return Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, camDist));
                }
                return (Vector2)transform.position + (Vector2)transform.right * 10f;
            }
        }

        private void Start()
        {
            // Sync capacity with ShipDefinition if present
            var prog = GetComponent<PlayerProgression>();
            if (prog != null && prog.currentShip != null && prog.currentShip.droneHangarCapacity > 0)
            {
                droneCapacity = prog.currentShip.droneHangarCapacity;
            }

            // Spawn initial escort squadron
            SpawnInitialSquadron();
        }

        public void SpawnInitialSquadron()
        {
            while (activeDrones.Count < droneCapacity)
            {
                LaunchNewDrone();
            }
        }

        private void Update()
        {
            HandlePlayerInput();
            UpdateFoundryProduction();
            if (enableAutomatedPDC)
            {
                UpdatePointDefense();
            }
        }

        private void HandlePlayerInput()
        {
            Keyboard kb = Keyboard.current;
            if (kb != null && kb.fKey.wasPressedThisFrame)
            {
                ToggleStance();
            }

            Mouse mouse = Mouse.current;
            if (mouse != null && mouse.rightButton.wasPressedThisFrame)
            {
                // Quick command: right-click switches to Offensive Strike
                SetStance(DroneCommandStance.OffensiveStrike);
            }
        }

        public void ToggleStance()
        {
            if (currentStance == DroneCommandStance.DefensiveEscort)
            {
                SetStance(DroneCommandStance.OffensiveStrike);
            }
            else if (currentStance == DroneCommandStance.OffensiveStrike)
            {
                SetStance(DroneCommandStance.DockAndRepair);
            }
            else
            {
                SetStance(DroneCommandStance.DefensiveEscort);
            }
        }

        public void SetStance(DroneCommandStance stance)
        {
            if (currentStance == stance) return;
            currentStance = stance;
            OnStanceChanged?.Invoke(currentStance);
        }

        private void UpdateFoundryProduction()
        {
            if (activeDrones.Count >= droneCapacity)
            {
                currentBuildTimer = 0f;
                return;
            }

            currentBuildTimer += Time.deltaTime;
            if (currentBuildTimer >= droneBuildTime && Time.time - lastLaunchTime >= launchCooldown)
            {
                currentBuildTimer = 0f;
                LaunchNewDrone();
            }
        }

        public CarrierEscortDrone LaunchNewDrone()
        {
            int slotIndex = activeDrones.Count;
            Vector2 spawnWorldPos = GetLaunchBayWorldPosition(nextLaunchBayIndex);
            nextLaunchBayIndex = (nextLaunchBayIndex + 1) % launchBayOffsets.Length;

            GameObject droneGo = new GameObject($"EscortDrone_{slotIndex + 1}");
            droneGo.transform.position = spawnWorldPos;
            droneGo.transform.rotation = transform.rotation;

            var drone = droneGo.AddComponent<CarrierEscortDrone>();
            drone.Initialize(this, slotIndex);

            activeDrones.Add(drone);
            lastLaunchTime = Time.time;
            OnDroneLaunched?.Invoke(drone);

            return drone;
        }

        public void NotifyDroneDestroyed(CarrierEscortDrone drone)
        {
            if (drone == null) return;
            activeDrones.Remove(drone);
            OnDroneDestroyed?.Invoke(drone);
        }

        public void ClearSquadron()
        {
            for (int i = 0; i < activeDrones.Count; i++)
            {
                if (activeDrones[i] != null)
                {
                    if (Application.isPlaying) Destroy(activeDrones[i].gameObject);
                    else DestroyImmediate(activeDrones[i].gameObject);
                }
            }
            activeDrones.Clear();
        }

        private void OnDestroy()
        {
            ClearSquadron();
        }

        public Vector2 GetLaunchBayWorldPosition(int bayIndex)
        {
            if (launchBayOffsets == null || launchBayOffsets.Length == 0) return transform.position;
            Vector2 localOffset = launchBayOffsets[bayIndex % launchBayOffsets.Length];
            return transform.TransformPoint(localOffset);
        }

        private void UpdatePointDefense()
        {
            if (Time.time - lastPdcFireTime < pointDefenseFireRate) return;

            // Scan for incoming enemy projectiles, missiles, or close threats
            Collider2D[] threats = Physics2D.OverlapCircleAll(transform.position, pointDefenseRange);
            Transform targetThreat = null;
            float closestDist = float.MaxValue;

            for (int i = 0; i < threats.Length; i++)
            {
                var hit = threats[i];
                if (hit == null || hit.gameObject == gameObject) continue;
                if (hit.CompareTag("Player") || hit.CompareTag("Laser")) continue;

                bool isThreat = hit.CompareTag("EnemyLaser") || hit.CompareTag("Enemy") || hit.CompareTag("Asteroid");
                if (isThreat)
                {
                    float dist = Vector2.Distance(transform.position, hit.transform.position);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        targetThreat = hit.transform;
                    }
                }
            }

            if (targetThreat != null)
            {
                lastPdcFireTime = Time.time;
                FirePdcBurst(targetThreat.position);
            }
        }

        private void FirePdcBurst(Vector2 targetPos)
        {
            Vector2 muzzlePos = (Vector2)transform.position + (Vector2)transform.right * 1.5f;
            Vector2 fireDir = (targetPos - muzzlePos).normalized;

            GameObject flakObj = new GameObject("PDC_FlakTracer");
            flakObj.transform.position = muzzlePos;
            flakObj.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(fireDir.y, fireDir.x) * Mathf.Rad2Deg);
            flakObj.tag = "Laser";

            var proj = flakObj.AddComponent<LaserProjectile>();
            proj.Initialize(pointDefenseDamage, 32f, false);
        }
    }
}