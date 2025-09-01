# 4-Digit Combination Padlock Puzzle System

A complete puzzle system for Unity that allows you to create interactive 4-digit combination padlocks with hints, progression tracking, and multiple puzzle scenarios.

## 🎮 Features

- **4-Digit Combination Padlocks**: Interactive padlocks with customizable combinations
- **Puzzle Management**: Track multiple padlocks and puzzle progression
- **Hint System**: Place hint objects in the world to help players
- **Audio Feedback**: Sound effects for interactions and success/failure
- **UI Integration**: Clean, intuitive user interface
- **Event System**: Trigger events when puzzles are solved
- **Visual Effects**: Glow effects and material changes for interaction feedback

## 🚀 Quick Setup Guide

### Step 1: Create the Padlock UI

1. **Create Canvas**: Right-click in Hierarchy → UI → Canvas
2. **Create Padlock Panel**: Right-click Canvas → UI → Panel
   - Rename to "PadlockUI"
   - Set Anchor to center
   - Set size to 400x500

3. **Add UI Elements**:
   ```
   PadlockUI/
   ├── Title Text ("Combination Lock")
   ├── Combination Display Text ("----")
   ├── Message Text ("")
   ├── Number Grid (3x4 grid of buttons: 1-9, 0)
   ├── Enter Button
   ├── Clear Button
   └── Close Button
   ```

### Step 2: Setup the Padlock GameObject

1. **Create Padlock Object**: Create an empty GameObject
2. **Add Components**:
   - `CombinationPadlock` script
   - `AudioSource` component
   - `Collider` (for interaction range)

3. **Configure the Script**:
   - Set `correctCombination` (e.g., "1234")
   - Assign UI references from Step 1
   - Add audio clips for feedback

### Step 3: Create Hint Objects (Optional)

1. **Create Hint Object**: Create a GameObject with a visible mesh
2. **Add Components**:
   - `PuzzleHint` script
   - `AudioSource` component
   - `Renderer` component

3. **Configure the Script**:
   - Set `hintText` and `combinationHint`
   - Create hint UI similar to padlock UI
   - Add visual effects (glow, materials)

### Step 4: Setup Puzzle Manager

1. **Create Puzzle Manager**: Create an empty GameObject
2. **Add Component**: `PuzzleManager` script
3. **Configure**:
   - Add padlocks to the list
   - Set puzzle settings (sequential, require all, etc.)
   - Add hints array
   - Configure events

## 📋 Detailed Component Setup

### CombinationPadlock Component

**Padlock Settings:**
- `correctCombination`: The 4-digit code (e.g., "1234")
- `isLocked`: Whether the padlock starts locked
- `isSolved`: Whether the puzzle has been solved

**UI References:**
- `padlockUI`: The main UI panel
- `combinationDisplay`: Text showing current input (****)
- `messageText`: Text for feedback messages
- `numberButtons`: Array of 10 buttons (0-9)
- `enterButton`: Button to submit combination
- `clearButton`: Button to clear input
- `closeButton`: Button to close UI

**Audio:**
- `buttonClickSound`: Sound for button presses
- `correctSound`: Sound for correct combination
- `incorrectSound`: Sound for wrong combination
- `unlockSound`: Sound for unlocking

**Interaction:**
- `interactionDistance`: How close player needs to be
- `interactKey`: Key to interact (default: E)

### PuzzleManager Component

**Puzzle Settings:**
- `padlocks`: List of all padlocks in the puzzle
- `requireAllPadlocks`: Whether all padlocks must be solved
- `sequentialUnlocking`: Whether padlocks unlock in order

**Hints System:**
- `enableHints`: Whether hints are shown
- `hintDelay`: Time before hint appears (seconds)
- `puzzleHints`: Array of hint messages

**Events:**
- `onAllPuzzlesComplete`: Triggered when all puzzles are solved
- `onPuzzleProgress`: Triggered when any puzzle progresses
- `onPuzzleFailed`: Triggered when puzzle fails

### PuzzleHint Component

**Hint Settings:**
- `hintText`: General hint text
- `combinationHint`: Specific combination hint
- `isRevealed`: Whether hint has been revealed
- `canBeRevealed`: Whether hint can be revealed

**Display Settings:**
- `hintUI`: The hint UI panel
- `hintTextDisplay`: Text for general hint
- `combinationTextDisplay`: Text for combination hint
- `closeButton`: Button to close hint UI
- `revealButton`: Button to reveal hint

**Visual Effects:**
- `glowEffect`: GameObject that glows when in range
- `normalMaterial`: Default material
- `highlightedMaterial`: Material when highlighted
- `objectRenderer`: The object's renderer component

## 🎨 UI Layout Examples

### Padlock UI Layout
```
┌─────────────────────────┐
│    Combination Lock     │
├─────────────────────────┤
│        ****             │
│                         │
│    [1] [2] [3]          │
│    [4] [5] [6]          │
│    [7] [8] [9]          │
│       [0]               │
│                         │
│  [Enter] [Clear]        │
│                         │
│  [Close]                │
└─────────────────────────┘
```

### Hint UI Layout
```
┌─────────────────────────┐
│        Hint             │
├─────────────────────────┤
│ Look for clues in the   │
│ environment...          │
│                         │
│ [Reveal Hint]           │
│                         │
│ [Close]                 │
└─────────────────────────┘
```

## 🔧 Advanced Configuration

### Multiple Padlock Scenarios

**Scenario 1: Sequential Unlocking**
```csharp
// In PuzzleManager
requireAllPadlocks = true;
sequentialUnlocking = true;
```

**Scenario 2: Any Order**
```csharp
// In PuzzleManager
requireAllPadlocks = true;
sequentialUnlocking = false;
```

**Scenario 3: Single Padlock**
```csharp
// Just use CombinationPadlock without PuzzleManager
// Or set requireAllPadlocks = false
```

### Dynamic Combination Setting

```csharp
// In another script
public CombinationPadlock padlock;

void SetNewCombination()
{
    padlock.SetCombination("5678");
}
```

### Event Handling

```csharp
// In another script
public PuzzleManager puzzleManager;

void Start()
{
    puzzleManager.onAllPuzzlesComplete.AddListener(OnPuzzlesComplete);
}

void OnPuzzlesComplete()
{
    Debug.Log("All puzzles solved!");
    // Open door, trigger cutscene, etc.
}
```

## 🎵 Audio Setup

### Recommended Audio Clips
- **Button Click**: Short, mechanical click sound
- **Correct**: Success chime or unlock sound
- **Incorrect**: Error buzzer or failure sound
- **Unlock**: Mechanical unlock sound
- **Examine**: Paper rustle or object interaction sound
- **Reveal**: Magical or discovery sound

### Audio Settings
- Set `AudioSource` volume to 0.5-0.8
- Use 3D sound for spatial audio
- Keep clips short (0.1-2 seconds)

## 🎨 Visual Effects

### Glow Effect Setup
1. Create a child GameObject with a sphere mesh
2. Add a `Light` component
3. Set color to cyan or yellow
4. Set intensity to 1-2
5. Assign to `glowEffect` in PuzzleHint

### Material Setup
1. Create two materials: normal and highlighted
2. Normal: Standard material
3. Highlighted: Same material with emission or different color
4. Assign to `normalMaterial` and `highlightedMaterial`

## 🐛 Troubleshooting

### Common Issues

**UI not showing:**
- Check Canvas is set to Screen Space - Overlay
- Verify UI elements are active
- Check UI references in inspector

**Interaction not working:**
- Ensure player has "Player" tag
- Check interaction distance
- Verify Input.GetKeyDown is working

**Audio not playing:**
- Check AudioSource component is attached
- Verify audio clips are assigned
- Check volume settings

**Visual effects not working:**
- Ensure renderer component is assigned
- Check materials are assigned
- Verify glow effect GameObject is active

### Debug Tips

1. **Enable Gizmos**: Select padlock/hint objects to see interaction ranges
2. **Check Console**: Look for debug messages
3. **Test in Play Mode**: All interactions only work during play
4. **Verify Tags**: Ensure player has correct tag

## 🎮 Example Usage Scenarios

### Scenario 1: Office Safe
- Place padlock on a safe
- Hide combination in office documents
- Use PuzzleHint on documents
- Require all hints to be found

### Scenario 2: Multiple Doors
- Create 3 padlocks on different doors
- Each door leads to different area
- Use sequential unlocking for story progression
- Add hints in previous areas

### Scenario 3: Escape Room
- Multiple padlocks in one room
- Hints scattered around room
- All must be solved to escape
- Add time pressure with hint delays

## 🔄 Integration with Existing Systems

### With Player Health System
```csharp
// In CombinationPadlock
public PlayerHealthSystem playerHealth;

void OnWrongCombination()
{
    // Optional: Damage player for wrong attempts
    if (playerHealth != null)
    {
        playerHealth.TakeDamage(10f);
    }
}
```

### With Lighting System
```csharp
// In PuzzleManager
public SimpleLightingController lighting;

void OnPuzzleComplete()
{
    // Change lighting when puzzle is solved
    if (lighting != null)
    {
        lighting.MakeItNormal();
    }
}
```

### With Scene Management
```csharp
// In PuzzleManager
void OnAllPuzzlesComplete()
{
    // Load next scene when all puzzles solved
    UnityEngine.SceneManagement.SceneManager.LoadScene("NextLevel");
}
```

## 📝 Best Practices

1. **Test Thoroughly**: Test all combinations and edge cases
2. **Use Clear Hints**: Make hints logical and solvable
3. **Balance Difficulty**: Don't make puzzles too easy or hard
4. **Provide Feedback**: Always give player feedback on actions
5. **Save Progress**: Consider saving puzzle state between sessions
6. **Accessibility**: Ensure puzzles are accessible to all players
7. **Performance**: Keep UI updates efficient
8. **Documentation**: Document custom puzzle setups

## 🎯 Performance Tips

- Use object pooling for multiple padlocks
- Optimize UI updates (only when needed)
- Use efficient audio compression
- Minimize material switches
- Cache component references

This system provides a solid foundation for creating engaging puzzle experiences in your Unity game! 