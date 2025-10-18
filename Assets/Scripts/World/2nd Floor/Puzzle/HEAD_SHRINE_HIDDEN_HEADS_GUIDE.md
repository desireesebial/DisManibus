# 🏛️ Head Shrine Puzzle - Hidden Heads System

## 🎯 **Overview**
The Head Shrine Puzzle is now a **single altar** built from different assets (table, candles, etc.) with **3 hidden heads** scattered around the map. Players must find and place these heads on the altar, with different rewards for each head type.

## 🎮 **Head Types & Rewards**

### **🗝️ Real Head (Center Placement)**
- **Reward**: Opens main door + spawns key
- **Placement**: Center position between candles
- **Special**: Enhanced visual/audio effects

### **🚪 Wrong Head 1 (Left Placement)**
- **Reward**: Opens door 1
- **Placement**: Left side of altar
- **Special**: Standard activation effects

### **🚪 Wrong Head 2 (Right Placement)**
- **Reward**: Opens door 2 (or no reward)
- **Placement**: Right side of altar
- **Special**: Standard activation effects

## 🛠️ **Setup Guide**

### **Step 1: Create the Shrine**
1. Create an **empty GameObject** in your scene
2. Name it `"HeadShrine"`
3. Add the `HeadShrinePuzzle` script to it

### **Step 2: Build the Altar**
Create the altar using different assets (like your Unity scene):

```
HeadShrine (GameObject with HeadShrinePuzzle script)
├── TableV2 (Your wooden table asset)
├── CandleV1 (Left candle holder)
├── CandleV2 (Center candle holder) 
├── CandleV3 (Right candle holder)
├── Placement1 (Empty GameObject - CENTER for real head)
├── Placement2 (Empty GameObject - Left side)
└── Placement3 (Empty GameObject - Right side)
```

### **Step 3: Configure in Inspector**

#### **🏛️ Shrine Settings:**
```csharp
requiredHeadIDs = { 1, 2, 3 };           // Head IDs for each placement
realHeadPlacementIndex = 0;              // Center placement (0 = first)
interactionDistance = 4f;                // How close player needs to be
```

#### **🎯 Head Rewards:**
```csharp
wrongHead1Door = [doorscript that opens for wrong head 1];
wrongHead2Door = [doorscript that opens for wrong head 2];
realHeadDoor = [doorscript that opens for real head];
keyReward = [Key prefab that spawns];
keySpawnPoint = [Transform where key spawns];
```

#### **🎨 Visual Settings:**
```csharp
emptyPlacementMaterial = [Empty placement material];
filledPlacementMaterial = [Filled placement material];
realHeadPlacementMaterial = [Special material for center];
altarBaseMaterial = [Altar base material];
```

#### **🔥 Fire Effects:**
```csharp
activationParticlePrefab = [Activation particle system];
placementLightPrefab = [Placement light prefab];
```

#### **🎵 Audio:**
```csharp
headPlacedSound = [Placement sound];
placementActivatedSound = [Activation sound];
realHeadPlacedSound = [Real head placement sound];
doorOpenSound = [Door opening sound];
keySpawnSound = [Key spawning sound];
shrineCompleteSound = [Completion sound];
wrongHeadSound = [Wrong head sound];
mysticalChantingSound = [Chanting sound];
```

## 🎮 **How It Works**

### **Player Experience:**
1. **Player explores map** → Finds 3 hidden heads
2. **Player approaches shrine** → Mystical chanting plays
3. **Player gets within 4 units** → Can interact with placement points
4. **Player presses F** → Attempts to place current head
5. **Head placed** → Placement activates with particles and light
6. **Rewards granted** → Doors open, keys spawn based on head type

### **Reward System:**
```
Real Head (Center) → Main Door Opens + Key Spawns
Wrong Head 1 (Left) → Door 1 Opens
Wrong Head 2 (Right) → Door 2 Opens (or no reward)
```

## 🎯 **Puzzle Flow**

```
1. Player finds Real Head (ID: 1) → Places on center → Main door opens + key spawns
2. Player finds Wrong Head 1 (ID: 2) → Places on left → Door 1 opens
3. Player finds Wrong Head 2 (ID: 3) → Places on right → Door 2 opens
4. All placements activated → Shrine completion effects
```

## 🎨 **Visual Design**

### **Materials Needed:**
- **Empty Placement Material**: Dark stone for empty placement points
- **Filled Placement Material**: Glowing material when head is placed
- **Real Head Placement Material**: Special material for center placement
- **Altar Base Material**: Wooden/stone material for the main altar

### **Effects:**
- **Activation Particles**: When placement is activated
- **Placement Lights**: Glow when activated
- **Special Center Effect**: Enhanced visuals for real head placement
- **Door Opening Effects**: Visual feedback when doors open
- **Key Spawn Effects**: Visual feedback when key spawns

## 🎁 **Reward System**

### **Real Head Rewards:**
- **Main door unlocks** (if assigned)
- **Key spawns** at designated point
- **Special audio/visual effects**
- **Event manager notification**

### **Wrong Head Rewards:**
- **Wrong Head 1**: Opens door 1
- **Wrong Head 2**: Opens door 2 (or no reward)
- **Standard activation effects**

### **General Rewards:**
- **Shrine completion effects** when all placements are activated
- **Additional reward items** (if assigned)
- **Shrine activation effects** (if assigned)

## 🔧 **Advanced Features**

### **Flexible Setup:**
- **Automatic creation** if no placements exist
- **Manual setup** with custom placement points
- **Configurable head IDs** for each placement
- **Individual door assignments** for each head type

### **Visual Feedback:**
- **Clear progression** as placements activate
- **Different materials** for different states
- **Particle effects** for activation
- **Light effects** for glow
- **Special effects** for real head placement

### **Audio Integration:**
- **Individual sounds** for each head type
- **Door opening sounds** for each door
- **Key spawning sound** for real head
- **Mystical chanting** background audio

## 🎯 **Perfect for Your Vision**

This system perfectly matches your requirements:
- **Single altar** built from different assets (table, candles, etc.)
- **3 hidden heads** scattered around the map
- **2 wrong heads** with door rewards
- **1 real head** with key spawn and main door reward
- **Simple setup** for indie developers
- **Engaging gameplay** with clear rewards

The puzzle provides a satisfying exploration and puzzle-solving experience where players must find the hidden heads and discover which one is the real head that grants the ultimate reward!
