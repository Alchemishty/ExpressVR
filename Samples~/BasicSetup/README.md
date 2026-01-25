# VR Emote Wheel - Basic Setup Guide

## Overview

VR Emote Wheel is a self-contained Unity module for VR joystick-controlled emote wheels with networked speech bubbles. It works with Unity Netcode for GameObjects (NGO) and supports Meta/Oculus SDK or Unity Input System.

## Requirements

- Unity 2022.3 LTS or later
- Netcode for GameObjects 1.5+
- Meta XR SDK (optional, for Oculus/Meta devices)
- Unity Input System (optional, for non-Oculus VR devices)

## Quick Start

### 1. Create Configuration Assets

1. Right-click in Project window
2. Create > VREmoteWheel > Configuration
3. Create > VREmoteWheel > Speech Bubble Settings

### 2. Configure Emotes

In your EmoteWheelConfig asset:

1. Add emotes to the list (4-12 recommended)
2. For each emote, assign:
   - **Emote Name**: Display name
   - **Emote Sprite**: 2D sprite for menu UI
   - **Emote Material**: Material for 3D speech bubble (with emoji texture)
   - **Emote Sound** (optional): Sound effect when selected

3. Adjust settings:
   - **Joystick Threshold**: How far to push the stick (0.5 = halfway)
   - **Menu Radius**: Size of the radial menu
   - **Animation Duration**: Selection animation speed

### 3. Set Up EmoteWheelManager

1. Create empty GameObject named "EmoteWheelManager"
2. Add `NetworkObject` component
3. Add `EmoteWheelManager` component
4. Add `EmoteWheelInput` component
5. Assign your configuration assets

```
EmoteWheelManager (GameObject)
├── NetworkObject
├── EmoteWheelManager (script)
│   ├── Config: [Your EmoteWheelConfig]
│   └── Bubble Settings: [Your SpeechBubbleSettings]
└── EmoteWheelInput (script)
    └── Config: [Your EmoteWheelConfig]
```

### 4. Create Radial Menu UI

1. Create World Space Canvas as child of your VR camera/controller
2. Add `RadialEmoteMenu` component
3. Create an EmoteMenuItem template:
   - Image component with emote sprite
   - `EmoteMenuItem` component
4. Assign template to RadialEmoteMenu

### 5. Add Speech Bubble to Avatar

1. Add empty child GameObject to your networked avatar prefab
2. Position it above the head
3. Add `NetworkedSpeechBubble` component
4. Create child structure:

```
SpeechBubble (GameObject)
├── NetworkedSpeechBubble (script)
├── BubbleQuad (child) - Background quad with bubble sprite/material
└── EmoteQuad (child) - Quad that displays the emote
```

5. Assign references in the inspector

### 6. Register Avatar with Manager

In your avatar spawn code, the `NetworkedSpeechBubble` will automatically register itself with `EmoteWheelManager` when spawned.

## Input Configuration

### Meta/Oculus SDK

Set `Input Provider Type` to `OVR` on EmoteWheelInput. Choose left or right controller.

### Unity Input System

Set `Input Provider Type` to `UnityInputSystem`. Requires Input System package.

### Auto Detection

Set to `Auto` to automatically detect available input system.

## Events

Subscribe to events for custom behavior:

```csharp
// When local player selects an emote
EmoteWheelManager.Instance.OnEmoteSelected += (emoteIndex) => {
    Debug.Log($"Selected emote {emoteIndex}");
};

// When any player broadcasts an emote (on all clients)
EmoteWheelManager.Instance.OnEmoteBroadcast += (clientId, emoteIndex) => {
    Debug.Log($"Client {clientId} showed emote {emoteIndex}");
};
```

## Customization

### Custom Input Provider

Implement `IXRInputProvider` for other VR platforms:

```csharp
public class MyCustomInputProvider : IXRInputProvider
{
    public bool IsThumbstickTouched { get; private set; }
    public Vector2 ThumbstickValue { get; private set; }
    public bool IsInitialized { get; private set; }

    public void Initialize() { /* Setup code */ }
    public void UpdateInput() { /* Read input each frame */ }
}
```

### Runtime Configuration

```csharp
// Change settings at runtime
var config = EmoteWheelManager.Instance.Config;
config.joystickThreshold = 0.3f;

// Reinitialize menu with new config
radialMenu.Reinitialize(newConfig);
```

## Network Flow

```
1. Player touches thumbstick → Menu opens
2. Player moves thumbstick → Selection highlights
3. Player releases thumbstick → Emote confirmed
4. EmoteWheelInput → EmoteWheelManager.SelectEmote()
5. EmoteWheelManager → SendEmoteServerRpc()
6. Server → BroadcastEmoteClientRpc() to all clients
7. All clients → NetworkedSpeechBubble.ShowEmote()
```

## Troubleshooting

### Menu not appearing
- Check that EmoteWheelConfig is assigned
- Verify input provider is initialized (check console for warnings)
- Ensure RadialEmoteMenu has item template assigned

### Emotes not syncing
- Verify EmoteWheelManager has NetworkObject component
- Check that NetworkManager is running
- Ensure speech bubbles are registered (check console logs)

### Input not working
- For OVR: Verify Meta XR SDK is installed
- For Unity Input System: Verify package is installed
- Check `OCULUS_XR_SDK` or `UNITY_INPUT_SYSTEM` defines in asmdef

## Support

For issues and feature requests, please contact the development team.
