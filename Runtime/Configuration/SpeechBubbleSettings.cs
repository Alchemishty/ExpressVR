using UnityEngine;

namespace VREmoteWheel
{
    /// <summary>
    /// Configuration ScriptableObject for speech bubble appearance and animation.
    /// </summary>
    [CreateAssetMenu(fileName = "SpeechBubbleSettings", menuName = "VREmoteWheel/Speech Bubble Settings")]
    public class SpeechBubbleSettings : ScriptableObject
    {
        [Header("Animation")]
        [Tooltip("Duration of the pop animation in seconds")]
        public float popDuration = 2f;

        [Tooltip("Maximum stretch amount during pop animation (multiplier)")]
        [Range(1f, 2f)]
        public float stretchAmount = 1.4f;

        [Tooltip("Minimum squash amount during pop animation (multiplier)")]
        [Range(0.5f, 1f)]
        public float squashAmount = 0.8f;

        [Header("Display")]
        [Tooltip("How long the bubble stays visible before auto-hiding (seconds)")]
        public float displayDuration = 5f;

        [Tooltip("Scale of the speech bubble")]
        public Vector3 bubbleScale = new Vector3(1.5f, 1.5f, 1.5f);

        [Tooltip("Position offset from the anchor point (typically above avatar head)")]
        public Vector3 positionOffset = Vector3.up * 0.3f;

        [Header("Appearance")]
        [Tooltip("Background sprite for the speech bubble")]
        public Sprite bubbleBackground;

        [Tooltip("Tint color applied to the bubble")]
        public Color bubbleTint = Color.white;

        [Tooltip("Whether to use sprite-based emotes (true) or material-based (false)")]
        public bool useSpriteEmotes = false;

        /// <summary>
        /// Creates a copy of these settings that can be modified at runtime.
        /// </summary>
        public SpeechBubbleSettings CreateRuntimeCopy()
        {
            var copy = CreateInstance<SpeechBubbleSettings>();
            copy.popDuration = popDuration;
            copy.stretchAmount = stretchAmount;
            copy.squashAmount = squashAmount;
            copy.displayDuration = displayDuration;
            copy.bubbleScale = bubbleScale;
            copy.positionOffset = positionOffset;
            copy.bubbleBackground = bubbleBackground;
            copy.bubbleTint = bubbleTint;
            copy.useSpriteEmotes = useSpriteEmotes;
            return copy;
        }
    }
}
