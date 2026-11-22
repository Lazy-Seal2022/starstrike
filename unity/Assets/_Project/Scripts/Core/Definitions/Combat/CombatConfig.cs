using UnityEngine;

namespace StarStrike.Core
{
    [CreateAssetMenu(fileName = "CombatConfig", menuName = "StarStrike/Configs/Combat Config")]
    public class CombatConfig : ScriptableObject
    {
        [Header("Laser Projectiles")]
        public Sprite playerLaserSprite;
        public Sprite enemyLaserSprite;
        public GameObject playerLaserPrefab;
        public GameObject enemyLaserPrefab;
        public float defaultPulseDamage = 18f;
        public float defaultPulseSpeed = 22f;
        public float projectileLifetime = 3f;
        public float laserColliderRadius = 0.35f;

        [Header("Specialized Projectile Sprites & Prefabs")]
        public Sprite gatlingBulletSprite;
        public Sprite scattergunPelletSprite;
        public Sprite missileSprite;
        public GameObject gatlingBulletPrefab;
        public GameObject scattergunPelletPrefab;
        public GameObject missilePrefab;

        [Header("Weapon Systems")]
        public float defaultFireRate = 0.22f;
        public float minFireRate = 0.10f;
        public float defaultEnergyCost = 6f;
        public float recoilForce = 1.5f;
        public float muzzleForwardOffset = 1.55f;
        public float dualWeaponSpread = 0.3f;
        public float bulletInaccuracyAngle = 2.5f;
    }
}
