using System.Collections.Generic;
using UnityEngine;

namespace VREmoteWheel
{
    /// <summary>
    /// Main configuration ScriptableObject for the emote wheel system.
    /// </summary>
    [CreateAssetMenu(fileName = "EmoteWheelConfig", menuName = "VREmoteWheel/Configuration")]
    public class EmoteWheelConfig : ScriptableObject
    {
        [Header("Emotes")]
        [Tooltip("List of emotes available in the wheel. Supports 4-12 emotes.")]
        public List<EmoteData> emotes = new List<EmoteData>();

        [Header("Input")]
        [Tooltip("Minimum joystick magnitude to register a selection")]
        [Range(0.1f, 0.9f)]
        public float joystickThreshold = 0.5f;

        [Tooltip("Require thumbstick touch to open menu (false = always check joystick position)")]
        public bool requireThumbstickTouch = true;

        [Header("Menu Appearance")]
        [Tooltip("Radius of the radial menu layout in UI units")]
        public float menuRadius = 100f;

        [Tooltip("Scale of unselected menu items")]
        [Range(0.1f, 1.0f)]
        public float itemScale = 0.6f;

        [Tooltip("Scale of the selected menu item")]
        [Range(0.1f, 1.5f)]
        public float selectedItemScale = 0.7f;

        [Header("Animation")]
        [Tooltip("Duration of the selection animation in seconds")]
        public float selectionAnimDuration = 0.2f;

        [Tooltip("Alpha of unselected items")]
        [Range(0f, 1f)]
        public float unselectedAlpha = 0.3f;

        [Tooltip("Alpha of the selected item")]
        [Range(0f, 1f)]
        public float selectedAlpha = 1.0f;

        /// <summary>
        /// Gets the number of emotes configured.
        /// </summary>
        public int EmoteCount => emotes?.Count ?? 0;

        /// <summary>
        /// Gets the angle in degrees between each emote in the wheel.
        /// </summary>
        public float AngleBetweenEmotes => EmoteCount > 0 ? 360f / EmoteCount : 0f;

        /// <summary>
        /// Validates the configuration and logs warnings for issues.
        /// </summary>
        public void Validate()
        {
            if (EmoteCount < 4)
            {
                Debug.LogWarning($"[VREmoteWheel] EmoteWheelConfig has {EmoteCount} emotes. Minimum recommended is 4.");
            }
            else if (EmoteCount > 12)
            {
                Debug.LogWarning($"[VREmoteWheel] EmoteWheelConfig has {EmoteCount} emotes. Maximum recommended is 12.");
            }

            for (int i = 0; i < EmoteCount; i++)
            {
                if (emotes[i] == null || !emotes[i].IsValid)
                {
                    Debug.LogWarning($"[VREmoteWheel] Emote at index {i} is invalid or missing display data.");
                }
            }
        }

        /// <summary>
        /// Gets the emote data at the specified index.
        /// </summary>
        public EmoteData GetEmote(int index)
        {
            if (index < 0 || index >= EmoteCount)
            {
                Debug.LogError($"[VREmoteWheel] Emote index {index} out of range (0-{EmoteCount - 1})");
                return null;
            }
            return emotes[index];
        }
    }
}
