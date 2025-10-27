# Stamina Tutorial Setup Guide

This guide will help you set up the stamina tutorial notification system that displays a helpful message when players first sprint.

## What It Does

When the player presses the sprint key (Shift) for the first time:
- A notification appears: **"Sprinting drains stamina! You'll slow down when stamina runs low."**
- The message fades in, stays for 5 seconds, then fades out
- It only shows once per player (saved in PlayerPrefs)
- The sprint bar color changes (green → yellow → red) as stamina depletes

---

## Setup Instructions

### Step 1: Create the Tutorial UI

1. **Open your scene** (e.g., `4th Floor (better version).unity`)

2. **Find the Canvas**:
   - In the Hierarchy, locate your main Canvas (usually contains UI elements like health, stamina bar, etc.)
   - Right-click on the Canvas and select **Create Empty** (or press Ctrl+Shift+N)
   - Name it: **"StaminaTutorialPanel"**

3. **Create the Background Panel** (optional but recommended):
   - Right-click on `StaminaTutorialPanel` → **UI → Panel**
   - Rename it to **"Background"**
   - In the Inspector:
     - Set **Color**: Black with alpha ~150 (for semi-transparent background)
     - Adjust **RectTransform** to desired size (e.g., width: 600, height: 100)
     - Set **Anchors** to center-top (or wherever you want the notification)

4. **Create the Text Element**:
   - Right-click on `StaminaTutorialPanel` → **UI → Text - TextMeshPro**
   - Rename it to **"TutorialText"**
   - In the Inspector:
     - Set **Text**: "Sprinting drains stamina! You'll slow down when stamina runs low."
     - Set **Font Size**: 24 (or your preferred size)
     - Set **Color**: White (or your preferred color)
     - Set **Alignment**: Center, Middle
     - Enable **Word Wrapping** if needed
     - Adjust **RectTransform** to fill the panel

5. **Position the Notification**:
   - Select `StaminaTutorialPanel`
   - In RectTransform:
     - Set **Anchors**: Bottom-Center (0.5, 0)
     - Set initial size (e.g., width: 600, height: 80)
   - **Note**: The script will automatically position it above the stamina bar!

### Step 2: Add the StaminaTutorial Component

1. **Create a Tutorial Manager GameObject**:
   - In the Hierarchy, create a new Empty GameObject
   - Name it: **"StaminaTutorialManager"**
   - Place it under your Canvas or UI manager

2. **Add the Script**:
   - Select `StaminaTutorialManager`
   - Click **Add Component**
   - Search for **"StaminaTutorial"** and add it

3. **Configure the Script**:
   In the Inspector, you should see:

   **Tutorial Settings:**
   - **Tutorial Message**: Leave default or customize
   - **Display Duration**: 5 seconds (adjust if needed)
   - **Fade Duration**: 0.5 seconds

   **UI References:**
   - **Notification Text**: Drag the `TutorialText` object here
   - **Notification Panel**: Drag the `StaminaTutorialPanel` object here

   **Positioning:**
   - **Position Above Stamina Bar**: ✓ Checked (enabled by default)
   - **Stamina Bar Transform**: This is automatically set by FirstPersonController
   - **Vertical Offset**: 40 pixels (distance above stamina bar, adjust if needed)
   - **Manual Screen Position**: Only used if you uncheck "Position Above Stamina Bar"

   **Styling:**
   - Customize colors/font if desired

   **Debug:**
   - **Reset On Start**: Check this box for testing (tutorial will show every time)
   - Uncheck for production (tutorial shows only once per player)

### Step 3: Connect to FirstPersonController

1. **Find the Player**:
   - In the Hierarchy, find your **Player** GameObject (usually named "FirstPersonController" or "Player")

2. **Locate the FirstPersonController Component**:
   - Select the Player GameObject
   - In the Inspector, scroll to the **FirstPersonController** script component

3. **Link the Tutorial**:
   - Scroll to the bottom of the FirstPersonController component
   - Find the new **"Stamina Tutorial"** section
   - **Stamina Tutorial**: Drag the `StaminaTutorialManager` GameObject here

### Step 4: Test the Setup

1. **Enter Play Mode**
2. **Load the scene** (4th Floor or any scene with the player)
3. **Press Shift** (or your sprint key)
4. **Verify**:
   - The notification should appear and fade in
   - Message should display: "Sprinting drains stamina! You'll slow down when stamina runs low."
   - After 5 seconds, it should fade out
   - Try pressing Shift again - notification should NOT appear (already shown)

---

## Customization Options

### Change the Message

In `StaminaTutorialManager` component:
- Modify **Tutorial Message** field

### Change Display Time

In `StaminaTutorialManager` component:
- Adjust **Display Duration** (in seconds)

### Change Position

**To adjust vertical distance above stamina bar:**
- In `StaminaTutorialManager` component, change **Vertical Offset** (default: 40 pixels)
- Increase for more space, decrease for less space

**To use custom position instead:**
- Uncheck **Position Above Stamina Bar**
- Set **Manual Screen Position** (0-1 values, 0.5 = center)

### Change Colors

In `StaminaTutorialManager` component:
- Modify **Text Color**
- In the panel Background, change its Image color

### Reset Tutorial (For Testing)

**Option 1: Using Inspector**
- Check **Reset On Start** in StaminaTutorialManager component

**Option 2: Using Context Menu**
- Right-click on the `StaminaTutorial` component
- Select **"Force Show Tutorial"**

**Option 3: Using Code/Console**
- Call `staminaTutorial.ResetTutorial()` from another script

---

## Troubleshooting

### Tutorial doesn't appear

**Check:**
1. Is `StaminaTutorialManager` properly assigned to FirstPersonController's **Stamina Tutorial** field?
2. Are **Notification Text** and **Notification Panel** assigned in StaminaTutorial component?
3. Is the `StaminaTutorialPanel` active in the Hierarchy?
4. Check Console for error messages

### Tutorial already shown and won't reset

**Solution:**
- Check **Reset On Start** in the component, OR
- Clear PlayerPrefs:
  - In Unity Editor menu: **Edit → Clear All PlayerPrefs**
  - OR use the context menu: Right-click component → "Force Show Tutorial"

### Tutorial shows every time

**Fix:**
- Uncheck **Reset On Start** in the StaminaTutorial component

### Text looks wrong

**Fix:**
- Make sure you're using **TextMeshPro**, not legacy Text
- If prompted to import TMP Essentials, click "Import TMP Essentials"

---

## Files Modified/Created

### Created:
1. **`Assets/Scripts/StaminaTutorial.cs`**
   - New script for managing the tutorial notification

2. **`Assets/Scripts/STAMINA_TUTORIAL_SETUP.md`** (this file)
   - Setup instructions

### Modified:
1. **`Assets/.../FirstPersonController.cs`**
   - Added tutorial reference and trigger
   - Added new "Stamina Tutorial" section in Inspector

2. **Scene file** (after following setup):
   - Added `StaminaTutorialManager` GameObject
   - Added `StaminaTutorialPanel` with UI elements

---

## How It Works

1. **Auto-Positioning** (on Start):
   - FirstPersonController passes the sprint bar reference to StaminaTutorial
   - StaminaTutorial automatically positions itself above the sprint bar
   - Uses the configured vertical offset (default: 40 pixels)

2. **First Sprint Detection**:
   - When player presses Shift for the first time
   - FirstPersonController calls `staminaTutorial.ShowTutorial()`

3. **PlayerPrefs Check**:
   - StaminaTutorial checks if tutorial was shown before (saved to PlayerPrefs)
   - If not shown, displays the notification

4. **Fade Animation**:
   - Fades in over 0.5 seconds
   - Displays for 5 seconds
   - Fades out over 0.5 seconds

5. **One-Time Only**:
   - Saves to PlayerPrefs: `StaminaTutorial_Shown = 1`
   - Won't show again unless reset

---

## Additional Features

### Stamina Bar Color Warning (Already Implemented)

The sprint bar now changes color based on stamina level:
- **Green** (above 30%): Normal stamina
- **Yellow/Orange** (15-30%): Low stamina warning
- **Red** (below 15%): Critical stamina

This complements the tutorial by providing continuous visual feedback!

---

**Last Updated**: 2025-10-27
**Created By**: Claude Code
**Feature**: Stamina Tutorial Notification System
