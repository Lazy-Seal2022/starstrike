using UnityEngine;

namespace StarStrike.Core
{
    [CreateAssetMenu(fileName = "LootConfig", menuName = "StarStrike/Configs/Loot Config")]
    public class LootConfig : ScriptableObject
    {
        [Header("Gems")]
        public int defaultGemValue = 1;
        public float magnetRadius = 6.5f;
        public float magnetSpeed = 14f;
        public float minEjectionSpeed = 1.5f;
        public float maxEjectionSpeed = 4.0f;
        public float gemColliderRadius = 0.5f;

        [Header("Power-Ups")]
        public float shieldOverchargeBonus = 100f;
        public float weaponOverdriveDamageMultiplier = 1.5f;
        public float weaponOverdriveFireRateMultiplier = 0.5f;
    }
}
