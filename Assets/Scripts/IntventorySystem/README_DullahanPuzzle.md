# Dullahan Head Inventory & Puzzle System Setup Guide

## Overview
This system provides a complete, robust solution for the Dullahan head puzzle with comprehensive error handling and integration capabilities.

## Flashlight System Setup

### 1. Flashlight UI Setup
Create a flashlight UI in your Canvas:

1. **Create Battery Indicator:**
   - Add an Image component to your Canvas
   - Set Image Type to "Filled"
   - Set Fill Method to "Radial 360" or "Horizontal"
   - Position it in the bottom-right corner of the screen
   - Assign it to `batteryIndicator` in DullahanHeadInventory

2. **Create Battery Text:**
   - Add a TextMeshPro Text component
   - Position it near the battery indicator
   - Assign it to `batteryText` in DullahanHeadInventory

3. **Create Flashlight UI Container:**
   - Create an empty GameObject as parent for flashlight UI elements
   - Assign it to `flashlightUI` in DullahanHeadInventory

### 2. Flashlight Audio Setup
Add these audio clips to your DullahanAudioManager:

- **flashlightOnSound**: Click/switch sound when turning on
- **flashlightOffSound**: Click/switch sound when turning off  
- **batteryLowSound**: Warning beep for low battery
- **batteryDeadSound**: Final warning when battery dies

### 3. Flashlight Controls
- **Toggle Flashlight**: Press `T` key (configurable)
- **Battery Management**: 
  - Drains while on (1 second per real second)
  - Recharges while off (0.5 seconds per real second)
  - 5-minute total battery life (configurable)

### 4. Flashlight Features
- **Follows Camera**: Automatically follows player camera
- **Battery Warnings**: Audio warnings at 20% and 0% battery
- **Infinite Battery Mode**: Toggle for testing
- **Battery Recharge**: Recharges when turned off
- **Visual Feedback**: Battery indicator with color coding

### 5. Integration with Head System
The flashlight works independently of the head inventory system, allowing players to:
- Use flashlight while holding heads
- Toggle flashlight with one hand
- Manage battery while exploring
- Get audio feedback for battery status

## 🎯 **Complete Setup Guide for Dullahan Head Inventory System**

This system provides a specialized inventory for Dullahan heads with visual hand-holding, effects, and puzzle completion mechanics.

## 📁 **New Scripts Created**

### **Core Scripts:**
1. **`DullahanHeadInventory.cs`** - Specialized inventory for Dullahan heads
2. **`DullahanHeadSO.cs`** - Scriptable Object for head data
3. **`DullahanHeadPickable.cs`** - Makes heads pickable in the world
4. **`DullahanHeadEffectManager.cs`** - Manages buffs/debuffs from fake heads
5. **`DullahanBody.cs`** - Handles head attachment to Dullahan body
6. **`DullahanAudioManager.cs`** - Centralized audio management
7. **`DullahanPuzzleManager.cs`** - Coordinates all puzzle components

## 🛠️ **Step-by-Step Setup**

### **Step 1: Create DullahanHeadSO Assets**

1. **Right-click in Project** → **Create** → **Scriptable Objects** → **DullahanHead**
2. **Create 3 head assets**:
   - `DullahanHead_Real.asset`
   - `DullahanHead_Fake1.asset` 
   - `DullahanHead_Fake2.asset`

3. **Configure each head**:
   ```
   Real Head:
   - Head Name: "Real Dullahan Head"
   - Head ID: 1
   - Head Type: Real
   - Has Effect: false
   
   Fake Head 1:
   - Head Name: "Fake Head 1"
   - Head ID: 2
   - Head Type: Fake1
   - Has Effect: true
   - Effect Type: SpeedBoost
   - Effect Strength: 0.3
   - Effect Duration: 15
   
   Fake Head 2:
   - Head Name: "Fake Head 2"
   - Head ID: 3
   - Head Type: Fake2
   - Has Effect: true
   - Effect Type: SpeedDebuff
   - Effect Strength: 0.2
   - Effect Duration: 10
   ```

### **Step 2: Setup DullahanHeadInventory**

1. **Add DullahanHeadInventory component** to your FirstPersonController
2. **Configure the component**:
   - **Head Inventory UI**: Assign 3 Image components for head slots
   - **Player Head GameObjects**: Assign your existing head GameObjects
   - **Head Prefabs**: Assign prefabs for dropping heads
   - **Integration**: Leave empty (auto-finds other components)

### **Step 3: Setup Head GameObjects**

1. **Select each head GameObject** in your Hand hierarchy
2. **Add DullahanHeadPickable component**
3. **Assign corresponding DullahanHeadSO** to each head
4. **Add Colliders** (Box Collider) and set as trigger
5. **Add AudioSource** (optional - for local sounds)

### **Step 4: Create Manager GameObjects**

1. **Create "DullahanAudioManager"** GameObject
   - Add **DullahanAudioManager** component
   - Add 4 **AudioSource** components as children
   - Assign audio clips for chase, pickup, effects

2. **Create "DullahanHeadEffectManager"** GameObject
   - Add **DullahanHeadEffectManager** component
   - Create UI for effect notifications (optional)

3. **Create "DullahanPuzzleManager"** GameObject
   - Add **DullahanPuzzleManager** component
   - Create UI for puzzle status (optional)

### **Step 5: Setup Dullahan Body**

1. **Add DullahanBody component** to your existing Dullahan GameObject
2. **Configure the component**:
   - **Required Head ID**: Set to 1 (matches real head)
   - **Final Door**: Assign your final door
   - **Head Attachment Point**: Create child GameObject for visual attachment
   - **Attached Head Visual**: Create visual representation of attached head

### **Step 6: Create Final Door**

1. **Find or create your final door**
2. **Set Required Key ID** to 999 (special key for puzzle completion)
3. **Assign to DullahanBody** final door reference

## 🎮 **How It Works**

### **Head Pickup Flow:**
1. Player approaches head in world
2. Press E to pick up
3. Head appears in player's hands (visual)
4. Head added to head inventory
5. Effects applied if fake head

### **Head Attachment Flow:**
1. Player approaches Dullahan body with real head
2. Press F to attach head
3. Head disappears from hands
4. Puzzle completes
5. Final door unlocks

### **Effect System:**
- **Fake Head 1**: Speed boost (30% for 15 seconds)
- **Fake Head 2**: Speed debuff (20% for 10 seconds)
- Effects automatically expire and reset player stats

## 🔧 **Error Handling Features**

### **Missing Prefabs:**
- System gracefully handles missing head prefabs
- Logs warnings but doesn't crash
- Continues to function with available components

### **Missing References:**
- All scripts auto-find missing references
- Fallback to default values when needed
- Comprehensive null checks throughout

### **Audio Fallbacks:**
- Multiple audio source options
- Fallback to local audio if manager unavailable
- Graceful degradation of audio features

## 🎵 **Audio Integration**

### **Required Audio Clips:**
- Chase start/end sounds
- Head pickup sounds
- Effect sounds
- Puzzle completion sound
- Wrong head sound

### **Audio Sources:**
- **DullahanAudio**: 3D spatial audio for Dullahan
- **PlayerAudio**: 2D audio for player effects
- **AmbientAudio**: 3D ambient sounds
- **EffectAudio**: 2D effect sounds

## 🎨 **Visual Effects**

### **Head Visuals:**
- Glow effects for different head types
- Pulsing light effects
- Material changes based on head type
- Visual feedback for selection

### **Body Effects:**
- Light color changes on completion
- Visual head attachment
- Particle effects (optional)

## 🐛 **Debug Features**

### **Debug Mode:**
Enable debug mode in DullahanPuzzleManager for:
- **F1**: Complete puzzle instantly
- **F2**: Reset puzzle
- **F3**: Add all heads to inventory

### **Console Logging:**
- Comprehensive debug messages
- Warning messages for missing components
- Error handling with helpful messages

## 🔄 **Integration with Existing Systems**

### **PlayerInventory Compatibility:**
- Separate from main inventory
- No conflicts with existing items
- Can be integrated if needed

### **FirstPersonController Integration:**
- Modifies player stats for effects
- Integrates with existing movement system
- No breaking changes to original controller

### **Door System Integration:**
- Uses existing Door (Lockingsystem) component
- Compatible with current door mechanics
- Extends functionality for puzzle completion

## ✅ **Testing Checklist**

- [ ] Heads can be picked up from world
- [ ] Heads appear in player's hands
- [ ] Head inventory UI updates correctly
- [ ] Fake heads apply effects
- [ ] Effects expire properly
- [ ] Real head can be attached to body
- [ ] Puzzle completion unlocks door
- [ ] Audio plays correctly
- [ ] Visual effects work
- [ ] Debug features function

## 🚨 **Common Issues & Solutions**

### **Heads Not Appearing in Hands:**
- Check head GameObject assignments in DullahanHeadInventory
- Verify head GameObjects are children of Hand
- Ensure head GameObjects are active

### **Effects Not Working:**
- Check DullahanHeadEffectManager is in scene
- Verify effect settings in DullahanHeadSO
- Check console for error messages

### **Audio Not Playing:**
- Verify audio clips are assigned
- Check audio source configurations
- Ensure audio manager is in scene

### **Puzzle Not Completing:**
- Verify real head ID matches DullahanBody required ID
- Check final door assignment
- Ensure DullahanBody component is on correct GameObject

## 📝 **Customization Options**

### **Effect Types Available:**
- SpeedBoost/SpeedDebuff
- VisionBoost/VisionDebuff
- StaminaBoost/StaminaDebuff
- FearEffect/CalmEffect

### **Audio Customization:**
- Multiple audio clips per event
- Volume and pitch control
- Spatial vs 2D audio options

### **Visual Customization:**
- Custom materials for heads
- Glow colors and effects
- Particle system integration

This system provides a complete, robust solution for the Dullahan head puzzle with comprehensive error handling and integration capabilities.
