# Treasure Chest Interaction Guide

## Problem Analysis
Your treasure chest is **locked** and requires a `LargeChestKey (Key SO)` to unlock before you can access the key and notes inside.

## The Complete Solution

### Step 1: Find the LargeChestKey

The `LargeChestKey` exists in your project at:
`Assets/Scripts/World/Chest/KeySO2ndFloor/LargeChestKey.asset`

**You need to find a GameObject in your scene that has this key.** Look for:
- A key GameObject with `KeyPickable` component
- The `KeyPickable.keyData` field should reference `LargeChestKey`
- The key should be somewhere else in the world (not inside the chest)

### Step 2: Pick Up the LargeChestKey

1. **Find the key GameObject** in your scene
2. **Make sure it has the `KeyPickable` component** (add it if missing)
3. **Assign `LargeChestKey` to the `keyData` field**
4. **Ensure it has a Collider** for raycast detection
5. **Pick it up** by looking at it and pressing E

### Step 3: Unlock the Treasure Chest

1. **Select the key in your inventory** (it should show in your inventory UI)
2. **Look at the treasure chest** with your crosshair
3. **Press E** to interact with the chest
4. **The chest should unlock and open** automatically

### Step 4: Access Chest Contents

Once the chest is unlocked and open:

#### For the Key Inside:
1. **Add `KeyPickable` component** to the key GameObject inside the chest
2. **Assign the appropriate KeySO** to the `keyData` field
3. **Ensure it has a Collider**
4. **Pick it up** by looking at it and pressing E

#### For the Notes:
1. **Add `NoteLetterPickable` component** to the note GameObject
2. **Assign a `NoteLetterSO`** to the `noteData` field
3. **Ensure it has a Collider**
4. **Read the note** by looking at it and pressing E

## Setup Instructions

### For the Key Inside the Chest:

```csharp
// Add this component to the key GameObject inside the chest
KeyPickable keyPickable = keyGameObject.AddComponent<KeyPickable>();
keyPickable.keyData = yourKeySO; // Assign the appropriate KeySO
```

### For the Notes Inside the Chest:

```csharp
// Add this component to the note GameObject inside the chest
NoteLetterPickable notePickable = noteGameObject.AddComponent<NoteLetterPickable>();
notePickable.noteData = yourNoteLetterSO; // Assign the appropriate NoteLetterSO
```

## Debugging Steps

### Check Console Messages:
The enhanced system will show debug messages like:
- `"TreasureChestController: Chest is locked, checking for key..."`
- `"Selected key: [key name]"`
- `"Key matches! Unlocking chest..."`
- `"KeyPickable: Ready to pickup key '[key name]'"`

### Common Issues:

1. **"No key selected"**: Make sure you have the `LargeChestKey` in your inventory and it's selected
2. **"Key doesn't match"**: The selected key doesn't match the required `LargeChestKey`
3. **"Inventory is full"**: Your inventory is full, remove some items first
4. **"No Collider found"**: Add a Collider to the key/note GameObjects

## Testing Checklist

- [ ] Found `LargeChestKey` in the world
- [ ] Picked up `LargeChestKey` (appears in inventory)
- [ ] Selected `LargeChestKey` in inventory
- [ ] Looked at treasure chest with crosshair
- [ ] Pressed E to unlock chest
- [ ] Chest opened successfully
- [ ] Key inside chest has `KeyPickable` component
- [ ] Notes inside chest have `NoteLetterPickable` component
- [ ] Can pick up key inside chest
- [ ] Can read notes inside chest

## Quick Fix for Testing

If you want to test the chest contents immediately:

1. **Temporarily disable the lock**:
   - Select the treasure chest
   - In `TreasureChestController`, uncheck "Start Locked"
   - This will make the chest open without requiring a key

2. **Add the required components** to the contents:
   - Add `KeyPickable` to the key inside
   - Add `NoteLetterPickable` to the notes inside

3. **Test the interaction**:
   - Look at the chest and press E to open
   - Look at the key inside and press E to pick up
   - Look at the notes and press E to read

## Notes System Setup

For the notes to work properly, you also need:

1. **NoteLetterSO asset**: Create one in Project → Create → Game → Note Letter
2. **NoteLetterUI component**: Add to a GameObject in your scene
3. **UI Canvas setup**: With `NotePanel`, `PaperImage`, `TextContent`, `CloseButton`

See the `README_NoteLetterSystem.md` for detailed note system setup.

## Summary

The main issue is that your treasure chest is locked and requires the `LargeChestKey` to unlock. Once unlocked, you can access the contents inside. Make sure all interactive objects (keys and notes) have the proper pickup components attached.
