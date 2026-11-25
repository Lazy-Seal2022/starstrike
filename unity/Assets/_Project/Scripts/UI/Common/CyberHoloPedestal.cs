using UnityEngine;
using UnityEngine.UI;

namespace StarStrike.UI
{
    /// <summary>
    /// Animates a holographic vector pedestal and anti-gravity craft hover
    /// for Wipeout/F-Zero cyber-vector presentation.
    /// </summary>
    public class CyberHoloPedestal : MonoBehaviour
    {
        [Header("Anti-Gravity Craft Bob")]
        [SerializeField] private RectTransform shipTransform;
        [SerializeField] private float bobAmplitude = 5.0f;
        [SerializeField] private float bobSpeed = 2.4f;

        [Header("Pedestal Pulse")]
        [SerializeField] private Image pedestalImage;
        [SerializeField] private float pulseSpeed = 3.2f;

        private Vector2 initialShipPos;
        private bool hasInitialPos;

        public void SetTargetShip(RectTransform shipRt)
        {
            shipTransform = shipRt;
            if (shipTransform != null)
            {
                initialShipPos = shipTransform.anchoredPosition;
                hasInitialPos = true;
            }
        }

        private void Awake()
        {
            if (pedestalImage == null)
            {
                pedestalImage = GetComponent<Image>();
            }
            if (pedestalImage != null)
            {
                pedestalImage.raycastTarget = false;
            }
        }

        private void Start()
        {
            IsolateCanvasIfNested(gameObject);
            if (shipTransform != null)
            {
                IsolateCanvasIfNested(shipTransform.gameObject);
            }
        }

        private static void IsolateCanvasIfNested(GameObject target)
        {
            if (target == null || target.GetComponent<Canvas>() != null) return;
            if (target.GetComponentInParent<Canvas>() != null)
            {
                target.AddComponent<Canvas>();
            }
        }

        private void OnEnable()
        {
            if (shipTransform != null && !hasInitialPos)
            {
                initialShipPos = shipTransform.anchoredPosition;
                hasInitialPos = true;
            }
        }

        private void Update()
        {
            float t = Time.unscaledTime;

            // 1. Anti-gravity hover bob on the ship
            if (shipTransform != null && hasInitialPos)
            {
                float bobOffset = Mathf.Sin(t * bobSpeed) * bobAmplitude;
                shipTransform.anchoredPosition = new Vector2(initialShipPos.x, initialShipPos.y + bobOffset);
            }

            // 2. Holographic pedestal brightness and scale pulse
            if (pedestalImage != null)
            {
                float pulse = 0.82f + 0.18f * Mathf.Sin(t * pulseSpeed);
                Color c = pedestalImage.color;
                pedestalImage.color = new Color(c.r, c.g, c.b, pulse);

                float scalePulse = 1.0f + 0.03f * Mathf.Sin(t * (pulseSpeed * 0.7f));
                transform.localScale = new Vector3(scalePulse, scalePulse, 1f);
            }
        }
    }
}
