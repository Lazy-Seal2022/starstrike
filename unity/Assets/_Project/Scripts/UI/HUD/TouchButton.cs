using UnityEngine;
using UnityEngine.EventSystems;
using StarStrike.Gameplay;

namespace StarStrike.UI
{
    public enum TouchButtonAction
    {
        Fire,
        Boost,
        Brake,
        Thrust
    }

    public class TouchButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public TouchButtonAction action = TouchButtonAction.Fire;
        public bool isPressed { get; private set; }

        public void OnPointerDown(PointerEventData eventData)
        {
            isPressed = true;
            ApplyState(true);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isPressed = false;
            ApplyState(false);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isPressed = false;
            ApplyState(false);
        }

        private void ApplyState(bool state)
        {
            if (PlayerInputReader.Instance == null) return;

            switch (action)
            {
                case TouchButtonAction.Fire:
                    PlayerInputReader.Instance.SetFire(state);
                    break;
                case TouchButtonAction.Boost:
                    PlayerInputReader.Instance.SetBoost(state);
                    break;
                case TouchButtonAction.Brake:
                    PlayerInputReader.Instance.SetBrake(state);
                    break;
                case TouchButtonAction.Thrust:
                    PlayerInputReader.Instance.SetThrust(state);
                    break;
            }
        }

        private void OnDisable()
        {
            isPressed = false;
            ApplyState(false);
        }
    }
}
