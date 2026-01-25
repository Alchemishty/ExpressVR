using UnityEngine;

namespace VREmoteWheel
{
    /// <summary>
    /// Meta/Oculus SDK implementation of IXRInputProvider.
    /// This class is only compiled when Meta XR SDK is installed.
    /// </summary>
    public class OVRInputProvider : IXRInputProvider
    {
        private bool _isInitialized;
        private bool _isThumbstickTouched;
        private Vector2 _thumbstickValue;

        /// <summary>
        /// Which hand's controller to use for input.
        /// </summary>
        public enum ControllerHand
        {
            Left,
            Right
        }

        private ControllerHand _hand;

        public OVRInputProvider(ControllerHand hand = ControllerHand.Left)
        {
            _hand = hand;
        }

        public bool IsThumbstickTouched => _isThumbstickTouched;

        public Vector2 ThumbstickValue => _thumbstickValue;

        public bool IsInitialized => _isInitialized;

        public void Initialize()
        {
            _isInitialized = true;
        }

        public void UpdateInput()
        {
            if (!_isInitialized) return;

            if (_hand == ControllerHand.Left)
            {
                _isThumbstickTouched = OVRInput.Get(OVRInput.Touch.PrimaryThumbstick);
                _thumbstickValue = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
            }
            else
            {
                _isThumbstickTouched = OVRInput.Get(OVRInput.Touch.SecondaryThumbstick);
                _thumbstickValue = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
            }
        }
    }
}
