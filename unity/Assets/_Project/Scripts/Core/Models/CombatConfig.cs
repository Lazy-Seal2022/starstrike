using UnityEngine;

namespace StarStrike.Core
{
    [CreateAssetMenu(fileName = "CombatConfig", menuName = "StarStrike/Configs/Combat Config")]
    public class CombatConfig : ScriptableObject
    {
        [Header("Laser Projectiles")]
        public float defaultPulseDamage = 18f;
        public float defaultPulseSpeed = 22f;
        public float projectileLifetime = 3f;
        public float laserColliderRadius = 0.35f;

        [Header("Weapon Systems")]
        public float defaultFireRate = 0.22f;
        public float defaultEnergyCost = 6f;
        public float recoilForce = 1.5f;
        public float muzzleForwardOffset = 0.65f;
        public float dualWeaponSpread = 0.3f;
    }
}
