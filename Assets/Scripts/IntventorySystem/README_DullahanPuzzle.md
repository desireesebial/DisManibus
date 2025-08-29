# 🎮 **Dullahan Chase & Puzzle System - Complete Comprehensive Guide**

## 📋 **Table of Contents**
1. [System Overview](#system-overview)
2. [Core Systems](#core-systems)
3. [ScriptableObject System](#scriptableobject-system)
4. [Setup Instructions](#setup-instructions)
5. [Integration Guide](#integration-guide)
6. [Audio System Integration](#audio-system-integration)
7. [UI Setup](#ui-setup)
8. [Testing & Debug](#testing--debug)
9. [Troubleshooting](#troubleshooting)
10. [Advanced Features](#advanced-features)
11. [Performance Optimization](#performance-optimization)

---

## 🎯 **System Overview**

### **What This System Provides:**
- **Proximity-based chase triggering** with 60-second timed sequences
- **Player choice mechanics** (Help Dullahan vs Leave)
- **Three-head puzzle system** with real/fake heads and effects
- **ScriptableObject-based lantern system** with full customization
- **Integrated inventory system** for heads, lantern, and flashlight
- **Health system** with visual effects and debuffs
- **Quest system** with popup notifications and progress tracking
- **Melee attack system** for the Dullahan
- **Complete audio integration** with your existing AudioManager
- **Multiple endings** (Good vs Bad)

### **Game Flow:**
1. **Exploration Phase** - Player explores, finds lantern, discovers quests
2. **Proximity Trigger** - Enter Dullahan's territory
3. **Chase Phase** - 60-second intense pursuit
4. **Choice Phase** - Decide to help Dullahan or leave
5. **Head Collection** - Find real head within 90 seconds (if helping)
6. **Puzzle Completion** - Attach real head to Dullahan body
7. **Ending** - Good ending (help) vs Bad ending (leave)

---

## 🏗️ **Core Systems**

### **1. ScriptableObject System**

#### **LanternSO (`LanternSO.cs`)**
**Purpose:** ScriptableObject for defining lantern properties and behavior

**Features:**
- Complete lantern configuration in one asset
- Light settings (color, intensity, range, flicker effects)
- Audio clips (pickup, toggle on/off sounds)
- Visual effects (materials, particles, glow effects)
- Control settings (toggle key, messages)

**Setup:**
```csharp
// Create via: Right Click → Create → Scriptable Objects → Lantern
[Header("Lantern Properties")]
public string lanternName = "Mysterious Lantern";
public int lanternID;
public Sprite lanternSprite;
public Sprite lanternIcon;
public string description = "An old lantern that provides light in the darkness.";

[Header("Light Settings")]
public Color lightColor = Color.yellow;
public float lightIntensity = 1f;
public float lightRange = 5f;
public bool flickerEffect = false;
public float flickerSpeed = 2f;
public float flickerAmount = 0.1f;

[Header("Audio")]
public AudioClip pickupSound;
public AudioClip toggleOnSound;
public AudioClip toggleOffSound;

[Header("Visual Effects")]
public GameObject lanternPrefab;
public Material lanternMaterial;
public Color lanternGlowColor = Color.yellow;
public bool hasGlowEffect = false;
public ParticleSystem lanternParticles;

[Header("Controls")]
public KeyCode toggleKey = KeyCode.L;
public string toggleMessage = "Press L to toggle lantern";
```

#### **DullahanHeadSO (`DullahanHeadSO.cs`)**
**Purpose:** ScriptableObject for defining Dullahan head properties

**Features:**
- Head type classification (Real, Fake1, Fake2)
- Visual properties (sprites, icons)
- Effects and buffs/debuffs
- Audio clips for interactions

**Setup:**
```csharp
// Create via: Right Click → Create → Scriptable Objects → Dullahan Head
[Header("Head Properties")]
public string headName;
public int headID;
public HeadType headType;
public Sprite headSprite;
public Sprite headIcon; // Icon for inventory display
public string description;

[Header("Effects")]
public float speedModifier = 1f;
public float fovModifier = 1f;
public bool hasSpecialEffect = false;
public string specialEffectDescription;

[Header("Audio")]
public AudioClip pickupSound;
public AudioClip attachSound;
```

---

### **2. Player Health System (`PlayerHealthSystem.cs`)**

**Purpose:** Manages player health with visual feedback and debuffs

**Features:**
- 3-bar health system (Full = Healthy, 2 bars = Minor injury, 1 bar = Critical)
- Camera shake on damage
- Damage flash effect
- Blur effect for critical health (every 5 seconds)
- Debuffs based on health state (speed, sensitivity)
- Invulnerability frames after damage

**Setup:**
```csharp
// Attach to Player GameObject
[Header("Health Settings")]
public int maxHealth = 3;
public float invulnerabilityTime = 1f;

[Header("Health UI")]
public GameObject healthUI;
public Image[] healthBars = new Image[3];

[Header("Camera Effects")]
public Camera playerCamera;
public float shakeIntensity = 0.5f;
public float shakeDuration = 0.3f;

[Header("Player Controller")]
public FirstPersonController playerController;
```

**Health States:**
- **Healthy (3 bars):** Full functionality
- **Minor Injury (2 bars):** Reduced speed (4f), sensitivity (1.5f)
- **Critical Injury (1 bar):** Major debuffs (3f speed, 1f sensitivity) + blur effect

**Debug Keys:**
- **H** - Take damage
- **J** - Heal
- **K** - Restore full health

---

### **3. Quest System (`QuestSystem.cs`)**

**Purpose:** Manages quests with popup notifications and progress tracking

**Features:**
- Quest popup notifications with fade animations
- Quest log with progress tracking
- Multiple quest types (Collection, Elimination, Exploration, etc.)
- Audio feedback for quest events
- Priority system (Low, Normal, High, Critical)

**Setup:**
```csharp
// Attach to GameManager GameObject
[Header("Quest UI")]
public GameObject questPopup;
public GameObject questLogUI;
public Transform questLogContent;
public GameObject questLogEntryPrefab;

[Header("Animation")]
public float popupDuration = 3f;
public float popupFadeInTime = 0.5f;
public float popupFadeOutTime = 0.5f;
```

**Quest Types:**
- **Collection** - Collect items
- **Elimination** - Defeat enemies
- **Exploration** - Visit locations
- **Interaction** - Interact with objects
- **Survival** - Survive for time
- **Escort** - Escort NPCs
- **Custom** - Custom objectives

**Controls:**
- **Q** - Toggle quest log
- **T** - Create test quest (debug)

---

### **4. Dullahan Head Inventory with ScriptableObject Lantern (`DullahanHeadInventory.cs`)**

**Purpose:** Integrated inventory system for heads, lantern, and flashlight using ScriptableObjects

**Features:**
- Head management (up to 3 heads) using DullahanHeadSO
- ScriptableObject-based lantern system using LanternSO
- Integrated flashlight functionality
- Visual representation of held items
- Battery management for flashlight
- Inventory UI with item icons
- Simple on/off lantern toggle (no battery management)

**Setup:**
```csharp
// Attach to Player GameObject
[Header("Inventory Settings")]
public List<DullahanHeadSO> inventoryList = new List<DullahanHeadSO>();
public int maxInventorySize = 3;
public int playerReach = 3;
public int selectedItem = 0;

[Header("Camera and UI")]
[SerializeField] Camera cam;
[SerializeField] GameObject pressToPickup_gameobject;

[Header("Inventory UI")]
[SerializeField] Image[] inventorySlotImage = new Image[3];
[SerializeField] Image[] inventoryBackgroundImage = new Image[3];
[SerializeField] Sprite emptySlotImage;

[Header("Player Item GameObjects")]
[SerializeField] GameObject realHead_item;
[SerializeField] GameObject fakeHead1_item;
[SerializeField] GameObject fakeHead2_item;
[SerializeField] GameObject lantern_item;

[Header("Lantern System")]
public bool hasLantern = false;
public bool isLanternOn = false;
public LanternSO currentLantern;
public Light lanternLight;

[Header("Flashlight System")]
public bool hasFlashlight = false;
public bool isFlashlightOn = false;
public Light flashlightLight;
public float flashlightBattery = 100f;
public float maxFlashlightBattery = 100f;
public float flashlightDrainRate = 5f;
public float flashlightRechargeRate = 2f;
```

**Controls:**
- **E** - Pick up items
- **1, 2, 3** - Select heads
- **L** - Toggle lantern (uses LanternSO.toggleKey)
- **F** - Toggle flashlight

**Lantern Features:**
- **Simple On/Off**: No battery management, pure toggle
- **ScriptableObject Integration**: All settings from LanternSO
- **Dynamic Controls**: Toggle key configurable in LanternSO
- **Audio Integration**: Sounds from LanternSO
- **Visual Effects**: Light settings from LanternSO

---

### **5. Lantern Pickable with ScriptableObject (`LanternPickable.cs`)**

**Purpose:** World-placed lantern that can be picked up using ScriptableObject system

**Features:**
- Pickup system with proximity detection
- ScriptableObject-based configuration (LanternSO)
- Visual effects and ambient lighting
- Integration with inventory system
- Pickup prompts and audio feedback
- Integration with your existing AudioManager

**Setup:**
```csharp
// Attach to Lantern GameObject in world
[Header("Lantern Settings")]
public bool isPickedUp = false;
public LanternSO lanternData; // Assign your LanternSO here

[Header("Visual Components")]
public GameObject lanternModel;
public Light lanternLight;
public ParticleSystem lanternParticles;
public Material lanternMaterial;
public Color onColor = Color.yellow;
public Color offColor = Color.gray;

[Header("Pickup Settings")]
public float pickupRange = 3f;
public KeyCode pickupKey = KeyCode.E;
public LayerMask playerLayer = 1;

[Header("UI")]
public GameObject pickupPrompt;
public TextMeshProUGUI promptText;

[Header("Audio")]
public AudioSource audioSource;

[Header("Integration")]
public DullahanHeadInventory headInventory;
public AudioManager audioManager; // Your existing audio system
```

**ScriptableObject Integration:**
- **Name & Description**: From `lanternData.lanternName` and `lanternData.description`
- **Audio**: Uses `lanternData.pickupSound`, `toggleOnSound`, `toggleOffSound`
- **UI Text**: Uses `lanternData.toggleMessage`
- **Inventory**: Passes `lanternData` to inventory system

---

### **6. Dullahan Melee Attack (`DullahanMeleeAttack.cs`)**

**Purpose:** Melee attack system for the Dullahan

**Features:**
- Multiple attack patterns with different damage/range
- Attack cooldowns and animations
- Visual and audio effects
- Integration with chase system
- Damage integration with health system

**Setup:**
```csharp
// Attach to Dullahan GameObject
[Header("Attack Settings")]
public float attackRange = 2f;
public float attackDamage = 1;
public float attackCooldown = 2f;

[Header("Attack Detection")]
public Transform attackPoint;
public LayerMask playerLayer = 1;

[Header("Visual Effects")]
public Animator dullahanAnimator;
public string attackTriggerName = "Attack";
public ParticleSystem attackParticles;
public Light attackLight;
```

**Attack Patterns:**
- **Basic Attack:** Standard melee attack
- **Heavy Attack:** High damage, longer cooldown
- **Quick Attack:** Fast attack, low damage
- **Stun Attack:** Can stun player

---

### **7. Dullahan Chase System (`DullahanChaseSystem.cs`)**

**Purpose:** Manages Dullahan's movement and chase behavior

**Features:**
- Intensity-based chase mechanics
- Dynamic speed and effects
- Patrol system when not chasing
- Integration with audio and visual effects
- Transition between chase and patrol

**Setup:**
```csharp
// Attach to Dullahan GameObject
[Header("Chase Settings")]
public float minChaseSpeed = 3f;
public float maxChaseSpeed = 6f;
public float chaseIntensity = 0f;

[Header("Patrol Settings")]
public float patrolSpeed = 2f;
public float patrolRadius = 10f;
public float patrolWaitTime = 2f;
public Transform[] patrolWaypoints;
```

**Chase Intensity:**
- **0.0-0.3:** Low intensity (walking speed)
- **0.3-0.7:** Medium intensity (jogging speed)
- **0.7-1.0:** High intensity (running speed)

---

### **8. Dullahan Chase Event Manager (`DullahanChaseEventManager.cs`)**

**Purpose:** Orchestrates the complete game flow

**Features:**
- Proximity-based triggering
- Timed chase sequences
- Player choice system
- Door management
- Scene transitions
- UI management

**Setup:**
```csharp
// Attach to GameManager GameObject
[Header("Proximity Settings")]
public float proximityRadius = 15f;
public Transform proximityCenter;

[Header("Timing")]
public float chaseDuration = 60f;
public float headCollectionTime = 90f;
public float choiceTimeLimit = 30f;

[Header("Doors")]
public Door exitDoor; // Bait door
public Door realHeadDoor; // Door to real head

[Header("Scenes")]
public string nextSceneName = "NextLevel";
public string badEndingSceneName = "BadEnding";
```

**Game States:**
- **Waiting** - Player exploring
- **Chase** - Dullahan chasing player
- **Choice** - Player choosing to help or leave
- **HeadCollection** - Collecting heads
- **Completion** - Puzzle completed
- **BadEnding** - Bad ending triggered

---

## 🔧 **ScriptableObject System**

### **Creating ScriptableObjects**

#### **1. Create LanternSO:**
```
1. Right-click in Project window
2. Create → Scriptable Objects → Lantern
3. Name it (e.g., "MysteriousLantern")
4. Configure all properties in Inspector
```

#### **2. Create DullahanHeadSO:**
```
1. Right-click in Project window
2. Create → Scriptable Objects → Dullahan Head
3. Name it (e.g., "RealHead", "FakeHead1", "FakeHead2")
4. Configure all properties in Inspector
```

### **ScriptableObject Benefits:**

**✅ Centralized Configuration:**
- All settings in one asset
- Easy to modify without code changes
- Version control friendly

**✅ Reusability:**
- Create multiple lantern types
- Create multiple head variants
- Share across different scenes

**✅ Consistency:**
- Same pattern as Dullahan heads
- Unified data structure
- Easy to extend

**✅ Performance:**
- No runtime object creation
- Efficient memory usage
- Fast property access

---

## 🔧 **Setup Instructions**

### **Step 1: Create ScriptableObjects**

1. **Create LanternSO:**
   ```
   Project → Right Click → Create → Scriptable Objects → Lantern
   ```
   - Set `lanternName` (e.g., "Mysterious Lantern")
   - Configure `lightColor`, `lightIntensity`, `lightRange`
   - Assign audio clips for pickup and toggle sounds
   - Set `toggleKey` (default: L)
   - Set `toggleMessage` (e.g., "Press L to toggle lantern")

2. **Create DullahanHeadSO:**
   ```
   Project → Right Click → Create → Scriptable Objects → Dullahan Head
   ```
   - Set `headName` (e.g., "Real Head", "Fake Head 1", "Fake Head 2")
   - Set `headType` (Real, Fake1, Fake2)
   - Assign `headSprite` and `headIcon`
   - Configure effects and modifiers

### **Step 2: Player Setup**

1. **Create Player GameObject:**
   ```
   Player (Empty GameObject)
   ├── FirstPersonController
   ├── PlayerHealthSystem
   ├── DullahanHeadInventory
   ├── AudioSource
   └── Camera (Child)
   ```

2. **Configure DullahanHeadInventory:**
   - Assign `cam` reference
   - Set up `pressToPickup_gameobject`
   - Configure `inventorySlotImage` array (3 slots)
   - Configure `inventoryBackgroundImage` array (3 slots)
   - Assign `emptySlotImage` sprite
   - Set up head GameObjects (`realHead_item`, `fakeHead1_item`, `fakeHead2_item`)
   - Assign `lantern_item` GameObject
   - Assign `lanternLight` component
   - Configure flashlight settings
   - Assign audio clips

### **Step 3: Dullahan Setup**

1. **Create Dullahan GameObject:**
   ```
   Dullahan (Empty GameObject)
   ├── DullahanChaseSystem
   ├── DullahanMeleeAttack
   ├── NavMeshAgent
   ├── Animator
   ├── AudioSource
   └── Model (Child)
   ```

2. **Configure DullahanChaseSystem:**
   - Set up patrol waypoints
   - Configure chase speeds
   - Set up audio integration

3. **Configure DullahanMeleeAttack:**
   - Set up attack patterns
   - Configure attack point
   - Set up animations
   - Configure visual effects

### **Step 4: Game Manager Setup**

1. **Create GameManager GameObject:**
   ```
   GameManager (Empty GameObject)
   ├── DullahanChaseEventManager
   ├── QuestSystem
   ├── DullahanPuzzleManager
   └── AudioManager (your existing)
   ```

2. **Configure DullahanChaseEventManager:**
   - Set up proximity radius
   - Configure timing settings
   - Assign door references
   - Set up UI elements

3. **Configure QuestSystem:**
   - Set up quest UI elements
   - Configure popup animations
   - Set up audio clips

### **Step 5: World Objects Setup**

1. **Create Lantern Pickable:**
   ```
   LanternPickable (Empty GameObject)
   ├── LanternPickable script
   ├── Lantern Model (Child)
   ├── Light component
   ├── Particle System
   ├── AudioSource
   ├── Collider (for pickup detection)
   └── Assign LanternSO to lanternData field
   ```

2. **Create Dullahan Body:**
   ```
   DullahanBody (Empty GameObject)
   ├── DullahanBody script
   ├── Collider (for interaction)
   └── Visual Model (Child)
   ```

3. **Create Head Pickables:**
   ```
   HeadPickable (Empty GameObject)
   ├── DullahanHeadPickable script
   ├── Head Model (Child)
   ├── Collider (for pickup detection)
   ├── AudioSource
   └── Assign DullahanHeadSO to headData field
   ```

### **Step 6: UI Setup**

1. **Create Canvas:**
   ```
   UI Canvas
   ├── Health UI
   │   ├── Health Bars (3 Images)
   │   └── Health Text
   ├── Inventory UI
   │   ├── Head Icons (3 Images)
   │   ├── Lantern Icon
   │   ├── Flashlight Icon
   │   └── Battery UI
   ├── Quest UI
   │   ├── Quest Popup
   │   └── Quest Log
   ├── Chase UI
   │   ├── Timer Text
   │   └── Choice Buttons
   └── Lantern UI
       ├── Battery Fill
       └── Battery Text
   ```

2. **Configure UI Elements:**
   - Set up proper anchors and positioning
   - Configure colors and fonts
   - Set up animations and transitions

---

## 🎵 **Audio System Integration**

### **Integration with Your Existing AudioManager**

The system is designed to work seamlessly with your existing `AudioManager` from `Assets/Scripts/Audio/`:

**Key Integration Points:**
```csharp
// In LanternPickable.cs
if (audioManager != null && lanternData != null && lanternData.pickupSound != null)
{
    audioManager.PlayRandomized(audioSource, lanternData.pickupSound, 1f);
}

// In DullahanHeadInventory.cs
if (audioSource != null)
{
    AudioClip soundToPlay = isLanternOn ? currentLantern.toggleOnSound : currentLantern.toggleOffSound;
    if (soundToPlay != null)
    {
        audioSource.PlayOneShot(soundToPlay);
    }
}
```

**Audio Clips to Add:**
- **Lantern pickup sound** (in LanternSO)
- **Lantern toggle on/off sounds** (in LanternSO)
- **Flashlight toggle sounds**
- **Battery warning sounds**
- **Head pickup/drop sounds**
- **Quest notification sounds**
- **Chase music and effects**

**Benefits of Integration:**
- **Distance-based audio** - Works with your proximity system
- **Fade system** - Integrates with your fade in/out functionality
- **Random ambience** - Can work with your random ambience system
- **Volume control** - Uses your existing volume settings

---

## 🎨 **UI Setup**

### **Health UI Configuration**

1. **Create Health Panel:**
   ```
   Health Panel (Panel)
   ├── Background Image
   ├── Health Bar 1 (Image - Fill)
   ├── Health Bar 2 (Image - Fill)
   ├── Health Bar 3 (Image - Fill)
   └── Health Text (TextMeshPro)
   ```

2. **Configure Health Bars:**
   - Set Image Type to "Filled"
   - Set Fill Method to "Horizontal"
   - Configure colors (Green/Yellow/Red/Gray)
   - Set up proper sizing and spacing

### **Inventory UI Configuration**

1. **Create Inventory Panel:**
   ```
   Inventory Panel (Panel)
   ├── Background Image
   ├── Head Icons Container
   │   ├── Head Icon 1 (Image)
   │   ├── Head Icon 2 (Image)
   │   └── Head Icon 3 (Image)
   ├── Lantern Icon (Image)
   ├── Flashlight Icon (Image)
   ├── Battery Panel
   │   ├── Battery Fill (Image)
   │   └── Battery Text (TextMeshPro)
   └── Inventory Text (TextMeshPro)
   ```

2. **Configure Icons:**
   - Set up proper sprites for each item type
   - Configure selection colors (Yellow for selected)
   - Set up hover effects

### **Quest UI Configuration**

1. **Create Quest Popup:**
   ```
   Quest Popup (Panel)
   ├── Background Image
   ├── Quest Title (TextMeshPro)
   ├── Quest Description (TextMeshPro)
   ├── Quest Progress (TextMeshPro)
   ├── Quest Icon (Image)
   └── CanvasGroup (for fade effects)
   ```

2. **Create Quest Log:**
   ```
   Quest Log (Panel)
   ├── Background Image
   ├── Title (TextMeshPro)
   ├── Scroll View
   │   ├── Viewport
   │   │   └── Content (for quest entries)
   │   └── Scrollbar
   └── Close Button
   ```

### **Chase UI Configuration**

1. **Create Chase Panel:**
   ```
   Chase Panel (Panel)
   ├── Background Image
   ├── Timer Text (TextMeshPro)
   ├── Choice Panel
   │   ├── Help Button
   │   └── Leave Button
   └── Progress Bar (Image)
   ```

---

## 🧪 **Testing & Debug**

### **Debug Keys Summary**

**Health System:**
- **H** - Take damage
- **J** - Heal
- **K** - Restore full health

**Lantern System:**
- **L** - Toggle lantern (uses LanternSO.toggleKey)
- **E** - Pick up lantern
- **N** - Give lantern (debug)

**Inventory System:**
- **E** - Pick up items
- **1, 2, 3** - Select heads
- **F** - Toggle flashlight
- **G** - Drop selected item

**Quest System:**
- **Q** - Toggle quest log
- **T** - Create test quest

**Event Manager:**
- **P** - Trigger proximity (debug)
- **C** - Skip chase (debug)
- **X** - Force choice (debug)

### **Testing Checklist**

**ScriptableObject System:**
- [ ] LanternSO can be created and configured
- [ ] DullahanHeadSO can be created and configured
- [ ] ScriptableObjects are properly assigned to pickable objects
- [ ] Inventory system reads ScriptableObject data correctly
- [ ] Audio clips from ScriptableObjects play correctly

**Health System:**
- [ ] Player takes damage correctly
- [ ] Camera shake works
- [ ] Health bars update properly
- [ ] Debuffs apply correctly
- [ ] Blur effect works for critical health

**Lantern System:**
- [ ] Lantern can be picked up
- [ ] Toggle on/off works (uses LanternSO.toggleKey)
- [ ] Light settings from LanternSO apply correctly
- [ ] Audio from LanternSO plays correctly
- [ ] Visual effects work

**Inventory System:**
- [ ] Heads can be picked up
- [ ] Head selection works
- [ ] Lantern integration works
- [ ] Flashlight integration works
- [ ] UI updates correctly

**Quest System:**
- [ ] Quests can be started
- [ ] Popup notifications work
- [ ] Quest log displays correctly
- [ ] Progress tracking works
- [ ] Audio feedback works

**Chase System:**
- [ ] Proximity detection works
- [ ] Chase starts correctly
- [ ] Timer works
- [ ] Choice system works
- [ ] Doors open/close correctly

### **Performance Testing**

**Frame Rate:**
- Monitor FPS during chase sequences
- Check performance with multiple effects
- Test with different graphics settings

**Memory Usage:**
- Monitor memory during long play sessions
- Check for memory leaks in audio system
- Test with multiple lanterns/heads

**Audio Performance:**
- Test with multiple audio sources
- Check for audio lag or stuttering
- Verify fade transitions work smoothly

---

## 🔧 **Troubleshooting**

### **Common Issues**

**1. Lantern Not Picking Up:**
- Check if `LanternSO` is assigned to `lanternData` field
- Verify `DullahanHeadInventory` is assigned
- Check pickup range is set correctly
- Ensure player has "Player" tag
- Check if `isPickedUp` is false

**2. Lantern Toggle Not Working:**
- Verify `LanternSO.toggleKey` is set correctly
- Check if `currentLantern` is assigned in inventory
- Ensure `lanternLight` component is assigned
- Verify audio clips are assigned in LanternSO

**3. Health System Not Working:**
- Verify `FirstPersonController` reference
- Check if health UI elements are assigned
- Ensure camera reference is set
- Verify audio clips are assigned

**4. Quest System Not Showing:**
- Check if UI elements are assigned
- Verify Canvas is set to "Screen Space - Overlay"
- Ensure QuestSystem is in the scene
- Check if audio clips are assigned

**5. Chase Not Triggering:**
- Verify proximity radius is set
- Check if `DullahanChaseEventManager` is in scene
- Ensure player is within proximity area
- Check if chase system is enabled

**6. Audio Not Playing:**
- Verify `AudioManager` is in scene
- Check if audio clips are assigned in ScriptableObjects
- Ensure AudioSource components exist
- Verify volume settings

### **ScriptableObject Issues**

**1. LanternSO Not Found:**
- Check if LanternSO asset exists in project
- Verify the asset is assigned to `lanternData` field
- Ensure the asset is not corrupted

**2. Properties Not Applying:**
- Check if ScriptableObject properties are set
- Verify the script is reading the correct properties
- Ensure the asset is saved

**3. Audio Not Playing from ScriptableObject:**
- Check if audio clips are assigned in LanternSO
- Verify audio clips are valid
- Ensure AudioSource component exists

### **Debug Solutions**

**Enable Debug Logging:**
```csharp
// Add to any script for debugging
Debug.Log("Debug message here");
```

**Check Component References:**
```csharp
// Add to Start() method
if (component == null)
{
    Debug.LogError("Component not assigned!");
}
```

**Test Individual Systems:**
- Test each system in isolation
- Use debug keys to trigger events
- Check console for error messages

---

## 🚀 **Advanced Features**

### **Custom Lantern Creation**

**Creating Custom Lanterns:**
```csharp
// Create custom LanternSO
LanternSO customLantern = ScriptableObject.CreateInstance<LanternSO>();
customLantern.lanternName = "Custom Lantern";
customLantern.lightColor = Color.blue;
customLantern.lightIntensity = 2f;
customLantern.lightRange = 8f;
customLantern.toggleKey = KeyCode.X;
customLantern.toggleMessage = "Press X to toggle custom lantern";
```

### **Custom Quest Creation**

**Creating Custom Quests:**
```csharp
// Create quest ScriptableObject
Quest customQuest = ScriptableObject.CreateInstance<Quest>();
customQuest.questID = "custom_quest";
customQuest.questTitle = "Custom Quest";
customQuest.questDescription = "Complete this custom quest";
customQuest.requiredProgress = 5;
customQuest.questType = QuestType.Custom;

// Start quest
questSystem.StartQuest(customQuest);
```

### **Custom Attack Patterns**

**Creating Custom Attack Patterns:**
```csharp
// In DullahanMeleeAttack
AttackPattern customPattern = new AttackPattern
{
    patternName = "Custom Attack",
    damage = 2f,
    range = 3f,
    cooldown = 1.5f,
    animationTrigger = "CustomAttack",
    canStun = true,
    stunDuration = 2f
};
```

### **Custom Head Effects**

**Creating Custom Head Effects:**
```csharp
// In DullahanHeadSO
[Header("Custom Effects")]
public float customSpeedModifier = 1.5f;
public float customFOVModifier = 1.2f;
public bool customEffect = true;
```

### **Integration with Other Systems**

**Health System Integration:**
```csharp
// Subscribe to health events
healthSystem.OnHealthChanged += (health) => {
    Debug.Log($"Health changed to: {health}");
};

healthSystem.OnCriticalHealth += () => {
    Debug.Log("Player is critically injured!");
};
```

**Quest System Integration:**
```csharp
// Subscribe to quest events
questSystem.OnQuestStarted += (quest) => {
    Debug.Log($"Quest started: {quest.questTitle}");
};

questSystem.OnQuestCompleted += (quest) => {
    Debug.Log($"Quest completed: {quest.questTitle}");
};
```

---

## ⚡ **Performance Optimization**

### **Optimization Tips**

**1. Audio Optimization:**
- Use object pooling for audio sources
- Limit concurrent audio clips
- Use spatial audio for 3D sounds
- Implement audio culling

**2. Visual Effects:**
- Limit particle system counts
- Use LOD for complex effects
- Implement effect culling
- Optimize light sources

**3. UI Optimization:**
- Use object pooling for UI elements
- Limit UI updates per frame
- Use canvas groups for batch rendering
- Implement UI culling

**4. Script Optimization:**
- Cache component references
- Use coroutines for timing
- Implement object pooling
- Optimize Update() methods

### **ScriptableObject Optimization**

**Benefits:**
- **Memory Efficient**: No runtime object creation
- **Fast Access**: Direct property access
- **Shared Data**: Multiple objects can reference same data
- **Version Control**: Easy to track changes

**Best Practices:**
- Create ScriptableObjects in editor, not runtime
- Use ScriptableObjects for shared data
- Cache ScriptableObject references
- Avoid modifying ScriptableObjects at runtime

### **Memory Management**

**Object Pooling Example:**
```csharp
public class ObjectPool : MonoBehaviour
{
    public GameObject prefab;
    public int poolSize = 10;
    private Queue<GameObject> pool;

    void Start()
    {
        pool = new Queue<GameObject>();
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(prefab);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public GameObject GetObject()
    {
        if (pool.Count > 0)
        {
            GameObject obj = pool.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        return Instantiate(prefab);
    }

    public void ReturnObject(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}
```

---

## 🎯 **Final Checklist**

### **Before Release**

**ScriptableObject System:**
- [ ] All ScriptableObjects are created and configured
- [ ] LanternSO assets are properly assigned to LanternPickable objects
- [ ] DullahanHeadSO assets are properly assigned to HeadPickable objects
- [ ] ScriptableObject properties are set correctly
- [ ] Audio clips are assigned in ScriptableObjects

**Core Systems:**
- [ ] All scripts compile without errors
- [ ] All components are properly assigned
- [ ] Audio system integration works
- [ ] UI elements display correctly
- [ ] Debug keys function properly

**Gameplay:**
- [ ] Chase system triggers correctly
- [ ] Health system works as expected
- [ ] Lantern system functions properly (ScriptableObject-based)
- [ ] Quest system operates correctly
- [ ] Puzzle completion works

**Performance:**
- [ ] Frame rate is acceptable
- [ ] Memory usage is stable
- [ ] Audio performance is good
- [ ] No memory leaks detected

**User Experience:**
- [ ] Controls are intuitive
- [ ] Visual feedback is clear
- [ ] Audio feedback is appropriate
- [ ] UI is user-friendly

---

## 🎮 **Conclusion**

This comprehensive system provides a complete, immersive Dullahan experience with:

✅ **ScriptableObject-based lantern system** with full customization  
✅ **Proximity-based chase triggering** with 60-second timed sequences  
✅ **Player choice mechanics** (Help vs Leave)  
✅ **90-second head collection phase**  
✅ **Multiple endings** (Good vs Bad)  
✅ **3-bar health system** with visual effects and debuffs  
✅ **Camera shake and blur effects**  
✅ **Quest system** with popup notifications and progress tracking  
✅ **Integrated inventory system** for heads, lantern, and flashlight  
✅ **Dullahan melee attacks** with patterns  
✅ **Integration with your existing audio system**  
✅ **Debug tools** for testing  
✅ **Reusable across different scenes**  
✅ **Performance optimized** with ScriptableObjects  

The system now features a modern ScriptableObject-based architecture that makes it easy to create multiple lantern types, customize all properties without code changes, and maintain consistency across the entire system! 🎮👻

---

## 📞 **Support**

If you encounter any issues or need assistance:

1. **Check the troubleshooting section** above
2. **Review the debug logs** in Unity Console
3. **Test individual systems** using debug keys
4. **Verify all component references** are assigned
5. **Check ScriptableObject assignments** are correct
6. **Verify audio system integration** with your existing AudioManager

**Happy developing!** 🚀
