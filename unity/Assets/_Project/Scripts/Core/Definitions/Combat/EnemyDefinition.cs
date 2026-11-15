using UnityEngine;

namespace StarStrike.Core
{
    public enum EnemyArchetype
    {
        Drone = 0,
        Kamikaze = 1,
        Sniper = 2,
        SupportShield = 3,
        Boss = 4
    }

    [CreateAssetMenu(fileName = "New Enemy Definition", menuName = "StarStrike/Enemy Definition")]
    public class EnemyDefinition : ScriptableObject
    {
        public string id;
        public string enemyName;
        public EnemyArchetype archetype = EnemyArchetype.Drone;
        
        [Header("Base Stats")]
        public float maxShield = 60f;
        public float shieldRegen = 0f;
        public float speed = 6f;
        public float agility = 5f;
        
        [Header("Weapon Settings")]
        public float pulseDamage = 14f;
        public float pulseSpeed = 18f;
        public float fireRate = 1.1f;
        
        [Header("AI Settings")]
        public float sightRadius = 16f;
        public float attackRadius = 10f;

        [Header("Archetype Specific Tuning")]
        public Color tintColor = Color.white;
        public float kamikazeBlastRadius = 4f;
        public float kamikazeBlastDamage = 35f;
        public float sniperChargeTime = 1.2f;
        public float supportShieldRadius = 4.5f;
        
        [Header("Presentation")]
        public Sprite sprite;
    }
}
