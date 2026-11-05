using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Collections;
using Unity.Netcode;
using StarStrike.Core;
using StarStrike.Gameplay;
using StarStrike.Gameplay.Audio;

namespace StarStrike.Network
{
    [RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D), typeof(NetworkObject))]
    public class NetworkPlayerShip : NetworkBehaviour
    {
        [Header("Prefabs")]
        public GameObject laserPrefab;

        [Header("Flight Stats")]
        public float maxSpeed = 12f;
        public float acceleration = 24f;
        public float agility = 5f;
        public float fireRate = 0.2f;

        [Header("Synchronized State")]
        public NetworkVariable<FixedString32Bytes> NetworkShipId = new NetworkVariable<FixedString32Bytes>(
            new FixedString32Bytes("scout"),
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        public NetworkVariable<FixedString32Bytes> NetworkPlayerName = new NetworkVariable<FixedString32Bytes>(
            new FixedString32Bytes("Pilot"),
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        public NetworkVariable<float> NetworkShield = new NetworkVariable<float>(
            100f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        public NetworkVariable<float> NetworkMaxShield = new NetworkVariable<float>(
            100f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        public NetworkVariable<bool> NetworkIsDead = new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        public NetworkVariable<Vector2> NetworkPosition = new NetworkVariable<Vector2>(
            Vector2.zero,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);

        public NetworkVariable<float> NetworkRotation = new NetworkVariable<float>(
            0f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);

        private Rigidbody2D rb;
        private CircleCollider2D col;
        private SpriteRenderer sr;
        private float lastFireTime = -999f;

        private void Awake()
        {
            if (laserPrefab == null)
            {
                laserPrefab = Resources.Load<GameObject>("Prefabs/Network/NetworkLaserProjectile");
            }

            rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.gravityScale = 0f;
                rb.linearDamping = 0.5f;
                rb.angularDamping = 1.0f;
            }

            col = GetComponent<CircleCollider2D>();
            if (col != null)
            {
                col.radius = 0.6f;
                col.isTrigger = false;
            }

            sr = GetComponent<SpriteRenderer>();
            if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 10;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            NetworkShipId.OnValueChanged += HandleShipIdChanged;
            NetworkIsDead.OnValueChanged += HandleDeadChanged;

            UpdateVisuals(NetworkShipId.Value.ToString());

            if (IsOwner)
            {
                // Register with camera in arena
                if (Camera.main != null)
                {
                    Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, -10f);
                }
            }
        }

        public override void OnNetworkDespawn()
        {
            NetworkShipId.OnValueChanged -= HandleShipIdChanged;
            NetworkIsDead.OnValueChanged -= HandleDeadChanged;
            base.OnNetworkDespawn();
        }

        private void HandleShipIdChanged(FixedString32Bytes oldVal, FixedString32Bytes newVal)
        {
            UpdateVisuals(newVal.ToString());
        }

        private void HandleDeadChanged(bool oldVal, bool newVal)
        {
            if (sr != null) sr.enabled = !newVal;
            if (col != null) col.enabled = !newVal;
        }

        private void UpdateVisuals(string shipId)
        {
            if (sr != null)
            {
                sr.sprite = ProceduralSpriteHelper.GetShipSprite(shipId);
            }
        }

        private void Update()
        {
            if (NetworkIsDead.Value) return;

            if (IsOwner)
            {
                HandleOwnerInput();
            }
            else
            {
                // Smooth interpolation for remote proxies
                transform.position = Vector3.Lerp(transform.position, NetworkPosition.Value, 15f * Time.deltaTime);
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, NetworkRotation.Value), 15f * Time.deltaTime);
            }
        }

        private void HandleOwnerInput()
        {
            Vector2 moveDir = Vector2.zero;
            if (Keyboard.current != null)
            {
                if (Keyboard.current.wKey.isPressed) moveDir.y += 1f;
                if (Keyboard.current.sKey.isPressed) moveDir.y -= 1f;
                if (Keyboard.current.aKey.isPressed) moveDir.x -= 1f;
                if (Keyboard.current.dKey.isPressed) moveDir.x += 1f;
            }
            if (Gamepad.current != null)
            {
                Vector2 stick = Gamepad.current.leftStick.ReadValue();
                if (stick.sqrMagnitude > 0.05f) moveDir = stick;
            }

            if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();

            // Physics movement
            if (rb != null)
            {
                rb.AddForce(moveDir * acceleration, ForceMode2D.Force);
                if (rb.linearVelocity.magnitude > maxSpeed)
                {
                    rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
                }
            }

            // Aim toward mouse / right stick
            Vector2 aimTarget = Vector2.zero;
            bool hasAim = false;
            if (Mouse.current != null && Camera.main != null)
            {
                Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                Vector2 toMouse = (Vector2)mouseWorld - (Vector2)transform.position;
                if (toMouse.sqrMagnitude > 0.1f)
                {
                    aimTarget = toMouse.normalized;
                    hasAim = true;
                }
            }
            if (Gamepad.current != null)
            {
                Vector2 rStick = Gamepad.current.rightStick.ReadValue();
                if (rStick.sqrMagnitude > 0.1f)
                {
                    aimTarget = rStick.normalized;
                    hasAim = true;
                }
            }

            if (hasAim)
            {
                float targetAngle = Mathf.Atan2(aimTarget.y, aimTarget.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, targetAngle), agility * 5f * Time.deltaTime);
            }
            else if (rb != null && rb.linearVelocity.sqrMagnitude > 0.1f)
            {
                float moveAngle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, moveAngle), agility * 3f * Time.deltaTime);
            }

            // Write owner state to network variable
            NetworkPosition.Value = transform.position;
            NetworkRotation.Value = transform.eulerAngles.z;

            // Camera follow for local player
            if (Camera.main != null)
            {
                Vector3 camPos = new Vector3(transform.position.x, transform.position.y, -10f);
                Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, camPos, 10f * Time.deltaTime);
            }

            // Firing
            bool isFiring = false;
            if (Mouse.current != null && Mouse.current.leftButton.isPressed) isFiring = true;
            if (Keyboard.current != null && Keyboard.current.spaceKey.isPressed) isFiring = true;
            if (Gamepad.current != null && (Gamepad.current.rightTrigger.isPressed || Gamepad.current.buttonWest.isPressed)) isFiring = true;

            if (isFiring && Time.time - lastFireTime >= fireRate)
            {
                lastFireTime = Time.time;
                Vector2 muzzle = (Vector2)transform.position + (Vector2)transform.right * 0.7f;
                FireLaserServerRpc(muzzle, transform.rotation);
            }
        }

        [ServerRpc]
        private void FireLaserServerRpc(Vector2 origin, Quaternion rot)
        {
            if (laserPrefab != null)
            {
                var laserGo = Instantiate(laserPrefab, origin, rot);
                var netProj = laserGo.GetComponent<NetworkLaserProjectile>();
                if (netProj != null)
                {
                    netProj.AttackerClientId = OwnerClientId;
                }
                var netObj = laserGo.GetComponent<NetworkObject>();
                if (netObj != null)
                {
                    netObj.Spawn();
                }
            }
        }

        public void ApplyNetworkDamage(float amount, ulong attackerClientId)
        {
            if (!IsServer || NetworkIsDead.Value) return;

            NetworkShield.Value -= amount;

            if (NetworkShield.Value <= 0f)
            {
                NetworkShield.Value = 0f;
                NetworkIsDead.Value = true;

                if (NetworkMatchManager.Instance != null)
                {
                    NetworkMatchManager.Instance.RecordKill(attackerClientId, OwnerClientId);
                }

                PlayExplosionClientRpc(transform.position);

                StartCoroutine(RespawnRoutine());
            }
        }

        private IEnumerator RespawnRoutine()
        {
            yield return new WaitForSeconds(3.0f);

            Vector2 spawnPos = Vector2.zero;
            if (NetworkMatchManager.Instance != null)
            {
                spawnPos = NetworkMatchManager.Instance.GetSpawnPoint(OwnerClientId);
            }

            transform.position = spawnPos;
            NetworkPosition.Value = spawnPos;
            NetworkShield.Value = NetworkMaxShield.Value;
            NetworkIsDead.Value = false;
        }

        [ClientRpc]
        private void PlayExplosionClientRpc(Vector3 pos)
        {
            GameEvents.TriggerAsteroidDestroyed(pos, "Large");
            AudioService.Instance?.PlaySound("explosion", 0.7f);
        }
    }
}
