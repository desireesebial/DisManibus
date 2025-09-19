# Flashlight System Setup Guide

## Overview
The flashlight system provides a virtual flashlight that doesn't require a physical flashlight object. The light is attached to the player's camera and can be toggled on/off using the **T key**.

## Features
- **No Physical Object Required**: The flashlight provides light without needing an inventory item
- **Camera-Attached**: Light follows the player's view direction automatically
- **T Key Control**: Simple toggle on/off with the T key
- **Configurable**: Adjustable intensity, range, color, and spotlight angle
- **Audio Feedback**: Optional sound effects for turning on/off
- **Multiple Player Controller Support**: Works with both FirstPersonController and SimplePlayerMovement

## Setup Instructions

### Step 1: Add the FlashlightController to Your Scene
1. Create an empty GameObject in your scene
2. Name it "FlashlightManager" or similar
3. Add the `FlashlightController` script to this GameObject

### Step 2: Configure the Settings (Optional)
In the Inspector, you can customize:

#### Flashlight Settings:
- **Flashlight Key**: The key to toggle the flashlight (default: T)
- **Is Flashlight On**: Starting state of the flashlight

#### Light Properties:
- **Light Intensity**: Brightness of the flashlight (default: 2)
- **Light Range**: How far the light reaches (default: 10)
- **Light Color**: Color of the light (default: White)
- **Light Type**: Type of light (Spot recommended for flashlight effect)
- **Spot Angle**: Cone angle for spotlight (default: 60°)

#### Audio (Optional):
- **Turn On Sound**: Audio clip played when turning flashlight on
- **Turn Off Sound**: Audio clip played when turning flashlight off

### Step 3: Test the Setup
1. Enter Play mode
2. Press **T** to toggle the flashlight on/off
3. The light should follow your camera's view direction
4. Check the Console for initialization messages

## How It Works

The system automatically:
1. **Finds the Player Camera**: Searches for FirstPersonController, SimplePlayerMovement, or falls back to Camera.main
2. **Creates the Light**: Dynamically creates a Light component as a child of the player camera
3. **Handles Input**: Listens for the T key press to toggle the flashlight
4. **Manages State**: Keeps track of on/off state and provides public methods for external control

## Public Methods

You can control the flashlight from other scripts:

```csharp
FlashlightController flashlight = FindAnyObjectByType<FlashlightController>();

// Toggle the flashlight
flashlight.ToggleFlashlight();

// Set specific state
flashlight.SetFlashlightState(true);  // Turn on
flashlight.SetFlashlightState(false); // Turn off

// Check current state
bool isOn = flashlight.IsFlashlightOn();

// Update properties at runtime
flashlight.UpdateFlashlightProperties(intensity: 3f, range: 15f, color: Color.blue);
```

## Troubleshooting

### Flashlight Not Working
- Check that a FlashlightController GameObject exists in the scene
- Verify that a player camera was found (check Console messages)
- Ensure the player controller is properly set up

### No Audio
- Assign audio clips in the Inspector
- Check that the GameObject has an AudioSource component (auto-created if missing)

### Light Not Following Camera
- Ensure your player controller is one of the supported types (FirstPersonController or SimplePlayerMovement)
- Check Console for camera detection messages

## Integration Notes

- The flashlight system is independent of the inventory system
- It doesn't conflict with existing lantern or flashlight items
- Can be used alongside other lighting systems
- Automatically adapts to different player controller setups

## Performance

The system is lightweight:
- Creates only one Light component
- No physics calculations
- Minimal Update() overhead (just input checking)
- No ongoing coroutines or complex calculations
