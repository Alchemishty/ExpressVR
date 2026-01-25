using UnityEngine;

namespace VREmoteWheel
{
    /// <summary>
    /// Data container for a single emote.
    /// </summary>
    [System.Serializable]
    public class EmoteData
    {
        [Tooltip("Display name for this emote")]
        public string emoteName;

        [Tooltip("Sprite for UI display in the emote wheel")]
        public Sprite emoteSprite;

        [Tooltip("Material for 3D speech bubble quad display")]
        public Material emoteMaterial;

        [Tooltip("Optional sound effect to play when emote is selected")]
        public AudioClip emoteSound;

        /// <summary>
        /// Returns true if this emote has valid display data.
        /// </summary>
        public bool IsValid => emoteSprite != null || emoteMaterial != null;
    }
}
