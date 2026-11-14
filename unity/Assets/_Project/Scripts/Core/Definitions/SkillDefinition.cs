using UnityEngine;

namespace StarStrike.Core
{
    [CreateAssetMenu(fileName = "NewSkill", menuName = "StarStrike/Combat/Skill Definition")]
    public class SkillDefinition : ScriptableObject
    {
        public string skillId = "shockwave_burst";
        public string displayName = "Forward Shockwave";
        [TextArea]
        public string description = "Fires a piercing shockwave that repels asteroids and enemies while pushing the ship backward.";
        public Sprite icon;
        public SkillSlotType slotType = SkillSlotType.Active1;
        public float energyCost = 20f;
        public float cooldownDuration = 6f;
        public GameObject effectPrefab;
    }
}
