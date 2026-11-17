using UnityEngine;

namespace StarStrike.Core
{
    [CreateAssetMenu(fileName = "New Ship Definition", menuName = "StarStrike/Ship Definition")]
    public class ShipDefinition : ScriptableObject
    {
        public string id;
        public string shipName;
        public int tier;
        
        [Header("Progression")]
        public int maxUpgradeLevel;
        public ShipDefinition[] evolvesTo;
        
        [Header("Base Stats")]
        public StatValues baseStats;

        [Header("Weapon Settings")]
        public int weaponSlots = 1;

        [Header("Physical Sockets & Hitbox")]
        [Tooltip("Tuned forgiving circle collider radius for bullet-hell gameplay")]
        public float hitboxRadius = 0.55f;

        [Tooltip("Physical weapon hardpoint locations in local coordinates (X = forward, Y = lateral)")]
        public Vector2[] weaponSockets = new Vector2[0];

        [Tooltip("Engine exhaust hardpoint locations in local coordinates (X = forward, Y = lateral)")]
        public Vector2[] thrusterSockets = new Vector2[0];

        [Header("Prefab Reference")]
        public GameObject prefab;
        
        [Header("Presentation")]
        public Sprite sprite;
        public Color color = Color.white;
        public float scale = 1.0f;
    }
}
