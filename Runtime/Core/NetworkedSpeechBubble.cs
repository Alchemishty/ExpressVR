using System;
using System.Collections;
using UnityEngine;
using Unity.Netcode;

namespace VREmoteWheel
{
    /// <summary>
    /// Networked speech bubble component that displays emotes above avatars.
    /// Attach as a child of the avatar prefab.
    /// </summary>
    public class NetworkedSpeechBubble : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject bubbleRoot;
        [SerializeField] private Renderer emoteRenderer;
        [SerializeField] private SpriteRenderer emoteSprite;

        [Header("Settings Override")]
        [SerializeField] private SpeechBubbleSettings settingsOverride;

        [Header("Owner Info")]
        [SerializeField] private NetworkObject parentNetworkObject;

        private SpeechBubbleSettings settings;
        private Vector3 originalScale;
        private Coroutine hideCoroutine;
        private Coroutine popCoroutine;

        /// <summary>
        /// Fired when the bubble is shown.
        /// </summary>
        public event Action OnBubbleShown;

        /// <summary>
        /// Fired when the bubble is hidden.
        /// </summary>
        public event Action OnBubbleHidden;

        /// <summary>
        /// Gets the client ID of the owner of this bubble.
        /// </summary>
        public ulong OwnerClientId
        {
            get
            {
                if (parentNetworkObject != null)
                {
                    return parentNetworkObject.OwnerClientId;
                }
                return ulong.MaxValue;
            }
        }

        private void Awake()
        {
            // Try to find NetworkObject in parent hierarchy
            if (parentNetworkObject == null)
            {
                parentNetworkObject = GetComponentInParent<NetworkObject>();
            }

            // Use override settings or get from manager
            settings = settingsOverride;

            if (bubbleRoot != null)
            {
                bubbleRoot.SetActive(false);
            }
        }

        private void Start()
        {
            // Get settings from manager if not overridden
            if (settings == null && EmoteWheelManager.Instance != null)
            {
                settings = EmoteWheelManager.Instance.BubbleSettings;
            }

            // Apply position offset
            if (settings != null)
            {
                transform.localPosition = settings.positionOffset;
            }

            // Register with manager
            RegisterWithManager();
        }

        private void OnEnable()
        {
            RegisterWithManager();
        }

        private void OnDisable()
        {
            UnregisterFromManager();
        }

        private void OnDestroy()
        {
            UnregisterFromManager();
        }

        private void RegisterWithManager()
        {
            if (EmoteWheelManager.Instance != null && parentNetworkObject != null)
            {
                EmoteWheelManager.Instance.RegisterSpeechBubble(OwnerClientId, this);
            }
        }

        private void UnregisterFromManager()
        {
            if (EmoteWheelManager.Instance != null && parentNetworkObject != null)
            {
                EmoteWheelManager.Instance.UnregisterSpeechBubble(OwnerClientId);
            }
        }

        /// <summary>
        /// Shows the speech bubble with the specified emote.
        /// </summary>
        public void ShowEmote(int emoteIndex)
        {
            if (EmoteWheelManager.Instance == null || EmoteWheelManager.Instance.Config == null)
            {
                Debug.LogError("[VREmoteWheel] Cannot show emote: EmoteWheelManager or Config not available");
                return;
            }

            var emoteData = EmoteWheelManager.Instance.Config.GetEmote(emoteIndex);
            if (emoteData == null)
            {
                Debug.LogError($"[VREmoteWheel] Invalid emote index: {emoteIndex}");
                return;
            }

            ShowEmote(emoteData);
        }

        /// <summary>
        /// Shows the speech bubble with the specified emote data.
        /// </summary>
        public void ShowEmote(EmoteData emoteData)
        {
            if (bubbleRoot == null)
            {
                Debug.LogError("[VREmoteWheel] BubbleRoot not assigned on NetworkedSpeechBubble");
                return;
            }

            if (settings == null)
            {
                settings = EmoteWheelManager.Instance?.BubbleSettings;
                if (settings == null)
                {
                    Debug.LogWarning("[VREmoteWheel] No SpeechBubbleSettings available, using defaults");
                    settings = ScriptableObject.CreateInstance<SpeechBubbleSettings>();
                }
            }

            // Show bubble
            bubbleRoot.SetActive(true);
            originalScale = settings.bubbleScale;

            // Set emote display
            if (settings.useSpriteEmotes && emoteSprite != null && emoteData.emoteSprite != null)
            {
                emoteSprite.sprite = emoteData.emoteSprite;
            }
            else if (emoteRenderer != null && emoteData.emoteMaterial != null)
            {
                emoteRenderer.material = emoteData.emoteMaterial;
            }

            // Play pop animation
            if (popCoroutine != null)
            {
                StopCoroutine(popCoroutine);
            }
            popCoroutine = StartCoroutine(PlayPopAnimation());

            // Schedule auto-hide
            if (hideCoroutine != null)
            {
                StopCoroutine(hideCoroutine);
            }
            hideCoroutine = StartCoroutine(HideAfterDelay(settings.displayDuration));

            OnBubbleShown?.Invoke();
        }

        /// <summary>
        /// Immediately hides the speech bubble.
        /// </summary>
        public void Hide()
        {
            if (hideCoroutine != null)
            {
                StopCoroutine(hideCoroutine);
                hideCoroutine = null;
            }

            if (popCoroutine != null)
            {
                StopCoroutine(popCoroutine);
                popCoroutine = null;
            }

            if (bubbleRoot != null)
            {
                bubbleRoot.SetActive(false);
            }

            OnBubbleHidden?.Invoke();
        }

        private IEnumerator HideAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            Hide();
        }

        private IEnumerator PlayPopAnimation()
        {
            if (bubbleRoot == null || settings == null) yield break;

            float frameDuration = settings.popDuration / 2f;
            Transform bubbleTransform = bubbleRoot.transform;

            // Phase 1: Stretch Y from squash to stretch
            float elapsed = 0f;
            while (elapsed < frameDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / frameDuration;

                float x = originalScale.x;
                float y = Mathf.Lerp(originalScale.y * settings.squashAmount, originalScale.y * settings.stretchAmount, progress);
                float z = originalScale.z;

                bubbleTransform.localScale = new Vector3(x, y, z);
                yield return null;
            }

            // Phase 2: Return to normal
            elapsed = 0f;
            float returnDuration = frameDuration / 2f;
            while (elapsed < returnDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / returnDuration;

                float x = originalScale.x;
                float y = Mathf.Lerp(originalScale.y * settings.stretchAmount, originalScale.y, progress);
                float z = originalScale.z;

                bubbleTransform.localScale = new Vector3(x, y, z);
                yield return null;
            }

            // Ensure final scale is correct
            bubbleTransform.localScale = originalScale;
            popCoroutine = null;
        }
    }
}
