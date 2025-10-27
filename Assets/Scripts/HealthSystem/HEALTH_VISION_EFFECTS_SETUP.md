# Health-Based Vision Effects Setup Guide

This guide will help you set up the health-based vision effects system that creates tunnel vision and blur effects as player health decreases.

## What It Does

When enemies attack and player health decreases:
- **Vignette (Tunnel Vision)**: Dark edges close in around the screen
- **Blur Effect**: Screen becomes progressively blurrier
- **Pulse Effect**: At critical health, effects pulse to create urgency
- **Smooth Transitions**: Effects fade in/out smoothly as health changes

### Effect Scaling:
- **Full Health (3/3 HP)**: No effects - clear vision
- **Medium Health (2/3 HP)**: Mild vignette (20%), slight blur
- **Low Health (1/3 HP)**: Strong vignette (50%), heavy blur
- **Critical Health**: Maximum effects + pulsing

---

## Setup Instructions

### Step 1: Generate the Vignette Texture

1. **In Unity menu, go to: Tools → Generate Vignette Texture**
2. A window will open with these settings:
   - **Texture Size**: 512 (default is fine)
   - **Vignette Strength**: 0.8
   - **Vignette Size**: 0.6
   - **Vignette Color**: Black
   - **Texture Name**: HealthVignette

3. **Click "Generate Vignette Texture"**
4. The texture will be created at: `Assets/Textures/UI/HealthVignette.png`
5. Unity will automatically select it in the Project window

### Step 2: Create the UI Overlay

1. **In your main Canvas** (the one containing your health UI):
   - Right-click → **UI → Image**
   - Name it: **"VignetteOverlay"**

2. **Configure the VignetteOverlay**:

   **RectTransform:**
   - **Anchors**: Stretch (full screen)
     - Min: (0, 0)
     - Max: (1, 1)
   - **Left, Right, Top, Bottom**: All set to 0
   - **Position**: (0, 0, 0)

   **Image Component:**
   - **Source Image**: Drag the `HealthVignette` texture here
   - **Color**: Black with Alpha = 0 (will be controlled by script)
   - **Raycast Target**: ❌ Unchecked (so it doesn't block clicks)

3. **Set Layer Order**:
   - Make sure VignetteOverlay is **at the bottom** of your Canvas hierarchy
   - Or if you want it on top, put it **at the very top**
   - Recommendation: Place it between UI elements and gameplay (above health/quest UI)

### Step 3: Add the HealthVisionEffects Component

1. **Create a new GameObject**:
   - In Hierarchy: Right-click → **Create Empty**
   - Name it: **"HealthVisionEffects"**
   - Place it under your Canvas or UI manager

2. **Add the component**:
   - Select `HealthVisionEffects`
   - Click **Add Component**
   - Search for: **HealthVisionEffects**
   - Add it

3. **Configure the component** (in Inspector):

   **References:**
   - **Player Health System**: Drag your PlayerHealthSystem GameObject here (usually on Player)
   - **Vignette Overlay**: Drag the VignetteOverlay Image here
   - **Post Process Volume**: (Optional) Drag if you have post-processing

   **Vignette Settings:**
   - **Vignette Color**: Black (RGB: 0, 0, 0)
   - **Max Vignette Intensity**: 0.8
   - **Min Vignette Intensity**: 0

   **Intensity at Each Threshold:**
   - **Vignette At Full**: 0 (no effect at full health)
   - **Vignette At High**: 0.2 (mild at 66% health)
   - **Vignette At Mid**: 0.5 (moderate at 33% health)
   - **Vignette At Low**: 0.8 (strong at critical health)

   **Blur Settings:**
   - **Enable Blur**: ✓ Checked (if you have post-processing)
   - **Max Blur Intensity**: 5.0

   **Transition Settings:**
   - **Transition Speed**: 3.0 (how fast effects change)
   - **Use Smooth Easing**: ✓ Checked

   **Pulse Effect:**
   - **Enable Pulse At Critical**: ✓ Checked
   - **Pulse Speed**: 2.0
   - **Pulse Amount**: 0.15

   **Debug:**
   - **Show Debug**: ✓ Checked (for testing, uncheck later)

### Step 4: Test the Effects

1. **Enter Play Mode**
2. **Take damage** from an enemy or use console commands
3. **Watch the effects**:
   - Health decreases → vignette darkens
   - At low health → screen pulses
   - Heal → effects fade away

4. **Manual Testing** (right-click on HealthVisionEffects component):
   - **Test - Set to Critical Health**: See maximum effects
   - **Test - Set to Mid Health**: See medium effects
   - **Test - Reset Effects**: Clear all effects

---

## Customization Options

### Adjust Vignette Intensity

Change these values in HealthVisionEffects component:
- **Vignette At Full**: 0 = no effect when healthy
- **Vignette At High**: 0-0.3 = subtle at 66% health
- **Vignette At Mid**: 0.3-0.6 = moderate at 33% health
- **Vignette At Low**: 0.6-0.9 = intense at critical health

### Change Vignette Color

Want a red vignette instead of black?
- Set **Vignette Color** to dark red (RGB: 0.5, 0, 0)
- Regenerate texture with red color
- More dramatic/bloody effect!

### Adjust Transition Speed

- **Transition Speed: 1-2**: Slow, gradual transition
- **Transition Speed: 3-5**: Medium, noticeable transition
- **Transition Speed: 6-10**: Fast, immediate transition

### Disable Pulse Effect

If pulsing is too distracting:
- Uncheck **Enable Pulse At Critical**

### Change Pulse Speed

- **Pulse Speed: 1-2**: Slow, subtle pulse
- **Pulse Speed: 3-4**: Faster, more urgent
- **Pulse Speed: 5+**: Rapid, intense pulse

---

## Troubleshooting

### Vignette doesn't appear

**Check:**
1. Is VignetteOverlay assigned in HealthVisionEffects?
2. Is VignetteOverlay active in Hierarchy?
3. Is the HealthVignette texture assigned to the Image component?
4. Check Console for error messages

### Vignette is always visible

**Fix:**
- Select VignetteOverlay
- In Image component, set **Color Alpha to 0**
- Make sure **Vignette At Full** is set to 0

### Effects don't change with health

**Fix:**
1. Check that **Player Health System** is assigned
2. Make sure PlayerHealthSystem has `OnHealthChanged` UnityEvent
3. Check Console for `[HealthVisionEffects]` debug messages
4. Try testing with context menu options

### Vignette blocks UI clicks

**Fix:**
- Select VignetteOverlay
- In Image component, uncheck **Raycast Target**

### Effects transition too fast/slow

**Fix:**
- Adjust **Transition Speed** in HealthVisionEffects component
- Lower = slower transitions
- Higher = faster transitions

---

## Advanced: Adding Blur

If you have Unity Post Processing Stack installed:

### Option 1: Post Processing Stack v2 (Built-in Render Pipeline)

1. Add a **Post Process Volume** to your scene
2. Enable **Depth of Field** or **Motion Blur**
3. Assign the volume to **Post Process Volume** field in HealthVisionEffects

### Option 2: URP Volume

1. Add a **Volume** component to your scene
2. Add **Depth of Field** override
3. Assign it to **Post Process Volume** field

### Option 3: No Post-Processing

The vignette effect works without any post-processing! The blur feature is optional.

---

## How It Works

### Event System:
1. Enemy attacks → PlayerHealthSystem.TakeDamage()
2. Health decreases → OnHealthChanged event fires
3. HealthVisionEffects receives event → calculates new intensity
4. Effects lerp to new intensity over time
5. Vignette alpha updates every frame

### Intensity Calculation:
```
Health 100% → Vignette 0%
Health 66%  → Vignette 20%
Health 33%  → Vignette 50%
Health <10% → Vignette 80% + Pulse
```

### Smooth Transitions:
- Uses `Mathf.Lerp()` for smooth interpolation
- Transition speed controls how fast effects change
- Pulse effect uses `Mathf.Sin()` for rhythmic variation

---

## Files Created/Modified

### Created:
1. **`Assets/Scripts/HealthSystem/HealthVisionEffects.cs`**
   - Main script controlling vision effects

2. **`Assets/Editor/VignetteTextureGenerator.cs`**
   - Editor tool to generate vignette textures

3. **`Assets/Textures/UI/HealthVignette.png`** (after generation)
   - Radial gradient texture for vignette effect

4. **`Assets/Scripts/HealthSystem/HEALTH_VISION_EFFECTS_SETUP.md`** (this file)
   - Setup instructions

### Scene Setup:
- **VignetteOverlay** (UI Image GameObject)
- **HealthVisionEffects** (GameObject with component)

---

## Tips for Best Results

### Visual Design:
- Keep **Max Vignette Intensity** below 0.9 so player can always see
- Use **Pulse Effect** sparingly - it can be distracting
- Black vignette is classic, but dark red adds urgency

### Game Feel:
- **Fast Transition (5+)**: Arcadey, immediate feedback
- **Slow Transition (2-3)**: Cinematic, dramatic
- **Pulse Speed 2-3**: Creates tension without being annoying

### Performance:
- Vignette is very lightweight (just a UI Image alpha)
- Blur can be expensive - test on target hardware
- Disable blur on mobile/low-end devices

### Accessibility:
- Some players may find pulsing effects uncomfortable
- Consider adding an option to disable pulse
- Keep max intensity below 0.9 for visibility

---

**Last Updated**: 2025-10-27
**Created By**: Claude Code
**Feature**: Health-Based Vision Effects (Tunnel Vision + Blur)
