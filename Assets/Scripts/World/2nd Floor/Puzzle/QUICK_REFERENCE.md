# Dullahan Head Placement Puzzle - Quick Reference

## Quick Setup (5 Minutes)

### Step 1: Create Basic Objects
```
1. Create Empty GameObject named "DullahanHeadPlacementPuzzle"
2. Add child: 3D Object > Sphere named "HeadPlaceholder" (scale 0.3, 0.3, 0.3)
3. Add child: Empty GameObject named "HeadAttachmentPoint"
```

### Step 2: Add Scripts
```
1. DullahanHeadPlacementPuzzle → Add Component → DullahanHeadPlacementPuzzle
2. HeadPlaceholder → Add Component → HeadPlaceholder
```

### Step 3: Connect References
```
DullahanHeadPlacementPuzzle Inspector:
- Head Placeholder: Drag HeadPlaceholder GameObject
- Head Attachment Point: Drag HeadAttachmentPoint Transform
- Required Head ID: 1 (or your correct head's ID)
```

### Step 4: Test
```
1. Play the scene
2. Pick up a Dullahan head
3. Approach the puzzle
4. Look at it and press F to place head
```

## Key Features

### ✓ Dullahan Freeze System (NEW!)
- **Dullahan stops moving when player picks up a head**
- Player can safely place the head without being chased
- Dullahan resumes movement when head is placed/dropped
- Can be toggled on/off in inspector

### ✓ Placeholder Visibility
- Initially invisible
- Appears when player approaches
- Fades in/out smoothly
- Pulses to attract attention

### ✓ Visual Feedback
- **Empty**: White/transparent
- **Correct Head**: Green glow
- **Wrong Head**: Red warning

### ✓ Interaction
- **Raycast-based**: Must look at puzzle
- **Distance-based**: Just be nearby
- Customizable interaction key (default: F)

### ✓ Wrong Head Handling
- Head is consumed from inventory
- Displayed briefly (configurable duration)
- Then removed automatically
- Placeholder returns

### ✓ Correct Head Handling
- Head attaches permanently
- Puzzle completes
- Rewards are granted
- Events are triggered

## Common Settings

| Setting | Recommended Value | Purpose |
|---------|------------------|---------|
| Required Head ID | 1 | ID of correct head |
| Interaction Range | 3 | Distance in meters |
| Interaction Key | F | Key to place head |
| **Freeze Dullahan When Player Has Head** | **true** | **Stop Dullahan when head picked up** |
| Placeholder Initially Visible | false | Hidden until player approaches |
| Use Raycast Interaction | true | Must look at puzzle |
| Show Fake Head Briefly | true | Show wrong heads temporarily |
| Fake Head Display Duration | 2.0 | Seconds to show wrong head |
| Use Fade Transition | true | Smooth appearance |
| Enable Pulsing | true | Animated breathing effect |

## Materials Setup (Optional)

### Create Three Materials:

**1. Mat_PlaceholderEmpty**
- Rendering Mode: Transparent
- Albedo: White (255, 255, 255, 50)
- Emission: Off or subtle

**2. Mat_PlaceholderValid**
- Rendering Mode: Transparent
- Albedo: Green (0, 255, 0, 100)
- Emission: Green glow

**3. Mat_PlaceholderInvalid**
- Rendering Mode: Transparent
- Albedo: Red (255, 0, 0, 100)
- Emission: Red glow

Assign these in the DullahanHeadPlacementPuzzle inspector.

## Code Examples

### Check Puzzle Completion
```csharp
DullahanHeadPlacementPuzzle puzzle = FindObjectOfType<DullahanHeadPlacementPuzzle>();
if (puzzle.IsPuzzleCompleted())
{
    // Puzzle is complete!
}
```

### Control Placeholder
```csharp
HeadPlaceholder placeholder = FindObjectOfType<HeadPlaceholder>();
placeholder.Show(); // Show placeholder
placeholder.Hide(); // Hide placeholder
placeholder.SetState(HeadPlaceholder.PlaceholderState.ValidHover); // Set state
```

### Reset Puzzle (Testing)
```csharp
puzzle.ResetPuzzle(); // Reset to initial state
```

## Troubleshooting

| Problem | Solution |
|---------|----------|
| Placeholder doesn't appear | Check interactionRange, make sure player is close enough |
| Can't place head | Make sure player has DullahanHeadInventory and a head |
| No visual feedback | Assign materials in inspector |
| Wrong interaction key | Change interactionKey in inspector |
| Placeholder always visible | Set placeholderInitiallyVisible to false |

## Video Reference

Similar to this mechanic: https://youtu.be/hHPOHYZeEq0?si=VZukXpGHPZBSRnlG
(But placing head on Dullahan's neck instead)

## Integration

Works automatically with:
- ✓ DullahanHeadInventory system
- ✓ Floor2EndingEventManager
- ✓ DullahanChaseEventManager
- ✓ Door unlocking system

No additional code needed!

## Inspector Layout

```
DullahanHeadPlacementPuzzle
├─ Puzzle Settings
│  ├─ Required Head ID: 1
│  └─ Puzzle Completed: false
├─ Interaction Settings
│  ├─ Interaction Range: 3
│  ├─ Interaction Key: F
│  ├─ Interaction Prompt: "Press F to..."
│  ├─ No Head Prompt: "You need a head..."
│  └─ Use Raycast Interaction: true
├─ Placeholder Settings
│  ├─ Head Placeholder: [GameObject]
│  ├─ Placeholder Initially Visible: false
│  ├─ Placeholder Empty Material: [Material]
│  ├─ Placeholder Valid Material: [Material]
│  └─ Placeholder Invalid Material: [Material]
├─ Head Attachment
│  ├─ Head Attachment Point: [Transform]
│  ├─ Attached Head Visual: [GameObject]
│  ├─ Show Fake Head Briefly: true
│  └─ Fake Head Display Duration: 2.0
├─ Visual Effects
│  ├─ Completion Light: [Light]
│  ├─ Completed Light Color: Green
│  ├─ Placement Particles: [ParticleSystem]
│  └─ Completion Particles: [ParticleSystem]
├─ Audio
│  ├─ Audio Source: [AudioSource]
│  ├─ Correct Head Sound: [AudioClip]
│  ├─ Wrong Head Sound: [AudioClip]
│  └─ Puzzle Complete Sound: [AudioClip]
├─ UI
│  ├─ Interaction UI: [GameObject]
│  └─ Interaction Text: [TextMeshProUGUI]
└─ Rewards
   ├─ Reward Door: [Door]
   ├─ Reward Items: [GameObject[]]
   └─ Reward Spawn Point: [Transform]
```

---

**Need more details?** See DULLAHAN_HEAD_PLACEMENT_SETUP.md

