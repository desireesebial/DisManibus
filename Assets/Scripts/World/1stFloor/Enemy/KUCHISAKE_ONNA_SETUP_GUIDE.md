# Kuchisake-onna Enemy Setup Guide

This guide will walk you through setting up the Kuchisake-onna (Slit-Mouthed Woman) enemy in your Unity scene with the timer-death mechanic.

---

## Table of Contents
1. [Prerequisites](#prerequisites)
2. [Enemy Setup](#enemy-setup)
3. [First Encounter Setup](#first-encounter-setup)
4. [UI Setup](#ui-setup)
5. [Player Setup](#player-setup)
6. [Audio Setup](#audio-setup)
7. [Testing](#testing)
8. [Customization](#customization)
9. [Troubleshooting](#troubleshooting)

---

## Prerequisites

### Required Unity Packages
- **TextMeshPro** - For UI text
- **NavMesh Components** - For AI pathfinding
- **Unity UI** - For buttons and panels

### Tags Required
- `Player` tag on your player GameObject

### Scripts Included
- `KuchisakeOnnaController.cs` - Main enemy AI
- `KuchisakeQuestionUI.cs` - Question UI system
- `PlayerHealth.cs` - Player health/death system

---

## Enemy Setup

### Step 1: Create the Enemy GameObject

1. Create an empty GameObject: `Right-click in Hierarchy → Create Empty`
2. Name it `KuchisakeOnna`
3. Add the following components:
   - **NavMesh Agent** (Component → Navigation → NavMesh Agent)
   - **Audio Source** (Component → Audio → Audio Source)
   - **Capsule Collider** (for physical presence)

### Step 2: Add Enemy Visual

**Option A: Simple Capsule (Quick Setup)**
1. Create a child Capsule: `Right-click KuchisakeOnna → 3D Object → Capsule`
2. Scale to approximately: `(0.5, 1, 0.5)`
3. Create a red material for the coat:
   - Right-click in Project → Create → Material → Name it `RedCoatMaterial`
   - Set Albedo color to red
   - Apply to capsule

**Option B: Custom Model (Recommended)**
1. Import your character model
2. Place as child of KuchisakeOnna GameObject
3. Ensure the model faces forward (Z-axis)

### Step 3: Create Mask and Face

1. **Create Mask GameObject:**
   - Add a child Quad: `Right-click KuchisakeOnna → 3D Object → Quad`
   - Name it `Mask`
   - Position in front of where face would be
   - Scale to cover face area
   - Create and apply a white mask texture/material

2. **Create Face Materials:**
   - Create material: `NormalFaceMaterial` (normal face texture)
   - Create material: `SlitMouthMaterial` (face with slit mouth)
   - These will swap when mask is removed

### Step 4: Configure NavMesh Agent

1. Select KuchisakeOnna GameObject
2. In NavMesh Agent component:
   - **Speed**: `2` (will be controlled by script)
   - **Angular Speed**: `120`
   - **Acceleration**: `8`
   - **Stopping Distance**: `1`
   - **Auto Braking**: ✓ (checked)
   - **Radius**: `0.5`
   - **Height**: `2`

### Step 5: Create Patrol Points

1. Create empty GameObjects for patrol points:
   - `Right-click in Hierarchy → Create Empty`
   - Name them: `PatrolPoint1`, `PatrolPoint2`, etc.
2. Position them around the area you want the enemy to patrol
3. Minimum 2 points, recommended 4-6 points
4. Make sure they're on the NavMesh surface

### Step 6: Attach and Configure Script

1. Add `KuchisakeOnnaController.cs` to KuchisakeOnna GameObject
2. Configure in Inspector:

**AI Settings:**
- **Patrol Points**: Drag all patrol point GameObjects here (she'll patrol after first encounter)
- **Patrol Speed**: `2`
- **Chase Speed**: `4`
- **Detection Range**: `8`
- **Chase Timeout**: `30`
- **Escape Distance**: `20`

**Question Settings:**
- **Question Timer**: `5` (seconds)
- **Chase Chance On Yes**: `0.7` (70% chase even if player says "Yes")

**Visual Effects:**
- **Mask Object**: Drag the Mask GameObject here
- **Normal Face Material**: Drag NormalFaceMaterial here
- **Slit Mouth Material**: Drag SlitMouthMaterial here
- **Face Renderer**: Drag the renderer showing the face

**First Encounter:** (see dedicated section below)
- **Require First Encounter**: ✓ (checked)
- **First Encounter Position**: Will setup next
- **First Encounter Trigger Distance**: `5`
- **Animator**: Drag animator component if using animations
- **Sitting Animation Trigger**: `"Sitting"`
- **Stand Up Animation Trigger**: `"StandUp"`
- **Stand Up Duration**: `2` (seconds)

**References:**
- **Question UI**: Will assign after UI setup
- **Player**: Leave empty (auto-finds player with "Player" tag)

---

## First Encounter Setup

This section covers the atmospheric first encounter where Kuchisake-onna is sitting and waiting for the player.

### Concept Overview

**The First Encounter Experience:**
1. Player explores area
2. Encounters sitting Kuchisake-onna (no jumpscare)
3. When player gets close, she **slowly** turns to face them
4. After brief tension, she asks "Am I beautiful?"
5. Player answers (or times out)
6. She **slowly** removes mask, revealing slit mouth
7. She **slowly** stands up (eerie, not rushed)
8. After standing, she becomes an active enemy and patrols
9. Future encounters = instant death if timer runs out

**Key Design Philosophy:**
- ✅ Slow, deliberate movements (unsettling)
- ✅ Build tension through stillness
- ✅ No sudden jumpscares
- ✅ Player sees the threat coming
- ✅ Memorable first impression
- ❌ No screaming or rushing at player
- ❌ No teleporting in front of player

### Step 1: Create First Encounter Location

1. **Find a good spot in your level:**
   - Hallway corner
   - End of corridor
   - Room entrance
   - Against a wall
   - Somewhere player will naturally walk toward

2. **Create the encounter marker:**
   - `Right-click in Hierarchy → Create Empty`
   - Name it `KuchisakeFirstEncounter`
   - Position it where you want her to sit
   - Rotate it to face the direction player will approach from

3. **Visual setup:**
   - The enemy GameObject will be teleported here on Start
   - She'll sit here waiting for player
   - When player enters trigger radius, encounter begins

### Step 2: Configure First Encounter Settings

1. Select KuchisakeOnna GameObject
2. In `KuchisakeOnnaController` component:

**First Encounter Settings:**
- **Require First Encounter**: ✓ (checked)
- **First Encounter Position**: Drag `KuchisakeFirstEncounter` GameObject here
- **First Encounter Trigger Distance**: `5` (adjust based on space)
  - Smaller value = player must get closer
  - Larger value = triggers from farther away
  - Recommended: 4-6 for hallways, 6-8 for large rooms

### Step 3: Setup Sitting Pose

**Option A: With Animator (Recommended)**

If you have sitting/standing animations:

1. Add **Animator** component to KuchisakeOnna
2. Create Animator Controller:
   - `Right-click in Project → Create → Animator Controller`
   - Name it `KuchisakeAnimator`
3. Add animation states:
   - **Sitting** (idle sitting loop)
   - **StandUp** (transition from sit to stand)
   - **Idle** (normal standing idle)
4. Create transitions:
   - Any State → Sitting (trigger: "Sitting")
   - Sitting → StandUp (trigger: "StandUp")
   - StandUp → Idle (exit time)
5. Assign to KuchisakeOnna:
   - **Animator**: Drag Animator component
   - **Sitting Animation Trigger**: `"Sitting"`
   - **Stand Up Animation Trigger**: `"StandUp"`
   - **Stand Up Duration**: Match your animation length

**Option B: Without Animator (Simple)**

If you don't have animations:

1. Manually position the enemy in sitting pose:
   - Lower Y position (sitting height)
   - Tilt forward slightly
   - Position legs/body to look seated
2. In script settings:
   - Leave **Animator** empty (None)
   - **Stand Up Duration**: `2` (time for smooth lerp to standing)
3. Script will smoothly move her to standing position

**Option C: Really Simple (Static)**

Just have her standing from the start:
- **Require First Encounter**: ✓ (still checked)
- **Animator**: None
- **Stand Up Duration**: `0.5` (very quick)
- She'll just turn and ask question

### Step 4: Test First Encounter Flow

1. Press Play
2. Walk toward first encounter location
3. You should see:
   - ✓ Enemy is at first encounter position
   - ✓ No patrol behavior yet
   - ✓ No ambient scissor sounds yet
   - ✓ Enemy slowly turns to face you when close
   - ✓ After ~1 second, question appears
   - ✓ Timer counts down

4. Answer the question:
   - **"Yes"**: Mask removes slowly → she stands → begins patrolling
   - **"No"**: Mask removes → anger sound → stands faster → chases you
   - **"Maybe"**: Mask removes → tilts head → stands → patrols
   - **Timeout**: Mask removes → stands → begins patrolling (mercy on first time)

5. After first encounter:
   - ✓ She now patrols the map
   - ✓ Ambient scissor sounds start
   - ✓ Future encounters = death if timer expires
   - ✓ Normal chase behavior active

### Step 5: Visual Polish for First Encounter

**Lighting:**
- Add dim spotlight on her sitting position
- Single lamp/candle nearby
- Creates silhouette effect
- Player sees her from distance

**Environment:**
- Place her against wall or in corner
- Maybe a chair (optional prop)
- Blood stains nearby (optional)
- Creates unsettling scene

**Camera Effects:**
When player gets close, consider adding:
- Slight vignette increase
- Subtle film grain
- Muffled audio (optional)
- Heartbeat sound start

### Step 6: Adjust Timing for Atmosphere

The timing is crucial for building dread:

**Slow Timing (More Atmospheric):**
```
firstEncounterTriggerDistance: 6-8
standUpDuration: 3
DelayedFirstQuestion: 2 seconds
```

**Medium Timing (Balanced):**
```
firstEncounterTriggerDistance: 5
standUpDuration: 2
DelayedFirstQuestion: 1 second
```

**Fast Timing (More Action):**
```
firstEncounterTriggerDistance: 3-4
standUpDuration: 1
DelayedFirstQuestion: 0.5 seconds
```

### Step 7: Optional - Disable First Encounter

If you want to skip straight to patrol behavior:

1. Select KuchisakeOnna GameObject
2. **Require First Encounter**: ✗ (uncheck)
3. She'll start patrolling immediately when scene loads

---

## UI Setup

### Step 1: Create Canvas

1. `Right-click in Hierarchy → UI → Canvas`
2. Name it `KuchisakeUI`
3. Set Canvas settings:
   - **Render Mode**: Screen Space - Overlay
   - **Canvas Scaler**: Scale With Screen Size
   - **Reference Resolution**: `1920 x 1080`

### Step 2: Create Question Panel

1. `Right-click Canvas → UI → Panel`
2. Name it `QuestionPanel`
3. Settings:
   - **Color**: Semi-transparent black (R:0, G:0, B:0, A:200)
   - **Anchor**: Stretch to full screen

### Step 3: Add Question Text

1. `Right-click QuestionPanel → UI → Text - TextMeshPro`
2. Name it `QuestionText`
3. Settings:
   - **Text**: "Am I beautiful?"
   - **Font Size**: `48`
   - **Alignment**: Center
   - **Color**: White
   - Position at top-center of panel

### Step 4: Add Timer Display

1. **Timer Text:**
   - `Right-click QuestionPanel → UI → Text - TextMeshPro`
   - Name it `TimerText`
   - Settings:
     - **Text**: "5"
     - **Font Size**: `72`
     - **Alignment**: Center
     - **Color**: White
   - Position below question text

2. **Timer Bar (Optional but recommended):**
   - `Right-click QuestionPanel → UI → Slider`
   - Name it `TimerBar`
   - Remove the handle (delete Handle Slide Area child)
   - Configure:
     - **Direction**: Left to Right
     - **Min Value**: 0
     - **Max Value**: 1
     - **Interactable**: ✗ (unchecked)
   - Style the Fill area to be red/danger color
   - Position below timer text

### Step 5: Create Answer Buttons

Create 3 buttons:

1. **Yes Button:**
   - `Right-click QuestionPanel → UI → Button - TextMeshPro`
   - Name it `YesButton`
   - Button text: "Yes"
   - Color: Green tint
   - Position: Bottom left

2. **No Button:**
   - Create similar to Yes Button
   - Name it `NoButton`
   - Button text: "No"
   - Color: Red tint
   - Position: Bottom right

3. **Maybe Button:**
   - Create similar to previous buttons
   - Name it `MaybeButton`
   - Button text: "I don't know..."
   - Color: Yellow tint
   - Position: Bottom center

Button Settings (for all):
- **Width**: `250`
- **Height**: `60`
- **Font Size**: `24`

### Step 6: Create Death Screen

1. `Right-click Canvas → UI → Panel`
2. Name it `DeathScreen`
3. Settings:
   - **Color**: Black (R:0, G:0, B:0, A:255)
   - **Anchor**: Stretch to full screen

4. Add Death Text:
   - `Right-click DeathScreen → UI → Text - TextMeshPro`
   - Name it `DeathText`
   - Settings:
     - **Font Size**: `48`
     - **Alignment**: Center
     - **Color**: Red
   - Center on screen

5. Add Death Flash Image:
   - `Right-click DeathScreen → UI → Image`
   - Name it `DeathFlash`
   - Settings:
     - **Color**: Red (R:255, G:0, B:0, A:0) ← Alpha at 0
     - **Anchor**: Stretch to full screen
     - **Order**: Above other elements

### Step 7: Configure UI Script

1. Create empty GameObject as child of Canvas
2. Name it `UIController`
3. Add `KuchisakeQuestionUI.cs` script
4. Configure in Inspector:

**UI References:**
- **Question Panel**: Drag QuestionPanel
- **Question Text**: Drag QuestionText
- **Timer Text**: Drag TimerText
- **Yes Button**: Drag YesButton
- **No Button**: Drag NoButton
- **Maybe Button**: Drag MaybeButton
- **Timer Bar Fill**: Drag the Fill component from TimerBar

**Death Screen:**
- **Death Screen**: Drag DeathScreen
- **Death Text**: Drag DeathText
- **Death Flash Image**: Drag DeathFlash

**Audio:**
- **UI Audio Source**: Add AudioSource component, then drag it here
- Assign audio clips (see Audio Setup section)

**Settings:**
- **Question String**: "Am I beautiful?"
- **Timer Normal Color**: White
- **Timer Warning Color**: Yellow
- **Timer Danger Color**: Red
- **Heartbeat Start Time**: `3`

### Step 8: Link UI to Enemy

1. Select KuchisakeOnna GameObject
2. In KuchisakeOnnaController component:
   - **Question UI**: Drag the UIController GameObject here

---

## Player Setup

### Step 1: Add PlayerHealth Script

1. Select your Player GameObject
2. Add `PlayerHealth.cs` script
3. Configure:

**Health Settings:**
- **Max Health**: `100`

**Death Settings:**
- **Respawn Delay**: `3` seconds
- **Respawn Point**: Create empty GameObject at spawn location, drag here
- **Reload Scene On Death**: ✓ or ✗ (your preference)

**Audio:**
- **Audio Source**: Drag player's AudioSource
- **Death Sound**: Assign audio clip

### Step 2: Ensure Player Tag

1. Select Player GameObject
2. Set Tag to `Player` (top of Inspector)

---

## Audio Setup

### Required Audio Clips

You'll need these sound effects (can find free ones on freesound.org):

1. **Scissors Snip** - For ambient sounds and death
2. **Question Voice** - "Am I beautiful?" voice line
3. **Death Scream** - Enemy scream when killing player
4. **Anger Sound** - Angry reaction
5. **Retreat Laugh** - When she retreats
6. **Heartbeat** - Tension builder during timer
7. **Scissor Close** - Sharp scissor close sound
8. **Button Click** - UI button feedback
9. **Player Death** - Player death sound

### Importing Audio

1. Drag audio files into `Assets/Audio/` folder (create if needed)
2. Select each audio file
3. Configure:
   - **Load Type**: Compressed In Memory (for small files)
   - **Compression Format**: Vorbis (for most sounds)
   - **Quality**: 70-100

### Assigning Audio

**KuchisakeOnna GameObject:**
1. Select KuchisakeOnnaController component
2. Assign audio clips:
   - **Scissors Snip Sound**
   - **Question Voice Clip**
   - **Death Scream Clip**
   - **Anger Sound**
   - **Retreat Laugh Sound**

**UIController GameObject:**
1. Select KuchisakeQuestionUI component
2. Assign audio clips:
   - **Heartbeat Sound**
   - **Scissor Close Sound**
   - **Button Click Sound**

**Player GameObject:**
1. Select PlayerHealth component
2. Assign:
   - **Death Sound**

---

## Testing

### Step 1: Bake NavMesh

1. `Window → AI → Navigation`
2. Go to **Bake** tab
3. Configure settings:
   - **Agent Radius**: `0.5`
   - **Agent Height**: `2`
   - **Max Slope**: `45`
4. Click **Bake**
5. Ensure patrol points are on NavMesh (blue overlay)

### Step 2: Initial Test

**First Encounter Test:**
1. Press Play
2. Walk toward first encounter position
3. Check:
   - ✓ Enemy is sitting/waiting at encounter position
   - ✓ NO ambient sounds yet
   - ✓ NO patrol behavior yet
   - ✓ When you get close, she slowly turns
   - ✓ Question appears after brief delay
   - ✓ Timer counts down correctly

**Post-First Encounter Test:**
1. Answer the question (any answer)
2. Wait for her to stand
3. Check:
   - ✓ She stands up (animation or position change)
   - ✓ NavMesh agent activates
   - ✓ She begins patrolling
   - ✓ Ambient scissor sounds start playing
   - ✓ Normal behavior is now active

### Step 3: Test Each Answer

**Test "Yes" Answer:**
1. Approach enemy
2. Click "Yes" when asked
3. Should either:
   - Chase you (70% chance)
   - Let you go (30% chance)

**Test "No" Answer:**
1. Approach enemy
2. Click "No"
3. Should immediately chase

**Test "Maybe" Answer:**
1. Approach enemy
2. Click "Maybe"
3. Should pause, then chase

**Test Timer Expiration:**

*First Encounter Timer:*
1. Start new game/reset
2. Approach sitting enemy
3. Don't answer question
4. When timer hits 0:
   - She removes mask
   - Makes angry sound
   - Stands up
   - Begins patrolling
   - **Player survives** (mercy on first encounter)

*Subsequent Encounter Timer:*
1. After first encounter, let her find you again
2. Don't answer the question
3. When timer hits 0:
   - Instant death
   - Death screen
   - Respawn

### Step 4: Test Chase Behavior

1. Answer question to trigger chase
2. Check:
   - ✓ Enemy follows you
   - ✓ Enemy catches and kills you if close
   - ✓ You can escape if you run far enough
   - ✓ Chase times out after 30 seconds

---

## Customization

### Difficulty Adjustment

**Easy Mode:**
```csharp
Question Timer: 7-8 seconds
Chase Speed: 3.5
Detection Range: 6
Chase Chance On Yes: 0.5 (50%)
```

**Normal Mode (Default):**
```csharp
Question Timer: 5 seconds
Chase Speed: 4
Detection Range: 8
Chase Chance On Yes: 0.7 (70%)
```

**Hard Mode:**
```csharp
Question Timer: 3 seconds
Chase Speed: 5
Detection Range: 10
Chase Chance On Yes: 0.9 (90%)
```

**Nightmare Mode:**
```csharp
Question Timer: 2 seconds
Chase Speed: 6
Detection Range: 12
Chase Chance On Yes: 1.0 (100%)
```

### Visual Enhancements

**Add Fog Effect:**
1. Add Particle System to KuchisakeOnna
2. Configure:
   - Shape: Sphere
   - Emission: 10
   - Start Size: 2-4
   - Start Color: Gray/white with transparency
   - Simulation Space: World

**Add Glowing Eyes:**
1. Create two small spheres as children
2. Apply emissive material (bright color)
3. Position where eyes would be
4. Add Point Light component to each

**Screen Effects When Near:**
1. Add Post-Processing to your camera
2. Create volume profile with:
   - Vignette (darkens edges)
   - Chromatic Aberration
   - Film Grain
3. Script can enable/disable based on distance

### Behavior Variations

**Multiple Questions:**
Modify `KuchisakeQuestionUI.cs`:
- Add second question: "Even like this?"
- Show after first answer
- Creates two-stage encounter

**Random Timer:**
Modify `KuchisakeOnnaController.cs` line with `questionTimer`:
```csharp
float randomTimer = Random.Range(3f, 7f);
questionUI.ShowQuestion(randomTimer, this);
```

**Escalating Encounters:**
Add counter to track encounters:
```csharp
private int encounterCount = 0;

void StartQuestionSequence() {
    encounterCount++;
    float timer = Mathf.Max(2f, 5f - encounterCount * 0.5f);
    // Timer gets shorter each encounter
}
```

---

## Troubleshooting

### Enemy Not Moving

**Problem:** Enemy stands still at start
**Solutions:**
- If **Require First Encounter** is checked:
  - This is CORRECT behavior - she waits for player
  - Walk toward her to trigger encounter
  - After first encounter, she'll patrol
- If she should be patrolling immediately:
  - Uncheck **Require First Encounter**
  - Check NavMesh is baked
  - Ensure patrol points are on NavMesh
  - Verify agent speed > 0

**Problem:** Enemy doesn't move after first encounter
**Solutions:**
- Check stand up animation completed
- Verify NavMesh Agent is being re-enabled
- Ensure patrol points are assigned
- Check Console for errors

### Question UI Not Showing

**Problem:** UI doesn't appear when enemy approaches
**Solutions:**
- Check Question UI reference is assigned in enemy controller
- Ensure QuestionPanel is initially inactive (unchecked)
- Verify Canvas is set to Screen Space - Overlay
- Check player has "Player" tag

### Timer Not Counting Down

**Problem:** Timer stays at same number
**Solutions:**
- Check `isQuestionActive` is being set to true
- Ensure Time.timeScale is not 0 (game not paused)
- Verify timer text is assigned in UI script

### Player Not Dying on Timer Expiration

**Problem:** Timer expires but nothing happens
**Solutions:**
- Check PlayerHealth script is on player
- Ensure OnQuestionTimeout() is being called
- Verify death screen is assigned
- Check audio sources exist

### Enemy Can't Find Player

**Problem:** Detection doesn't work
**Solutions:**
- Ensure player has "Player" tag
- Check detection range is large enough
- Verify layer masks aren't blocking raycast
- Increase detection range temporarily for testing

### Chase Not Working

**Problem:** Enemy doesn't chase after question
**Solutions:**
- Check NavMesh Agent speed is set correctly
- Ensure agent.isStopped is set to false
- Verify player is on NavMesh or near it
- Check chase state is being triggered

### Buttons Not Responding

**Problem:** Can't click answer buttons
**Solutions:**
- Check Canvas has GraphicRaycaster component
- Verify EventSystem exists in scene
- Ensure buttons are enabled
- Check button listeners are added in Start()

### Audio Not Playing

**Problem:** No sound effects
**Solutions:**
- Check AudioSource components exist
- Verify audio clips are assigned
- Ensure volume is > 0
- Check Audio Listener exists on camera
- Verify audio files imported correctly

---

## Advanced Features

### Save Last Answer

Track what player answered previously:
```csharp
private Answer lastAnswer;

public void OnPlayerAnswered(Answer answer) {
    lastAnswer = answer;
    // Make enemy more aggressive if player lies
}
```

### Dynamic Question Text

Change question based on game state:
```csharp
string[] questions = {
    "Am I beautiful?",
    "Do you think I'm pretty?",
    "You think I'm ugly, don't you?"
};
questionText.text = questions[Random.Range(0, questions.Length)];
```

### Multiple Enemy Instances

If you want multiple Kuchisake-onna enemies:
1. Duplicate the enemy GameObject
2. Assign different patrol points to each
3. Use the same UI controller (they'll take turns)
4. Adjust detection ranges so they don't overlap

---

## Performance Tips

1. **Disable when far from player:**
```csharp
void Update() {
    if (Vector3.Distance(transform.position, player.position) > 50f) {
        this.enabled = false;
    }
}
```

2. **Reduce NavMesh update frequency:**
```csharp
agent.updateRotation = false; // Handle rotation manually
```

3. **Use object pooling for particles:**
Instead of Instantiate(), pool particle effects

4. **LOD for enemy model:**
Use Unity LOD Group for distance-based quality

---

## Credits & Notes

**Kuchisake-onna** is a Japanese urban legend about a woman who covers her mouth with a surgical mask and asks "Am I beautiful?" The legend varies, but the core concept involves answering correctly to avoid death.

This implementation is designed for indie horror games with:
- Simple AI using Unity's NavMesh
- UI-driven encounters
- Audio-focused horror
- Performance-friendly approach

Feel free to modify and expand upon this system!

---

## Quick Reference

### Inspector Checklist

**KuchisakeOnna GameObject:**
- ✓ NavMesh Agent component
- ✓ Audio Source component
- ✓ KuchisakeOnnaController script
- ✓ Patrol points assigned (minimum 2)
- ✓ First encounter position assigned
- ✓ First encounter trigger distance set (4-6)
- ✓ Mask object assigned
- ✓ Face materials assigned
- ✓ Question UI reference assigned
- ✓ Animator (optional, for sitting/standing)

**UIController GameObject:**
- ✓ KuchisakeQuestionUI script
- ✓ All UI elements assigned
- ✓ Audio source and clips assigned
- ✓ Timer settings configured

**Player GameObject:**
- ✓ "Player" tag
- ✓ PlayerHealth script
- ✓ Respawn point assigned
- ✓ Audio source and clips assigned

**Scene:**
- ✓ NavMesh baked
- ✓ Canvas with EventSystem exists
- ✓ Camera has Audio Listener

---

## Support

If you encounter issues not covered in troubleshooting:
1. Check Unity Console for errors
2. Verify all references are assigned
3. Test with Debug.Log() statements
4. Ensure Unity version compatibility

Good luck with your horror game! 👻

