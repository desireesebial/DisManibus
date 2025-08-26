# Dullahan Head Inventory & Puzzle System Setup Guide

## Overview
This system provides a complete, robust solution for the Dullahan head puzzle with comprehensive error handling and integration capabilities.

## 🎯 **Complete Setup Checklist**

### Phase 1: Core Scripts Setup
- [ ] `DullahanHeadSO` Scriptable Objects created for all 3 heads
- [ ] `DullahanHeadInventory` script added to player
- [ ] `DullahanHeadPickable` scripts added to head GameObjects
- [ ] `DullahanBody` script added to Dullahan body GameObject
- [ ] `DullahanHeadEffectManager` script added to scene
- [ ] `DullahanAudioManager` script added to scene

### Phase 2: Chase Event System Setup
- [ ] `DullahanChaseEventManager` script added to scene
- [ ] `DullahanChaseSystem` script added to Dullahan GameObject
- [ ] 3 phase doors assigned to event manager
- [ ] Door key IDs configured (201, 202, 203)
- [ ] Timer UI created and assigned
- [ ] Phase text UI created and assigned

### Phase 3: Flashlight System Setup
- [ ] Flashlight UI elements created
- [ ] Battery indicator and text assigned
- [ ] Flashlight audio clips added to audio manager

### Phase 4: Audio Setup
- [ ] All required audio clips assigned to DullahanAudioManager
- [ ] Audio sources properly configured
- [ ] Volume and spatial settings adjusted

### Phase 5: Integration Testing
- [ ] All script references properly assigned
- [ ] No compilation errors in console
- [ ] Chase sequence starts automatically
- [ ] Timer UI displays correctly
- [ ] Doors open/close as expected
- [ ] Head attachment works properly
- [ ] Flashlight functions correctly

## Chase Event System Setup

### 1. Event Manager Setup
The `DullahanChaseEventManager` controls the entire chase sequence:

1. **Add to Scene:**
   - Create an empty GameObject named "DullahanEventManager"
   - Add the `DullahanChaseEventManager` script
   - Configure chase duration (default: 60 seconds)

2. **Assign Doors:**
   - Assign 3 doors to `phaseDoors` array:
     - Index 0: Door to Fake Head 1 room
     - Index 1: Door to Fake Head 2 room  
     - Index 2: Door to Real Head room

3. **Configure Key IDs:**
   - Set `doorKeyIDs` array: [201, 202, 203]
   - These are the key IDs for each phase door

### 2. Chase System Setup
The `DullahanChaseSystem` handles the actual chase mechanics:

1. **Add to Dullahan:**
   - Add `DullahanChaseSystem` script to your Dullahan GameObject
   - Assign Dullahan's NavMeshAgent and Animator
   - Configure chase speeds and detection ranges

2. **Visual Effects:**
   - Assign Dullahan's light for chase effects
   - Add particle systems for chase atmosphere
   - Configure material color changes

### 3. Timer UI Setup
Create a chase timer UI in your Canvas:

1. **Create Timer Container:**
   - Create an empty GameObject named "ChaseTimerUI"
   - Assign to `timerUI` in DullahanChaseEventManager

2. **Create Timer Text:**
   - Add TextMeshPro Text component
   - Position in top-center of screen
   - Assign to `timerText` in DullahanChaseEventManager

3. **Create Timer Fill:**
   - Add Image component with "Filled" type
   - Set Fill Method to "Horizontal" or "Radial 360"
   - Assign to `timerFillImage` in DullahanChaseEventManager

4. **Create Phase Text:**
   - Add TextMeshPro Text component
   - Position near timer
   - Assign to `phaseText` in DullahanChaseEventManager

### 4. Event Sequence
The system follows this sequence:

1. **Initial Chase (60s):** Player enters room → Chase starts → Door to Fake Head 1 opens
2. **Fake Head 1 Phase:** Player finds Fake Head 1 → Attaches to Dullahan → Chase starts → Door to Fake Head 2 opens
3. **Fake Head 2 Phase:** Player finds Fake Head 2 → Attaches to Dullahan → Chase starts → Door to Real Head opens
4. **Real Head Phase:** Player finds Real Head → Attaches to Dullahan → Game Complete!

### 5. Audio Integration
Add these audio clips to DullahanAudioManager:

- **timerWarningSound**: Warning beep when 10 seconds left
- **doorOpenSound**: Sound when phase door opens

### 6. Debug Features
- **Debug Mode**: Enable for testing
- **Skip Phase**: Press `P` to skip current chase
- **Reset Event**: Press `R` to reset entire sequence

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

### **Step 1: Create Scriptable Objects**
1. **Right-click in Project window** → Create → Scriptable Objects → DullahanHead
2. **Create 3 head assets:**
   - `DullahanHead_Real` (HeadType: Real)
   - `DullahanHead_Fake1` (HeadType: Fake1, with debuff effect)
   - `DullahanHead_Fake2` (HeadType: Fake2, with buff effect)
3. **Configure each head:**
   - Assign sprites, descriptions, effects
   - Set audio clips for pickup/drop/effects
   - Configure visual effects (glow, materials)

### **Step 2: Setup Player Head Inventory**
1. **Add `DullahanHeadInventory` script to player**
2. **Assign UI elements:**
   - Head inventory slot images (3 slots)
   - Background images for selection
   - Empty slot sprite
3. **Assign head GameObjects:**
   - `dullahanHead_Real` (child of player)
   - `dullahanHead_Fake1` (child of player)
   - `dullahanHead_Fake2` (child of player)
4. **Assign head prefabs:**
   - Prefabs for dropping heads with physics
5. **Configure flashlight settings:**
   - Battery life, drain rate, recharge rate
   - Flashlight intensity, range, angle
   - UI elements for battery indicator

### **Step 3: Setup Head Pickables**
1. **Add `DullahanHeadPickable` script to each head GameObject in world**
2. **Assign ScriptableObject data** to each head
3. **Configure visual effects:**
   - Glow effect, materials
   - Pickup range, interaction prompts
4. **Add physics components:**
   - Rigidbody for physics
   - Collider for interaction

### **Step 4: Setup Dullahan Body**
1. **Add `DullahanBody` script to Dullahan body GameObject**
2. **Assign attachment point** for head
3. **Configure interaction:**
   - Range, key bindings
   - Visual feedback, UI prompts
4. **Assign final door** that opens when puzzle is complete

### **Step 5: Setup Effect Manager**
1. **Add `DullahanHeadEffectManager` script to scene**
2. **Assign player controller** reference
3. **Configure effect durations** and strengths
4. **Test buff/debuff effects** on player

### **Step 6: Setup Audio Manager**
1. **Add `DullahanAudioManager` script to scene**
2. **Assign audio clips:**
   - Chase audio (start, intensity, end)
   - Head audio (pickup, drop, effects)
   - Flashlight audio (on/off, battery warnings)
   - Timer audio (warning, door open)
3. **Configure audio sources** and settings
4. **Test all audio** functionality

### **Step 7: Integration Testing**
1. **Test head pickup** and inventory management
2. **Test head selection** and visual representation
3. **Test head attachment** to Dullahan body
4. **Test buff/debuff effects** from fake heads
5. **Test flashlight** functionality and battery system
6. **Test chase event sequence** and timer
7. **Test audio integration** across all systems

## 🔧 **Troubleshooting Common Issues**

### **Issue: Heads not appearing in player's hand**
**Solution:**
- Check if head GameObjects are assigned in DullahanHeadInventory
- Verify head GameObjects are children of player
- Ensure `NewHeadSelected()` is called when head is selected

### **Issue: Head pickup not working**
**Solution:**
- Verify `DullahanHeadPickable` script is on head GameObject
- Check if `headData` ScriptableObject is assigned
- Ensure player has `DullahanHeadInventory` script
- Check pickup range and interaction key

### **Issue: Chase not starting**
**Solution:**
- Verify `DullahanChaseEventManager` is in scene
- Check if `startChaseOnGameStart` is enabled
- Ensure `DullahanChaseSystem` is on Dullahan GameObject
- Verify all references are properly assigned

### **Issue: Timer UI not showing**
**Solution:**
- Check if timer UI elements are assigned in event manager
- Verify timer UI is active when chase starts
- Ensure TextMeshPro components are properly configured

### **Issue: Doors not opening**
**Solution:**
- Verify door references in event manager
- Check door key IDs match configuration
- Ensure `Door` script has `UnlockDoor()` method
- Test door functionality independently

### **Issue: Audio not playing**
**Solution:**
- Check if audio clips are assigned in DullahanAudioManager
- Verify audio sources are properly configured
- Ensure volume settings are not muted
- Test audio manager methods directly

### **Issue: Flashlight not working**
**Solution:**
- Verify flashlight UI elements are assigned
- Check if flashlight key (T) is properly configured
- Ensure battery settings are reasonable
- Test flashlight methods directly

## 🎮 **Game Flow Summary**

1. **Game Start:** Player enters room, initial chase begins (60s timer)
2. **Chase Phase:** Dullahan chases player with increasing intensity
3. **Door Opens:** After chase ends, door to Fake Head 1 opens
4. **Head Collection:** Player finds and picks up Fake Head 1
5. **Head Attachment:** Player attaches Fake Head 1 to Dullahan body
6. **Next Chase:** Second chase begins, door to Fake Head 2 opens
7. **Repeat Cycle:** Player collects Fake Head 2, attaches it
8. **Final Chase:** Third chase begins, door to Real Head opens
9. **Puzzle Complete:** Player finds Real Head, attaches it, game wins!

## 🎯 **Key Features**

- **Complete Chase Sequence:** 4-phase chase system with timed events
- **Head Inventory System:** Specialized inventory for Dullahan heads
- **Flashlight System:** Battery-powered flashlight with UI feedback
- **Audio Integration:** Comprehensive audio system for all events
- **Visual Effects:** Dynamic lighting, particles, and material changes
- **Error Handling:** Graceful handling of missing references
- **Debug Tools:** Built-in debugging and testing features
- **Modular Design:** Easy to customize and extend

The system is now complete and ready for implementation! All scripts are properly integrated and error-free.
