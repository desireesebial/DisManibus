# Flashlight Pickup System Guide

## Overview
The flashlight now supports a pickup system where the player must find and collect a flashlight object before they can use it. This adds a story element where the flashlight isn't immediately available.

## How It Works

### FlashlightController Changes
- **New Field**: `requirePickup` (default: `true`) - When enabled, the player must pick up the flashlight before using it
- **New Field**: `hasFlashlight` (default: `false`) - Tracks whether the player has collected the flashlight
- **New Field**: `needPickupMessage` - Message shown when player tries to use flashlight without picking it up

### FlashlightPickup Script (New)
This script should be attached to a GameObject in your scene that represents the flashlight the player needs to find.

## Setup Instructions

### 1. Setup the Player's Flashlight Controller
1. On your player's `FlashlightController` component:
   - Set `requirePickup` to `true` (default)
   - Set `hasFlashlight` to `false` (default) - player starts without flashlight
   - Or set `hasFlashlight` to `true` if you want to start with it already unlocked

### 2A. If Flashlight Controller is on a Separate GameObject (Recommended)
If you want the flashlight functionality to be disabled until pickup:

1. Create a GameObject for the flashlight system (e.g., "FlashlightSystem")
   - Add the `FlashlightController` component to it
   - **Disable this GameObject** in the inspector (uncheck the checkbox at the top)
   - This keeps the flashlight completely inactive until picked up

2. Create a new GameObject for the pickup item (e.g., "FlashlightPickup")
   - Position it where you want the player to find it
   - Add a 3D model/mesh for the flashlight visual (optional but recommended)
   - Add the `FlashlightPickup` component
   
3. Configure the `FlashlightPickup` settings:
   - **flashlightGameObject**: Drag the disabled "FlashlightSystem" GameObject here
   - **flashlightController**: Drag the FlashlightController component here (or leave null to auto-find)
   - **autoPickupOnTrigger**: `true` for automatic pickup when player walks into it, `false` to require pressing E
   - **pickupKey**: Key to press for manual pickup (default: E)
   - **pickupRadius**: How close the player needs to be (default: 2 units)
   - **playerTag**: Tag of your player GameObject (default: "Player")
   - **visualObject**: The mesh/model to hide after pickup
   - **destroyOnPickup**: `true` to destroy the pickup, `false` to just disable it
   - **pickupSound**: Optional audio clip to play when picked up
   - **pickupEffect**: Optional particle effect prefab to spawn when picked up

**Result**: When player picks up the item, the FlashlightSystem GameObject activates and the flashlight becomes fully functional!

### 2B. If Flashlight Controller is on the Player
If the flashlight is already on the player but just locked:

1. Create a new GameObject in your scene (e.g., "FlashlightPickup")
2. Position it where you want the player to find it
3. Add a 3D model/mesh for the flashlight visual (optional but recommended)
4. Add the `FlashlightPickup` component
5. Configure the pickup settings:
   - **flashlightGameObject**: Leave this empty/null
   - **flashlightController**: Drag your player's FlashlightController here (or leave null to auto-find)
   - Configure other settings as needed

### 3. Setup Colliders
- The `FlashlightPickup` script will automatically add a SphereCollider if none exists
- If you already have a collider, make sure `isTrigger` is set to `true`
- The collider size should match or be slightly larger than your `pickupRadius`

### 4. Tag Your Player
- Make sure your player GameObject has the tag "Player" (or change the `playerTag` field to match your setup)

## Usage Examples

### Example 1: Automatic Pickup on Touch
```
FlashlightPickup Settings:
- autoPickupOnTrigger: true
- pickupRadius: 2
- playerTag: "Player"
```
Player walks near the flashlight → automatically picks it up

### Example 2: Manual Pickup with E Key
```
FlashlightPickup Settings:
- autoPickupOnTrigger: false
- pickupKey: E
- pickupRadius: 2
- playerTag: "Player"
```
Player walks near → sees "Press E to pick up Flashlight" → presses E → picks it up

### Example 3: Start with Flashlight Already Unlocked
```
FlashlightController Settings:
- requirePickup: true
- hasFlashlight: true
```
Player can use flashlight immediately (useful for testing or different game modes)

### Example 4: Disable Pickup System Entirely
```
FlashlightController Settings:
- requirePickup: false
```
Player can use flashlight from the start without needing to find it

## Debugging

### Gizmo Visualization
- Select the FlashlightPickup GameObject in the editor
- A yellow wireframe sphere shows the pickup radius

### Console Messages
- When player tries to use flashlight before pickup: "You need to find a flashlight first!"
- When flashlight is picked up: "Flashlight acquired! Press T to toggle."
- When player enters pickup range (manual mode): "Press E to pick up Flashlight"

## Integration with Other Systems

### Custom Interaction Systems
If you have your own interaction system, call `TriggerPickup()` directly:
```csharp
FlashlightPickup pickup = flashlightObject.GetComponent<FlashlightPickup>();
pickup.TriggerPickup();
```

### UI Integration
The pickup prompt is currently just logged to console. To show it in UI:
1. Add a UI Text element for prompts
2. Modify `FlashlightPickup.OnTriggerEnter()` to update your UI
3. Modify `FlashlightPickup.OnTriggerExit()` to hide the UI

### Save System Integration
To save the flashlight pickup state:
1. Save/load the `FlashlightController.hasFlashlight` boolean
2. If player has already picked up the flashlight, either:
   - Don't spawn the pickup object at all
   - Or spawn it but call `pickup.TriggerPickup()` immediately and silently

## Story Flow Example

1. **Game Start**: Player spawns without flashlight
   - `FlashlightController.requirePickup = true`
   - `FlashlightController.hasFlashlight = false`

2. **Player explores**: Finds flashlight pickup object in the world

3. **Player collects**: Walks into trigger or presses E
   - `FlashlightPickup` calls `FlashlightController.PickupFlashlight()`
   - Message: "Flashlight acquired! Press T to toggle."

4. **Player can now use flashlight**: Press T to toggle it on/off

This creates a natural progression where finding the flashlight is a milestone in your game's story.

## Disabling Pickup After Collection

The pickup script **automatically disables itself** after the flashlight is picked up:
- Sets `this.enabled = false` after pickup
- Prevents the player from picking up the same flashlight multiple times
- The pickup GameObject can still be destroyed or kept disabled as configured

If you need to manually disable/re-enable pickups from another script:
```csharp
FlashlightPickup pickup = GetComponent<FlashlightPickup>();
pickup.enabled = false; // Disable pickup functionality
pickup.enabled = true;  // Re-enable pickup functionality
```

## Summary of Both Workflows

### Workflow A: Separate GameObject (Your Case)
```
Hierarchy:
├── Player
├── FlashlightSystem (DISABLED)
│   └── FlashlightController component
└── FlashlightPickup (in world)
    └── FlashlightPickup component
        - flashlightGameObject = FlashlightSystem
        - flashlightController = FlashlightController

Result: Entire flashlight system is OFF until player collects it
```

### Workflow B: Controller on Player
```
Hierarchy:
├── Player
│   └── FlashlightController (requirePickup=true, hasFlashlight=false)
└── FlashlightPickup (in world)
    └── FlashlightPickup component
        - flashlightGameObject = null
        - flashlightController = Player's FlashlightController

Result: Flashlight exists but is locked until player collects the pickup item
```

