using System;
using System.Collections.Generic;
using UnityEngine;
using StarStrike.Core;

namespace StarStrike.Gameplay
{
    public class ShipSkillController : MonoBehaviour
    {
        [System.Serializable]
        public class RuntimeSkillSlot
        {
            public SkillSlotType slotType;
            public SkillDefinition definition;
            public ISkillBehavior behaviorInstance;
            public float currentCooldown;
            public bool IsReady => currentCooldown <= 0f;
        }

        [Header("Default Loadout")]
        public SkillDefinition defaultActiveSkill;
        public SkillDefinition defaultPassiveSkill;

        [Header("Runtime State")]
        public List<RuntimeSkillSlot> slots = new List<RuntimeSkillSlot>();

        private ShipHealth health;

        private void Awake()
        {
            health = GetComponent<ShipHealth>();
            InitializeDefaultSlots();
        }

        public void InitializeDefaultSlots()
        {
            if (health == null) health = GetComponent<ShipHealth>();
            slots.Clear();

            // 1. Setup Active 1 (Forward Shockwave)
            if (defaultActiveSkill == null)
            {
                defaultActiveSkill = ScriptableObject.CreateInstance<SkillDefinition>();
                defaultActiveSkill.skillId = "forward_shockwave";
                defaultActiveSkill.displayName = "Forward Shockwave";
                defaultActiveSkill.slotType = SkillSlotType.Active1;
                defaultActiveSkill.energyCost = 20f;
                defaultActiveSkill.cooldownDuration = 6.0f;
            }

            var activeSlot = new RuntimeSkillSlot
            {
                slotType = SkillSlotType.Active1,
                definition = defaultActiveSkill,
                behaviorInstance = new ForwardShockwaveBehavior(),
                currentCooldown = 0f
            };
            activeSlot.behaviorInstance.Initialize(gameObject, defaultActiveSkill);
            slots.Add(activeSlot);

            // 2. Setup Passive 1 (Reactive Barrier)
            if (defaultPassiveSkill == null)
            {
                defaultPassiveSkill = ScriptableObject.CreateInstance<SkillDefinition>();
                defaultPassiveSkill.skillId = "reactive_barrier";
                defaultPassiveSkill.displayName = "Deflector Surge";
                defaultPassiveSkill.slotType = SkillSlotType.Passive1;
                defaultPassiveSkill.energyCost = 0f;
                defaultPassiveSkill.cooldownDuration = 25.0f;
            }

            var passiveSlot = new RuntimeSkillSlot
            {
                slotType = SkillSlotType.Passive1,
                definition = defaultPassiveSkill,
                behaviorInstance = new ReactiveBarrierPassiveBehavior(),
                currentCooldown = 0f
            };
            passiveSlot.behaviorInstance.Initialize(gameObject, defaultPassiveSkill);
            slots.Add(passiveSlot);
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            for (int i = 0; i < slots.Count; i++)
            {
                var slot = slots[i];
                if (slot == null) continue;

                if (slot.currentCooldown > 0f)
                {
                    slot.currentCooldown = Mathf.Max(0f, slot.currentCooldown - dt);
                    GameEvents.TriggerSkillCooldownChanged(new SkillCooldownEvent(slot.slotType, slot.currentCooldown, slot.definition != null ? slot.definition.cooldownDuration : 1f));
                }

                if (slot.behaviorInstance is ReactiveBarrierPassiveBehavior barrier)
                {
                    barrier.UpdateCooldown(dt);
                }
            }
        }

        public bool TryCastActiveSkill(SkillSlotType slotType = SkillSlotType.Active1)
        {
            if (health == null) health = GetComponent<ShipHealth>();
            RuntimeSkillSlot slot = GetSlot(slotType);
            if (slot == null || slot.definition == null || slot.behaviorInstance == null)
            {
                return false;
            }

            if (slot.currentCooldown > 0f)
            {
                GameEvents.TriggerSkillBlocked(new SkillBlockedEvent(slotType, SkillBlockReason.OnCooldown));
                return false;
            }

            if (health != null && !health.HasEnergy(slot.definition.energyCost))
            {
                GameEvents.TriggerSkillBlocked(new SkillBlockedEvent(slotType, SkillBlockReason.InsufficientEnergy));
                return false;
            }

            // Consume Energy
            if (health != null && slot.definition.energyCost > 0f)
            {
                health.ConsumeEnergy(slot.definition.energyCost);
            }

            // Start Cooldown
            slot.currentCooldown = slot.definition.cooldownDuration;

            // Execute Behavior along nose heading
            Vector2 castOrigin = transform.position;
            Vector2 castDirection = transform.right;
            slot.behaviorInstance.Execute(gameObject, castOrigin, castDirection);

            // Dispatch Events
            GameEvents.TriggerSkillCast(new SkillCastEvent(slotType, slot.definition, castOrigin, castDirection));
            GameEvents.TriggerSkillCooldownChanged(new SkillCooldownEvent(slotType, slot.currentCooldown, slot.definition.cooldownDuration));

            return true;
        }

        public RuntimeSkillSlot GetSlot(SkillSlotType slotType)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i] != null && slots[i].slotType == slotType)
                {
                    return slots[i];
                }
            }
            return null;
        }

        private void OnDestroy()
        {
            for (int i = 0; i < slots.Count; i++)
            {
                slots[i]?.behaviorInstance?.Terminate();
            }
            slots.Clear();
        }
    }
}
