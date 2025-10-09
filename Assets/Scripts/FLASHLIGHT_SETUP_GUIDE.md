# Flashlight System Setup Guide

## Overview
The flashlight system has been completely rewritten for clarity and ease of use. It supports two modes:
1. **Pickup Required Mode**: Player must find and pick up a flashlight object before using it
2. **Direct Mode**: Player can use flashlight immediately without pickup

---

## Mode 1: Pickup Required (Default)

This mode requires the player to find and collect a flashlight item before they can use it.

### Setup Steps:

#### 1. Setup FlashlightController on Player
1. Find or create a GameObject for the flashlight system (can be on the player or separate)
2. Add the `FlashlightController` component
3. Configure settings:
   - ☑️ **Require Pickup**: `TRUE` (enforce pickup requirement)
   - ☐ **Has Flashlight**: `FALSE` (player starts without flashlight)
   - **Flashlight Key**: `T` (or your preferred key)

#### 2. Create Flashlight Pickup Object
1. Create a new GameObject in your scene (e.g., "FlashlightPickup")
2. Position it where you want the player to find it
3. Add a **3D model/mesh** for visual representation (recommended)
4. Add a **Collider** component:
   - Any collider type (Box, Sphere, Capsule, etc.)
   - ☑️ Check **"Is Trigger"**
5. Add a **Rigidbody** component:
   - ☑️ Check **"Is Kinematic"**
   - ☐ Uncheck **"Use Gravity"**
6. Add the `FlashlightPickup` component
7. Configure pickup settings:
   - **Flashlight Controller**: Drag your FlashlightController here (or leave null for auto-find)
   - **Auto Pickup On Trigger**: `TRUE` for automatic, `FALSE` to require key press
   - **Pickup Key**: `E` (if manual pickup)
   - **Player Tag**: `"Player"`
   - **Visual Object**: Drag the mesh/model here to hide it after pickup
   - **Destroy On Pickup**: `TRUE` to remove it, `FALSE` to just disable
   - ☐ **Enable Debug Logs**: Enable for troubleshooting

#### 3. Tag Your Player
- Select your player GameObject
- Set Tag to **"Player"** (in Inspector, top dropdown)

#### 4. Test
- Run the game
- Walk to the flashlight pickup
- If auto-pickup: it should automatically be collected
- If manual pickup: press E when near it
- Press T to toggle the flashlight on/off

---

## Mode 2: Direct Use (No Pickup Required)

This mode allows the player to use the flashlight immediately without finding a pickup object.

### Setup Steps:

#### 1. Setup FlashlightController on Player
1. Find or create a GameObject for the flashlight system
2. Add the `FlashlightController` component
3. Configure settings:
   - ☐ **Require Pickup**: `FALSE` (disable pickup requirement)
   - ☑️ **Has Flashlight**: `TRUE` (player starts with flashlight)
   - **Flashlight Key**: `T` (or your preferred key)

#### 2. That's It!
- No FlashlightPickup object needed
- Player can use flashlight immediately
- Press T to toggle on/off

---

## Troubleshooting

### Pickup Not Working?

1. **Enable Debug Logs**:
   - On FlashlightPickup component, check ☑️ **Enable Debug Logs**
   - Run the game and check the Console for detailed messages

2. **Common Issues**:

   | Problem | Console Message | Solution |
   |---------|----------------|----------|
   | No trigger detection | "CRITICAL ERROR: No Collider found!" | Add a Collider component, set "Is Trigger" to TRUE |
   | Collider not trigger | "Collider 'Is Trigger' is FALSE!" | Check "Is Trigger" in Collider settings |
   | No physics | "CRITICAL ERROR: No Rigidbody found!" | Add Rigidbody, set to Kinematic |
   | Player not found | "No GameObject with tag 'Player' found!" | Tag your player GameObject as "Player" |
   | No controller | "No FlashlightController found!" | Make sure FlashlightController exists in scene |
   | Already unlocked | "FlashlightController.requirePickup is FALSE" | Set `requirePickup` to TRUE on FlashlightController |

3. **Check Layers**:
   - Edit → Project Settings → Physics
   - Scroll to **Layer Collision Matrix**
   - Make sure player layer and pickup layer can collide

4. **Check Player Movement**:
   - If using `SimplePlayerMovement`, make sure it has a CharacterController
   - Physics-based movement is required for trigger detection

### Can Use Flashlight Before Pickup?

**Problem**: Player can toggle flashlight before picking up the item

**Solution**: On FlashlightController:
- Set **Require Pickup**: `TRUE`
- Set **Has Flashlight**: `FALSE`

### Flashlight Not Toggling?

**Problem**: Pressing T does nothing after pickup

**Solution**: 
1. Check Console for "Flashlight acquired! Press T to toggle."
2. Make sure FlashlightController has a Light component or can create one
3. Check battery isn't depleted

---

## Quick Reference

### FlashlightController Settings

| Setting | What It Does |
|---------|-------------|
| `requirePickup = TRUE` | Player must find FlashlightPickup object |
| `requirePickup = FALSE` | Player can use flashlight immediately |
| `hasFlashlight = TRUE` | Player starts with flashlight unlocked |
| `hasFlashlight = FALSE` | Player starts without flashlight |

### FlashlightPickup Settings

| Setting | What It Does |
|---------|-------------|
| `autoPickupOnTrigger = TRUE` | Automatically pick up when player touches it |
| `autoPickupOnTrigger = FALSE` | Require pressing pickup key (E) when near |
| `enableDebugLogs = TRUE` | Show detailed console messages for troubleshooting |

---

## Example Scenarios

### Scenario A: Horror Game - Find Flashlight First
```
FlashlightController:
- requirePickup = TRUE
- hasFlashlight = FALSE

FlashlightPickup:
- autoPickupOnTrigger = TRUE
- Hidden in dark corner of map
```

### Scenario B: Action Game - Start With Flashlight
```
FlashlightController:
- requirePickup = FALSE
- hasFlashlight = TRUE

No FlashlightPickup needed
```

### Scenario C: Tutorial - Forced Pickup Interaction
```
FlashlightController:
- requirePickup = TRUE
- hasFlashlight = FALSE

FlashlightPickup:
- autoPickupOnTrigger = FALSE
- pickupKey = E
- Place in obvious location with tutorial prompt
```

---

## Need More Help?

1. **Enable Debug Logs** on FlashlightPickup
2. Run the game
3. Check Unity Console for detailed error messages
4. Error messages now clearly state what's wrong and how to fix it

