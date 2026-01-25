using UnityEngine;

#if UNITY_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
#endif

namespace VREmoteWheel
{
    /// <summary>
    /// Generic XR input provider using Unity's Input System.
    /// Works with any XR device that supports the Unity Input System.
    /// </summary>
    public class GenericXRInputProvider : IXRInputProvider
    {
        private bool _isInitialized;
        private bool _isThumbstickTouched;
        private Vector2 _thumbstickValue;

#if UNITY_INPUT_SYSTEM
        private InputAction _thumbstickAction;
        private InputAction _thumbstickTouchAction;
#endif

        /// <summary>
        /// Which hand's controller to use for input.
        /// </summary>
        public enum ControllerHand
        {
            Left,
            Right
        }

        private ControllerHand _hand;

        public GenericXRInputProvider(ControllerHand hand = ControllerHand.Left)
        {
            _hand = hand;
        }

        public bool IsThumbstickTouched => _isThumbstickTouched;

        public Vector2 ThumbstickValue => _thumbstickValue;

        public bool IsInitialized => _isInitialized;

        public void Initialize()
        {
#if UNITY_INPUT_SYSTEM
            string handPath = _hand == ControllerHand.Left ? "<XRController>{LeftHand}" : "<XRController>{RightHand}";

            _thumbstickAction = new InputAction(
                name: "Thumbstick",
                type: InputActionType.Value,
                binding: $"{handPath}/thumbstick"
            );
            _thumbstickAction.Enable();

            _thumbstickTouchAction = new InputAction(
                name: "ThumbstickTouch",
                type: InputActionType.Button,
                binding: $"{handPath}/thumbstickTouched"
            );
            _thumbstickTouchAction.Enable();

            _isInitialized = true;
#else
            Debug.LogWarning("[VREmoteWheel] Unity Input System not found. GenericXRInputProvider will not function.");
            _isInitialized = false;
#endif
        }

        public void UpdateInput()
        {
#if UNITY_INPUT_SYSTEM
            if (!_isInitialized) return;

            _thumbstickValue = _thumbstickAction.ReadValue<Vector2>();
            _isThumbstickTouched = _thumbstickTouchAction.IsPressed();

            // Fallback: if touch detection isn't working, use magnitude threshold
            if (!_isThumbstickTouched && _thumbstickValue.magnitude > 0.1f)
            {
                _isThumbstickTouched = true;
            }
#endif
        }

        public void Dispose()
        {
#if UNITY_INPUT_SYSTEM
            _thumbstickAction?.Disable();
            _thumbstickAction?.Dispose();
            _thumbstickTouchAction?.Disable();
            _thumbstickTouchAction?.Dispose();
#endif
        }
    }
}
