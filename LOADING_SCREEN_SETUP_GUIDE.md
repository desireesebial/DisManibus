# Loading Screen Setup Guide

## Summary

The loading screen system has been updated and **WILL WORK** for:
- ✅ **New Game button** in Main Menu
- ✅ **Load Game button** in Main Menu  
- ✅ **Scene Portals** (when holding E to transition)
- ✅ **All scene transitions** using SceneTransitionManager

## Current Status

### ✅ What's Already Set Up

1. **LoadingScreenController exists** in Bootstrap scene
   - GameObject: "Loading Screen"
   - Has CanvasGroup, Slider, and Status Text properly assigned
   - Uses DontDestroyOnLoad (persists across scenes)

2. **Code is updated** with better defaults:
   - `LoadingScreenController.cs`: `minimumDisplayTime = 2.0f`
   - `SceneTransitionManager.cs`: `minimumLoadingTime = 2.0f`
   - `ScenePortal.cs`: Improved hold mechanics and loading screen integration

3. **All systems use the loading screen**:
   - `NewGameButton` → SceneTransitionManager → LoadingScreen ✅
   - `LoadGameScript/SaveManager` → SceneTransitionManager → LoadingScreen ✅
   - `ScenePortal` → SceneTransitionManager → LoadingScreen ✅

### ⚠️ What Needs To Be Done

**The Bootstrap scene file still has the OLD minimum display time value (0.35s)**

You need to update it to use the new 2.0 second duration.

## How to Fix (Choose ONE method)

### Method 1: Automatic Update (EASIEST) ⭐

1. Open Unity
2. Click **`Tools`** → **`Update Loading Screen Settings`** in the menu bar
3. Check the Console for confirmation message
4. Done! ✅

### Method 2: Manual Update

1. Open Unity
2. Open `Assets/Scenes/Bootstrap.unity`
3. Find "Loading Screen" GameObject in the hierarchy
4. In the Inspector, find the `LoadingScreenController` component
5. Change `Minimum Display Time` from `0.35` to `2.0`
6. Save the scene (Ctrl+S)
7. Also find "System" GameObject  
8. Find the `SceneTransitionManager` component
9. Change `Minimum Loading Time` from `0` to `2.0`
10. Save the scene again
11. Done! ✅

## Customizing Loading Times

You can adjust how long the loading screen displays:

### In LoadingScreenController
- **Location**: Bootstrap scene → "Loading Screen" GameObject
- **Field**: `Minimum Display Time`
- **Default**: 2.0 seconds
- **Affects**: All loading screens

### In SceneTransitionManager
- **Location**: Bootstrap scene → "System" GameObject
- **Field**: `Minimum Loading Time`
- **Default**: 2.0 seconds
- **Affects**: Fallback time if LoadingScreenController isn't found

### Per-Portal Messages
- **Location**: Any ScenePortal component
- **Field**: `Loading Message`
- **Default**: "Loading..."
- **Examples**: 
  - "Entering 2nd Floor..."
  - "Loading Courtyard..."
  - "Returning to Main Hall..."

## Testing

After updating the scene settings:

1. **Test New Game**:
   - Start game → Main Menu
   - Click "New Game"
   - Should see loading screen for ~2 seconds with "Starting New Game"

2. **Test Load Game**:
   - Start game → Main Menu
   - Click "Load Game"
   - Should see loading screen for ~2 seconds with "Loading Save"

3. **Test Scene Portal**:
   - Walk up to any portal
   - Hold E for 1.5 seconds
   - Slider should fill up
   - Should see loading screen for ~2 seconds with portal's custom message

## Debug Logging

The system now includes debug messages. Check the Console for:

✅ **Success messages**:
- `"SceneTransitionManager: Using LoadingScreenController with message 'Loading...'"`
- `"ScenePortal 'PortalName' activating - transitioning to scene"`

⚠️ **Warning messages**:
- `"SceneTransitionManager: LoadingScreenController not found!"`
- `"ScenePortal 'PortalName': LoadingScreenController not found"`

If you see warnings, the Bootstrap scene might not be loading properly.

## Troubleshooting

### "No loading screen appears"

1. Check Bootstrap scene is in Build Settings and loads first
2. Run the automatic update tool: `Tools` → `Update Loading Screen Settings`
3. Check Console for warning messages
4. Verify "Loading Screen" GameObject exists in Bootstrap scene

### "Loading screen flashes too quickly"

1. Open Bootstrap scene
2. Select "Loading Screen" GameObject
3. Increase `Minimum Display Time` to 3.0 or higher
4. Save scene

### "Scene portal hold doesn't work"

1. Check the portal GameObject in Inspector
2. Verify `Require Hold To Activate` is checked
3. Verify `Hold Duration` is set (default: 1.5 seconds)
4. Make sure UI elements are assigned:
   - Interaction UI Panel
   - Interaction Label
   - Hold Progress Slider or Hold Progress Image

### "Bootstrap scene doesn't persist"

Make sure Bootstrap scene is marked as "DontDestroyOnLoad":
- LoadingScreenController has `DontDestroyOnLoad(gameObject)` in Awake ✅
- SceneTransitionManager has `DontDestroyOnLoad(gameObject)` in Awake ✅

## Technical Details

### Scene Loading Flow

```
User Action
    ↓
[NewGameButton / LoadGame / ScenePortal]
    ↓
SceneTransitionManager.LoadScene()
    ↓
Fade Out (black screen)
    ↓
LoadingScreenController.Show()
    ↓
Track AsyncOperation (show progress bar)
    ↓
Wait for minimum display time (2.0s)
    ↓
LoadingScreenController.Hide()
    ↓
Fade In (reveal new scene)
    ↓
Scene fully loaded!
```

### Files Modified

- `Assets/Scripts/World/UI/LoadingScreenController.cs`
- `Assets/Scripts/SceneTransitionManager.cs`
- `Assets/Scripts/World/SceneTransition/ScenePortal.cs`
- `Assets/Editor/UpdateLoadingScreenSettings.cs` (NEW)

### Scene Files That Need Updating

- `Assets/Scenes/Bootstrap.unity` (contains LoadingScreenController and SceneTransitionManager)

---

**Last Updated**: 2025-10-09
**Status**: ✅ Code complete, awaiting scene file update

