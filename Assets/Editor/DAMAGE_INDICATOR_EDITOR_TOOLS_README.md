# Directional Damage Indicator - Editor Tools Guide

Quick guide for using the editor tools to set up the directional damage indicator system.

## Quick Setup (Recommended)

### Method 1: One-Click Quick Setup
This is the fastest way to set up the system with default settings.

1. In Unity, go to **Tools → Quick Setup Directional Damage Indicator**
2. The tool will automatically:
   - Find or create a Canvas
   - Create all 4 blood splatter UI images
   - Configure the DirectionalDamageIndicator component
   - Link to PlayerHealthSystem (if found)
3. Done! The system is ready to use.

**What if there's no blood splatter sprite?**
- The tool will create placeholder white images
- You can assign your own blood splatter sprites afterward in the Inspector

---

### Method 2: Interactive Setup Window
Use this for more control over the setup process.

1. In Unity, go to **Tools → Setup Directional Damage Indicator**
2. A setup window will appear with the following options:

#### Setup Window Fields:

**References:**
- **Target Canvas**: The Canvas where UI will be created (auto-detected)
- **Player Health System**: The PlayerHealthSystem component (auto-detected)
- **Blood Splatter Sprite**: Your blood splatter sprite (optional)

**Settings:**
- **Auto-Link to Health System**: Automatically connect to PlayerHealthSystem
- **Splatter Width**: Width of each blood splatter image (default: 1000px)
- **Splatter Height**: Height of each blood splatter image (default: 600px)
- **Edge Offset**: Distance from screen edge (default: 200px)

**Buttons:**
- **Refresh References**: Re-scan scene for Canvas and PlayerHealthSystem
- **Create Directional Damage Indicator**: Perform the setup

3. Click "Create Directional Damage Indicator" to complete setup
4. A success dialog will confirm what was created

---

## Custom Inspector Features

After setup, selecting the "Directional Damage Indicator" GameObject shows a custom Inspector with organized sections:

### Inspector Sections:

#### 1. Blood Splatter Images
- Assign/view the 4 Image components (top, bottom, left, right)
- Validation messages show if any are missing

#### 2. Indicator Settings
- **Fade Out Duration**: How long before indicator disappears (2.5s default)
- **Min Damage Alpha**: Opacity for 1 HP damage (0.5 default)
- **Max Damage Alpha**: Opacity for 2-3 HP damage (0.9 default)
- **High Damage Scale**: Size multiplier for high damage (1.2x default)

#### 3. References
- **Player Camera**: Camera used for direction calculations
- **Player Transform**: Player GameObject transform
- **Auto-Find Buttons**: Click to automatically find camera/player

#### 4. Testing
Test the indicator in both Edit Mode and Play Mode:

**Direction Tests:**
- **Test Top (Front)**: Show indicator for frontal damage
- **Test Bottom (Back)**: Show indicator for rear damage
- **Test Left**: Show indicator for left-side damage
- **Test Right**: Show indicator for right-side damage

**Damage Amount Tests:**
- **1 HP Damage**: Test low-damage appearance (subtle)
- **2 HP Damage**: Test medium-damage appearance
- **3 HP Damage**: Test high-damage appearance (intense)

All tests simulate damage from the specified direction so you can verify positioning and appearance.

#### 5. Integration
- Shows link status to PlayerHealthSystem
- **Link to PlayerHealthSystem** button for manual linking

---

## Workflow Examples

### Example 1: Fresh Setup with Custom Sprite

1. Import your blood splatter sprite into Unity
2. Go to **Tools → Setup Directional Damage Indicator** (window method)
3. Assign your sprite to the **Blood Splatter Sprite** field
4. Click **Create Directional Damage Indicator**
5. Test using the Inspector test buttons

### Example 2: Quick Setup, Add Sprites Later

1. Go to **Tools → Quick Setup Directional Damage Indicator**
2. System creates placeholder images (white squares)
3. Select each "Blood Splatter" child object
4. In Inspector, assign your sprite to the **Source Image** field
5. Repeat for all 4 splatters

### Example 3: Testing in Play Mode

1. Enter Play Mode
2. Select "Directional Damage Indicator" in Hierarchy
3. In Inspector, click test buttons to preview indicators
4. Adjust settings (fade duration, alpha, scale) in real-time
5. Exit Play Mode (changes will be lost unless you copy component)

### Example 4: Manual Setup (Without Tools)

If you prefer manual setup, follow these steps:

1. **Create Canvas** (if needed)
   - Right-click Hierarchy → UI → Canvas
   - Set to Screen Space Overlay

2. **Create Container**
   - Right-click Canvas → Create Empty
   - Name: "Directional Damage Indicator"
   - Stretch to fill canvas (anchor to all edges)

3. **Create 4 Images**
   - Right-click container → UI → Image
   - Name: "Blood Splatter - Top/Bottom/Left/Right"
   - Position at screen edges (see setup guide for details)
   - Set rotation for each direction
   - Set alpha to 0 on all

4. **Add Component**
   - Select container
   - Add Component → Directional Damage Indicator
   - Assign all 4 Image references
   - Assign Player Camera and Player Transform

5. **Link to Health System**
   - Select Player GameObject
   - Find PlayerHealthSystem component
   - Assign container to **Damage Indicator** field

---

## Troubleshooting the Editor Tools

### Tool says "No Canvas found"
**Solution**:
- Click "Yes, Create Canvas" to auto-create one
- Or create a Canvas manually first

### Blood splatters don't appear in setup
**Solution**:
- Check Console for errors
- Ensure Canvas exists and is active
- Try the setup again

### "Not linked to PlayerHealthSystem" warning
**Solution**:
- Click "Link to PlayerHealthSystem" in Inspector
- Or manually assign in PlayerHealthSystem component

### Test buttons don't work in Edit Mode
**Solution**:
- Assign Player Transform in the References section
- Or enter Play Mode for testing

### Created duplicate indicators
**Solution**:
- Delete the old one (or the new one)
- Only one DirectionalDamageIndicator should exist per scene

### Auto-find buttons don't find camera/player
**Solution**:
- Ensure Camera has MainCamera tag
- Ensure Player has "Player" tag
- Or manually assign the references

---

## Editor Tool Files

The editor tools consist of 2 files:

1. **DirectionalDamageIndicatorSetup.cs**
   - Main setup window and quick setup menu item
   - Creates all UI elements automatically
   - Handles Canvas creation and linking

2. **DirectionalDamageIndicatorEditor.cs**
   - Custom Inspector for DirectionalDamageIndicator component
   - Organized sections with foldouts
   - Testing buttons for each direction
   - Auto-find and linking features

Both files are located in: `Assets/Editor/`

---

## Tips and Best Practices

### Design Tips
- **Use transparent PNG sprites** for blood splatters with smooth edges
- **Higher resolution sprites** (1024x1024) look better than low-res
- **Red color tint** can be applied to white sprites for easy color changes
- **Test all 4 directions** to ensure consistent appearance

### Performance Tips
- Only **one indicator per scene** (system replaces old indicators automatically)
- **Compressed texture format** for sprites (RGBA Compressed DXT5)
- Keep sprite resolution **reasonable** (512x512 to 1024x1024)

### Integration Tips
- **Set up early in development** so all scenes have the indicator
- **Create a prefab** of the indicator for reuse across scenes
- **Test with different enemies** to verify direction accuracy
- **Adjust fade duration** based on game pacing (slower for horror, faster for action)

### Testing Tips
- **Use test buttons** before Play Mode to verify setup
- **Test in Play Mode** with actual enemies for real-world behavior
- **Check all 4 directions** and damage amounts
- **Verify on different screen resolutions** (Canvas Scaler handles this)

---

## Keyboard Shortcuts (None by Default)

The editor tools don't have keyboard shortcuts, but you can add them:

```csharp
// Add to DirectionalDamageIndicatorSetup.cs
[MenuItem("Tools/Quick Setup Directional Damage Indicator %#d")] // Ctrl+Shift+D
```

---

## Advanced Usage

### Creating Blood Splatter Presets

You can save different configurations as ScriptableObjects:

1. Create a ScriptableObject with settings
2. Load settings in the setup window
3. Apply to new scenes quickly

### Multi-Scene Setup

To set up the indicator in multiple scenes:

1. Set up in one scene using the tool
2. Create a prefab from the indicator
3. Add prefab to other scenes
4. Use "Link to PlayerHealthSystem" button in each scene

### Custom Sprites Per Scene

Different scenes can use different blood splatter styles:

1. Set up indicator using the tool
2. Assign different sprites in Inspector for each scene
3. Maintain separate prefabs for different visual styles

---

## Support

For issues or questions:
- Check the main setup guide: `DIRECTIONAL_DAMAGE_INDICATOR_SETUP.md`
- Review the component script: `DirectionalDamageIndicator.cs`
- Check Unity Console for error messages

---

## Summary

**Quick Setup**: Tools → Quick Setup Directional Damage Indicator
**Interactive Setup**: Tools → Setup Directional Damage Indicator
**Testing**: Select indicator → Inspector → Testing section
**Linking**: Inspector → Integration → Link to PlayerHealthSystem

The editor tools make setup a 30-second process instead of 10+ minutes of manual work!
