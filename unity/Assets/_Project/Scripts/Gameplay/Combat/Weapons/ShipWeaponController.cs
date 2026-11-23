using System.Collections.Generic;
using UnityEngine;
using StarStrike.Core;

namespace StarStrike.Gameplay
{
    public class ShipWeaponController : MonoBehaviour
    {
        [System.Serializable]
        public struct HardpointSocketData
        {
            public int index;
            public string name;
            public Vector2 localPosition;
        }

        public class MountedHardpointInfo
        {
            public int socketIndex;
            public string socketName;
            public Vector2 localPosition;
            public string weaponId;
            public IWeapon weaponInstance;
        }

        [Header("Weapons")]
        [SerializeField] private int currentWeaponIndex = 0;
        private readonly List<IWeapon> availableWeapons = new List<IWeapon>();
        private readonly List<MountedHardpointInfo> mountedHardpoints = new List<MountedHardpointInfo>();
        private readonly List<IWeapon> activeSocketWeapons = new List<IWeapon>();

        public IWeapon CurrentWeapon => (availableWeapons.Count > 0 && currentWeaponIndex >= 0 && currentWeaponIndex < availableWeapons.Count) 
            ? availableWeapons[currentWeaponIndex] 
            : null;

        public int CurrentIndex => currentWeaponIndex;
        public int WeaponCount => availableWeapons.Count;
        public IReadOnlyList<IWeapon> AvailableWeapons => availableWeapons;
        public IReadOnlyList<MountedHardpointInfo> MountedHardpoints => mountedHardpoints;
        public bool HasMultiSocketLoadout => mountedHardpoints.Count > 0;

        private void Awake()
        {
            InitializeWeapons();
        }

        private void Start()
        {
            bool isPlayer = CompareTag("Player") || gameObject.name == "Player";
            if (isPlayer)
            {
                var prog = GetComponent<PlayerProgression>();
                string shipId = (prog != null && prog.currentShip != null) ? prog.currentShip.id : "scout";
                EquipFromStorage(shipId);
            }
        }

        public static HardpointSocketData[] GetShipHardpointSockets(string shipId)
        {
            if (string.IsNullOrEmpty(shipId)) shipId = "scout";
            shipId = shipId.ToLowerInvariant().Trim();

            switch (shipId)
            {
                case "scout":
                    return new HardpointSocketData[]
                    {
                        new HardpointSocketData { index = 0, name = "SOCKET 01 // NOSE SPINAL", localPosition = new Vector2(1.55f, 0f) }
                    };
                case "fighter":
                    return new HardpointSocketData[]
                    {
                        new HardpointSocketData { index = 0, name = "SOCKET 01 // PORT WING", localPosition = new Vector2(0.35f, 1.25f) },
                        new HardpointSocketData { index = 1, name = "SOCKET 02 // STARBOARD WING", localPosition = new Vector2(0.35f, -1.25f) }
                    };
                case "miner":
                    return new HardpointSocketData[]
                    {
                        new HardpointSocketData { index = 0, name = "SOCKET 01 // PORT MINING MOUNT", localPosition = new Vector2(1.25f, 0.85f) },
                        new HardpointSocketData { index = 1, name = "SOCKET 02 // STARBOARD MINING MOUNT", localPosition = new Vector2(1.25f, -0.85f) }
                    };
                case "interceptor":
                    return new HardpointSocketData[]
                    {
                        new HardpointSocketData { index = 0, name = "SOCKET 01 // PORT PURSUIT RAIL", localPosition = new Vector2(0.45f, 1.15f) },
                        new HardpointSocketData { index = 1, name = "SOCKET 02 // STARBOARD PURSUIT RAIL", localPosition = new Vector2(0.45f, -1.15f) }
                    };
                case "juggernaut":
                    return new HardpointSocketData[]
                    {
                        new HardpointSocketData { index = 0, name = "SOCKET 01 // SPINAL CANNON", localPosition = new Vector2(2.75f, 0f) },
                        new HardpointSocketData { index = 1, name = "SOCKET 02 // PORT SPONSON", localPosition = new Vector2(0.70f, 1.50f) },
                        new HardpointSocketData { index = 2, name = "SOCKET 03 // STARBOARD SPONSON", localPosition = new Vector2(0.70f, -1.50f) }
                    };
                case "carrier":
                case "vanguard":
                    return new HardpointSocketData[]
                    {
                        new HardpointSocketData { index = 0, name = "SOCKET 01 // MAIN BATTERY", localPosition = new Vector2(2.50f, 0f) },
                        new HardpointSocketData { index = 1, name = "SOCKET 02 // PORT PROW", localPosition = new Vector2(2.75f, 1.12f) },
                        new HardpointSocketData { index = 2, name = "SOCKET 03 // STARBOARD PROW", localPosition = new Vector2(2.75f, -1.12f) }
                    };
                case "phantom":
                    return new HardpointSocketData[]
                    {
                        new HardpointSocketData { index = 0, name = "SOCKET 01 // PORT STEALTH BAY", localPosition = new Vector2(0.50f, 0.80f) },
                        new HardpointSocketData { index = 1, name = "SOCKET 02 // STARBOARD STEALTH BAY", localPosition = new Vector2(0.50f, -0.80f) }
                    };
                default:
                    return new HardpointSocketData[]
                    {
                        new HardpointSocketData { index = 0, name = "SOCKET 01 // PRIMARY NOSE", localPosition = new Vector2(1.55f, 0f) }
                    };
            }
        }

        public void EquipFromStorage(string shipId = null)
        {
            InitializeWeapons();

            if (string.IsNullOrEmpty(shipId)) shipId = StarStrike.Platform.Storage.StorageService.GetLastShipEvolved();
            if (string.IsNullOrEmpty(shipId)) shipId = "scout";
            shipId = shipId.ToLowerInvariant().Trim();

            var socketDefs = GetShipHardpointSockets(shipId);
            mountedHardpoints.Clear();
            activeSocketWeapons.Clear();

            var weaponSocketMap = new Dictionary<string, List<Vector2>>();

            for (int i = 0; i < socketDefs.Length; i++)
            {
                var sDef = socketDefs[i];
                string wId = StarStrike.Platform.Storage.StorageService.GetSocketWeapon(shipId, sDef.index);
                int targetIdx = WeaponIdToIndex(wId);
                IWeapon weaponComp = (targetIdx >= 0 && targetIdx < availableWeapons.Count) ? availableWeapons[targetIdx] : availableWeapons[0];

                mountedHardpoints.Add(new MountedHardpointInfo
                {
                    socketIndex = sDef.index,
                    socketName = sDef.name,
                    localPosition = sDef.localPosition,
                    weaponId = wId,
                    weaponInstance = weaponComp
                });

                if (weaponComp != null && !activeSocketWeapons.Contains(weaponComp))
                {
                    activeSocketWeapons.Add(weaponComp);
                }

                if (!weaponSocketMap.TryGetValue(wId, out var posList))
                {
                    posList = new List<Vector2>();
                    weaponSocketMap[wId] = posList;
                }
                posList.Add(sDef.localPosition);
            }

            // Assign dedicated socket positions to each weapon
            foreach (var kvp in weaponSocketMap)
            {
                int targetIdx = WeaponIdToIndex(kvp.Key);
                if (targetIdx >= 0 && targetIdx < availableWeapons.Count)
                {
                    AssignSocketsToWeapon(availableWeapons[targetIdx], kvp.Value.ToArray());
                }
            }

            // Keep primary weapon index on socket 0
            string primaryWeaponId = StarStrike.Platform.Storage.StorageService.GetSocketWeapon(shipId, 0);
            int primaryIdx = WeaponIdToIndex(primaryWeaponId);
            if (primaryIdx >= 0 && primaryIdx < availableWeapons.Count)
            {
                SwitchWeapon(primaryIdx);
            }
        }

        private static void AssignSocketsToWeapon(IWeapon weapon, Vector2[] sockets)
        {
            if (weapon == null) return;
            if (weapon is ShipWeapon sw) sw.SetSockets(sockets);
            else if (weapon is GatlingWeapon gw) gw.SetSockets(sockets);
            else if (weapon is HomingMissileWeapon mw) mw.SetSockets(sockets);
            else if (weapon is ScattergunWeapon scw) scw.SetSockets(sockets);
            else if (weapon is ContinuousBeamWeapon bw) bw.SetSockets(sockets);
        }

        public bool TryFireSockets(IEnergyConsumer energySource = null)
        {
            if (activeSocketWeapons.Count == 0)
            {
                return CurrentWeapon != null && CurrentWeapon.TryFire(energySource);
            }

            bool firedAny = false;
            for (int i = 0; i < activeSocketWeapons.Count; i++)
            {
                var weapon = activeSocketWeapons[i];
                if (weapon != null && weapon.TryFire(energySource))
                {
                    firedAny = true;
                }
            }

            return firedAny;
        }

        public void StopFiringAllSockets()
        {
            for (int i = 0; i < availableWeapons.Count; i++)
            {
                availableWeapons[i]?.StopFiring();
            }
        }

        public float GetFastestFireRate()
        {
            if (activeSocketWeapons.Count == 0)
            {
                return CurrentWeapon != null ? Mathf.Max(0.05f, CurrentWeapon.FireRate) : 0.22f;
            }

            float fastest = 999f;
            for (int i = 0; i < activeSocketWeapons.Count; i++)
            {
                var w = activeSocketWeapons[i];
                if (w != null && w.FireRate > 0f && w.FireRate < fastest)
                {
                    fastest = w.FireRate;
                }
            }

            if (fastest > 100f) fastest = 0.22f;
            return Mathf.Clamp(fastest, 0.05f, 1.0f);
        }

        public static int WeaponIdToIndex(string weaponId)
        {
            if (string.IsNullOrEmpty(weaponId)) return 0;
            string lower = weaponId.ToLowerInvariant().Trim();
            if (lower.Contains("gatling") || lower == "4") return 4;
            if (lower.Contains("beam") || lower == "3") return 3;
            if (lower.Contains("missile") || lower.Contains("homing") || lower == "2") return 2;
            if (lower.Contains("scatter") || lower == "1") return 1;
            return 0; // pulse
        }

        public static string IndexToWeaponId(int index)
        {
            switch (index)
            {
                case 1: return "scattergun";
                case 2: return "missile";
                case 3: return "beam";
                case 4: return "gatling";
                default: return "pulse";
            }
        }

        public void InitializeWeapons()
        {
            availableWeapons.Clear();
            var components = GetComponents<IWeapon>();
            foreach (var comp in components)
            {
                if (!availableWeapons.Contains(comp))
                {
                    availableWeapons.Add(comp);
                }
            }

            // If player ship and only standard weapon attached, ensure all 5 arsenals are equipped
            bool isPlayer = CompareTag("Player") || gameObject.name == "Player";
            if (isPlayer)
            {
                EnsureWeapon<ShipWeapon>();
                EnsureWeapon<ScattergunWeapon>();
                EnsureWeapon<HomingMissileWeapon>();
                EnsureWeapon<ContinuousBeamWeapon>();
                EnsureWeapon<GatlingWeapon>();
            }

            if (currentWeaponIndex >= availableWeapons.Count)
            {
                currentWeaponIndex = 0;
            }
        }

        private T EnsureWeapon<T>() where T : MonoBehaviour, IWeapon
        {
            T weapon = GetComponent<T>();
            if (weapon == null)
            {
                weapon = gameObject.AddComponent<T>();
            }
            if (!availableWeapons.Contains(weapon))
            {
                availableWeapons.Add(weapon);
            }
            return weapon;
        }

        public bool SwitchWeapon(int index)
        {
            if (index < 0 || index >= availableWeapons.Count) return false;
            if (index == currentWeaponIndex)
            {
                if (CurrentWeapon != null)
                {
                    GameEvents.TriggerWeaponSwapped(CurrentWeapon.WeaponName, currentWeaponIndex);
                }
                return true;
            }

            CurrentWeapon?.StopFiring();
            currentWeaponIndex = index;
            CurrentWeapon?.ResetCooldown();

            StarStrike.Gameplay.Audio.AudioService.Instance?.PlaySound("menu_navigate", 0.4f);

            if (CurrentWeapon != null)
            {
                GameEvents.TriggerWeaponSwapped(CurrentWeapon.WeaponName, currentWeaponIndex);
            }

            return true;
        }

        public void NextWeapon()
        {
            if (availableWeapons.Count <= 1) return;
            SwitchWeapon((currentWeaponIndex + 1) % availableWeapons.Count);
        }

        public void PreviousWeapon()
        {
            if (availableWeapons.Count <= 1) return;
            SwitchWeapon((currentWeaponIndex - 1 + availableWeapons.Count) % availableWeapons.Count);
        }

        public void AddWeapon(IWeapon weapon)
        {
            if (weapon != null && !availableWeapons.Contains(weapon))
            {
                availableWeapons.Add(weapon);
            }
        }
    }
}
