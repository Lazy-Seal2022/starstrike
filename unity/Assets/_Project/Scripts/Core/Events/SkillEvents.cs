using UnityEngine;

namespace StarStrike.Core
{
    public enum SkillBlockReason
    {
        OnCooldown,
        InsufficientEnergy
    }

    public struct SkillCastEvent
    {
        public SkillSlotType Slot;
        public SkillDefinition Definition;
        public Vector2 CastOrigin;
        public Vector2 CastDirection;

        public SkillCastEvent(SkillSlotType slot, SkillDefinition definition, Vector2 origin, Vector2 direction)
        {
            Slot = slot;
            Definition = definition;
            CastOrigin = origin;
            CastDirection = direction;
        }
    }

    public struct SkillCooldownEvent
    {
        public SkillSlotType Slot;
        public float RemainingSeconds;
        public float TotalSeconds;

        public SkillCooldownEvent(SkillSlotType slot, float remaining, float total)
        {
            Slot = slot;
            RemainingSeconds = remaining;
            TotalSeconds = total;
        }
    }

    public struct SkillBlockedEvent
    {
        public SkillSlotType Slot;
        public SkillBlockReason Reason;

        public SkillBlockedEvent(SkillSlotType slot, SkillBlockReason reason)
        {
            Slot = slot;
            Reason = reason;
        }
    }
}
