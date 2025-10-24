# Pause Menu Border Fix - Setup Instructions

## Overview
This guide fixes the issue where the border overlay doesn't disappear when clicking the Resume button in the pause menu with Settings/Controls panels.

## What Was Updated

### Code Changes
- **PauseMenu.cs** - Added border overlay and camera control functionality
- The script now properly shows/hides the border and disables/enables camera look

---

## Setup Instructions

### Step 1: Remove PauseMenuController Component (IMPORTANT!)

You currently have **both** PauseMenu and PauseMenuController components on the same GameObject. This causes conflicts!

1. Select the **PauseManager** GameObject in the Hierarchy
2. In the Inspector, find the **PauseMenuController** component
3. Right-click on the component header (where it says "Pause Menu Controller (Script)")
4. Select **"Remove Component"**
5. Confirm the removal

**Why?** You only need ONE pause system. Since you're using the Settings/Controls UI, you need PauseMenu.cs, not PauseMenuController.cs.

---

### Step 2: Assign the Border Overlay

1. Make sure **PauseManager** is still selected in the Hierarchy
2. In the Inspector, find the **Pause Menu (Script)** component
3. Scroll down to the **"Border Overlay"** section (this is a new field)
4. Drag your **BorderOverlay** GameObject from the Hierarchy into this field
   - Or click the circle icon and select BorderOverlay

**You'll know it worked when**: The field shows "BorderOverlay (Image)" instead of "None (Image)"

---

### Step 3: Assign the Player Controller (Optional but Recommended)

The script will auto-find your player, but it's better to assign it manually:

1. Still in the **Pause Menu (Script)** component on PauseManager
2. Find the **"Player Camera Control"** section (new field)
3. Drag your **Player GameObject** (the one with FirstPersonController) from the Hierarchy into the **"Player Controller"** field

**If you skip this step**: The script will automatically find your FirstPersonController when the scene starts. But manual assignment is more reliable.

---

### Step 4: Verify Your Resume Button

Make sure your Resume button calls the correct method:

1. In the Hierarchy, find your **Resume button** (under your pause menu UI)
2. Select it and look at the Inspector
3. Find the **Button** component
4. In the **On Click ()** section, verify:
   - Object slot: Should have PauseManager (or whatever GameObject has PauseMenu script)
   - Function: Should show **PauseMenu.Resume**

**If it's wrong or empty**:
1. Clear the On Click list (remove old entries with the - button)
2. Click the **+ button** to add a new event
3. Drag **PauseManager** into the Object slot
4. Click the "No Function" dropdown
5. Navigate to **PauseMenu > Resume ()**

---

### Step 5: Test Everything

1. **Save your scene** (Ctrl+S / Cmd+S)
2. **Click Play**
3. **Press ESC** to pause:
   - ✅ Border should appear
   - ✅ Camera should not rotate when you move the mouse
4. **Click the Resume button**:
   - ✅ Border should disappear
   - ✅ Camera rotation should work again
   - ✅ Game should resume
5. **Press ESC again** to pause
6. **Press ESC again** to resume (keyboard method):
   - ✅ Should also work correctly

---

## Repeat for All Scenes

You need to set this up in each scene that uses the pause menu:

1. 1st Floor.unity
2. 2nd Floor (Better Version).unity
3. 3rd floor (better version).unity
4. 4th Floor (better version).unity

For each scene:
1. Remove PauseMenuController component (if it exists)
2. Assign BorderOverlay to PauseMenu component
3. Assign Player Controller to PauseMenu component (optional)
4. Save the scene

---

## Troubleshooting

### Problem: Border still doesn't disappear when clicking Resume

**Solutions:**
1. Make sure you **removed the PauseMenuController component**
2. Check that BorderOverlay is assigned in the PauseMenu (Script) component
3. Verify the Resume button calls **PauseMenu.Resume**, not PauseMenuController.Resume
4. Check the Console for any error messages

### Problem: Camera still rotates when paused

**Solutions:**
1. Make sure Player Controller is assigned (or let it auto-find)
2. Verify you have FirstPersonController component on your player
3. Check that FirstPersonController.cs has the DisableCameraLook() and EnableCameraLook() methods
4. Look for errors in the Console

### Problem: Settings or Controls panels don't work

**Solutions:**
1. The border should remain visible when you open Settings or Controls
2. Make sure you didn't remove the wrong script - you need **PauseMenu.cs**, not PauseMenuController
3. Check that your Settings/Controls panels are properly assigned in PauseMenu component

### Problem: Script errors about FirstPersonController

**Solutions:**
1. Make sure FirstPersonController.cs was saved and recompiled
2. If you use a different player controller (like SimplePlayerMovement), let me know
3. The script will work even if FirstPersonController is not found - you just won't have camera lock

### Problem: Border appears but looks wrong

**Solutions:**
1. Check that the border sprite (border.png) is assigned to the BorderOverlay's Image component
2. Verify the BorderOverlay's RectTransform is set to stretch (anchors at Min 0,0 and Max 1,1)
3. Make sure the border GameObject is on the Canvas

---

## Summary

After completing these steps:
- ✅ Only one pause system active (PauseMenu.cs)
- ✅ Border appears when pausing (ESC key or directly)
- ✅ Border disappears when resuming (ESC key OR Resume button)
- ✅ Camera rotation is disabled when paused
- ✅ Camera rotation is enabled when resuming
- ✅ Settings and Controls panels work correctly

The key fix was removing the conflicting PauseMenuController and adding the border/camera functionality to the PauseMenu.cs script that you're actually using.

If you still have issues, check the Console window for error messages!
