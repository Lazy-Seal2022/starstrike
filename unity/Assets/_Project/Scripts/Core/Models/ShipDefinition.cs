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
        
        [Header("Presentation")]
        public Sprite sprite;
        public Color color = Color.white;
        public float scale = 1.0f;
    }
}
