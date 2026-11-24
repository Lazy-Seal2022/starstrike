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
        private string currentShipId = "scout";

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
            this.currentShipId = shipId;

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

            // Update visual socket hardpoint mounts on the ship hull
            UpdateSocketVisuals();
        }

        public static GameObject GetWeaponPrefab(string weaponId)
        {
            var combatConfig = Resources.Load<CombatConfig>("Data/CombatConfig");
            string lower = string.IsNullOrEmpty(weaponId) ? "pulse" : weaponId.ToLowerInvariant().Trim();

            if (lower.Contains("gatling") || lower == "4")
            {
                if (combatConfig != null && combatConfig.gatlingWeaponPrefab != null) return combatConfig.gatlingWeaponPrefab;
                return Resources.Load<GameObject>("Prefabs/Weapons/Weapon_RotaryGatling");
            }
            if (lower.Contains("beam") || lower == "3")
            {
                if (combatConfig != null && combatConfig.continuousBeamWeaponPrefab != null) return combatConfig.continuousBeamWeaponPrefab;
                return Resources.Load<GameObject>("Prefabs/Weapons/Weapon_ContinuousBeam");
            }
            if (lower.Contains("missile") || lower.Contains("homing") || lower == "2")
            {
                if (combatConfig != null && combatConfig.homingMissileWeaponPrefab != null) return combatConfig.homingMissileWeaponPrefab;
                return Resources.Load<GameObject>("Prefabs/Weapons/Weapon_HomingMissiles");
            }
            if (lower.Contains("scatter") || lower == "1")
            {
                if (combatConfig != null && combatConfig.scattergunWeaponPrefab != null) return combatConfig.scattergunWeaponPrefab;
                return Resources.Load<GameObject>("Prefabs/Weapons/Weapon_Scattergun");
            }

            if (combatConfig != null && combatConfig.pulseLaserWeaponPrefab != null) return combatConfig.pulseLaserWeaponPrefab;
            return Resources.Load<GameObject>("Prefabs/Weapons/Weapon_PulseLaser");
        }

        public static GameObject GetSocketMountPrefab()
        {
            var combatConfig = Resources.Load<CombatConfig>("Data/CombatConfig");
            if (combatConfig != null && combatConfig.socketMountPrefab != null) return combatConfig.socketMountPrefab;
            return Resources.Load<GameObject>("Prefabs/Weapons/Weapon_SocketMount");
        }

        public void UpdateSocketVisuals()
        {
            // Clean up existing visual hardpoint mounts
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i);
                if (child.name.StartsWith("SocketMount_") || child.name.StartsWith("SocketBase_"))
                {
                    if (Application.isPlaying) Destroy(child.gameObject);
                    else DestroyImmediate(child.gameObject);
                }
            }

            if (string.IsNullOrEmpty(currentShipId))
            {
                var prog = GetComponent<PlayerProgression>();
                if (prog != null && prog.currentShip != null) currentShipId = prog.currentShip.id;
                else currentShipId = "scout";
            }

            var sockets = GetShipHardpointSockets(currentShipId);

            bool hasMountedInfos = mountedHardpoints.Count > 0;
            if (hasMountedInfos)
            {
                activeSocketWeapons.Clear();
            }

            // Spawn visual socket mounts on top of each ship weapon socket
            for (int i = 0; i < sockets.Length; i++)
            {
                var sDef = sockets[i];
                var mountGo = new GameObject($"SocketMount_{sDef.index}");
                mountGo.transform.SetParent(transform, false);
                mountGo.transform.localPosition = new Vector3(sDef.localPosition.x, sDef.localPosition.y, 0f);
                mountGo.transform.localRotation = Quaternion.identity; // Forward along +X
                mountGo.transform.localScale = Vector3.one;

                // 1. Find equipped weapon for this socket
                string weaponId = null;
                MountedHardpointInfo mInfo = null;
                for (int m = 0; m < mountedHardpoints.Count; m++)
                {
                    if (mountedHardpoints[m].socketIndex == sDef.index)
                    {
                        mInfo = mountedHardpoints[m];
                        weaponId = mountedHardpoints[m].weaponId;
                        break;
                    }
                }

                if (string.IsNullOrEmpty(weaponId) && !hasMountedInfos)
                {
                    weaponId = StarStrike.Platform.Storage.StorageService.GetSocketWeapon(currentShipId, sDef.index);
                    if (string.IsNullOrEmpty(weaponId) && sDef.index == 0) weaponId = "pulse";
                }

                GameObject weaponPrefab = !string.IsNullOrEmpty(weaponId) ? GetWeaponPrefab(weaponId) : null;
                GameObject socketMountPrefab = GetSocketMountPrefab();

                if (weaponPrefab != null)
                {
                    GameObject wpnObj = Instantiate(weaponPrefab, mountGo.transform, false);
                    wpnObj.name = $"Mounted_{weaponId}";
                    wpnObj.transform.localPosition = Vector3.zero;
                    wpnObj.transform.localRotation = Quaternion.identity;
                    wpnObj.transform.localScale = Vector3.one;

                    var wComp = wpnObj.GetComponent<IWeapon>();
                    if (wComp != null)
                    {
                        if (mInfo != null)
                        {
                            mInfo.weaponInstance = wComp;
                        }
                        if (!activeSocketWeapons.Contains(wComp))
                        {
                            activeSocketWeapons.Add(wComp);
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(weaponId))
                {
                    var socketSr = mountGo.AddComponent<SpriteRenderer>();
                    socketSr.sprite = ProceduralSpriteHelper.GetWeaponSprite(weaponId);
                    socketSr.sortingOrder = 3;
                    mountGo.transform.localRotation = Quaternion.Euler(0f, 0f, -90f);
                    mountGo.transform.localScale = new Vector3(0.38f, 0.38f, 1f);
                }
                else
                {
                    if (socketMountPrefab != null)
                    {
                        GameObject mountVisual = Instantiate(socketMountPrefab, mountGo.transform, false);
                        mountVisual.name = "EmptySocketVisual";
                        mountVisual.transform.localPosition = Vector3.zero;
                        mountVisual.transform.localRotation = Quaternion.identity;
                        mountVisual.transform.localScale = Vector3.one;
                    }
                    else
                    {
                        var socketSr = mountGo.AddComponent<SpriteRenderer>();
                        socketSr.sprite = ProceduralSpriteHelper.GetWeaponSocketSprite();
                        socketSr.sortingOrder = 2;
                        mountGo.transform.localRotation = Quaternion.Euler(0f, 0f, -90f);
                        mountGo.transform.localScale = new Vector3(0.38f, 0.38f, 1f);
                    }
                }
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
            for (int i = 0; i < activeSocketWeapons.Count; i++)
            {
                activeSocketWeapons[i]?.StopFiring();
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

        public static WeaponArchetype GetWeaponArchetype(string weaponId)
        {
            if (string.IsNullOrEmpty(weaponId)) return WeaponArchetype.Projectile;
            string lower = weaponId.ToLowerInvariant().Trim();
            if (lower.Contains("beam") || lower == "3") return WeaponArchetype.Beam;
            if (lower.Contains("missile") || lower.Contains("homing") || lower == "2") return WeaponArchetype.Traceable;
            return WeaponArchetype.Projectile;
        }

        public static string GetArchetypeLabel(WeaponArchetype archetype)
        {
            switch (archetype)
            {
                case WeaponArchetype.Beam: return "BEAM // DIRECTED ENERGY";
                case WeaponArchetype.Traceable: return "TRACEABLE // GUIDED ORDNANCE";
                default: return "PROJECTILE // BALLISTIC & PLASMA";
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
