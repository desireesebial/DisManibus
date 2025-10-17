# 🏛️ Head Shrine Puzzle - Updated Design

## 🎯 **New Design Overview**

Based on your image and requirements, the Head Shrine Puzzle has been updated to use **one single altar** with **placement points** (empty GameObjects) positioned around it, where the **real head must be placed in the center** between the candles.

## 🏗️ **Structure**

```
🏛️ Head Shrine (Single Altar Design)
┌─────────────────────────────────────┐
│           [Candles]                 │
│                                     │
│  [Placement2]  [Placement1]  [Placement3]
│     (Left)      (CENTER)      (Right)
│     Fake 1      REAL HEAD     Fake 2
│                                     │
│           [Altar Base]              │
└─────────────────────────────────────┘
```

## 🎮 **How It Works**

### **Setup:**
1. **One altar base** (like the wooden table in your image)
2. **Three placement points** positioned around the altar
3. **Center placement** is special - requires the REAL head
4. **Left and right placements** can take fake heads

### **Player Experience:**
1. **Player approaches shrine** → Mystical chanting plays
2. **Player gets within 4 units** → Can interact with any placement point
3. **Player presses F** → Attempts to place current head
4. **Correct head placed** → Placement activates with particles and light
5. **Real head in center** → Special sound and visual effect
6. **All placements activated** → Shrine completes, rewards granted

## 🎨 **Visual Design**

### **Materials:**
- **Empty Placement Material**: Dark stone for empty placement points
- **Filled Placement Material**: Glowing material when head is placed
- **Real Head Placement Material**: Special material for center placement
- **Altar Base Material**: Wooden/stone material for the main altar

### **Effects:**
- **Activation Particles**: When placement is activated
- **Placement Lights**: Glow when activated
- **Special Center Effect**: Enhanced visuals for real head placement

## 🔧 **Key Features**

### **Real Head Priority:**
- **Center placement** is marked as the real head placement
- **Special materials and sounds** for the center
- **Enhanced visual feedback** when real head is placed

### **Flexible Setup:**
- **Automatic creation** if no placements exist
- **Manual setup** with custom placement points
- **Configurable head IDs** for each placement

### **Visual Feedback:**
- **Clear progression** as placements activate
- **Different materials** for different states
- **Particle effects** for activation
- **Light effects** for glow

## 🎯 **Configuration**

### **Inspector Settings:**
```csharp
[Header("🏛️ Shrine Settings")]
requiredHeadIDs = { 1, 2, 3 };           // Head IDs for each placement
realHeadPlacementIndex = 0;              // Center placement (0 = first)
interactionDistance = 4f;                // How close player needs to be

[Header("🎨 Visual Settings")]
emptyPlacementMaterial = [Empty Material];
filledPlacementMaterial = [Filled Material];
realHeadPlacementMaterial = [Real Head Material]; // Special for center
altarBaseMaterial = [Altar Base Material];

[Header("🔥 Fire Effects")]
activationParticlePrefab = [Particle System];
placementLightPrefab = [Light Prefab];

[Header("🎵 Audio")]
headPlacedSound = [Placement Sound];
placementActivatedSound = [Activation Sound];
realHeadPlacedSound = [Real Head Sound]; // Special for center
shrineCompleteSound = [Completion Sound];
wrongHeadSound = [Wrong Head Sound];
mysticalChantingSound = [Chanting Sound];
```

## 🎮 **Puzzle Flow**

```
1. Player finds Real Head (ID: 1)
   ↓
2. Approaches Center Placement → Presses F
   ↓
3. Real head placed → Special sound & effect → Center activates
   ↓
4. Player finds Fake Head 1 (ID: 2)
   ↓
5. Approaches Left Placement → Presses F
   ↓
6. Fake head placed → Left placement activates
   ↓
7. Player finds Fake Head 2 (ID: 3)
   ↓
8. Approaches Right Placement → Presses F
   ↓
9. Fake head placed → Right placement activates
   ↓
10. All placements activated → SHRINE COMPLETE! → Rewards granted
```

## ✨ **Benefits of This Design**

### **Matches Your Image:**
- **Single altar** like the wooden table in your image
- **Placement points** positioned around the altar
- **Center focus** for the real head
- **Candle-like atmosphere** with mystical effects

### **Easy for Indie Developers:**
- **Simple setup** - just create placement points
- **Clear objectives** - place heads on specific points
- **Visual feedback** - obvious when placements activate
- **Forgiving mechanics** - wrong heads just don't work

### **Engaging for Players:**
- **Mystical atmosphere** with chanting and effects
- **Clear progression** as placements light up
- **Special center placement** for the real head
- **Satisfying completion** with rewards

## 🎯 **Perfect for Your Vision**

This updated design perfectly matches your image of a single altar with placement points, where the real head must be placed in the center between the candles. It's simple to implement, visually appealing, and provides an engaging puzzle experience for players!
