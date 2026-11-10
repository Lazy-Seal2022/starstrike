using UnityEngine;
using UnityEngine.InputSystem;
using StarStrike.Core;
using StarStrike.Gameplay.FX;
using StarStrike.Platform.Storage;

namespace StarStrike.Gameplay
{
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        public static CameraController Instance { get; private set; }

        [Header("Framing & Zoom")]
        public float defaultOrthographicSize = 18f;
        public float minOrthographicSize = 12f;
        public float maxOrthographicSize = 30f;
        public float zoomSpeed = 8f;

        [Header("Dynamic Speed Zoom")]
        public bool enableSpeedZoom = true;
        public float speedZoomFactor = 0.25f; // Extra zoom units per speed unit
        public float maxSpeedZoomOffset = 6f;

        [Header("Follow Settings")]
        public float followDamping = 8f;
        public float velocityLeadTime = 0.35f;

        public Transform target;
        private Camera cam;
        private Rigidbody2D targetRb;
        private float targetZoom;
        private float currentZoom;

        private void Awake()
        {
            EnsureInitialized();
        }

        public void EnsureInitialized()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            if (cam == null)
            {
                cam = GetComponent<Camera>();
                if (cam == null) cam = Camera.main;
            }

            if (cam != null && targetZoom <= 0.01f)
            {
                cam.orthographic = true;
                float savedZoom = StorageService.GetCameraZoom();
                defaultOrthographicSize = Mathf.Clamp(savedZoom, minOrthographicSize, maxOrthographicSize);
                targetZoom = defaultOrthographicSize;
                currentZoom = targetZoom;
                cam.orthographicSize = currentZoom;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Start()
        {
            EnsureInitialized();
            FindTargetIfNull();
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            targetRb = target != null ? target.GetComponent<Rigidbody2D>() : null;
        }

        public void SetBaseZoom(float zoom)
        {
            EnsureInitialized();
            targetZoom = Mathf.Clamp(zoom, minOrthographicSize, maxOrthographicSize);
            StorageService.SaveCameraZoom(targetZoom);
            if (cam != null)
            {
                cam.orthographicSize = targetZoom;
            }
        }

        public float GetBaseZoom()
        {
            EnsureInitialized();
            return targetZoom;
        }

        public void AdjustZoom(float delta)
        {
            EnsureInitialized();
            SetBaseZoom(targetZoom + delta);
        }

        private void FindTargetIfNull()
        {
            if (target != null && target.gameObject.activeInHierarchy) return;

            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null && player.activeInHierarchy)
            {
                SetTarget(player.transform);
            }
        }

        private void Update()
        {
            HandleScrollZoomInput();
        }

        private void HandleScrollZoomInput()
        {
            if (Mouse.current != null)
            {
                Vector2 scroll = Mouse.current.scroll.ReadValue();
                if (Mathf.Abs(scroll.y) > 0.01f)
                {
                    // Scroll down = zoom out (increase size, see broader), scroll up = zoom in (decrease size)
                    float zoomDelta = -Mathf.Sign(scroll.y) * 1.5f;
                    AdjustZoom(zoomDelta);
                }
            }
        }

        private void LateUpdate()
        {
            FindTargetIfNull();

            // Calculate dynamic zoom based on ship speed
            float extraZoom = 0f;
            if (enableSpeedZoom && targetRb != null)
            {
                float speed = targetRb.linearVelocity.magnitude;
                extraZoom = Mathf.Min(speed * speedZoomFactor, maxSpeedZoomOffset);
            }

            float desiredSize = Mathf.Clamp(targetZoom + extraZoom, minOrthographicSize, maxOrthographicSize + maxSpeedZoomOffset);
            currentZoom = Mathf.Lerp(currentZoom, desiredSize, zoomSpeed * Time.deltaTime);
            if (cam != null)
            {
                cam.orthographicSize = currentZoom;
            }

            // Follow target with forward lead
            if (target != null)
            {
                Vector3 leadOffset = Vector3.zero;
                if (targetRb != null && targetRb.linearVelocity.sqrMagnitude > 0.1f)
                {
                    leadOffset = (Vector3)targetRb.linearVelocity * velocityLeadTime;
                }

                Vector3 targetPos = new Vector3(target.position.x, target.position.y, -10f) + leadOffset;
                transform.position = Vector3.Lerp(transform.position, targetPos, followDamping * Time.deltaTime);

                if (FXService.Instance != null)
                {
                    transform.position += FXService.CameraOffset;
                }
            }
        }
    }
}
