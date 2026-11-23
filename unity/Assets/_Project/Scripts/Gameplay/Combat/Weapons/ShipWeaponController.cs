using System.Collections.Generic;
using UnityEngine;
using StarStrike.Core;

namespace StarStrike.Gameplay
{
    public class ShipWeaponController : MonoBehaviour
    {
        [Header("Weapons")]
        [SerializeField] private int currentWeaponIndex = 0;
        private readonly List<IWeapon> availableWeapons = new List<IWeapon>();

        public IWeapon CurrentWeapon => (availableWeapons.Count > 0 && currentWeaponIndex >= 0 && currentWeaponIndex < availableWeapons.Count) 
            ? availableWeapons[currentWeaponIndex] 
            : null;

        public int CurrentIndex => currentWeaponIndex;
        public int WeaponCount => availableWeapons.Count;
        public IReadOnlyList<IWeapon> AvailableWeapons => availableWeapons;

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

        public void EquipFromStorage(string shipId = null)
        {
            string weaponId = StarStrike.Platform.Storage.StorageService.GetEquippedWeapon(shipId);
            int targetIdx = WeaponIdToIndex(weaponId);
            if (targetIdx >= 0 && targetIdx < availableWeapons.Count)
            {
                SwitchWeapon(targetIdx);
            }
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
