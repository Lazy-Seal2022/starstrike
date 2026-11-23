using UnityEngine;
using UnityEngine.InputSystem;
using StarStrike.Core;
using StarStrike.Platform.Storage;

namespace StarStrike.Gameplay
{
    public class PlayerInputReader : MonoBehaviour, IShipInputProvider
    {
        public static PlayerInputReader Instance { get; private set; }

        public ShipMovement movement;
        public ShipWeapon weapon;
        public ShipWeaponController weaponController;
        public IWeapon ActiveWeapon => (weaponController != null && weaponController.CurrentWeapon != null) ? weaponController.CurrentWeapon : (IWeapon)weapon;
        public ShipHealth health;
        public ShipSkillController skillController;

        [Header("Control Settings")]
        public int aimMode = 0; // 0 = Mouse / Direct Aim, 1 = Keyboard Turn
        public float mouseSensitivity = 1.0f;
        public bool autoBrakeEnabled = true;
        public bool invertTurn = false;

        [Header("Input Actions")]
        private InputAction moveAction;
        private InputAction lookAction;
        private InputAction attackAction;
        private InputAction sprintAction;
        private InputAction skillAction;

        [Header("Input State")]
        public bool inputThrust;
        public bool inputBrake;
        public bool inputBoost;
        public bool inputFire;
        public float inputTurn;
        public Vector2 inputAimDirection;

        public Vector2 AimDirection => inputAimDirection;
        public float TurnInput => inputTurn;
        public bool IsThrusting => inputThrust;
        public bool IsBoosting => inputBoost;
        public bool IsBraking => inputBrake;
        public bool hasDirectAim = false;

        [Header("Firing Management")]
        public bool autoFireEnabled = true;
        private bool wasFirePressedLastFrame = false;
        private float fireHeldDuration = 0f;
        private bool hasAutoFired = false;

        private bool virtualThrust;
        private bool virtualBrake;
        private bool virtualBoost;
        private bool virtualFire;
        private bool hasVirtualAim;
        private Vector2 virtualAimDirection;

        private void Awake()
        {
            Instance = this;
            ApplySettings();
            ResolveComponents();
            SetupInputActions();
        }

        private void Start()
        {
            ApplySettings();
            ResolveComponents();
        }

        public void ApplySettings()
        {
            aimMode = StorageService.GetControlAimMode();
            mouseSensitivity = StorageService.GetMouseSensitivity();
            autoBrakeEnabled = StorageService.GetAutoBrake();
            invertTurn = StorageService.GetInvertTurn();
        }

        private void ResolveComponents()
        {
            if (movement == null) movement = GetComponent<ShipMovement>();
            if (weaponController == null) weaponController = GetComponent<ShipWeaponController>();
            if (weaponController == null && (CompareTag("Player") || gameObject.name == "Player"))
            {
                weaponController = gameObject.AddComponent<ShipWeaponController>();
            }
            if (weapon == null) weapon = GetComponent<ShipWeapon>();
            if (health == null) health = GetComponent<ShipHealth>();
            if (skillController == null)
            {
                skillController = GetComponent<ShipSkillController>();
                if (skillController == null && (CompareTag("Player") || gameObject.name == "Player"))
                {
                    skillController = gameObject.AddComponent<ShipSkillController>();
                }
            }
        }

        private void SetupInputActions()
        {
            moveAction = new InputAction("Move", type: InputActionType.Value, binding: "<Gamepad>/leftStick");
            moveAction.AddCompositeBinding("Dpad")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");

            lookAction = new InputAction("Look", type: InputActionType.Value, binding: "<Gamepad>/rightStick");
            lookAction.AddBinding("<Pointer>/position");

            attackAction = new InputAction("Attack", type: InputActionType.Button);
            attackAction.AddBinding("<Gamepad>/rightTrigger");
            attackAction.AddBinding("<Gamepad>/buttonWest"); // X on Xbox
            attackAction.AddBinding("<Keyboard>/space");
            attackAction.AddBinding("<Mouse>/leftButton");

            sprintAction = new InputAction("Sprint", type: InputActionType.Button);
            sprintAction.AddBinding("<Gamepad>/buttonSouth"); // A on Xbox
            sprintAction.AddBinding("<Gamepad>/leftShoulder");
            sprintAction.AddBinding("<Keyboard>/shift");
            sprintAction.AddBinding("<Mouse>/rightButton");

            skillAction = new InputAction("Skill", type: InputActionType.Button);
            skillAction.AddBinding("<Keyboard>/e");
            skillAction.AddBinding("<Gamepad>/buttonNorth"); // Y on Xbox

            moveAction.Enable();
            lookAction.Enable();
            attackAction.Enable();
            sprintAction.Enable();
            skillAction.Enable();

            string savedOverrides = StorageService.GetInputBindingOverrides();
            if (!string.IsNullOrEmpty(savedOverrides))
            {
                LoadBindingOverrides(savedOverrides);
            }
        }

        [System.Serializable]
        public class BindingOverridesData
        {
            public string moveOverrides;
            public string attackOverrides;
            public string sprintOverrides;
            public string skillOverrides;
        }

        public string SaveBindingOverrides()
        {
            var data = new BindingOverridesData
            {
                moveOverrides = moveAction?.SaveBindingOverridesAsJson(),
                attackOverrides = attackAction?.SaveBindingOverridesAsJson(),
                sprintOverrides = sprintAction?.SaveBindingOverridesAsJson(),
                skillOverrides = skillAction?.SaveBindingOverridesAsJson()
            };
            string json = JsonUtility.ToJson(data);
            StorageService.SaveInputBindingOverrides(json);
            return json;
        }

        public void LoadBindingOverrides(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            try
            {
                var data = JsonUtility.FromJson<BindingOverridesData>(json);
                if (data != null)
                {
                    if (!string.IsNullOrEmpty(data.moveOverrides)) moveAction?.LoadBindingOverridesFromJson(data.moveOverrides);
                    if (!string.IsNullOrEmpty(data.attackOverrides)) attackAction?.LoadBindingOverridesFromJson(data.attackOverrides);
                    if (!string.IsNullOrEmpty(data.sprintOverrides)) sprintAction?.LoadBindingOverridesFromJson(data.sprintOverrides);
                    if (!string.IsNullOrEmpty(data.skillOverrides)) skillAction?.LoadBindingOverridesFromJson(data.skillOverrides);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Failed to deserialize binding overrides: {ex.Message}");
            }
        }

        public void ResetBindingOverrides()
        {
            moveAction?.RemoveAllBindingOverrides();
            attackAction?.RemoveAllBindingOverrides();
            sprintAction?.RemoveAllBindingOverrides();
            skillAction?.RemoveAllBindingOverrides();
            StorageService.SaveInputBindingOverrides("");
        }

        private void OnDisable()
        {
            ResetInputState();
        }

        private void OnDestroy()
        {
            moveAction?.Disable();
            lookAction?.Disable();
            attackAction?.Disable();
            sprintAction?.Disable();
            skillAction?.Disable();
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
            if (movement == null || weapon == null || health == null || skillController == null)
            {
                ResolveComponents();
            }

            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
            {
                ResetInputState();
                
                if (movement != null)
                {
                    movement.IsThrusting = false;
                    movement.IsBoosting = false;
                    movement.IsBraking = true; // Auto-brake in menus
                    movement.TurnInput = 0f;
                }
                return;
            }

            UpdateInput();

            if (movement != null)
            {
                movement.ApplyInput(this);
            }

            if (skillAction != null && skillAction.WasPressedThisFrame())
            {
                TriggerSkill(SkillSlotType.Active1);
            }

            ProcessFiring(Time.deltaTime);
        }

        public void TriggerSkill(SkillSlotType slot = SkillSlotType.Active1)
        {
            if (skillController != null)
            {
                skillController.TryCastActiveSkill(slot);
            }
        }

        public void ResetInputState()
        {
            inputThrust = false;
            inputBrake = false;
            inputBoost = false;
            inputFire = false;
            inputTurn = 0f;
            virtualThrust = false;
            virtualBrake = false;
            virtualBoost = false;
            virtualFire = false;
            wasFirePressedLastFrame = false;
            fireHeldDuration = 0f;
            hasAutoFired = false;
        }

        public void ProcessFiring(float dt)
        {
            IWeapon currentWeapon = ActiveWeapon;
            bool isFireTriggered = false;
            if (inputFire)
            {
                if (!wasFirePressedLastFrame)
                {
                    // Tap / Initial press: immediately fire exactly 1 shot
                    isFireTriggered = true;
                    fireHeldDuration = 0f;
                    hasAutoFired = false;
                }
                else
                {
                    // Button held down: advance hold timer
                    fireHeldDuration += dt;
                    float minRepeat = (currentWeapon is ShipWeapon sw && sw.combatConfig != null) ? sw.combatConfig.minFireRate : 0.05f;
                    float repeatInterval = (weaponController != null && weaponController.HasMultiSocketLoadout)
                        ? weaponController.GetFastestFireRate()
                        : (currentWeapon != null ? Mathf.Max(minRepeat, currentWeapon.FireRate) : 0.22f);
                    float requiredTime = hasAutoFired ? repeatInterval : Mathf.Max(0.12f, repeatInterval);
                    if (autoFireEnabled && fireHeldDuration >= requiredTime)
                    {
                        isFireTriggered = true;
                        hasAutoFired = true;
                    }
                }
            }
            else
            {
                fireHeldDuration = 0f;
                hasAutoFired = false;
                if (weaponController != null && weaponController.HasMultiSocketLoadout)
                {
                    weaponController.StopFiringAllSockets();
                }
                else
                {
                    currentWeapon?.StopFiring();
                }
            }
            wasFirePressedLastFrame = inputFire;

            if (isFireTriggered)
            {
                bool fired = false;
                if (weaponController != null && weaponController.HasMultiSocketLoadout)
                {
                    fired = weaponController.TryFireSockets(health);
                }
                else if (currentWeapon != null)
                {
                    fired = currentWeapon.TryFire(health);
                }

                if (fired)
                {
                    fireHeldDuration = 0f; // Reset repeat timer after each successful shot
                }
            }
        }

        private void UpdateInput()
        {
            Keyboard kb = Keyboard.current;
            Mouse mouse = Mouse.current;
            Gamepad pad = Gamepad.current;

            if (weaponController != null)
            {
                if (kb != null)
                {
                    if (kb.digit1Key.wasPressedThisFrame) weaponController.SwitchWeapon(0);
                    else if (kb.digit2Key.wasPressedThisFrame) weaponController.SwitchWeapon(1);
                    else if (kb.digit3Key.wasPressedThisFrame) weaponController.SwitchWeapon(2);
                    else if (kb.digit4Key.wasPressedThisFrame) weaponController.SwitchWeapon(3);
                    else if (kb.digit5Key.wasPressedThisFrame) weaponController.SwitchWeapon(4);
                }

                if (mouse != null)
                {
                    float scroll = mouse.scroll.ReadValue().y;
                    if (scroll > 0.5f) weaponController.PreviousWeapon();
                    else if (scroll < -0.5f) weaponController.NextWeapon();
                }

                if (pad != null)
                {
                    if (pad.dpad.left.wasPressedThisFrame) weaponController.PreviousWeapon();
                    else if (pad.dpad.right.wasPressedThisFrame) weaponController.NextWeapon();
                }
            }

            bool keyW = kb != null && (kb.wKey.isPressed || kb.upArrowKey.isPressed);
            bool keyS = kb != null && (kb.sKey.isPressed || kb.downArrowKey.isPressed);
            bool keyA = kb != null && (kb.aKey.isPressed || kb.leftArrowKey.isPressed);
            bool keyD = kb != null && (kb.dKey.isPressed || kb.rightArrowKey.isPressed);
            bool keySpace = kb != null && kb.spaceKey.isPressed;
            bool keyShift = kb != null && (kb.leftShiftKey.isPressed || kb.rightShiftKey.isPressed);

            bool isPointerOverUI = UnityEngine.EventSystems.EventSystem.current != null && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
            bool mouseLeft = mouse != null && mouse.leftButton.isPressed && !isPointerOverUI;
            bool mouseRight = mouse != null && mouse.rightButton.isPressed && !isPointerOverUI;

            Vector2 moveVal = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
            float padY = pad != null ? pad.leftStick.y.ReadValue() : 0f;
            float padX = pad != null ? pad.leftStick.x.ReadValue() : 0f;
            bool padFire = pad != null && (pad.rightTrigger.isPressed || pad.buttonWest.isPressed);
            bool padBoost = pad != null && (pad.buttonSouth.isPressed || pad.leftShoulder.isPressed);

            bool actionFire = (attackAction != null && attackAction.IsPressed()) || keySpace || mouseLeft || padFire;
            bool actionSprint = (sprintAction != null && sprintAction.IsPressed()) || keyShift || mouseRight || padBoost;

            bool hasThrust = virtualThrust || keyW || moveVal.y > 0.15f || padY > 0.15f;
            inputThrust = hasThrust;

            if (autoBrakeEnabled && !hasThrust && !actionSprint && !virtualBoost)
            {
                inputBrake = true;
            }
            else
            {
                inputBrake = virtualBrake || keyS || moveVal.y < -0.15f || padY < -0.15f;
            }

            inputBoost = virtualBoost || actionSprint;
            inputFire = virtualFire || actionFire;

            // Turn input: +1 for counter-clockwise / Left, -1 for clockwise / Right
            float turnMult = invertTurn ? -1f : 1f;
            float turn = 0f;
            if (keyA) turn += 1f * turnMult;
            if (keyD) turn -= 1f * turnMult;
            inputTurn = turn;

            // Aim direction resolution: Virtual -> Gamepad Right Stick -> Mouse Cursor -> Gamepad Left Stick
            if (aimMode == 1) // Keyboard Turn only (Classic Asteroids style)
            {
                inputAimDirection = Vector2.zero;
                hasDirectAim = false;
                if (Mathf.Abs(padX) > 0.15f)
                {
                    inputTurn = -padX * turnMult;
                }
            }
            else // Mouse Direct / Stick Aim (Modern Twin-Stick / Arena style)
            {
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
                    inputTurn = -padX * turnMult;
                }
            }
        }
    }
}
