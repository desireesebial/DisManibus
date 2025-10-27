# CutsceneDelayedStart - Usage Guide

## Overview

`CutsceneDelayedStart.cs` is a utility script that prevents physics initialization race conditions when cutscenes auto-play on scene load. It ensures the scene is fully initialized before starting cutscene playback.

## When to Use This Script

Use this script whenever you have a cutscene that:
- Plays automatically when a scene loads
- Affects or disables the player controller
- Might cause the player to fall through the floor or behave incorrectly
- Needs to wait for physics colliders to initialize

## Problem It Solves

**Without this script:**
```
Scene Load → Cutscene Auto-Plays → Player Controller Disabled →
Physics Not Ready → Player Falls Through Floor
```

**With this script:**
```
Scene Load → Wait for Initialization → Physics Ready →
Cutscene Plays → Everything Works Correctly
```

## Quick Setup (3 Steps)

### 1. Prepare Your PlayableDirector
- Select the GameObject with your PlayableDirector component
- Set **"Initial State"** to **"Paused"** (not "Playing")
  - This prevents the cutscene from auto-playing immediately

### 2. Add the Script
- Click **"Add Component"** on the same GameObject
- Search for and add **"CutsceneDelayedStart"**

### 3. Configure (Optional)
The script auto-configures, but you can adjust:
- **Startup Delay**: How long to wait (default: 0.1s)
- **Play On Start**: Whether to auto-play (default: true)
- **Enable Debug Logs**: Show timing messages (default: true)

That's it! Your cutscene will now play after a safe initialization delay.

---

## Component Settings Explained

### Director
- **What**: Reference to the PlayableDirector to control
- **Default**: Auto-finds the PlayableDirector on the same GameObject
- **When to set manually**: If your PlayableDirector is on a different GameObject

### Startup Delay
- **What**: Delay in seconds before playing the cutscene
- **Default**: 0.1 seconds
- **Range**: 0.05 - 1.0 seconds
- **Adjust when**:
  - Increase if player still falls through floor (try 0.2-0.3s)
  - Decrease if you want cutscene to start faster (minimum 0.05s)

### Play On Start
- **What**: Should the cutscene play automatically when the scene loads?
- **Default**: True (checked)
- **Uncheck when**: You want to trigger the cutscene manually via script or trigger

### Enable Debug Logs
- **What**: Show detailed logging in the Console during cutscene startup
- **Default**: True (checked)
- **Useful for**: Debugging timing issues, verifying initialization
- **Uncheck when**: You're done testing and want cleaner Console output

---

## Manual Triggering (Advanced)

If you uncheck "Play On Start", you can trigger the cutscene manually:

### From Another Script:
```csharp
// Get reference to the script
CutsceneDelayedStart cutsceneStarter = cutsceneObject.GetComponent<CutsceneDelayedStart>();

// Play immediately
cutsceneStarter.PlayCutscene();

// Or play with custom delay
cutsceneStarter.PlayCutsceneWithDelay(0.5f); // 0.5 second delay
```

### From Unity Events:
1. Add a UI Button or trigger GameObject
2. In the Inspector, find the OnClick() or OnTriggerEnter() event
3. Click the **+** button to add a listener
4. Drag the GameObject with CutsceneDelayedStart into the Object field
5. Select **CutsceneDelayedStart → PlayCutscene()**

---

## How It Works (Technical)

The script follows this sequence:

1. **Awake()**:
   - Auto-finds PlayableDirector if not assigned
   - Ensures director is paused initially

2. **Start()**:
   - Starts the delayed playback coroutine (if playOnStart is true)

3. **StartCutsceneAfterDelay() Coroutine**:
   - Waits for `startupDelay` seconds
   - Waits one additional `WaitForFixedUpdate()` frame
     - This ensures physics has run at least once
   - Calls `director.Play()` to start the cutscene

### Why WaitForFixedUpdate()?
Unity's physics engine runs in FixedUpdate(). By waiting for one FixedUpdate frame, we ensure:
- All colliders are initialized
- Rigidbodies are properly set up
- Ground detection raycast will work correctly
- No objects fall through floors due to uninitialized colliders

---

## Common Use Cases

### Use Case 1: Intro Cutscene on Scene Load
**Scenario**: You want an intro cutscene to play when the level starts

**Setup**:
- PlayableDirector: Initial State = "Paused"
- CutsceneDelayedStart: Play On Start = ✓, Startup Delay = 0.1s

**Result**: Cutscene plays automatically after a brief initialization delay

### Use Case 2: Cutscene Triggered by Player
**Scenario**: Cutscene plays when player enters a trigger zone

**Setup**:
- PlayableDirector: Initial State = "Paused"
- CutsceneDelayedStart: Play On Start = ✗, Startup Delay = 0.05s
- Trigger script calls `PlayCutscene()` or `PlayCutsceneWithDelay()`

**Result**: Cutscene plays when triggered, with optional delay for smooth transition

### Use Case 3: Multiple Cutscenes in One Scene
**Scenario**: Scene has multiple cutscenes that play at different times

**Setup**:
- Each PlayableDirector GameObject gets its own CutsceneDelayedStart
- Set "Play On Start" based on which should auto-play
- Use manual triggering for others

**Result**: Each cutscene can be controlled independently with safe initialization

---

## Troubleshooting

### Issue: Script not working / Cutscene doesn't play
**Check**:
- PlayableDirector "Initial State" is set to "Paused"
- "Play On Start" is checked in CutsceneDelayedStart
- PlayableDirector has a valid Timeline asset assigned
- Console shows debug logs (if enabled)

**Fix**:
- Enable Debug Logs and check Console for error messages
- Verify Director field is populated in Inspector

### Issue: Cutscene starts too late / noticeable delay
**Check**:
- Startup Delay value

**Fix**:
- Reduce Startup Delay to 0.05 seconds (minimum safe value)
- Only use 0.1s+ if you're having physics issues

### Issue: Player still falls through floor
**Check**:
- Floor colliders are active and properly configured
- Player Rigidbody is not kinematic
- Collision layer matrix allows Player-Floor collision

**Fix**:
- Increase Startup Delay to 0.3 or 0.5 seconds
- Check floor GameObject has enabled collider
- Verify player spawn position is above the floor mesh

### Issue: Cutscene plays twice
**Check**:
- PlayableDirector "Initial State" setting

**Fix**:
- Ensure "Initial State" is "Paused", NOT "Playing"
- Check for duplicate scripts or multiple things triggering the cutscene

---

## Best Practices

1. **Always use "Paused" initial state** when using this script
   - Never set PlayableDirector to "Playing" with CutsceneDelayedStart

2. **Keep delay minimal** for better player experience
   - 0.1s is usually sufficient
   - Only increase if you encounter physics issues

3. **Enable debug logs during development**
   - Helps catch timing issues early
   - Disable before final build for cleaner Console

4. **Test in both Editor and Build**
   - Physics timing can differ between Editor and standalone builds
   - Test on target platform to ensure consistent behavior

5. **Consider player position** when designing cutscenes
   - Ensure player spawns on solid ground
   - Avoid moving player during very first frames of scene load

---

## Integration with Existing Cutscene Scripts

This script works alongside your existing cutscene control scripts:

- **CutsceneControl.cs**: Handles player controller disabling and UI hiding
- **AfterCutsceneCameraReset.cs**: Handles camera transition after cutscene
- **CutsceneEndHook.cs**: Triggers post-cutscene events

**CutsceneDelayedStart** only handles the INITIAL playback timing. All other cutscene functionality remains unchanged.

**Execution Order**:
```
CutsceneDelayedStart.Start() → Delay → director.Play() →
CutsceneControl.OnPlayed() → [Cutscene Plays] →
CutsceneControl.OnStopped() → AfterCutsceneCameraReset.OnCutsceneEnd()
```

---

## Script Location

- **File**: `Assets/Cutscenes/CutsceneDelayedStart.cs`
- **Namespace**: None (default)
- **Dependencies**:
  - UnityEngine
  - UnityEngine.Playables
  - System.Collections (for IEnumerator)

---

## Version History

### v1.0 (2025-10-27)
- Initial release
- Fixes player falling through floor on 4th Floor scene
- Auto-finds PlayableDirector on same GameObject
- Configurable startup delay with physics frame wait
- Manual trigger support for advanced use cases
- Debug logging for development

---

## Related Files

- `CutsceneControl.cs` - Main cutscene control (disable player, hide UI)
- `AfterCutsceneCameraReset.cs` - Camera transition after cutscene
- `CutsceneEndHook.cs` - Post-cutscene event hooks
- `4TH_FLOOR_CUTSCENE_FIX_INSTRUCTIONS.md` - Detailed fix instructions for 4th Floor scene

---

## Support

If you encounter issues:
1. Enable Debug Logs and check Console messages
2. Review the troubleshooting section above
3. Check `4TH_FLOOR_CUTSCENE_FIX_INSTRUCTIONS.md` for detailed setup steps
4. Verify your scene setup matches the best practices listed here

---

**Last Updated**: 2025-10-27
**Created By**: Claude Code (Anthropic)
**Purpose**: Fix physics initialization race conditions in cutscene-heavy scenes
