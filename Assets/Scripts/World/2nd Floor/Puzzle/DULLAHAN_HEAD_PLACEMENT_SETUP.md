# Dullahan Head Placement Puzzle - Setup Guide

## Overview
This puzzle requires the player to place the correct Dullahan head onto the body. The placeholder is initially invisible and becomes visible when the player approaches. The placeholder material changes based on whether the player is holding the correct or wrong head.

## Quick Start

### 1. Create the Puzzle GameObject
1. Create an empty GameObject in your scene: `Right-click in Hierarchy > Create Empty`
2. Name it: `DullahanHeadPlacementPuzzle`
3. Position it where you want the Dullahan body to be

### 2. Add the Main Script
1. Select the `DullahanHeadPlacementPuzzle` GameObject
2. Add the script: `Add Component > Dullahan Head Placement Puzzle`

### 3. Create the Placeholder
1. Create a 3D object for the placeholder: `Right-click on DullahanHeadPlacementPuzzle > 3D Object > Sphere`
2. Name it: `HeadPlaceholder`
3. Scale it to head size: `Scale (0.3, 0.3, 0.3)`
4. Position it at the neck/head attachment point
5. Add the script: `Add Component > Head Placeholder`

### 4. Create the Attachment Point
1. Create an empty GameObject: `Right-click on DullahanHeadPlacementPuzzle > Create Empty`
2. Name it: `HeadAttachmentPoint`
3. Position it exactly where the head should attach (at the neck)

### 5. Configure the Puzzle Script

#### Required Settings:
- **Required Head ID**: Set to the ID of the correct head (default: 1)
- **Interaction Range**: Distance player needs to be within (default: 3)
- **Interaction Key**: Key to place head (default: F)

#### Dullahan Chase Integration (NEW!):
- **Freeze Dullahan When Player Has Head**: Check this (recommended: true)
  - When enabled, Dullahan stops moving the moment player picks up ANY head
  - Player can safely place the head without being chased
  - Dullahan resumes movement when head is placed or dropped
- **Dullahan Chase System**: Auto-found (optional manual assignment)
- **Dullahan Agent**: Auto-found (optional manual assignment)
- **Follow Dullahan Transform**: Check this to keep the puzzle anchor moving with the Dullahan so the placeholder stays on the neck (recommended: true)
- **Freeze Dullahan At Start**: Leave checked by default so he stays still until the player picks up a head

#### References:
- **Head Placeholder**: Drag the `HeadPlaceholder` GameObject here
- **Head Attachment Point**: Drag the `HeadAttachmentPoint` Transform here
- **Attached Head Visual**: (Optional) Pre-made head visual to show when correct head is placed

#### Materials (Optional but Recommended):
- **Placeholder Empty Material**: Semi-transparent material for empty state
- **Placeholder Valid Material**: Green/glowing material when holding correct head
- **Placeholder Invalid Material**: Red/warning material when holding wrong head

### 6. Configure the Placeholder Script

On the `HeadPlaceholder` GameObject:
- **Is Visible By Default**: Keep unchecked (false) - placeholder appears when player approaches
- **Use Fade Transition**: Check this for smooth fade in/out
- **Enable Pulsing**: Check this for animated pulsing effect
- **Enable Glow**: Check this for glow effect

## Creating Materials

### Empty Placeholder Material
1. Create new material: `Assets > Create > Material`
2. Name it: `Mat_PlaceholderEmpty`
3. Set Rendering Mode to: `Transparent`
4. Set Albedo color: White with low alpha (e.g., RGBA: 255, 255, 255, 50)
5. Optional: Enable Emission for glow effect

### Valid Placeholder Material
1. Create new material: `Assets > Create > Material`
2. Name it: `Mat_PlaceholderValid`
3. Set Rendering Mode to: `Transparent`
4. Set Albedo color: Green with medium alpha (e.g., RGBA: 0, 255, 0, 100)
5. Enable Emission with green color

### Invalid Placeholder Material
1. Create new material: `Assets > Create > Material`
2. Name it: `Mat_PlaceholderInvalid`
3. Set Rendering Mode to: `Transparent`
4. Set Albedo color: Red with medium alpha (e.g., RGBA: 255, 0, 0, 100)
5. Enable Emission with red color

## Adding Visual Effects (Optional)

### Completion Light
1. Add a Light component to the puzzle GameObject: `Add Component > Light`
2. Set Type to: `Point Light` or `Spot Light`
3. Set it to disabled initially
4. Drag this Light to the **Completion Light** field in the puzzle script

### Particles
1. Add particle systems for placement and completion effects
2. Stop them initially (uncheck "Play On Awake")
3. Drag them to **Placement Particles** and **Completion Particles** fields

## Adding Audio

### Audio Setup
1. The puzzle will auto-create an AudioSource if needed
2. Assign these audio clips in the inspector:
   - **Correct Head Sound**: Sound when correct head is placed
   - **Wrong Head Sound**: Sound when wrong head is placed
   - **Puzzle Complete Sound**: Celebratory sound when puzzle completes

## Adding UI Prompt

### Create Interaction UI
1. Create a Canvas if you don't have one: `Right-click in Hierarchy > UI > Canvas`
2. Create a Text element: `Right-click on Canvas > UI > Text - TextMeshPro`
3. Name it: `HeadPlacementPrompt`
4. Position it where you want the prompt (usually center bottom)
5. Set default text: "Press F to place head on Dullahan's body"
6. Add a Panel as background (optional)
7. Disable the prompt initially
8. Drag the prompt to **Interaction UI** field in puzzle script
9. Drag the TextMeshProUGUI component to **Interaction Text** field

## Adding Rewards

### Unlock Door
1. If you have a Door that should unlock when puzzle completes:
2. Drag that Door GameObject to the **Reward Door** field

### Spawn Items
1. Create prefabs of items to spawn (keys, health, etc.)
2. Add them to the **Reward Items** array
3. Create an empty GameObject for spawn location: `RewardSpawnPoint`
4. Drag it to **Reward Spawn Point** field

## Testing the Puzzle

### Test Checklist:
1. **Player Approach**
   - [ ] Placeholder appears when player gets close
   - [ ] Placeholder fades in smoothly (if fade enabled)
   - [ ] Interaction prompt appears

2. **No Head in Hand**
   - [ ] Prompt says "You need a head to place here"
   - [ ] Can't place anything

3. **Wrong Head in Hand**
   - [ ] Placeholder turns red/invalid material
   - [ ] Wrong head sound plays when placed
   - [ ] Wrong head appears briefly then disappears
   - [ ] Head is consumed from inventory
   - [ ] Placeholder returns

4. **Correct Head in Hand**
   - [ ] Placeholder turns green/valid material
   - [ ] Correct head sound plays when placed
   - [ ] Head attaches permanently
   - [ ] Puzzle complete sound plays
   - [ ] Completion light activates
   - [ ] Rewards are granted
   - [ ] Placeholder disappears

## Advanced Configuration

### Raycast vs Distance Interaction
- **Use Raycast Interaction** (checked): Player must look at the puzzle to interact
- **Use Raycast Interaction** (unchecked): Player just needs to be within range

### Fake Head Display
- **Show Fake Head Briefly**: Enable to show wrong heads temporarily
- **Fake Head Display Duration**: How long wrong head stays visible (seconds)

### Placeholder Behavior
- **Placeholder Initially Visible**: Start visible (not recommended)
- **Use Fade Transition**: Smooth fade in/out vs instant show/hide
- **Fade Duration**: Time for fade transition
- **Enable Pulsing**: Animated pulsing/breathing effect
- **Pulse Speed**: How fast the pulsing animation is
- **Pulse Min/Max Scale**: Scale range for pulsing

## Integration with Existing Systems

### DullahanHeadInventory
The puzzle automatically finds and uses the `DullahanHeadInventory` system. Make sure:
- Player has the `DullahanHeadInventory` component
- Player can pick up Dullahan heads
- Heads have `DullahanHeadSO` ScriptableObjects assigned

### Event Managers
The puzzle automatically notifies these event managers when completed:
- `Floor2EndingEventManager`
- `DullahanChaseEventManager`

No additional setup needed - it finds them automatically.

## Troubleshooting

### Placeholder Doesn't Appear
- Check that `placeholderInitiallyVisible` is false
- Check that player is within `interactionRange`
- Make sure placeholder GameObject is not disabled
- Check that placeholder has a Renderer component

### Can't Place Head
- Make sure player has `DullahanHeadInventory` component
- Check that player has picked up a head
- Verify `interactionKey` matches what you're pressing
- Make sure puzzle is not already completed

### Wrong Materials
- Check that materials are assigned in inspector
- Make sure materials have Transparent rendering mode
- Verify that placeholder has a Renderer component

### No Sound
- Check that audio clips are assigned
- Make sure AudioSource is not muted
- Check scene audio volume settings

### Rewards Not Working
- Make sure Door is assigned and has `Door` script
- Check that reward prefabs are valid
- Verify reward spawn point is positioned correctly

## Example Scene Setup

```
DullahanHeadPlacementPuzzle (0, 1, 0)
├── DullahanBodyVisual (your 3D model)
├── HeadAttachmentPoint (0, 1.8, 0)
├── HeadPlaceholder (0, 1.8, 0)
│   ├── Light (Point Light, disabled initially)
│   └── ParticleSystem (ambient particles)
├── PlacementParticles (particle system, Play On Awake: off)
├── CompletionParticles (particle system, Play On Awake: off)
└── CompletionLight (disabled initially)
```

## Script Reference

### Public Methods

#### DullahanHeadPlacementPuzzle
- `bool IsPuzzleCompleted()`: Check if puzzle is complete
- `void ResetPuzzle()`: Reset puzzle to initial state
- `void ForceComplete()`: Force complete puzzle (testing)

#### HeadPlaceholder
- `void Show(bool instant = false)`: Show placeholder
- `void Hide(bool instant = false)`: Hide placeholder
- `void SetState(PlaceholderState state)`: Set placeholder state
- `void ResetToDefault()`: Reset to default state
- `void Flash(Color flashColor, float duration)`: Flash effect
- `void PlayPulseAnimation()`: Play pulse animation

## Example Usage in Code

```csharp
// Check if puzzle is completed
DullahanHeadPlacementPuzzle puzzle = FindObjectOfType<DullahanHeadPlacementPuzzle>();
if (puzzle != null && puzzle.IsPuzzleCompleted())
{
    Debug.Log("Player completed the head placement puzzle!");
}

// Manually show/hide placeholder
HeadPlaceholder placeholder = FindObjectOfType<HeadPlaceholder>();
if (placeholder != null)
{
    placeholder.Show(); // Show placeholder
    placeholder.SetState(HeadPlaceholder.PlaceholderState.ValidHover);
}

// Reset puzzle for testing
puzzle.ResetPuzzle();
```

## Tips and Best Practices

1. **Visual Feedback**: Use distinct materials for empty/valid/invalid states
2. **Audio Cues**: Add clear audio feedback for actions
3. **UI Clarity**: Make interaction prompts clear and visible
4. **Testing**: Test with both correct and wrong heads
5. **Balance**: Adjust fake head display duration based on difficulty
6. **Accessibility**: Consider different interaction methods (raycast vs distance)

## Related Scripts

- `DullahanHeadInventory.cs`: Player inventory system
- `DullahanHeadSO.cs`: ScriptableObject for head data
- `DullahanBody.cs`: Original head attachment system
- `Floor2EndingEventManager.cs`: Event manager for floor 2

## Support

For issues or questions:
1. Check the Debug.Log messages in Console
2. Review this guide thoroughly
3. Ensure all required components are assigned
4. Test with simple setup first, then add complexity

---

**Version**: 1.0  
**Last Updated**: December 10, 2025  
**Compatible With**: DisManibus Unity Project

