using System;
using UnityEngine;

namespace VREmoteWheel
{
    /// <summary>
    /// Handles VR controller input for the emote wheel.
    /// Attach this to the same GameObject as EmoteWheelManager or the player.
    /// </summary>
    public class EmoteWheelInput : MonoBehaviour
    {
#if !UNITY_SERVER
        [Header("Configuration")]
        [SerializeField] private EmoteWheelConfig config;

        [Header("UI Reference")]
        [SerializeField] private RadialEmoteMenu radialMenu;

        [Header("Input Provider")]
        [SerializeField] private InputProviderType inputProviderType = InputProviderType.Auto;
        [SerializeField] private ControllerHand controllerHand = ControllerHand.Left;

        public enum InputProviderType
        {
            Auto,
            OVR,
            UnityInputSystem
        }

        public enum ControllerHand
        {
            Left,
            Right
        }

        private IXRInputProvider inputProvider;
        private int currentIndex = -1;
        private bool isMenuOpen = false;

        /// <summary>
        /// Gets or sets whether input is enabled.
        /// </summary>
        public bool InputEnabled { get; set; } = true;

        /// <summary>
        /// Gets the currently selected emote index, or -1 if none.
        /// </summary>
        public int CurrentSelectionIndex => currentIndex;

        /// <summary>
        /// Gets whether the emote menu is currently open.
        /// </summary>
        public bool IsMenuOpen => isMenuOpen;

        private void Start()
        {
            InitializeInputProvider();

            if (config == null && EmoteWheelManager.Instance != null)
            {
                config = EmoteWheelManager.Instance.Config;
            }
        }

        private void InitializeInputProvider()
        {
            switch (inputProviderType)
            {
                case InputProviderType.OVR:
                    inputProvider = TryCreateOVRProvider();
                    if (inputProvider == null)
                    {
                        Debug.LogError("[VREmoteWheel] OVR input provider not available. Is Meta XR SDK installed?");
                    }
                    break;
                case InputProviderType.UnityInputSystem:
                    inputProvider = TryCreateUnityInputProvider();
                    if (inputProvider == null)
                    {
                        Debug.LogError("[VREmoteWheel] Unity Input System provider not available. Is Unity Input System installed?");
                    }
                    break;
                case InputProviderType.Auto:
                default:
                    // Try OVR first, fall back to Unity Input System
                    inputProvider = TryCreateOVRProvider();
                    if (inputProvider == null)
                    {
                        inputProvider = TryCreateUnityInputProvider();
                    }
                    if (inputProvider == null)
                    {
                        Debug.LogError("[VREmoteWheel] No supported input system found. Install Meta XR SDK or Unity Input System.");
                        return;
                    }
                    break;
            }

            inputProvider?.Initialize();

            if (inputProvider == null || !inputProvider.IsInitialized)
            {
                Debug.LogWarning("[VREmoteWheel] Failed to initialize input provider.");
            }
        }

        private IXRInputProvider TryCreateOVRProvider()
        {
            // Use reflection to find OVRInputProvider since it's in a separate assembly
            // that only compiles when Meta XR SDK is installed
            try
            {
                var ovrType = Type.GetType("VREmoteWheel.OVRInputProvider, VREmoteWheel.OVR");
                if (ovrType != null)
                {
                    // Get the nested ControllerHand enum type
                    var handEnumType = ovrType.GetNestedType("ControllerHand");
                    if (handEnumType != null)
                    {
                        var handValue = Enum.ToObject(handEnumType, (int)controllerHand);
                        return (IXRInputProvider)Activator.CreateInstance(ovrType, handValue);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[VREmoteWheel] Failed to create OVR input provider: {e.Message}");
            }
            return null;
        }

        private IXRInputProvider TryCreateUnityInputProvider()
        {
#if UNITY_INPUT_SYSTEM
            return new GenericXRInputProvider((GenericXRInputProvider.ControllerHand)controllerHand);
#else
            return null;
#endif
        }

        private void Update()
        {
            if (!InputEnabled || inputProvider == null || !inputProvider.IsInitialized) return;

            inputProvider.UpdateInput();
            HandleInput();
        }

        private void HandleInput()
        {
            bool isTouchingThumbstick = inputProvider.IsThumbstickTouched;
            bool requireTouch = config != null ? config.requireThumbstickTouch : true;

            // Open menu when thumbstick is touched (if required)
            if (!isMenuOpen && (isTouchingThumbstick || !requireTouch))
            {
                if (isTouchingThumbstick || inputProvider.ThumbstickValue.magnitude > 0.1f)
                {
                    OpenMenu();
                }
            }

            // Close menu when thumbstick is released
            if (isMenuOpen && requireTouch && !isTouchingThumbstick)
            {
                // Confirm selection before closing
                if (currentIndex != -1)
                {
                    ConfirmSelection();
                }
                CloseMenu();
                return;
            }

            if (!isMenuOpen) return;

            // Handle selection based on joystick position
            Vector2 joystickInput = inputProvider.ThumbstickValue;
            float threshold = config != null ? config.joystickThreshold : 0.5f;

            if (joystickInput.magnitude > threshold)
            {
                int newIndex = GetSelectionIndex(joystickInput);
                if (newIndex != currentIndex)
                {
                    UpdateSelection(newIndex);
                }
            }
            else if (currentIndex != -1)
            {
                // Clear selection when joystick returns to center
                ClearSelection();
            }
        }

        private void OpenMenu()
        {
            isMenuOpen = true;
            currentIndex = -1;

            if (radialMenu != null)
            {
                radialMenu.Show();
            }
        }

        private void CloseMenu()
        {
            isMenuOpen = false;
            currentIndex = -1;

            if (radialMenu != null)
            {
                radialMenu.Hide();
            }
        }

        private void UpdateSelection(int newIndex)
        {
            int previousIndex = currentIndex;
            currentIndex = newIndex;

            if (radialMenu != null)
            {
                radialMenu.SetSelection(newIndex, previousIndex);
            }
        }

        private void ClearSelection()
        {
            int previousIndex = currentIndex;
            currentIndex = -1;

            if (radialMenu != null && previousIndex != -1)
            {
                radialMenu.ClearSelection(previousIndex);
            }
        }

        private void ConfirmSelection()
        {
            if (currentIndex == -1) return;

            if (EmoteWheelManager.Instance != null)
            {
                EmoteWheelManager.Instance.SelectEmote(currentIndex);
            }
        }

        /// <summary>
        /// Calculates the selection index based on joystick angle.
        /// Supports dynamic emote counts.
        /// </summary>
        private int GetSelectionIndex(Vector2 joystickInput)
        {
            int emoteCount = config != null ? config.EmoteCount : 8;
            if (emoteCount <= 0) return -1;

            // Calculate angle from joystick input
            float angle = Mathf.Atan2(joystickInput.y, joystickInput.x) * Mathf.Rad2Deg;
            angle = (angle + 360f) % 360f;

            // Calculate segment size based on emote count
            float segmentSize = 360f / emoteCount;

            // Offset angle so "up" (90 degrees) maps to index 0
            // Adjust by half segment to center selections
            float adjustedAngle = (angle - 90f + segmentSize / 2f + 360f) % 360f;

            // Calculate index (clockwise from top)
            int index = Mathf.FloorToInt(adjustedAngle / segmentSize);
            index = (emoteCount - index) % emoteCount; // Reverse for counter-clockwise selection

            return Mathf.Clamp(index, 0, emoteCount - 1);
        }

        private void OnDestroy()
        {
            if (inputProvider is GenericXRInputProvider genericProvider)
            {
                genericProvider.Dispose();
            }
        }
#endif
    }
}
