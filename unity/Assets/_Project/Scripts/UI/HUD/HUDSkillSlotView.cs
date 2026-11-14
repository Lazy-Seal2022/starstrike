using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using StarStrike.Core;
using StarStrike.Gameplay;
using StarStrike.Gameplay.Audio;

namespace StarStrike.UI
{
    public class HUDSkillSlotView : MonoBehaviour
    {
        [Header("Configuration")]
        public SkillSlotType targetSlot = SkillSlotType.Active1;

        [Header("UI Bindings")]
        public Image iconImage;
        public Image cooldownOverlay;
        public Text cooldownText;
        public Image borderImage;
        public Text keybindLabel;
        public Button triggerButton;

        [Header("Colors")]
        public Color normalBorderColor = new Color(0.38f, 0.74f, 0.98f, 0.85f);
        public Color energyStarvedBorderColor = new Color(0.95f, 0.2f, 0.2f, 1f);

        private Coroutine flashCoroutine;

        private void OnEnable()
        {
            GameEvents.OnSkillCooldownChanged += HandleCooldownChanged;
            GameEvents.OnSkillBlocked += HandleSkillBlocked;
            GameEvents.OnSkillCast += HandleSkillCast;

            if (triggerButton != null)
            {
                triggerButton.onClick.AddListener(OnButtonClicked);
            }

            ResetVisuals();
        }

        private void OnDisable()
        {
            GameEvents.OnSkillCooldownChanged -= HandleCooldownChanged;
            GameEvents.OnSkillBlocked -= HandleSkillBlocked;
            GameEvents.OnSkillCast -= HandleSkillCast;

            if (triggerButton != null)
            {
                triggerButton.onClick.RemoveListener(OnButtonClicked);
            }
        }

        public void ResetVisuals()
        {
            if (cooldownOverlay != null) cooldownOverlay.fillAmount = 0f;
            if (cooldownText != null) cooldownText.text = "";
            if (borderImage != null) borderImage.color = normalBorderColor;
        }

        private void OnButtonClicked()
        {
            if (PlayerInputReader.Instance != null)
            {
                PlayerInputReader.Instance.TriggerSkill(targetSlot);
            }
        }

        private void HandleCooldownChanged(SkillCooldownEvent evt)
        {
            if (evt.Slot != targetSlot) return;

            if (cooldownOverlay != null)
            {
                cooldownOverlay.fillAmount = evt.TotalSeconds > 0f ? evt.RemainingSeconds / evt.TotalSeconds : 0f;
            }

            if (cooldownText != null)
            {
                if (evt.RemainingSeconds > 0.05f)
                {
                    cooldownText.text = evt.RemainingSeconds >= 1f ? evt.RemainingSeconds.ToString("F1") + "s" : evt.RemainingSeconds.ToString("F1");
                }
                else
                {
                    cooldownText.text = "";
                }
            }
        }

        private void HandleSkillBlocked(SkillBlockedEvent evt)
        {
            if (evt.Slot != targetSlot) return;

            if (evt.Reason == SkillBlockReason.InsufficientEnergy)
            {
                TriggerEnergyWarning();
            }
        }

        private void HandleSkillCast(SkillCastEvent evt)
        {
            if (evt.Slot != targetSlot) return;

            if (borderImage != null)
            {
                borderImage.color = Color.white;
                Invoke(nameof(RestoreBorderColor), 0.12f);
            }
        }

        public void TriggerEnergyWarning()
        {
            if (flashCoroutine != null) StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(EnergyFlashRoutine());

            if (AudioService.Instance != null)
            {
                AudioService.Instance.PlayUISound("ui_error", 1f);
            }
        }

        private IEnumerator EnergyFlashRoutine()
        {
            if (borderImage != null)
            {
                for (int i = 0; i < 2; i++)
                {
                    borderImage.color = energyStarvedBorderColor;
                    yield return new WaitForSeconds(0.1f);
                    borderImage.color = normalBorderColor;
                    yield return new WaitForSeconds(0.08f);
                }
            }
            RestoreBorderColor();
        }

        private void RestoreBorderColor()
        {
            if (borderImage != null)
            {
                borderImage.color = normalBorderColor;
            }
        }
    }
}
