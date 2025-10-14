# 🔥 Sequential Torch Lighting Puzzle Setup Guide

## 🎯 Overview

This puzzle system allows players to light torches or candles in a specific sequence to unlock doors or reveal secrets. Each torch can only be lit when the previous one in the sequence is already lit, creating a chain reaction puzzle.

## ✨ Features

- **Sequential Lighting**: Torches must be lit in order (1, 2, 3, 4, 5...)
- **Visual Feedback**: Ready torches glow, lit torches have flames and light
- **Audio Feedback**: Different sounds for success, failure, and completion
- **Particle Effects**: Fire particles, ready-to-light effects, completion celebration
- **Auto-Reset**: Optional reset when wrong sequence is attempted
- **Reward System**: Unlock doors, spawn items on completion
- **Debug Tools**: Built-in testing and debugging features

---

## 🚀 Quick Setup (5 Minutes)

### **Step 1: Create the Puzzle Manager**

1. Create an empty GameObject in your scene
2. Name it `SequentialTorchManager`
3. Add the `SequentialTorchManager` script
4. Configure the settings (see Configuration section below)

### **Step 2: Set Up Individual Torches**

For each torch/candle in your puzzle:

1. Create a GameObject for the torch
2. Add the `SequentialTorch` script
3. Set the `Sequence Number` (1, 2, 3, etc.)
4. Assign visual components (see Visual Setup below)
5. Add the torch to the manager's `Torches` array

### **Step 3: Configure Visual Components**

Each torch needs:
- **Flame Object**: The fire/flame visual (initially disabled)
- **Light Component**: For illumination when lit
- **Particle Systems**: For fire effects and ready-to-light effects
- **Renderer**: For color changes (ready = yellow, lit = orange)

### **Step 4: Test the Puzzle**

1. Play the scene
2. Walk up to the first torch
3. Press **F** to light it
4. Move to the next torch and repeat
5. Watch the door unlock when all torches are lit!

---

## ⚙️ Configuration

### **SequentialTorchManager Settings**

| Field | Description | Recommended Value |
|-------|-------------|-------------------|
| **Torches** | Array of all torches in sequence | Drag all torch GameObjects here |
| **Reset Delay** | How long to wait before resetting | `3` seconds |
| **Reset On Wrong Sequence** | Auto-reset when wrong torch is lit | ✅ Checked |
| **Reward Door** | Door to unlock on completion | Drag your door script |
| **Reward Items** | Items to spawn on completion | Optional |
| **Reward Spawn Point** | Where to spawn items | Create empty GameObject |

### **SequentialTorch Settings**

| Field | Description | Recommended Value |
|-------|-------------|-------------------|
| **Sequence Number** | Order in the sequence (1, 2, 3...) | `1`, `2`, `3`, etc. |
| **Interaction Distance** | How close player needs to be | `3` units |
| **Flame Object** | The fire visual GameObject | Drag flame GameObject |
| **Torch Light** | Light component for illumination | Drag Light component |
| **Fire Particles** | Particle system for fire effects | Drag ParticleSystem |
| **Ready Particles** | Particles for ready-to-light effect | Drag ParticleSystem |
| **Interaction Text** | UI text for "Press F" prompt | Drag TextMeshProUGUI |

---

## 🎨 Visual Setup

### **Torch GameObject Structure**

```
Torch_1
├── TorchBase (with SequentialTorch script)
├── FlameObject (initially disabled)
├── TorchLight (Light component)
├── FireParticles (ParticleSystem)
├── ReadyParticles (ParticleSystem)
└── InteractionText (TextMeshProUGUI)
```

### **Required Components**

#### **1. Flame Object**
- Create a child GameObject with fire/flame visual
- Initially **disabled** in the scene
- Will be enabled when torch is lit

#### **2. Light Component**
- Add `Light` component to the torch
- Initially **disabled**
- Will be enabled when torch is lit
- Recommended settings:
  - **Type**: Point
  - **Range**: 10-15
  - **Intensity**: 1-2
  - **Color**: Orange/Yellow

#### **3. Particle Systems**

**Fire Particles:**
- **Start Lifetime**: 2-3 seconds
- **Start Speed**: 0.5-1
- **Start Size**: 0.1-0.3
- **Start Color**: Orange/Red
- **Emission Rate**: 20-50
- **Shape**: Cone (pointing up)

**Ready Particles:**
- **Start Lifetime**: 1-2 seconds
- **Start Speed**: 0.2-0.5
- **Start Size**: 0.05-0.1
- **Start Color**: Yellow/White
- **Emission Rate**: 5-15
- **Shape**: Sphere

#### **4. Interaction Text**
- Create a `TextMeshProUGUI` component
- Position it above the torch
- Text: "Press F to light torch"
- Initially **disabled**

---

## 🎵 Audio Setup

### **Recommended Audio Clips**

| Sound Type | Description | File Format |
|------------|-------------|-------------|
| **Light Sound** | When torch is successfully lit | .wav, .mp3 |
| **Wrong Sequence Sound** | When wrong torch is attempted | .wav, .mp3 |
| **Ready Sound** | When torch becomes ready to light | .wav, .mp3 |
| **Puzzle Complete Sound** | When all torches are lit | .wav, .mp3 |
| **Puzzle Reset Sound** | When puzzle resets | .wav, .mp3 |

### **Audio Settings**
- **Volume**: 0.5-0.8
- **Pitch**: 1.0 (normal)
- **3D Sound**: Enable for spatial audio

---

## 🎮 Player Interaction

### **Controls**
- **F Key**: Attempt to light torch (when in range)
- **Movement**: Walk to different torches
- **Lantern/Flashlight**: Must be in inventory to light torches

### **Interaction Flow**
1. Player approaches torch
2. If torch is ready to light: "Press F to light torch" appears
3. If torch is not ready: No prompt appears
4. Player presses F
5. If correct sequence: Torch lights, next torch becomes ready
6. If wrong sequence: Red flash, sound, possible reset

---

## 🐛 Testing & Debugging

### **Built-in Debug Tools**

#### **Context Menu Commands**
Right-click on `SequentialTorchManager` in Inspector:
- **Reset Puzzle**: Reset all torches to unlit state
- **Complete Puzzle**: Light all torches for testing

#### **Console Logs**
Look for these debug messages:
```
[SequentialTorch] Torch 1 initialized
[SequentialTorchManager] Puzzle initialized with 5 torches
[SequentialTorch] ✓ Torch 1 LIT!
[SequentialTorchManager] Torch 2 is now ready to light
[SequentialTorchManager] 🎉 PUZZLE COMPLETED!
```

### **Common Issues & Solutions**

#### **Problem: F key doesn't work**
**Solutions:**
1. Check `Interaction Distance` - increase to 5-10
2. Make sure player has lantern/flashlight in inventory
3. Check Console for `[SequentialTorch]` logs
4. Verify torch is ready to light (should have yellow glow)

#### **Problem: Torches don't light in sequence**
**Solutions:**
1. Check `Sequence Number` on each torch (should be 1, 2, 3, etc.)
2. Verify torches are added to manager's `Torches` array
3. Make sure torches are sorted by sequence number
4. Check Console for sequence validation logs

#### **Problem: No visual effects**
**Solutions:**
1. Assign `Flame Object` to each torch
2. Assign `Torch Light` component
3. Assign `Fire Particles` and `Ready Particles`
4. Check that particle systems are configured properly

#### **Problem: Door doesn't unlock**
**Solutions:**
1. Assign `Reward Door` in manager settings
2. Make sure door script has `UnlockDoor()` method
3. Check Console for completion logs
4. Test with debug "Complete Puzzle" command

---

## 🎯 Advanced Features

### **Custom Sequence Patterns**
You can modify the sequence logic in `SequentialTorchManager.CanLightTorch()` to create custom patterns:
- **Reverse sequence**: Light torches 5, 4, 3, 2, 1
- **Skip pattern**: Light torches 1, 3, 5, 2, 4
- **Multiple paths**: Different sequences lead to different rewards

### **Integration with Other Systems**
- **Inventory System**: Require specific items to light torches
- **Time Limits**: Add countdown timers
- **Environmental Effects**: Weather affects torch lighting
- **Enemy AI**: Dullahan can extinguish torches

### **Performance Optimization**
- **Object Pooling**: Reuse particle systems
- **LOD System**: Reduce effects at distance
- **Culling**: Disable effects when not visible

---

## 📋 Checklist

### **Setup Checklist**
- [ ] Created `SequentialTorchManager` GameObject
- [ ] Added `SequentialTorchManager` script
- [ ] Created all torch GameObjects
- [ ] Added `SequentialTorch` script to each torch
- [ ] Set sequence numbers (1, 2, 3, etc.)
- [ ] Assigned all torches to manager's array
- [ ] Set up visual components (flame, light, particles)
- [ ] Set up audio components
- [ ] Created interaction text UI
- [ ] Assigned reward door/items
- [ ] Tested the puzzle

### **Testing Checklist**
- [ ] First torch lights immediately
- [ ] Second torch only lights after first
- [ ] Wrong sequence shows feedback
- [ ] Puzzle resets after wrong sequence (if enabled)
- [ ] All torches light in correct order
- [ ] Door unlocks on completion
- [ ] Reward items spawn on completion
- [ ] Audio plays correctly
- [ ] Visual effects work properly

---

## 🎉 You're Done!

Your sequential torch lighting puzzle is now ready! Players will need to light torches in the correct sequence to progress, creating an engaging and challenging puzzle experience.

For more advanced features or custom modifications, refer to the script comments and feel free to extend the system to fit your specific needs.
