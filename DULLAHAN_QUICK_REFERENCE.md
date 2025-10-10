# Dullahan System - Quick Reference Guide

## Player Controls

### Inventory
- **Number Keys (1-3)**: Select head in inventory slot
- **Mouse Wheel Up**: Next head
- **Mouse Wheel Down**: Previous head
- **E Key**: Pick up head
- **F Key**: Attach head to Dullahan body

---

## Expected Behavior

### ❌ Wrong Head Inserted
1. Head appears on Dullahan body
2. Body light flashes **red 3 times**
3. Head disappears after **1 second**
4. **Player gets debuffed** (slower, reduced vision, etc.)
5. **Dullahan gets buffed** (faster, more aggressive)
6. Head consumed from inventory

### ✅ Correct Head Inserted (Real Head)
1. Head appears permanently on Dullahan
2. Body light turns **green**
3. Puzzle completion sound
4. Final door unlocks
5. Head consumed from inventory
6. **No debuffs or buffs**

---

## Inspector Setup

### DullahanBody Component
Required fields:
- ✅ Head Attachment Point
- ✅ Attached Head Visual
- ✅ Body Light
- ✅ Show Fake Head Placement = true
- ✅ Fake Head Visual Duration = 1.0
- ✅ Required Head ID = 1

### DullahanHeadSO (Fake Heads)
- ✅ Head Prefab assigned
- ✅ Has Effect = true
- ✅ Effect Type (SpeedDebuff, FearEffect, etc.)
- ✅ Effect Strength (0.0-1.0)
- ✅ Effect Duration (seconds)

### DullahanHeadSO (Real Head)
- ✅ Head Prefab assigned
- ✅ Has Effect = false
- ✅ Head ID = 1 (matches Required Head ID)

---

## Debug Console Tags

Look for these tags in console:
- `[AttachHead]` - Attachment process
- `[Visual]` - Visual feedback
- `[Effect]` - Player effects
- `[Dullahan Effect]` - Dullahan buffs
- `[Inventory]` - Inventory operations
- `[Inventory Selection]` - Input handling

---

## Effect Types

### Player Debuffs (Wrong Heads)
- **SpeedDebuff**: Slower movement
- **VisionDebuff**: Reduced FOV
- **StaminaDebuff**: Less sprint time
- **HealthDebuff**: Take damage
- **FearEffect**: Increases anxiety (visual/audio)

### Dullahan Buffs (Wrong Heads)
- **SpeedBoost**: Faster chase
- **FearEffect**: More aggressive
- **Increased Intensity**: Harder to escape

---

## Troubleshooting

### No visual when inserting wrong head?
→ Check head prefab assigned & showFakeHeadPlacement = true

### Effects not working?
→ Check DullahanHeadEffectManager exists in scene

### Head not consumed?
→ Check DullahanHeadInventory on player exists

### Can't select heads?
→ Check heads are picked up (in inventory)

---

## Key Files
- `DullahanBody.cs` - Head attachment logic
- `DullahanHeadInventory.cs` - Inventory system
- `DullahanHeadEffectManager.cs` - Player effects
- `DullahanChaseSystem.cs` - Dullahan AI/buffs

---

**Quick Test**: Pick up fake head → Press 1 to select → Walk to Dullahan → Press F → See head flash on body for 1 sec → Feel slower

