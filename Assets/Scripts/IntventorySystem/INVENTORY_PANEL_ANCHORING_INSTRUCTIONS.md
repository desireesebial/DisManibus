# Inventory Panel Anchoring - Setup Instructions

## Overview
This guide will help you fix the inventory panel positioning issue across all scenes. The panel will now stay at the bottom of the screen regardless of resolution changes.

## What We Created
- **InventoryPanelAnchoring.cs**: A script that automatically anchors the inventory panel to the bottom-center of the screen

## Setup Instructions

### For Each Scene (1st Floor, 2nd Floor, 3rd Floor, 4th Floor)

Follow these steps for each scene that has an inventory panel:

---

### Step 1: Open the Scene
1. In Unity, open the scene you want to fix (e.g., "1st Floor.unity")
2. Open the Hierarchy window

### Step 2: Locate the Inventory Panel GameObject

The inventory panel is likely nested under the Canvas. Look for:
- A Canvas GameObject (should be at the root of UI elements)
- Under Canvas, find the parent GameObject that contains your inventory slot images
  - Common names might be: "InventoryPanel", "Inventory", "UI", "HUD", or similar
  - Look for the GameObject that has all 5 inventory slots as children (or 3 slots for 2nd Floor)

**How to identify it:**
1. Look in the Hierarchy for a GameObject that has multiple child Image components
2. The GameObject should be the parent of your inventory slot background images
3. You can also look for the GameObject that's referenced in the PlayerInventory or DullahanHeadInventory component

### Step 3: Add the InventoryPanelAnchoring Component

Once you've found the inventory panel GameObject:

1. **Select** the inventory panel parent GameObject in the Hierarchy
2. In the **Inspector** window, click **Add Component**
3. Search for **"InventoryPanelAnchoring"** and select it
4. The script will be added to the GameObject

### Step 4: Configure the Settings (Optional)

In the Inspector, you'll see the InventoryPanelAnchoring component with these settings:

- **Bottom Offset**: Distance from the bottom edge (default: 30 pixels)
  - Increase this value to move the panel higher from the bottom
  - Decrease to move it closer to the bottom edge

- **Horizontal Padding**: Padding from left/right edges (default: 20 pixels)
  - Currently not used, reserved for future enhancements

- **Auto Configure On Awake**: Should be checked (enabled)
  - This ensures the anchoring is applied automatically when the scene loads

### Step 5: Apply the Anchoring Immediately (Optional)

To see the changes immediately without running the game:

1. Right-click on the **InventoryPanelAnchoring** component header in the Inspector
2. Select **"Configure Bottom-Center Anchoring"** from the context menu
3. The panel will be repositioned immediately

### Step 6: Verify the RectTransform Settings

After applying the anchoring, check the **RectTransform** component on the same GameObject:

You should see:
- **Anchors**:
  - Min: X = 0.5, Y = 0
  - Max: X = 0.5, Y = 0
- **Pivot**: X = 0.5, Y = 0
- **Anchored Position**: X = 0, Y = 30 (or your custom bottom offset)

This configuration means:
- The panel is anchored to the **bottom-center** of the screen
- It will maintain its position regardless of screen resolution
- It grows upward from the bottom

### Step 7: Test in Different Resolutions

1. **Play** the scene (click the Play button)
2. While the game is running, go to the **Game** view
3. Try different aspect ratios and resolutions from the dropdown at the top of the Game view:
   - Test: "Free Aspect", "16:9", "16:10", "4:3", etc.
   - The inventory panel should stay at the bottom in all cases

4. You can also test in windowed mode:
   - Build and run the game
   - Resize the window
   - The panel should maintain its bottom position

### Step 8: Save the Scene

1. **File > Save** (or Ctrl+S / Cmd+S)
2. Your changes are now saved

---

## Scenes to Update

Apply the above steps to these scenes:

1. **1st Floor.unity** - PlayerInventory (5 slots)
2. **2nd Floor (Better Version).unity** - DullahanHeadInventory (3 slots)
3. **3rd floor (better version).unity** - PlayerInventory (5 slots)
4. **4th Floor (better version).unity** - PlayerInventory (5 slots)

---

## Troubleshooting

### Problem: Can't find the inventory panel GameObject

**Solution:**
1. Look for the GameObject that has the **PlayerInventory** or **DullahanHeadInventory** component
2. In that component, check the **Inventory Slot Image** array
3. Click on one of the elements to highlight it in the Hierarchy
4. Navigate up the parent hierarchy to find the common parent of all slots

### Problem: The panel is too high or too low

**Solution:**
1. Select the inventory panel GameObject
2. In the **InventoryPanelAnchoring** component, adjust the **Bottom Offset** value
3. Right-click the component and select **"Configure Bottom-Center Anchoring"** to apply
4. Test until you find the right position

### Problem: The anchoring doesn't apply automatically

**Solution:**
1. Make sure **Auto Configure On Awake** is checked
2. Try manually applying: Right-click component > "Configure Bottom-Center Anchoring"
3. Check the Console for any error messages

### Problem: The inventory slots look stretched or wrong

**Solution:**
- The InventoryPanelAnchoring script only affects the **parent panel's position**, not the individual slots
- If slots look wrong, check their individual RectTransform settings
- Make sure the parent panel has the correct **size** (width and height)

---

## Additional Notes

- **Prefabs**: If you later want to create a prefab for the inventory panel, make sure to include the InventoryPanelAnchoring component
- **Custom Adjustments**: You can tweak the Bottom Offset value per scene if needed (e.g., different floors might need different positions)
- **Script Location**: The script is located at `Assets/Scripts/IntventorySystem/InventoryPanelAnchoring.cs`

---

## Summary

After following these steps for all scenes:
- Inventory panel will stay at the bottom of the screen
- Position will be maintained across all resolutions and aspect ratios
- The panel will be centered horizontally
- You can easily adjust the bottom offset if needed

If you have any issues, check the Console window for error messages and verify that the RectTransform anchors are set correctly.
