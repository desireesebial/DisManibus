# Directional Damage Indicator - Unity Setup Guide

This guide explains how to set up the directional damage indicator system in Unity Editor.

## Overview

The directional damage indicator displays blood splatter effects on screen edges to show the direction of incoming damage. The system:
- Shows blood splatters at the screen edge closest to the damage source
- Fades out over 2-3 seconds
- Scales intensity based on damage amount (1 HP = subtle, 2-3 HP = intense)
- Only shows the most recent damage (replaces older indicators)

## Prerequisites

The following scripts must be present in your project:
- `DirectionalDamageIndicator.cs` (in `Assets/Scripts/HealthSystem/`)
- `PlayerHealthSystem.cs` (modified with damage indicator integration)

## Step 1: Create Blood Splatter UI Images

### 1.1 Import Blood Splatter Sprites
1. Create or import 4 blood splatter images (PNG with transparency)
   - Recommended size: 512x512 or 1024x1024
   - Should have transparent backgrounds
   - Red/dark red color scheme
2. Place them in your project (e.g., `Assets/Textures/UI/DamageIndicators/`)
3. Set **Texture Type** to **Sprite (2D and UI)** in the Inspector

### 1.2 Find or Create the Health UI Canvas
Your PlayerHealthSystem likely already has a Canvas. Look for:
- A Canvas GameObject in your Player hierarchy or scene
- The Canvas referenced in `PlayerHealthSystem.healthUI` field

**If you don't have a Canvas:**
1. Right-click in Hierarchy → **UI** → **Canvas**
2. Name it "Player HUD Canvas"
3. Set **Render Mode** to **Screen Space - Overlay**
4. Add a **CanvasScaler** component (if not present)
   - Set **UI Scale Mode** to **Scale With Screen Size**
   - Set **Reference Resolution** to your game's target resolution (e.g., 1920x1080)

### 1.3 Create Directional Damage Indicator UI Container
1. Right-click on your Canvas → **Create Empty**
2. Name it "Directional Damage Indicator"
3. Set **RectTransform** to stretch full screen:
   - Anchor Preset: **Stretch** (bottom-right option while holding Alt+Shift)
   - Left: 0, Right: 0, Top: 0, Bottom: 0
   - Position: (0, 0, 0)

### 1.4 Create Blood Splatter Images

#### Top Splatter
1. Right-click "Directional Damage Indicator" → **UI** → **Image**
2. Name it "Blood Splatter - Top"
3. Assign your blood splatter sprite to the **Source Image** field
4. Configure **RectTransform**:
   - **Anchor Preset**: Top-Center
   - **Width**: 800-1200 (adjust based on your sprite)
   - **Height**: 400-600
   - **Pos X**: 0
   - **Pos Y**: -200 (adjust so it's visible at top edge)
   - **Rotation**: 180 (so blood drips downward from top)
5. Set **Color** alpha to 0 (R: 255, G: 255, B: 255, **A: 0**)
   - This makes it invisible by default
6. Set **Raycast Target** to OFF (unchecked)

#### Bottom Splatter
1. Duplicate "Blood Splatter - Top" (Ctrl+D)
2. Name it "Blood Splatter - Bottom"
3. Configure **RectTransform**:
   - **Anchor Preset**: Bottom-Center
   - **Pos Y**: 200 (adjust so it's visible at bottom edge)
   - **Rotation**: 0 (blood drips upward from bottom)
4. Ensure **Color** alpha is still 0

#### Left Splatter
1. Duplicate "Blood Splatter - Top" (Ctrl+D)
2. Name it "Blood Splatter - Left"
3. Configure **RectTransform**:
   - **Anchor Preset**: Middle-Left
   - **Pos X**: 200 (adjust so it's visible at left edge)
   - **Pos Y**: 0
   - **Rotation**: 90 (blood drips rightward from left)
4. Ensure **Color** alpha is still 0

#### Right Splatter
1. Duplicate "Blood Splatter - Top" (Ctrl+D)
2. Name it "Blood Splatter - Right"
3. Configure **RectTransform**:
   - **Anchor Preset**: Middle-Right
   - **Pos X**: -200 (adjust so it's visible at right edge)
   - **Pos Y**: 0
   - **Rotation**: -90 (blood drips leftward from right)
4. Ensure **Color** alpha is still 0

**Pro Tip**: You can use different sprites for each direction if desired, but the same sprite works well when rotated.

## Step 2: Add DirectionalDamageIndicator Component

1. Select the "Directional Damage Indicator" GameObject
2. Click **Add Component**
3. Search for "Directional Damage Indicator" and add it
4. Configure the component:

### Component Settings

#### Blood Splatter Images (Drag & Drop)
- **Top Splatter**: Drag "Blood Splatter - Top" Image
- **Bottom Splatter**: Drag "Blood Splatter - Bottom" Image
- **Left Splatter**: Drag "Blood Splatter - Left" Image
- **Right Splatter**: Drag "Blood Splatter - Right" Image

#### Indicator Settings
- **Fade Out Duration**: 2.5 (how long before it disappears)
- **Min Damage Alpha**: 0.5 (opacity for 1 HP damage)
- **Max Damage Alpha**: 0.9 (opacity for 2-3 HP damage)
- **High Damage Scale**: 1.2 (scale multiplier for high damage)

#### References
- **Player Camera**: Drag your Main Camera (usually a child of the Player)
- **Player Transform**: Drag your Player GameObject

**Note**: If you leave these empty, the script will try to find them automatically, but it's recommended to assign them manually.

## Step 3: Connect to PlayerHealthSystem

1. Select your **Player GameObject** (the one with `PlayerHealthSystem` component)
2. Find the **PlayerHealthSystem** component in the Inspector
3. Scroll down to the **Directional Damage Indicator** section (new header added)
4. Drag the "Directional Damage Indicator" GameObject into the **Damage Indicator** field

## Step 4: Verify Setup

### Quick Checklist
- [ ] 4 blood splatter Image GameObjects created
- [ ] All images have alpha set to 0
- [ ] All images have Raycast Target disabled
- [ ] DirectionalDamageIndicator component added
- [ ] All 4 Image references assigned in DirectionalDamageIndicator
- [ ] Player Camera and Player Transform assigned
- [ ] PlayerHealthSystem has Damage Indicator reference assigned

### Testing in Editor

1. Select the "Directional Damage Indicator" GameObject
2. In the Inspector, find the **DirectionalDamageIndicator** component
3. Right-click on the component header → **Test Top Indicator**
   - You should see a blood splatter appear at the top of the Game view and fade out
4. Test the other directions:
   - Right-click → **Test Bottom Indicator**
   - Right-click → **Test Left Indicator**
   - Right-click → **Test Right Indicator**
   - Right-click → **Test High Damage** (should be more intense)

**If indicators don't show:**
- Check that Game view is visible
- Verify all Image references are assigned
- Check that images have the correct alpha settings (should be 0 initially)
- Ensure Canvas is in Screen Space - Overlay mode

### Testing in Play Mode

1. Enter Play Mode
2. Let an enemy attack you
3. You should see a blood splatter appear on the side of the screen where the enemy is
4. The indicator should:
   - Appear quickly (fade in over 0.1s)
   - Hold at full intensity briefly (0.3s)
   - Fade out gradually (2.5s)
   - Scale up if you take 2+ damage

## Customization Options

### Adjusting Appearance

**Blood Splatter Sprites:**
- Replace with your own custom blood sprites
- Use different sprites for each direction
- Adjust sprite colors in Image component

**Positioning:**
- Move splatters closer/further from edges by adjusting Pos X/Y
- Change size by adjusting Width/Height in RectTransform

**Colors:**
- You can tint the blood color using the Image Color property (keep alpha at 0)
- Example: For darker blood, set Color to (120, 0, 0, 0)

### Adjusting Behavior

**Duration:**
- Increase `Fade Out Duration` for longer-lasting indicators
- Decrease for quicker feedback

**Intensity:**
- Increase `Max Damage Alpha` for more visible damage indicators
- Increase `High Damage Scale` for more dramatic high-damage effects

**Direction Sensitivity:**
- The system uses the strongest directional component (X or Z)
- Enemies at 45-degree angles will show on the dominant axis (left/right or front/back)

### Advanced: Corner Indicators

If you want 8-directional indicators (including corners):

1. Create 4 additional blood splatter Images for corners:
   - Blood Splatter - Top Left
   - Blood Splatter - Top Right
   - Blood Splatter - Bottom Left
   - Blood Splatter - Bottom Right

2. Modify `DirectionalDamageIndicator.cs`:
   - Add 4 new public Image fields
   - Update `GetSplatterFromDirection()` to check for diagonal directions
   - Use threshold comparison (e.g., if absX and absZ are both > 0.5, it's a corner)

## Troubleshooting

### Indicator doesn't appear at all
- **Check PlayerHealthSystem**: Ensure Damage Indicator reference is assigned
- **Check Image references**: All 4 splatters must be assigned in DirectionalDamageIndicator
- **Check Canvas**: Must be active and rendering in Screen Space - Overlay

### Indicator appears in wrong direction
- **Check Player Transform**: Make sure the correct Player GameObject is assigned
- **Check Player Camera**: Verify Main Camera is assigned
- **Check enemy position**: Enemy must have a valid world position

### Indicator is too faint or too bright
- **Adjust Min/Max Damage Alpha** in DirectionalDamageIndicator settings
- **Check Image color**: Ensure base color is white (255, 255, 255)

### Indicator doesn't fade out
- **Check for errors**: Look in Console for any coroutine errors
- **Test in Play Mode**: Editor test methods might behave differently

### Multiple indicators show at once
- This shouldn't happen - the system only shows the most recent
- If this occurs, check for multiple DirectionalDamageIndicator components

## Integration with Existing Systems

### Health Bar UI
The damage indicator works alongside your existing health bar:
- Both can be children of the same Canvas
- Damage indicator is typically rendered on top (later in hierarchy)

### Other Damage Feedback
The indicator complements existing feedback:
- Camera shake (still happens)
- Damage flash (still happens)
- Damage sounds (still happen)
- Knockback (still happens)

### Enemy Attack Scripts
All enemy attack scripts have been updated to pass enemy position:
- `PlayerHealthSystem.TryApplyEnemyContactDamage()` - contact damage
- `EnemyDamageController.AttackPlayer()` - scripted attacks
- `KuchisakeOnnaController` - special enemy attacks

## Performance Considerations

### Optimization Tips
- Use compressed texture formats for blood splatter sprites
- Keep sprite resolution reasonable (512x512 or 1024x1024 max)
- Only one indicator animates at a time (no performance issues)

### Memory Usage
- 4 Image components (~minimal overhead)
- 1 active coroutine when indicator is visible
- Negligible impact on performance

## Future Enhancements

### Possible Additions
- Color variation based on damage type (red for physical, purple for magic, etc.)
- Particle effects in addition to images
- Directional arrow overlays
- Distance-based intensity (closer enemies = brighter indicator)
- Minimap integration (show enemy position on map)

---

## Credits

Directional Damage Indicator System
- Created for DisManibus horror game
- Integrates with existing PlayerHealthSystem
- Supports multiple enemy types and attack patterns

For questions or issues, refer to the main health system documentation.
