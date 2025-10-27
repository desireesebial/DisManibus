# 4th Floor Cutscene Fix - Setup Instructions

## Problem Summary
The player was falling through the map after the cutscene in the first seconds of playtest on the 4th Floor scene. This was caused by a race condition where the cutscene auto-played immediately on scene load, before physics colliders and the player's Rigidbody were fully initialized.

## Root Cause
- The `Cutscene_Player` GameObject's PlayableDirector was set to **"Initial State: Playing"**
- This caused the intro cutscene to start immediately when the scene loaded
- The player controller was disabled during cutscene playback
- Physics colliders and Rigidbody weren't fully initialized, causing the player to fall through the floor

## Solution
Add a small startup delay before playing the cutscene, allowing the scene to fully initialize.

---

## Setup Instructions (Step-by-Step)

### Step 1: Open the Scene
1. Open Unity Editor
2. Load the scene: `Assets/Scenes/4th Floor (better version).unity`

### Step 2: Locate the Cutscene GameObject
1. In the Hierarchy window, expand the **"Cutscenes"** folder/group
2. Find and select the **"Cutscene_Player"** GameObject
3. This object should have:
   - PlayableDirector component
   - CutsceneEndHook component
   - AfterCutsceneCameraReset component

### Step 3: Modify PlayableDirector Settings
1. With `Cutscene_Player` selected, look at the Inspector window
2. Find the **PlayableDirector** component
3. Change the **"Initial State"** dropdown from **"Playing"** to **"Paused"**
   - This prevents the cutscene from auto-playing on scene load
4. Leave all other settings unchanged

### Step 4: Add the CutsceneDelayedStart Component
1. With `Cutscene_Player` still selected
2. Click **"Add Component"** at the bottom of the Inspector
3. Search for **"CutsceneDelayedStart"** and add it
   - (The script is located at: `Assets/Cutscenes/CutsceneDelayedStart.cs`)

### Step 5: Configure CutsceneDelayedStart Settings
The component should auto-configure itself, but verify these settings:

1. **Director**: Should auto-populate with the PlayableDirector on the same GameObject
   - If not, drag the PlayableDirector component into this field
2. **Startup Delay**: Set to **0.1** seconds (default)
   - You can adjust this if needed (0.05 - 1.0 range)
   - Increase if the issue persists
3. **Play On Start**: Check this box ✓
   - This ensures the cutscene plays automatically after the delay
4. **Enable Debug Logs**: Check this box ✓ (optional, for testing)
   - Helps verify the cutscene timing in the Console

### Step 6: Save the Scene
1. Press **Ctrl+S** (Windows) or **Cmd+S** (Mac) to save the scene
2. Or go to **File → Save**

### Step 7: Test the Fix
1. Enter Play Mode in Unity Editor
2. Load/play the 4th Floor scene
3. Watch the Console for debug messages (if debug logs enabled):
   ```
   [CutsceneDelayedStart] Waiting 0.1 seconds for scene initialization...
   [CutsceneDelayedStart] Scene initialized - Starting cutscene playback now
   ```
4. **Verify the player DOES NOT fall through the floor**
5. The cutscene should play after a brief (0.1s) delay
6. After the cutscene ends, the player should have full control

---

## Alternative Solutions (If Issue Persists)

### Option A: Increase Startup Delay
If the player still falls:
1. Select `Cutscene_Player` GameObject
2. In the CutsceneDelayedStart component
3. Increase **"Startup Delay"** to **0.2** or **0.3** seconds
4. Test again

### Option B: Check Floor Colliders
1. In the Hierarchy, search for floor/ground objects
2. Verify they have active **Collider** components (BoxCollider, MeshCollider, etc.)
3. Check that colliders are **not disabled** on startup
4. Verify the layer collision matrix:
   - Go to **Edit → Project Settings → Physics**
   - Ensure the Player layer can collide with the Floor/Ground layer

### Option C: Player Rigidbody Settings
1. In the Hierarchy, find the **Player** GameObject (usually named "Player" or has FirstPersonController)
2. Check the **Rigidbody** component:
   - **Is Kinematic**: Should be **unchecked** (false)
   - **Use Gravity**: Should be **checked** (true)
   - **Constraints**: Should have "Freeze Rotation X, Y, Z" checked (to prevent player from tipping over)
   - **Collision Detection**: Set to **Continuous** for better physics
3. If the Rigidbody was kinematic, that could cause falling issues

---

## How It Works

### Before the Fix:
```
Scene Load → Cutscene Plays Immediately → Player Controller Disabled →
Physics Not Ready → Player Falls Through Floor → Cutscene Ends → Player Already Below Map
```

### After the Fix:
```
Scene Load → Wait 0.1s + 1 Physics Frame → Physics Fully Initialized →
Cutscene Plays → Player Controller Disabled (but player on solid ground) →
Cutscene Ends → Player Restored to Correct Position → Player Has Control
```

---

## Technical Details

The `CutsceneDelayedStart.cs` script:
1. Waits for `startupDelay` seconds (default 0.1s)
2. Waits for one additional FixedUpdate frame (ensures physics is ready)
3. Then calls `PlayableDirector.Play()` to start the cutscene

This gives the Unity physics engine time to:
- Initialize all colliders in the scene
- Set up the player's Rigidbody properly
- Establish collision detection systems
- Position the player correctly on the ground

---

## Troubleshooting

### Issue: Cutscene doesn't play at all
**Fix**:
- Verify "Play On Start" is checked in CutsceneDelayedStart component
- Check Console for error messages
- Verify PlayableDirector has a valid Timeline asset assigned

### Issue: Player still falls through floor
**Fix**:
- Increase startup delay to 0.3 seconds
- Check floor colliders are active and on correct layer
- Verify player's Rigidbody is not kinematic
- Check if there's a trigger collider that's causing the fall

### Issue: Cutscene plays twice
**Fix**:
- Ensure PlayableDirector "Initial State" is set to "Paused", not "Playing"
- Remove any other scripts that might be triggering the cutscene

---

## Files Modified/Created

1. **Created**: `Assets/Cutscenes/CutsceneDelayedStart.cs`
   - New script that handles delayed cutscene playback

2. **Modified** (in Unity Editor, not via script):
   - `Assets/Scenes/4th Floor (better version).unity`
   - Changes to Cutscene_Player GameObject:
     - PlayableDirector: Initial State changed to "Paused"
     - Added CutsceneDelayedStart component

3. **Created**: This documentation file
   - `Assets/Scenes/4TH_FLOOR_CUTSCENE_FIX_INSTRUCTIONS.md`

---

## Testing Checklist

- [ ] Scene loads without errors
- [ ] Player does not fall through the floor during cutscene
- [ ] Cutscene plays after a brief delay (0.1s)
- [ ] Player controller is disabled during cutscene
- [ ] Player regains control after cutscene ends
- [ ] Player is positioned correctly after cutscene
- [ ] UI elements are hidden during cutscene
- [ ] UI elements are restored after cutscene
- [ ] No console errors related to cutscene playback

---

## Questions or Issues?

If the problem persists after following these instructions:

1. Check the Unity Console for error messages
2. Increase the startup delay to 0.3-0.5 seconds
3. Verify all floor colliders are active and properly configured
4. Test in a build (not just Editor) to rule out Editor-specific issues
5. Review the player's spawn position - ensure it's above the floor mesh

---

**Last Updated**: 2025-10-27
**Author**: Claude Code
**Issue Fixed**: Player falling through map on 4th Floor after cutscene
