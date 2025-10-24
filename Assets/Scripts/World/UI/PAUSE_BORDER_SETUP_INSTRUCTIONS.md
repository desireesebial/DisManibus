# Pause Menu Border and Camera Lock - Setup Instructions

## Overview
This guide will help you set up the spooky border overlay and disable camera look when the game is paused.

## What Was Added

### Code Changes
1. **PauseMenuController.cs** - Added border overlay display functionality
2. **FirstPersonController.cs** - Added camera control methods (`DisableCameraLook()` and `EnableCameraLook()`)

### Features
- Spooky border.png appears when game is paused
- Camera rotation is completely disabled when paused (player can't look around)
- Border disappears when resuming or exiting to main menu

---

## Setup Instructions

### Part 1: Create the Border UI Element

#### Step 1: Open Your Scene with PauseMenuController

1. Open one of your gameplay scenes (e.g., "1st Floor", "2nd Floor", etc.)
2. Make sure the scene has a GameObject with the **PauseMenuController** component

#### Step 2: Find or Create the Canvas

1. In the **Hierarchy** window, look for a Canvas that contains your pause menu UI
   - Common names: "Canvas", "PauseCanvas", "UI", etc.
2. If you don't have a Canvas, create one:
   - Right-click in Hierarchy > **UI > Canvas**
   - This will also create an EventSystem if one doesn't exist

#### Step 3: Create the Border Image GameObject

1. Right-click on the Canvas in Hierarchy
2. Select **UI > Image**
3. Rename the new Image to **"BorderOverlay"** (or any descriptive name)

#### Step 4: Configure the Border Image

Select the **BorderOverlay** GameObject in the Hierarchy, then in the Inspector:

**A. Set the Sprite:**
1. Find the **Image** component
2. Click the circle icon next to **Source Image**
3. In the Select Sprite dialog, search for **"border"**
4. Select the **border** sprite (the one at `Assets/Menu/Settings/border.png`)

**B. Set the Rect Transform to Fill Screen:**
1. In the **Rect Transform** component, click the **Anchor Presets** square (top-left)
2. Hold **Alt + Shift** and click the **bottom-right option** (stretch both axes)
   - This sets anchors to fill the entire screen
3. Set these values:
   - **Left**: 0
   - **Right**: 0
   - **Top**: 0
   - **Bottom**: 0
   - This makes the border fill the entire screen

**C. Configure Image Settings:**
1. In the **Image** component:
   - **Image Type**: Simple (or Sliced if you want it to scale better)
   - **Color**: White (or adjust if you want to tint the border)
   - **Raycast Target**: Unchecked (so it doesn't block UI clicks)

**D. Set the Sorting Order:**
1. The border should appear BEHIND the pause menu buttons but IN FRONT of the game view
2. If your pause menu buttons are disappearing behind the border, you need to adjust the sorting:
   - **Option A**: Move the BorderOverlay GameObject ABOVE the pause menu panel in the Hierarchy (higher = rendered first = appears behind)
   - **Option B**: Add a Canvas component to BorderOverlay and set Sort Order to a lower value than the pause menu

#### Step 5: Deactivate the Border Initially

1. With **BorderOverlay** selected in Hierarchy
2. At the top of the Inspector, **uncheck the checkbox** next to the GameObject name
   - This deactivates it so it's hidden when the game starts
   - The PauseMenuController script will activate it when pausing

---

### Part 2: Connect the Border to PauseMenuController

#### Step 1: Locate the PauseMenuController Component

1. In the Hierarchy, find the GameObject that has the **PauseMenuController** component
   - This is often on a "PauseMenuManager" or "GameManager" GameObject
   - Or it might be on the Canvas itself

2. Select this GameObject and look at the Inspector

#### Step 2: Assign the Border Overlay

1. In the **PauseMenuController** component, find the **UI** section
2. You should see a new field called **Border Overlay**
3. Drag the **BorderOverlay** GameObject from the Hierarchy into this field
   - Or click the circle icon and select it from the list

---

### Part 3: Disable Camera Look When Paused

#### Step 1: Find the FirstPersonController

1. In the Hierarchy, find your player GameObject
   - Common names: "Player", "FPSController", "FirstPersonCharacter", etc.
2. It should have the **FirstPersonController** component attached
3. Select this GameObject

#### Step 2: Wire Up the UnityEvents

Now we need to connect the pause/resume events to disable/enable camera look:

1. Select the GameObject with **PauseMenuController** (the same one from Part 2)
2. In the Inspector, find the **PauseMenuController** component
3. Scroll down to the **Events** section

**A. Setup "On Paused" Event (Disable Camera):**
1. Under **On Paused**, click the **+** button to add a new event
2. Drag the **Player GameObject** (the one with FirstPersonController) into the object field
3. Click the dropdown that says **"No Function"**
4. Navigate to: **FirstPersonController > DisableCameraLook()**
5. Select it

**B. Setup "On Resumed" Event (Enable Camera):**
1. Under **On Resumed**, click the **+** button to add a new event
2. Drag the **Player GameObject** into the object field
3. Click the dropdown that says **"No Function"**
4. Navigate to: **FirstPersonController > EnableCameraLook()**
5. Select it

#### Step 3: Verify the Setup

Your PauseMenuController should now have:
- **Border Overlay**: Assigned to your BorderOverlay Image
- **On Paused (1 listener)**:
  - Player GameObject → FirstPersonController.DisableCameraLook()
- **On Resumed (1 listener)**:
  - Player GameObject → FirstPersonController.EnableCameraLook()

---

### Part 4: Test the Implementation

#### Test in Play Mode:

1. **Enter Play Mode** (press the Play button)
2. **Press ESC** to pause the game
   - The spooky border should appear
   - Try moving the mouse - the camera should NOT rotate
3. **Press ESC again** to resume
   - The border should disappear
   - Moving the mouse should now rotate the camera
4. **Test the pause menu buttons** (if you have any):
   - Make sure "Resume" hides the border and enables camera
   - Make sure "Exit to Main Menu" also works correctly

#### Test Different Resolutions:

1. While in Play Mode, go to the **Game** view
2. Change the aspect ratio dropdown at the top
   - Try: 16:9, 16:10, 4:3, Free Aspect, etc.
3. Pause the game (ESC)
4. The border should fill the screen properly in all resolutions

---

## Repeat for All Scenes

You need to set this up in **each scene** that has:
- A PauseMenuController
- A FirstPersonController

Common scenes to update:
1. 1st Floor.unity
2. 2nd Floor (Better Version).unity
3. 3rd floor (better version).unity
4. 4th Floor (better version).unity

For each scene:
1. Follow Parts 1-3 above
2. Save the scene (Ctrl+S / Cmd+S)

---

## Troubleshooting

### Problem: Border doesn't appear when pausing

**Solutions:**
1. Make sure the BorderOverlay is assigned in PauseMenuController
2. Check that the BorderOverlay GameObject exists in the scene
3. Verify the border sprite is assigned in the Image component
4. Check the Console for any error messages

### Problem: Border appears but is too small/wrong size

**Solutions:**
1. Check the RectTransform anchor settings (should stretch to fill screen)
2. Make sure Left, Right, Top, Bottom are all set to 0
3. Verify the Canvas is in "Screen Space - Overlay" mode

### Problem: Border blocks pause menu buttons

**Solutions:**
1. In Hierarchy, move BorderOverlay ABOVE the pause menu panel GameObject
   - Items higher in the hierarchy render first (appear behind)
2. Or add a Canvas component to BorderOverlay and set a lower Sort Order
3. Or uncheck "Raycast Target" on the BorderOverlay's Image component

### Problem: Border doesn't disappear when resuming

**Solutions:**
1. Check that the BorderOverlay is properly assigned
2. Look for errors in the Console
3. Try manually deactivating it in the Hierarchy while in Play Mode to verify it's the right object

### Problem: Camera still rotates when paused

**Solutions:**
1. Make sure the OnPaused event is set up correctly:
   - Player GameObject is assigned
   - FirstPersonController.DisableCameraLook() is selected
2. Verify you're using the correct player GameObject (the one with FirstPersonController, not DullahanHeadInventory's controller if different)
3. Check if your scene uses SimplePlayerMovement instead of FirstPersonController
   - If so, you'll need to add similar DisableCameraLook/EnableCameraLook methods to SimplePlayerMovement.cs
   - Contact for help if needed

### Problem: Camera doesn't rotate after resuming

**Solutions:**
1. Make sure the OnResumed event is set up correctly:
   - Player GameObject is assigned
   - FirstPersonController.EnableCameraLook() is selected
2. Check if another script is disabling camera movement
3. Try manually setting `cameraCanMove = true` in the Inspector while in Play Mode to debug

### Problem: Border appears in the wrong color/looks wrong

**Solutions:**
1. Check the Image component's Color property (should be white for original colors)
2. Verify the correct border sprite is assigned
3. Check the Image Type (try Simple or Sliced)
4. Make sure the Canvas Render Mode is "Screen Space - Overlay"

---

## Advanced: Creating a Border Prefab (Optional)

If you want to reuse the border setup across multiple scenes:

1. **Create the BorderOverlay** following Part 1 in one scene
2. **Drag the BorderOverlay** from Hierarchy into your Project window (Assets folder)
   - This creates a prefab
3. **In other scenes**:
   - Drag the BorderOverlay prefab into the Canvas
   - Position it appropriately in the Hierarchy
   - Assign it to the PauseMenuController

This ensures consistent border appearance across all scenes.

---

## Summary

After completing these steps:
- The spooky cobweb border appears when you pause the game
- Players cannot look around with the mouse when paused
- The border disappears when resuming or exiting to main menu
- Everything works across different screen resolutions

If you encounter any issues not covered in the troubleshooting section, check the Console for error messages and verify each step was completed correctly.
