using UnityEngine;
using UnityEngine.EventSystems;
using StarStrike.Gameplay;

namespace StarStrike.UI
{
    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [Header("Joystick Settings")]
        public RectTransform background;
        public RectTransform handle;
        public float handleRange = 50f;
        public bool autoThrustOnMove = true;
        public float autoThrustThreshold = 0.25f;

        [Header("Output")]
        public Vector2 inputVector = Vector2.zero;

        private Vector2 bgCenter;

        private void Start()
        {
            if (background == null)
                background = GetComponent<RectTransform>();

            if (handle == null && transform.childCount > 0)
                handle = transform.GetChild(0).GetComponent<RectTransform>();

            ResetJoystick();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (background == null) return;

            Vector2 localPoint;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(background, eventData.position, eventData.pressEventCamera, out localPoint))
            {
                // Constrain position within handleRange
                float dist = localPoint.magnitude;
                Vector2 direction = dist > 0.001f ? localPoint / dist : Vector2.zero;
                float clampedDist = Mathf.Clamp(dist, 0f, handleRange);

                Vector2 handlePos = direction * clampedDist;
                if (handle != null)
                {
                    handle.anchoredPosition = handlePos;
                }

                inputVector = handleRange > 0f ? handlePos / handleRange : Vector2.zero;

                // Send to ShipController
                if (ShipController.Instance != null)
                {
                    ShipController.Instance.SetAimDirection(inputVector);

                    if (autoThrustOnMove)
                    {
                        bool shouldThrust = inputVector.magnitude >= autoThrustThreshold;
                        ShipController.Instance.SetThrust(shouldThrust);
                    }
                }
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            ResetJoystick();
        }

        private void ResetJoystick()
        {
            inputVector = Vector2.zero;
            if (handle != null)
            {
                handle.anchoredPosition = Vector2.zero;
            }

            if (ShipController.Instance != null)
            {
                ShipController.Instance.SetAimDirection(Vector2.zero);
                if (autoThrustOnMove)
                {
                    ShipController.Instance.SetThrust(false);
                }
            }
        }

        private void OnDisable()
        {
            ResetJoystick();
        }
    }
}
