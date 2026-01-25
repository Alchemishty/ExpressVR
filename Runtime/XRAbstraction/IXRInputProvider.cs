using UnityEngine;

namespace VREmoteWheel
{
    /// <summary>
    /// Interface for XR input abstraction.
    /// Implement this interface to support different XR platforms.
    /// </summary>
    public interface IXRInputProvider
    {
        /// <summary>
        /// Whether the thumbstick is currently being touched.
        /// </summary>
        bool IsThumbstickTouched { get; }

        /// <summary>
        /// The current thumbstick position as a 2D vector.
        /// X: horizontal (-1 left, +1 right)
        /// Y: vertical (-1 down, +1 up)
        /// </summary>
        Vector2 ThumbstickValue { get; }

        /// <summary>
        /// Whether the input provider has been successfully initialized.
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Initialize the input provider.
        /// Call this before using other properties.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Update the input state. Call this once per frame.
        /// </summary>
        void UpdateInput();
    }
}
