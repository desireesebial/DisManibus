# KeypadDoorController Setup Guide

## Overview
The KeypadDoorController provides a complete keypad-based door unlocking system with UI, audio feedback, and integration with the existing door system.

## Prerequisites
- Unity project with UI system (Canvas, EventSystem)
- Player GameObject with "Player" tag
- SimplePlayerMovement component on Player
- doorscript component for target door

## Setup Steps

### 1. Create KeypadDoor GameObject
1. Create an empty GameObject in your scene
2. Name it "KeypadDoor" or similar
3. Position it near the door you want to control
4. Add the `KeypadDoorController` component

### 2. Create Target Door
1. Create the actual door GameObject (or use existing)
2. Add the `doorscript` component
3. Configure the door settings:
   - Set `isLocked = true` initially
   - Configure `openAngle`, `openSpeed` as needed
   - Assign door audio clips if desired

### 3. Configure KeypadDoorController

#### Code Settings
- **correctCode**: The code that unlocks the door (e.g., "1234")
- **codeLength**: Length of the code (auto-set when using SetCode())
- **autoCloseOnUnlock**: Whether to close UI after successful unlock

#### Interaction Settings
- **interactionDistance**: How close player must be (default: 3f)
- **interactKey**: Key to open keypad (default: KeyCode.E)
- **interactionPrompt**: Text shown to player

#### UI Settings (Optional - Auto-built if empty)
- **buildUIIfMissing**: Enable auto-build UI system
- Leave UI fields empty to use auto-generated interface
- Or assign custom UI elements if you have them

#### Audio Settings
- **buttonClickSound**: Sound for number/button presses
- **correctSound**: Sound when correct code entered
- **incorrectSound**: Sound when wrong code entered
- **unlockSound**: Sound when door unlocks

#### Door Integration
- **targetDoor**: Assign the doorscript component
- **openDoorOnUnlock**: Whether to auto-open door after unlock

### 4. Player Setup
Ensure your Player GameObject has:
- Tag: "Player"
- `SimplePlayerMovement` component
- Proper camera setup for cursor locking/unlocking

### 5. Scene Setup
- Ensure you have a Canvas in the scene (will be auto-created if missing)
- EventSystem should be present for UI interaction

## Usage

### Runtime Behavior
1. Player approaches keypad (within interactionDistance)
2. Press E to open keypad interface
3. Enter code using mouse clicks or keyboard (0-9, Backspace, Enter)
4. Correct code unlocks and optionally opens the door
5. Wrong code shows error message and clears input

### Keyboard Controls (when UI is open)
- **0-9 Keys**: Enter digits
- **Backspace**: Remove last digit
- **Enter**: Submit code
- **E**: Close keypad interface

### Public Methods
- `SetCode(string newCode)`: Change the required code
- Access via events: `onCorrectCode`, `onWrongCode`

## Integration with Existing Systems

### Door System Integration
The KeypadDoorController works with the existing `doorscript` system:
- Calls `targetDoor.UnlockDoor()` when correct code entered
- Optionally calls `targetDoor.OpenDoor()` if `openDoorOnUnlock` is true
- Temporarily disables door interaction while keypad is open

### Player Movement Integration
- Disables `SimplePlayerMovement` when keypad UI is open
- Unlocks cursor for UI interaction
- Restores movement and cursor lock when UI closes

## Troubleshooting

### Common Issues
1. **Keypad doesn't appear**: Check interaction distance and player tag
2. **No audio**: Assign AudioSource component and audio clips
3. **Door doesn't unlock**: Verify targetDoor assignment and doorscript setup
4. **UI issues**: Ensure Canvas and EventSystem exist in scene

### Debug Features
- Visual gizmo shows interaction range in Scene view
- Console logs for interaction events
- Auto-built UI includes all necessary components

## Example Setup Code
```csharp
// Set keypad code programmatically
KeypadDoorController keypad = GetComponent<KeypadDoorController>();
keypad.SetCode("5678");

// Listen for events
keypad.onCorrectCode.AddListener(() => {
    Debug.Log("Player entered correct code!");
    // Add custom logic here
});
```

