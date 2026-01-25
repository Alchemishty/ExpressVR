# ExpressVR

A self-contained Unity module for expressive social communication in multiplayer VR experiences using network-synced animated speech bubbles.

This module was originally developed by me while building a production multiplayer VR experience at Evermorrow Labs. This repository is a cleaned-up, standalone version intended for reuse and documentation. No proprietary code or assets from the original project are included.

[![Unity 2022.3+](https://img.shields.io/badge/Unity-2022.3%20LTS-blue.svg)](https://unity.com/)
[![Netcode 1.5.0](https://img.shields.io/badge/Netcode-1.5.0-green.svg)](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.8/manual/index.html)

---

## Table of Contents

- [Features](#features)
- [Architecture Overview](#architecture-overview)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Core Systems](#core-systems)
- [Configuration](#configuration)
- [API Reference](#api-reference)
- [Extending the System](#extending-the-system)

---

## Features

- **Joystick-Controlled Radial Menu** - Intuitive thumbstick-based emote selection with 4-12 configurable options
- **Networked Speech Bubbles** - Real-time emote broadcasting via Unity Netcode RPCs, visible to all players
- **Animated Pop Effects** - Satisfying stretch/squash animations when bubbles appear
- **Auto-Hide System** - Configurable display duration with smooth transitions
- **Cross-Platform XR** - Supports Meta/Oculus SDK and generic Unity Input System devices
- **Hand Agnostic** - Works with left or right controller
- **Audio Feedback** - Optional sound effects per emote
- **Event-Driven Architecture** - Hook into selection and broadcast events for custom behavior
- **Fully Configurable** - ScriptableObject-based settings for easy tweaking

---

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                        EmoteWheelManager                        │
│                    (Singleton + NetworkObject)                  │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐  │
│  │  Speech Bubble  │  │   RPC System    │  │  Event System   │  │
│  │    Registry     │  │ Server ↔ Client │  │ OnEmoteSelected │  │
│  │ Dict<ulong,Bub> │  │ SendEmoteServer │  │ OnEmoteBroadcast│  │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
         ↑                      ↑                      ↓
         │                      │                      │
┌────────┴────────┐   ┌─────────┴─────────┐   ┌────────┴─────────┐
│ NetworkedSpeech │   │  EmoteWheelInput  │   │  RadialEmoteMenu │
│     Bubble      │   │                   │   │                  │
│ (Per Avatar)    │   │ IXRInputProvider  │   │  EmoteMenuItem   │
└─────────────────┘   └───────────────────┘   └──────────────────┘
```

| Component               | Responsibility                                                 |
| ----------------------- | -------------------------------------------------------------- |
| `EmoteWheelManager`     | Central hub for networking, speech bubble registry, and events |
| `EmoteWheelInput`       | Processes VR controller input and maps joystick to selection   |
| `RadialEmoteMenu`       | Generates and animates the circular UI layout                  |
| `NetworkedSpeechBubble` | Displays emotes above avatars with pop animations              |
| `IXRInputProvider`      | Abstracts platform-specific input (Meta/Generic)               |

---

## Installation

### Unity Package Manager (Git URL)

1. Open Window → Package Manager
2. Click "+" → "Add package from git URL"
3. Enter: `https://github.com/Alchemishty/ExpressVR.git`

### Manual Installation

Download and extract into your `Packages/` folder.

### Dependencies

| Package                         | Version | Required                   |
| ------------------------------- | ------- | -------------------------- |
| `com.unity.netcode.gameobjects` | 1.5.0+  | ✅ Yes                     |
| `com.meta.xr.sdk.core`          | Any     | ⚪ Optional (Meta devices) |
| `com.unity.inputsystem`         | Any     | ⚪ Optional (Generic XR)   |

---

## Quick Start

### 1. Scene Setup

Add the `EmoteWheelManager` prefab to your scene (or create one):

```
EmoteWheelManager (GameObject)
├── NetworkObject
├── EmoteWheelManager
├── EmoteWheelInput
├── RadialEmoteMenu
└── AudioSource (for emote sounds)
```

### 2. Avatar Setup

Add a speech bubble to your networked avatar prefab:

```
PlayerAvatar (NetworkObject)
└── SpeechBubble (child, positioned above head)
    ├── NetworkedSpeechBubble
    ├── BubbleQuad (background visual)
    └── EmoteQuad (displays the emote)
```

### 3. Configuration

Create your emote configuration:

- Right-click in Project → Create → VREmoteWheel → Configuration
- Add 4-12 emotes with sprites/materials and optional sounds
- Assign to `EmoteWheelManager` and `EmoteWheelInput`

### 4. Basic Integration

```csharp
using VREmoteWheel;

public class MyGameManager : MonoBehaviour
{
    void Start()
    {
        // Subscribe to emote events
        EmoteWheelManager.Instance.OnEmoteSelected += OnLocalEmoteSelected;
        EmoteWheelManager.Instance.OnEmoteBroadcast += OnAnyEmoteBroadcast;
    }

    void OnLocalEmoteSelected(int emoteIndex)
    {
        Debug.Log($"Local player selected emote {emoteIndex}");
    }

    void OnAnyEmoteBroadcast(ulong clientId, int emoteIndex)
    {
        Debug.Log($"Player {clientId} expressed emote {emoteIndex}");
    }
}
```

---

## Core Systems

### Input Processing

The input system abstracts VR controller differences through `IXRInputProvider`:

```csharp
public interface IXRInputProvider
{
    bool IsInitialized { get; }
    bool IsThumbstickTouched { get; }
    Vector2 ThumbstickValue { get; }

    void Initialize();
    void UpdateInput();
}
```

**Supported Platforms:**

- **Meta/Oculus** - Uses `OVRInput` API (requires `OCULUS_XR_SDK` define)
- **Generic XR** - Uses Unity Input System with XR bindings

Selection is calculated from joystick angle:

```csharp
float angle = Mathf.Atan2(joystick.y, joystick.x) * Mathf.Rad2Deg;
int selection = Mathf.FloorToInt((90f - angle + 360f) % 360f / (360f / emoteCount));
```

### Network Flow

```
Player A selects emote
        ↓
EmoteWheelManager.SelectEmote(index)
        ↓
SendEmoteServerRpc(index) ─────→ Server validates
        ↓                              ↓
Local bubble shows              BroadcastEmoteClientRpc(clientId, index)
        ↓                              ↓
OnEmoteSelected fires           All clients receive
                                       ↓
                               Each client updates the correct
                               speech bubble via registry lookup
                                       ↓
                               OnEmoteBroadcast fires everywhere
```

### Speech Bubble Animation

Bubbles use a two-phase pop animation for satisfying visual feedback:

```csharp
// Phase 1: Stretch (overshoot)
scale = Vector3.Lerp(Vector3.zero, targetScale * stretchAmount, t);

// Phase 2: Squash (settle)
scale = Vector3.Lerp(stretchedScale, targetScale, t);
```

---

## Configuration

### EmoteWheelConfig

| Property                            | Type              | Description                          |
| ----------------------------------- | ----------------- | ------------------------------------ |
| `emotes`                            | `List<EmoteData>` | Emote definitions (4-12 recommended) |
| `joystickThreshold`                 | `float`           | Dead zone for input (0.1-0.9)        |
| `menuRadius`                        | `float`           | UI radius in units                   |
| `itemScale` / `selectedItemScale`   | `float`           | Normal/selected item sizes           |
| `unselectedAlpha` / `selectedAlpha` | `float`           | Transparency values                  |
| `animationDuration`                 | `float`           | Selection animation time             |
| `requireTouchToOpen`                | `bool`            | Require thumbstick touch             |

### SpeechBubbleSettings

| Property            | Type      | Description                |
| ------------------- | --------- | -------------------------- |
| `popDuration`       | `float`   | Animation duration         |
| `stretchAmount`     | `float`   | Overshoot scale multiplier |
| `squashAmount`      | `float`   | Settle scale multiplier    |
| `displayDuration`   | `float`   | Auto-hide delay            |
| `bubbleScale`       | `Vector3` | Base bubble size           |
| `positionOffset`    | `Vector3` | Offset from avatar         |
| `useSpriteRenderer` | `bool`    | Sprite vs Material display |

### EmoteData

```csharp
[Serializable]
public class EmoteData
{
    public string emoteName;        // Display name
    public Sprite emoteSprite;      // 2D sprite for UI
    public Material emoteMaterial;  // 3D material for bubble
    public AudioClip emoteSound;    // Optional sound effect
}
```

---

## API Reference

### EmoteWheelManager

```csharp
// Singleton access
EmoteWheelManager.Instance

// Trigger emote selection (call from local player)
void SelectEmote(int emoteIndex)

// Get speech bubble for a specific player
NetworkedSpeechBubble GetSpeechBubble(ulong clientId)

// Events
Action<int> OnEmoteSelected           // Local selection (emoteIndex)
Action<ulong, int> OnEmoteBroadcast   // Any player broadcast (clientId, emoteIndex)
```

### NetworkedSpeechBubble

```csharp
// Display an emote
void ShowEmote(int emoteIndex)
void ShowEmote(EmoteData emote)

// Manual control
void Hide()

// Events
Action OnBubbleShown
Action OnBubbleHidden
```

---

## Extending the System

### Custom Input Provider

Implement `IXRInputProvider` for additional platforms:

```csharp
public class MyCustomInputProvider : IXRInputProvider
{
    public bool IsInitialized { get; private set; }
    public bool IsThumbstickTouched => /* your implementation */;
    public Vector2 ThumbstickValue => /* your implementation */;

    public void Initialize() { /* setup */ IsInitialized = true; }
    public void UpdateInput() { /* poll input */ }
}
```

### Custom Bubble Behavior

Subscribe to events for custom reactions:

```csharp
EmoteWheelManager.Instance.OnEmoteBroadcast += (clientId, emoteIndex) =>
{
    // Trigger particle effects, animations, achievements, etc.
    var bubble = EmoteWheelManager.Instance.GetSpeechBubble(clientId);
    if (bubble != null)
    {
        // Access bubble transform for positioning effects
        SpawnParticles(bubble.transform.position);
    }
};
```

---

## Project Structure

```
VREmoteWheel/
├── package.json
├── VREmoteWheel.asmdef
├── Runtime/
│   ├── Core/
│   │   ├── EmoteWheelManager.cs
│   │   ├── EmoteWheelInput.cs
│   │   └── NetworkedSpeechBubble.cs
│   ├── UI/
│   │   ├── RadialEmoteMenu.cs
│   │   └── EmoteMenuItem.cs
│   ├── Configuration/
│   │   ├── EmoteWheelConfig.cs
│   │   ├── EmoteData.cs
│   │   └── SpeechBubbleSettings.cs
│   └── XRAbstraction/
│       ├── IXRInputProvider.cs
│       ├── GenericXRInputProvider.cs
│       └── OVR/
│           ├── OVRInputProvider.cs
│           └── VREmoteWheel.OVR.asmdef
├── Settings/
│   ├── DefaultEmoteWheelConfig.asset
│   └── DefaultSpeechBubbleSettings.asset
└── Samples~/
    └── BasicSetup/
```

---

## License

MIT

---

## Acknowledgments

Built with:

- [Unity Netcode for GameObjects](https://docs-multiplayer.unity3d.com/netcode/current/about/)
- [Meta XR SDK](https://developers.meta.com/horizon/documentation/unity/unity-overview/)
- [Unity Input System](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.7/manual/index.html)
