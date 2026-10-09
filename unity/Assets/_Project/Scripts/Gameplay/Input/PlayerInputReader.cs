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

        [Header("Input State (Multiplatform)")]
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
            var kb = Keyboard.current;
            var mouse = Mouse.current;
            var pad = Gamepad.current;

            bool keyW = kb != null && kb.wKey.isPressed;
            bool keyS = kb != null && kb.sKey.isPressed;
            bool keyShift = kb != null && kb.shiftKey.isPressed;
            bool keySpace = kb != null && kb.spaceKey.isPressed;
            bool mouseLeft = mouse != null && mouse.leftButton.isPressed;
            bool mouseRight = mouse != null && mouse.rightButton.isPressed;

            bool padThrust = pad != null && pad.rightTrigger.isPressed;
            bool padBrake = pad != null && pad.leftTrigger.isPressed;
            bool padBoost = pad != null && (pad.buttonSouth.isPressed || pad.leftShoulder.isPressed);
            bool padFire = pad != null && (pad.rightShoulder.isPressed || pad.buttonWest.isPressed);

            inputThrust = virtualThrust || keyW || padThrust;
            inputBrake = virtualBrake || keyS || padBrake;
            inputBoost = virtualBoost || keyShift || mouseRight || padBoost;
            inputFire = virtualFire || keySpace || mouseLeft || padFire;

            if (hasVirtualAim)
            {
                inputAimDirection = virtualAimDirection;
                hasDirectAim = true;
            }
            else if (pad != null && pad.leftStick.ReadValue().sqrMagnitude > 0.1f)
            {
                inputAimDirection = pad.leftStick.ReadValue().normalized;
                hasDirectAim = true;
            }
            else if (mouse != null && Camera.main != null)
            {
                Vector2 mousePos = mouse.position.ReadValue();
                Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 10f));
                Vector2 diff = (Vector2)mouseWorld - (Vector2)transform.position;
                if (diff.sqrMagnitude > 0.05f)
                {
                    inputAimDirection = diff.normalized;
                }
                hasDirectAim = false;
            }
        }
    }
}
