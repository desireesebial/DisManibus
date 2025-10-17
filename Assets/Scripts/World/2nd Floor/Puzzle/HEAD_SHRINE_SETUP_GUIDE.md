# 🏛️ Head Shrine Puzzle Setup Guide

## 🎯 **Overview**
The Head Shrine Puzzle is a mystical, atmospheric puzzle where players place Dullahan heads on placement points around a single altar. The real head must be placed in the center position between the candles. Each placement point activates when the correct head is placed, creating a beautiful visual progression.

## 🛠️ **Quick Setup (3 Steps)**

### **Step 1: Create the Shrine**
1. Create an **empty GameObject** in your scene
2. Name it `"HeadShrine"`
3. Add the `HeadShrinePuzzle` script to it

### **Step 2: Create Placement Points**
Create child objects for each placement point:

**Option A: Manual Creation**
```
HeadShrine (GameObject with HeadShrinePuzzle script)
├── AltarBase (Cube - the main altar)
├── Placement1 (Empty GameObject - CENTER for real head)
├── Placement2 (Empty GameObject - Left side)
└── Placement3 (Empty GameObject - Right side)
```

**Option B: Automatic Creation**
- The script will automatically create placement points if none are found
- Creates a single altar base with placement points positioned around it

### **Step 3: Configure in Inspector**
Set these values in the `HeadShrinePuzzle` script:

```csharp
[Header("🏛️ Shrine Settings")]
requiredHeadIDs = { 1, 2, 3 }; // Head IDs needed for each placement
realHeadPlacementIndex = 0;     // Center placement (0 = first, 1 = second, etc.)
interactionDistance = 4f;       // How close player needs to be

[Header("🎨 Visual Settings")]
emptyPlacementMaterial = [Your empty placement material];
filledPlacementMaterial = [Your filled placement material];
realHeadPlacementMaterial = [Your real head placement material]; // Special for center
altarBaseMaterial = [Your altar base material];

[Header("🔥 Fire Effects")]
activationParticlePrefab = [Your activation particle system];
placementLightPrefab = [Your placement light prefab];

[Header("🎵 Audio")]
headPlacedSound = [Your placement sound];
placementActivatedSound = [Your activation sound];
shrineCompleteSound = [Your completion sound];
wrongHeadSound = [Your wrong head sound];
realHeadPlacedSound = [Your real head placement sound];
mysticalChantingSound = [Your chanting sound];

[Header("🎁 Rewards")]
rewardDoor = [Your door to unlock];
rewardItems = [Your reward items array];
rewardSpawnPoint = [Your spawn point transform];
```

## 🎨 **Visual Setup**

### **Materials Needed:**
1. **Empty Placement Material**: Dark, stone-like material for empty placement points
2. **Filled Placement Material**: Glowing material when head is placed
3. **Real Head Placement Material**: Special material for the center placement (where real head goes)
4. **Altar Base Material**: Stone or marble material for the main altar

### **Activation Effects:**
1. **Particle System**: Create an activation particle system prefab
2. **Light Component**: Create a light prefab for placement glow
3. **Animation**: Optional shrine completion animation

### **Audio Setup:**
1. **Head Placed Sound**: Soft "thud" or "clink" sound
2. **Placement Activated Sound**: Activation or lighting sound
3. **Shrine Complete Sound**: Triumphant or mystical sound
4. **Wrong Head Sound**: Rejection or error sound
5. **Real Head Placed Sound**: Special sound when real head is placed in center
6. **Mystical Chanting**: Ambient background chanting

## 🎮 **How It Works**

### **Player Experience:**
1. **Player approaches shrine** → Mystical chanting plays
2. **Player gets within 4 units** → Can interact with placement points
3. **Player presses F** → Attempts to place current head
4. **Correct head placed** → Placement activates, particles start, light glows
5. **Real head placed in center** → Special sound and visual effect
6. **All placements activated** → Shrine activates, rewards granted

### **Visual Progression:**
```
[Empty Placements] → [Head Placed] → [Placement Activated] → [All Activated] → [Shrine Complete]
        🔥                🔥                🔥                🔥                ✨
     (Dark)           (Head)           (Glowing)        (All Glowing)      (Activated)
```

## 🎯 **Puzzle Flow**

```
Player finds Head ID 1 → Approaches Placement 1 (Center) → Presses F
    ↓
Real head placed on center placement → Special sound & effect → Placement activates
    ↓
Player finds Head ID 2 → Approaches Placement 2 (Left) → Presses F
    ↓
Head placed on left placement → Placement activates → Particles & light start
    ↓
Player finds Head ID 3 → Approaches Placement 3 (Right) → Presses F
    ↓
Head placed on right placement → Placement activates → Particles & light start
    ↓
All placements activated → SHRINE COMPLETE! → Door unlocks, rewards spawn
```

## 🎁 **Rewards System**

### **Automatic Rewards:**
- **Door unlocks** (if assigned)
- **Reward items spawn** (if assigned)
- **Shrine activation effects** (if assigned)
- **Completion animation** (if assigned)

### **Event Integration:**
- Notifies `Floor2EndingEventManager` when complete
- Triggers other game events
- Can be integrated with quest system

## 🔧 **Advanced Features**

### **Automatic Setup:**
- Creates altars automatically if none exist
- Sets up materials and effects automatically
- Finds player and inventory automatically

### **Visual Feedback:**
- Different materials for each state
- Particle effects for fire
- Light effects for glow
- Audio feedback for all actions

### **Error Handling:**
- Wrong heads are removed (no infinite retry)
- Graceful handling of missing components
- Debug logging throughout

## 🎨 **Art Style Suggestions**

### **Atmospheric Elements:**
- **Stone altars** with mystical runes
- **Glowing braziers** with fire effects
- **Mystical lighting** and fog
- **Ancient architecture** feel

### **Color Scheme:**
- **Dark stone** for altars
- **Orange/red glow** for lit braziers
- **Blue/purple** for mystical effects
- **Gold** for completion effects

## 🚀 **Performance Tips**

1. **Use object pooling** for particle effects
2. **Limit particle count** for performance
3. **Use LOD system** for distant altars
4. **Optimize materials** with shared textures
5. **Use audio pooling** for sounds

## 🐛 **Troubleshooting**

### **Common Issues:**
1. **Altars not created**: Check if child objects exist or let script create them
2. **Materials not applied**: Ensure materials are assigned in inspector
3. **Audio not playing**: Check AudioSource component and audio clips
4. **Particles not working**: Ensure ParticleSystem component is present
5. **Lights not working**: Check Light component and intensity settings

### **Debug Features:**
- **Gizmos** show interaction distance and altar positions
- **Console logging** for all major events
- **Public methods** for checking completion status

## 🎯 **Integration with Existing Systems**

### **Inventory System:**
- Works with `DullahanHeadInventory`
- Uses `DullahanHeadSO` for head data
- Integrates with `DullahanHeadPickable`

### **Event System:**
- Notifies `Floor2EndingEventManager`
- Can trigger other game events
- Integrates with quest system

### **Audio System:**
- Uses `AudioSource` for sound effects
- Supports background chanting
- Can integrate with audio manager

## 🎮 **Player Experience**

### **Atmosphere:**
- **Mystical and atmospheric** with chanting
- **Visual progression** as altars light up
- **Satisfying completion** with rewards
- **Clear feedback** for all actions

### **Difficulty:**
- **Easy to understand** - place heads on altars
- **Clear objectives** - light all braziers
- **Forgiving mechanics** - wrong heads just don't work
- **Visual guidance** - glowing braziers show progress

This Head Shrine Puzzle provides a beautiful, atmospheric alternative to placing heads on a moving Dullahan body, perfect for indie developers who want engaging puzzle gameplay without complex implementation!
