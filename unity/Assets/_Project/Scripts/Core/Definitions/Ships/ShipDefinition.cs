using UnityEngine;

namespace StarStrike.Core
{
    [CreateAssetMenu(fileName = "New Ship Definition", menuName = "StarStrike/Ship Definition")]
    public class ShipDefinition : ScriptableObject
    {
        public string id;
        public string shipName;
        public int tier;

        [Header("Classification")]
        public ShipClass shipClass = ShipClass.Fighter;
        [TextArea(1, 3)]
        public string classRoleDescription = "";

        [Header("Tactical Camera & Physics Profile")]
        [Tooltip("Camera orthographic zoom size tailored to hull presence (e.g. 7.5 Fighter -> 15.0 Battleship)")]
        public float cameraOrthoSize = 8.5f;

        [Tooltip("Ship hull mass affecting recoil kick and collision impact inertia")]
        public float hullMass = 1.0f;

        [Tooltip("Collision damage resistance ratio (0 = full damage, 0.40 = 40% reduction)")]
        public float collisionArmorReduction = 0f;
        
        [Header("Progression")]
        public int maxUpgradeLevel;
        public ShipDefinition[] evolvesTo;
        
        [Header("Base Stats")]
        public StatValues baseStats;

        [Header("Weapon Settings")]
        public int weaponSlots = 1;

        [Header("Modular Hardpoint Sockets")]
        public ShipSocketDefinition[] hardpointSockets = new ShipSocketDefinition[0];

        [Header("Carrier & Drone Systems")]
        public int droneHangarCapacity = 0;
        public string defaultDroneType = "";

        [Header("Physical Sockets & Hitbox")]
        [Tooltip("Tuned forgiving circle collider radius for bullet-hell gameplay")]
        public float hitboxRadius = 0.55f;

        [Tooltip("Outward radial offset for smooth round shield bubble")]
        public float shieldHitboxOffset = 0.20f;

        [Tooltip("Physical weapon hardpoint locations in local coordinates (X = forward, Y = lateral)")]
        public Vector2[] weaponSockets = new Vector2[0];

        [Tooltip("Engine exhaust hardpoint locations in local coordinates (X = forward, Y = lateral)")]
        public Vector2[] thrusterSockets = new Vector2[0];

        [Header("Prefab Reference")]
        public GameObject prefab;
        
        [Header("Presentation")]
        public Sprite sprite;
        public Texture2D emissiveTexture;
        public Color emissiveColor = new Color(0.25f, 0.9f, 1f, 1f);
        public float emissiveIntensity = 1.8f;
        public Color color = Color.white;
        public float scale = 1.0f;

        /// <summary>
        /// Returns physical weapon socket positions, prioritizing hardpointSockets if defined, else fallback to legacy weaponSockets.
        /// </summary>
        public Vector2[] GetWeaponSocketPositions()
        {
            if (hardpointSockets != null && hardpointSockets.Length > 0)
            {
                var positions = new Vector2[hardpointSockets.Length];
                for (int i = 0; i < hardpointSockets.Length; i++)
                {
                    positions[i] = hardpointSockets[i].localPosition;
                }
                return positions;
            }
            return weaponSockets ?? new Vector2[0];
        }

        /// <summary>
        /// Retrieves socket definition by index with fallback to legacy vectors.
        /// </summary>
        public ShipSocketDefinition GetSocket(int index)
        {
            if (hardpointSockets != null && index >= 0 && index < hardpointSockets.Length)
            {
                return hardpointSockets[index];
            }
            if (weaponSockets != null && index >= 0 && index < weaponSockets.Length)
            {
                return new ShipSocketDefinition($"Socket_{index}", weaponSockets[index]);
            }
            return default;
        }

        public int SocketCount => (hardpointSockets != null && hardpointSockets.Length > 0) ? hardpointSockets.Length : (weaponSockets != null ? weaponSockets.Length : weaponSlots);
    }
}
