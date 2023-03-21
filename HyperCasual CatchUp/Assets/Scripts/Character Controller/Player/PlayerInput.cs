using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Fallencake.CharacterController
{
    public class PlayerInput : MonoBehaviour
    {
        public InputData InputData { get => GetInputData(); }

#if ENABLE_INPUT_SYSTEM
    private PlayerInputActions _actions;
    private InputAction _move, _jump, _dash, _attack, _look;

        private void Awake()
        {
            _actions = new PlayerInputActions();
            _move = _actions.Player.Move;
            _jump = _actions.Player.Jump;
            _dash = _actions.Player.Dash;
            _attack = _actions.Player.Attack;
            _look = _actions.Player.Look;
        }

        private void OnEnable() => _actions.Enable();

        private void OnDisable() => _actions.Disable();

        private InputData GetInputData() {
        return new InputData {
            JumpDown = _jump.WasPressedThisFrame(),
            JumpHeld = _jump.IsPressed(),
            DashDown = _dash.WasPressedThisFrame(),
            AttackDown = _attack.WasPressedThisFrame(),
            Move = _move.ReadValue<Vector2>(),
            Look = _look.ReadValue<Vector2>()
        };
    }
#elif ENABLE_LEGACY_INPUT_MANAGER
        private InputData GetInputData()
        {
            return new InputData
            {
                JumpDown = Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.C),
                JumpHeld = Input.GetButton("Jump") || Input.GetKey(KeyCode.C),
                DashDown = Input.GetKeyDown(KeyCode.X),
                AttackDown = Input.GetKeyDown(KeyCode.Z),
                Move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")),
                Look = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"))
            };
        }
#endif
    }

    public struct InputData
    {
        public Vector3 Move;
        public Vector2 Look;
        public bool JumpDown;
        public bool JumpHeld;
        public bool DashDown;
        public bool AttackDown;
    }
}