using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.EnhancedTouch;
using ETouch = UnityEngine.InputSystem.EnhancedTouch;

namespace Fallencake.CharacterController
{
    public class PlayerTouchMovement : MonoBehaviour
    {
        [SerializeField] private Vector2 joystickSize = new Vector2(300, 300);
        [SerializeField] private FloatingJoystick joystick;

        private Finger movementFinger;
        private Vector2 movementAmount;
        public Vector2 MovementAmount { get => movementAmount; }

        private void OnEnable()
        {
            EnhancedTouchSupport.Enable();
            ETouch.Touch.onFingerDown += HandleFingerDown;
            ETouch.Touch.onFingerUp += HandleLoseFinger;
            ETouch.Touch.onFingerMove += HandleFingerMove;
        }

        private void OnDisable()
        {
            ETouch.Touch.onFingerDown -= HandleFingerDown;
            ETouch.Touch.onFingerUp -= HandleLoseFinger;
            ETouch.Touch.onFingerMove -= HandleFingerMove;
            EnhancedTouchSupport.Disable();
        }

        private void HandleFingerMove(Finger MovedFinger)
        {
            if (MovedFinger == movementFinger)
            {
                Vector2 knobPosition;
                float maxMovement = joystickSize.x / 2f;
                ETouch.Touch currentTouch = MovedFinger.currentTouch;

                if (Vector2.Distance(
                        currentTouch.screenPosition,
                        joystick.RectTransform.anchoredPosition
                    ) > maxMovement)
                {
                    knobPosition = (
                        currentTouch.screenPosition - joystick.RectTransform.anchoredPosition
                        ).normalized
                        * maxMovement;
                }
                else
                {
                    knobPosition = currentTouch.screenPosition - joystick.RectTransform.anchoredPosition;
                }

                joystick.Knob.anchoredPosition = knobPosition;
                movementAmount = knobPosition / maxMovement;
            }
        }

        private void HandleLoseFinger(Finger LostFinger)
        {
            if (LostFinger == movementFinger)
            {
                movementFinger = null;
                joystick.Knob.anchoredPosition = Vector2.zero;
                joystick.gameObject.SetActive(false);
                movementAmount = Vector2.zero;
            }
        }

        private void HandleFingerDown(Finger TouchedFinger)
        {
            if (movementFinger == null && TouchedFinger.screenPosition.x <= Screen.width / 2f)
            {
                movementFinger = TouchedFinger;
                movementAmount = Vector2.zero;
                joystick.gameObject.SetActive(true);
                joystick.RectTransform.sizeDelta = joystickSize;
                joystick.RectTransform.anchoredPosition = ClampStartPosition(TouchedFinger.screenPosition);
            }
        }

        private Vector2 ClampStartPosition(Vector2 StartPosition)
        {
            if (StartPosition.x < joystickSize.x / 2)
            {
                StartPosition.x = joystickSize.x / 2;
            }
            if (StartPosition.y < joystickSize.y / 2)
            {
                StartPosition.y = joystickSize.y / 2;
            }
            else if (StartPosition.y > Screen.height - joystickSize.y / 2)
            {
                StartPosition.y = Screen.height - joystickSize.y / 2;
            }
            return StartPosition;
        }
    }
}