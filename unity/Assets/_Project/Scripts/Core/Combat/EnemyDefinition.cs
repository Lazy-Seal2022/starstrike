using UnityEngine;

namespace StarStrike.Core
{
    [CreateAssetMenu(fileName = "New Enemy Definition", menuName = "StarStrike/Enemy Definition")]
    public class EnemyDefinition : ScriptableObject
    {
        public string id;
        public string enemyName;
        
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
        
        [Header("Presentation")]
        public Sprite sprite;
    }
}
