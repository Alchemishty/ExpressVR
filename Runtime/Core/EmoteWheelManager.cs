using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace VREmoteWheel
{
    /// <summary>
    /// Central manager for the emote wheel system with networking support.
    /// Handles registration of speech bubbles and broadcasting emotes across the network.
    /// </summary>
    public class EmoteWheelManager : NetworkBehaviour
    {
        public static EmoteWheelManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private EmoteWheelConfig config;
        [SerializeField] private SpeechBubbleSettings bubbleSettings;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;

        /// <summary>
        /// Registry of speech bubbles by client ID.
        /// </summary>
        private Dictionary<ulong, NetworkedSpeechBubble> speechBubbles = new Dictionary<ulong, NetworkedSpeechBubble>();

        /// <summary>
        /// Fired when the local player selects an emote.
        /// </summary>
        public event Action<int> OnEmoteSelected;

        /// <summary>
        /// Fired on all clients when any player broadcasts an emote.
        /// Parameters: (clientId, emoteIndex)
        /// </summary>
        public event Action<ulong, int> OnEmoteBroadcast;

        /// <summary>
        /// Gets the current emote wheel configuration.
        /// </summary>
        public EmoteWheelConfig Config => config;

        /// <summary>
        /// Gets the current speech bubble settings.
        /// </summary>
        public SpeechBubbleSettings BubbleSettings => bubbleSettings;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[VREmoteWheel] Multiple EmoteWheelManager instances detected. Destroying duplicate.");
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (config != null)
            {
                config.Validate();
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// Registers a speech bubble for a specific client.
        /// Called automatically by NetworkedSpeechBubble on spawn.
        /// </summary>
        public void RegisterSpeechBubble(ulong clientId, NetworkedSpeechBubble bubble)
        {
            if (bubble == null)
            {
                Debug.LogError($"[VREmoteWheel] Cannot register null speech bubble for client {clientId}");
                return;
            }

            if (speechBubbles.ContainsKey(clientId))
            {
                Debug.LogWarning($"[VREmoteWheel] Replacing existing speech bubble for client {clientId}");
            }

            speechBubbles[clientId] = bubble;
            Debug.Log($"[VREmoteWheel] Registered speech bubble for client {clientId}");
        }

        /// <summary>
        /// Unregisters a speech bubble for a specific client.
        /// Called automatically by NetworkedSpeechBubble on despawn.
        /// </summary>
        public void UnregisterSpeechBubble(ulong clientId)
        {
            if (speechBubbles.Remove(clientId))
            {
                Debug.Log($"[VREmoteWheel] Unregistered speech bubble for client {clientId}");
            }
        }

        /// <summary>
        /// Gets the speech bubble for a specific client.
        /// </summary>
        public NetworkedSpeechBubble GetSpeechBubble(ulong clientId)
        {
            speechBubbles.TryGetValue(clientId, out var bubble);
            return bubble;
        }

        /// <summary>
        /// Called by EmoteWheelInput when the local player selects an emote.
        /// Triggers the network broadcast.
        /// </summary>
        public void SelectEmote(int emoteIndex)
        {
            if (config == null)
            {
                Debug.LogError("[VREmoteWheel] No EmoteWheelConfig assigned to EmoteWheelManager");
                return;
            }

            if (emoteIndex < 0 || emoteIndex >= config.EmoteCount)
            {
                Debug.LogError($"[VREmoteWheel] Invalid emote index: {emoteIndex}");
                return;
            }

            // Fire local event
            OnEmoteSelected?.Invoke(emoteIndex);

            // Play sound locally
            PlayEmoteSound(emoteIndex);

            // Send to server for broadcast
            SendEmoteServerRpc(emoteIndex);
        }

        private void PlayEmoteSound(int emoteIndex)
        {
            if (audioSource == null || config == null) return;

            var emote = config.GetEmote(emoteIndex);
            if (emote?.emoteSound != null)
            {
                audioSource.PlayOneShot(emote.emoteSound);
            }
        }

        /// <summary>
        /// Server RPC to broadcast an emote selection to all clients.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        private void SendEmoteServerRpc(int emoteIndex, ServerRpcParams rpcParams = default)
        {
            ulong senderClientId = rpcParams.Receive.SenderClientId;
            BroadcastEmoteClientRpc(senderClientId, emoteIndex);
        }

        /// <summary>
        /// Client RPC to update all clients with the selected emote.
        /// </summary>
        [ClientRpc]
        private void BroadcastEmoteClientRpc(ulong clientId, int emoteIndex)
        {
            // Fire event for custom handling
            OnEmoteBroadcast?.Invoke(clientId, emoteIndex);

            // Update the speech bubble if registered
            if (speechBubbles.TryGetValue(clientId, out var bubble) && bubble != null)
            {
                bubble.ShowEmote(emoteIndex);
            }
        }

        /// <summary>
        /// Manually trigger an emote display for a specific client (local only, no network).
        /// Useful for testing or local-only effects.
        /// </summary>
        public void ShowEmoteLocal(ulong clientId, int emoteIndex)
        {
            if (speechBubbles.TryGetValue(clientId, out var bubble) && bubble != null)
            {
                bubble.ShowEmote(emoteIndex);
            }
        }
    }
}
