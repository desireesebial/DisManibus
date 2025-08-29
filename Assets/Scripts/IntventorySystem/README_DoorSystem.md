# Door System Integration Guide

## Overview
This guide explains how to set up and use the door system with the Dullahan puzzle and chase systems. The system supports multiple door types, linked doors, and automatic door management.

## Door Types

### 1. Exit Doors (Bait Doors)
- **Purpose**: Open after the chase sequence as "bait" for the player
- **Behavior**: Open simultaneously when chase ends
- **Location**: Usually near the player spawn or main area
- **Script**: `doorscript.cs`

### 2. Real Head Door
- **Purpose**: Door leading to the room containing the real Dullahan head
- **Behavior**: Opens during the head collection phase
- **Location**: Near the real head spawn point
- **Script**: `doorscript.cs`

### 3. Puzzle Doors
- **Purpose**: Doors that open when the puzzle is completed
- **Behavior**: Open when the real head is attached to the Dullahan body
- **Location**: Various locations in the level
- **Script**: `doorscript.cs`

## Setup Instructions

### Step 1: Create Door GameObjects
1. Create empty GameObjects for each door
2. Add the `doorscript.cs` component to each door
3. Name your doors appropriately:
   - Exit doors: "ExitDoor1", "ExitDoor2", etc.
   - Real head door: "RealHeadDoor" or "HeadDoor"
   - Puzzle doors: "PuzzleDoor1", "PuzzleDoor2", etc.

### Step 2: Configure Door Components
For each door, configure these settings:

#### Basic Settings
- **Open Angle**: 90 (degrees the door rotates)
- **Open Speed**: 2 (speed of door animation)
- **Is Locked**: true (initially locked)
- **Required Key ID**: -1 (no key required for puzzle doors)

#### Multiple Doors
- **Linked Doors**: Assign other doors that should open together
- **Auto Link Doors**: Enable to automatically link doors with similar names

#### Door Components
- **Door Animator**: Assign if you have door animations
- **Audio Source**: Assign for door sounds
- **Audio Clips**: Assign door open/close/locked sounds

#### UI
- **Interaction UI**: Assign UI panel for door interaction
- **Interaction Text**: Assign TextMeshPro component for prompts

### Step 3: Configure DullahanChaseEventManager
1. Find the `DullahanChaseEventManager` in your scene
2. Assign door references:
   - **Exit Doors**: Drag all exit doors to this array
   - **Real Head Door**: Drag the real head door
   - **Exit Doors Linked**: Enable for simultaneous opening

### Step 4: Configure DullahanPuzzleManager
1. Find the `DullahanPuzzleManager` in your scene
2. Assign door references:
   - **Puzzle Doors**: Drag all puzzle doors
   - **Real Head Door**: Drag the real head door

### Step 5: Door Colliders
1. Add trigger colliders to each door
2. Set the collider to "Is Trigger"
3. Ensure the collider covers the interaction area

## Door System Features

### Automatic Door Finding
The system can automatically find doors based on their names:
- Doors with "exit" or "bait" in the name → Exit doors
- Doors with "real" or "head" in the name → Real head door
- Other doors → Puzzle doors

### Linked Doors
- Multiple doors can be linked to open simultaneously
- Use the `linkedDoors` array or enable `autoLinkDoors`
- Linked doors will open/close together

### Door States
- **Locked**: Door cannot be opened by player
- **Unlocked**: Door can be opened by player
- **Open**: Door is fully opened
- **Closed**: Door is fully closed
- **Animating**: Door is currently moving

## Integration with Other Systems

### DullahanChaseEventManager Integration
```csharp
// Doors automatically open after chase
private IEnumerator OpenExitDoors()
{
    // Exit doors open simultaneously as bait
}

// Doors open for good ending
private IEnumerator OpenGoodEndingDoors()
{
    // Exit doors open for good ending
}
```

### DullahanPuzzleManager Integration
```csharp
// Doors open when puzzle is completed
private void OnPuzzleCompleted()
{
    OpenPuzzleDoors();
}

// Real head door opens for collection phase
public void SpawnAllHeads()
{
    realHeadDoor.ForceUnlock();
    realHeadDoor.OpenDoor();
}
```

### PlayerHealthSystem Integration
- Doors can be used as safe zones during chase
- Player can hide behind doors for protection

### DullahanMeleeAttack Integration
- Doors can block Dullahan's path
- Doors can be used for tactical gameplay

## Door Methods

### Public Methods Available
```csharp
// Basic door control
door.OpenDoor();           // Open door with animation
door.CloseDoor();          // Close door with animation
door.ToggleDoor();         // Toggle door state
door.ForceOpen();          // Open without animation
door.ForceClose();         // Close without animation

// Lock control
door.UnlockDoor();         // Unlock door
door.LockDoor();           // Lock door
door.ForceUnlock();        // Unlock without animation

// State checking
door.IsOpen();             // Check if door is open
door.IsLocked();           // Check if door is locked
door.IsAnimating();        // Check if door is moving

// Configuration
door.SetRequiredKeyID(id); // Set required key ID
door.SetLinkedDoors(doors); // Set linked doors
```

## Troubleshooting

### Common Issues

1. **Doors not opening**
   - Check if doors are assigned in the managers
   - Verify door colliders are set to "Is Trigger"
   - Ensure doors are not locked

2. **Linked doors not working**
   - Check `linkedDoors` array assignments
   - Verify door names for auto-linking
   - Ensure linked doors have proper colliders

3. **Door sounds not playing**
   - Assign AudioSource component
   - Assign audio clips in door settings
   - Check audio volume settings

4. **UI not showing**
   - Assign interaction UI components
   - Verify player has "Player" tag
   - Check trigger collider size

### Debug Features
- Enable debug mode in managers for additional logging
- Use debug keys for testing door functionality
- Check console for door-related messages

## Best Practices

1. **Naming Convention**
   - Use descriptive names for doors
   - Include door type in the name (Exit, Head, Puzzle)
   - Use consistent naming for linked doors

2. **Performance**
   - Limit the number of linked doors per door
   - Use object pooling for multiple doors
   - Optimize door animations

3. **User Experience**
   - Provide clear visual feedback for door states
   - Use appropriate sounds for different actions
   - Make interaction areas clearly visible

4. **Testing**
   - Test door functionality in all game states
   - Verify linked doors work correctly
   - Test door behavior with different player states

## Example Setup

### Exit Door Setup
```
GameObject: ExitDoor1
- doorscript component
- Name: "ExitDoor1"
- Is Locked: true
- Required Key ID: -1
- Linked Doors: [ExitDoor2]
- Auto Link Doors: true
```

### Real Head Door Setup
```
GameObject: RealHeadDoor
- doorscript component
- Name: "RealHeadDoor"
- Is Locked: true
- Required Key ID: -1
- Linked Doors: []
- Auto Link Doors: false
```

### Puzzle Door Setup
```
GameObject: PuzzleDoor1
- doorscript component
- Name: "PuzzleDoor1"
- Is Locked: true
- Required Key ID: -1
- Linked Doors: []
- Auto Link Doors: false
```

This door system provides a robust foundation for the Dullahan puzzle and chase mechanics, ensuring smooth integration with all other game systems.
