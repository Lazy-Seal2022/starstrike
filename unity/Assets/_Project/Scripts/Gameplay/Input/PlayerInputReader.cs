using UnityEngine;
using UnityEngine.InputSystem;

namespace StarStrike.Gameplay
{
    public class PlayerInputReader : MonoBehaviour
    {
        public static PlayerInputReader Instance { get; private set; }

        public ShipMovement movement;
        public ShipWeapon weapon;
        public ShipHealth health;

        [Header("Input Actions")]
        private InputAction moveAction;
        private InputAction lookAction;
        private InputAction attackAction;
        private InputAction sprintAction;

        [Header("Input State")]
        public bool inputThrust;
        public bool inputBrake;
        public bool inputBoost;
        public bool inputFire;
        public Vector2 inputAimDirection;
        public bool hasDirectAim = false;

        private bool virtualThrust;
        private bool virtualBrake;
        private bool virtualBoost;
        private bool virtualFire;
        private bool hasVirtualAim;
        private Vector2 virtualAimDirection;

        private void Awake()
        {
            Instance = this;
            SetupInputActions();
        }

        private void SetupInputActions()
        {
            moveAction = new InputAction("Move", binding: "<Gamepad>/leftStick");
            moveAction.AddCompositeBinding("Dpad")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s");

            lookAction = new InputAction("Look", binding: "<Gamepad>/rightStick");
            lookAction.AddBinding("<Pointer>/position");

            attackAction = new InputAction("Attack", binding: "<Gamepad>/rightTrigger");
            attackAction.AddBinding("<Gamepad>/buttonWest"); // X on Xbox
            attackAction.AddBinding("<Keyboard>/space");
            attackAction.AddBinding("<Mouse>/leftButton");

            sprintAction = new InputAction("Sprint", binding: "<Gamepad>/buttonSouth"); // A on Xbox
            sprintAction.AddBinding("<Gamepad>/leftShoulder");
            sprintAction.AddBinding("<Keyboard>/shift");
            sprintAction.AddBinding("<Mouse>/rightButton");

            moveAction.Enable();
            lookAction.Enable();
            attackAction.Enable();
            sprintAction.Enable();
        }

        private void OnDestroy()
        {
            moveAction?.Disable();
            lookAction?.Disable();
            attackAction?.Disable();
            sprintAction?.Disable();
        }

        public void SetThrust(bool value) => virtualThrust = value;
        public void SetBrake(bool value) => virtualBrake = value;
        public void SetBoost(bool value) => virtualBoost = value;
        public void SetFire(bool value) => virtualFire = value;
        
        public void SetAim(Vector2 dir)
        {
            if (dir.sqrMagnitude > 0.01f)
            {
                hasVirtualAim = true;
                virtualAimDirection = dir.normalized;
            }
            else
            {
                hasVirtualAim = false;
                virtualAimDirection = Vector2.zero;
            }
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
            {
                inputThrust = false;
                inputBrake = false;
                inputBoost = false;
                inputFire = false;
                
                if (movement != null)
                {
                    movement.IsThrusting = false;
                    movement.IsBraking = true; // Auto-brake in menus
                }
                return;
            }

            UpdateInput();

            if (movement != null)
            {
                movement.AimDirection = inputAimDirection;
                movement.IsThrusting = inputThrust || inputBoost;
                movement.IsBraking = inputBrake;
            }

            if (inputFire && weapon != null)
            {
                weapon.TryFire(health);
            }
        }

        private void UpdateInput()
        {
            // Action bindings
            Vector2 moveVal = moveAction.ReadValue<Vector2>();
            bool actionThrust = moveVal.y > 0.1f;
            bool actionBrake = moveVal.y < -0.1f;
            
            bool actionFire = attackAction.IsPressed();
            bool actionSprint = sprintAction.IsPressed();

            inputThrust = virtualThrust || actionThrust;
            inputBrake = virtualBrake || actionBrake;
            inputBoost = virtualBoost || actionSprint;
            inputFire = virtualFire || actionFire;

            if (hasVirtualAim)
            {
                inputAimDirection = virtualAimDirection;
                hasDirectAim = true;
            }
            else
            {
                Vector2 rawLook = lookAction.ReadValue<Vector2>();
                
                // If the pointer (mouse) is driving lookAction, we need to convert to world space
                if (Mouse.current != null && lookAction.activeControl?.device == Mouse.current)
                {
                    if (Camera.main != null)
                    {
                        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(new Vector3(rawLook.x, rawLook.y, 10f));
                        Vector2 diff = (Vector2)mouseWorld - (Vector2)transform.position;
                        if (diff.sqrMagnitude > 0.05f)
                        {
                            inputAimDirection = diff.normalized;
                        }
                        hasDirectAim = false;
                    }
                }
                else if (rawLook.sqrMagnitude > 0.1f) // Gamepad Right Stick
                {
                    inputAimDirection = rawLook.normalized;
                    hasDirectAim = true;
                }
                else if (Mathf.Abs(moveVal.x) > 0.1f) // Gamepad Left Stick / keyboard A/D fallback
                {
                    // Fallback to steering with movement keys if no right stick
                    inputAimDirection = moveVal.normalized;
                    hasDirectAim = true;
                }
            }
        }
    }
}
