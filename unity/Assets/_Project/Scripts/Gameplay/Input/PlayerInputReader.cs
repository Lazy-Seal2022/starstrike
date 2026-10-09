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
        public float inputTurn;
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
            ResolveComponents();
            SetupInputActions();
        }

        private void Start()
        {
            ResolveComponents();
        }

        private void ResolveComponents()
        {
            if (movement == null) movement = GetComponent<ShipMovement>();
            if (weapon == null) weapon = GetComponent<ShipWeapon>();
            if (health == null) health = GetComponent<ShipHealth>();
        }

        private void SetupInputActions()
        {
            moveAction = new InputAction("Move", binding: "<Gamepad>/leftStick");
            moveAction.AddCompositeBinding("Dpad")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");

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
            if (movement == null || weapon == null || health == null)
            {
                ResolveComponents();
            }

            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
            {
                inputThrust = false;
                inputBrake = false;
                inputBoost = false;
                inputFire = false;
                inputTurn = 0f;
                
                if (movement != null)
                {
                    movement.IsThrusting = false;
                    movement.IsBraking = true; // Auto-brake in menus
                    movement.TurnInput = 0f;
                }
                return;
            }

            UpdateInput();

            if (movement != null)
            {
                movement.AimDirection = inputAimDirection;
                movement.TurnInput = inputTurn;
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
            Keyboard kb = Keyboard.current;
            Mouse mouse = Mouse.current;
            Gamepad pad = Gamepad.current;

            bool keyW = kb != null && (kb.wKey.isPressed || kb.upArrowKey.isPressed);
            bool keyS = kb != null && (kb.sKey.isPressed || kb.downArrowKey.isPressed);
            bool keyA = kb != null && (kb.aKey.isPressed || kb.leftArrowKey.isPressed);
            bool keyD = kb != null && (kb.dKey.isPressed || kb.rightArrowKey.isPressed);
            bool keySpace = kb != null && kb.spaceKey.isPressed;
            bool keyShift = kb != null && (kb.leftShiftKey.isPressed || kb.rightShiftKey.isPressed);

            bool mouseLeft = mouse != null && mouse.leftButton.isPressed;
            bool mouseRight = mouse != null && mouse.rightButton.isPressed;

            Vector2 moveVal = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
            float padY = pad != null ? pad.leftStick.y.ReadValue() : 0f;
            float padX = pad != null ? pad.leftStick.x.ReadValue() : 0f;
            bool padFire = pad != null && (pad.rightTrigger.isPressed || pad.buttonWest.isPressed);
            bool padBoost = pad != null && (pad.buttonSouth.isPressed || pad.leftShoulder.isPressed);

            bool actionFire = (attackAction != null && attackAction.IsPressed()) || keySpace || mouseLeft || padFire;
            bool actionSprint = (sprintAction != null && sprintAction.IsPressed()) || keyShift || mouseRight || padBoost;

            inputThrust = virtualThrust || keyW || moveVal.y > 0.15f || padY > 0.15f;
            inputBrake = virtualBrake || keyS || moveVal.y < -0.15f || padY < -0.15f;
            inputBoost = virtualBoost || actionSprint;
            inputFire = virtualFire || actionFire;

            // Turn input: +1 for counter-clockwise / Left, -1 for clockwise / Right
            float turn = 0f;
            if (keyA) turn += 1f;
            if (keyD) turn -= 1f;
            inputTurn = turn;

            // Aim direction resolution: Virtual -> Gamepad Right Stick -> Mouse Cursor -> Gamepad Left Stick
            if (hasVirtualAim)
            {
                inputAimDirection = virtualAimDirection;
                hasDirectAim = true;
            }
            else if (pad != null && pad.rightStick.ReadValue().sqrMagnitude > 0.15f)
            {
                inputAimDirection = pad.rightStick.ReadValue().normalized;
                hasDirectAim = true;
            }
            else if (mouse != null && Camera.main != null)
            {
                Vector2 mousePos = mouse.position.ReadValue();
                float camDistance = Mathf.Abs(Camera.main.transform.position.z);
                Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, camDistance));
                Vector2 diff = (Vector2)mouseWorld - (Vector2)transform.position;
                if (diff.sqrMagnitude > 0.05f)
                {
                    inputAimDirection = diff.normalized;
                }
                hasDirectAim = false;
            }
            else if (Mathf.Abs(padX) > 0.15f)
            {
                inputTurn = -padX;
            }
        }
    }
}
