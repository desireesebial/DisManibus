# Complete Dullahan Puzzle System Setup Guide

## 🎯 **Puzzle Overview**
This guide sets up a complete Dullahan chase and puzzle system with the following flow:

1. **Proximity Trigger** → Player enters Dullahan's territory
2. **60-Second Chase** → Dullahan chases player for exactly 60 seconds
3. **Exit Doors Open** → Doors open as "bait" after chase ends
4. **Player Choice** → Help Dullahan or leave through exit doors
5. **Head Collection** → If helping, collect heads and place them on Dullahan
6. **Fake Head Effects** → Each fake head applies buffs/debuffs to player
7. **Real Head Victory** → Placing real head allows safe exit to next scene

## 🏗️ **Required Components**

### **Core Scripts:**
- `DullahanChaseEventManager.cs` - Main puzzle controller
- `DullahanChaseSystem.cs` - Dullahan AI behavior
- `DullahanPuzzleManager.cs` - Head spawning and management
- `DullahanBody.cs` - Head attachment logic
- `DullahanHeadInventory.cs` - Player inventory system
- `doorscript.cs` - Door system for exits and real head room
- `PlayerHealthSystem.cs` - Player health and debuff system

### **Scriptable Objects:**
- `DullahanHeadSO` - Head data with effects
- `LanternSO` - Lantern configuration

## 📋 **Step-by-Step Setup**

### **Step 1: Scene Setup**

#### **1.1 Create Dullahan GameObject**
```
GameObject: Dullahan
Components:
- DullahanChaseSystem
- DullahanMeleeAttack
- NavMeshAgent
- Animator
- AudioSource
```

#### **1.2 Create Player GameObject**
```
GameObject: Player
Components:
- FirstPersonController
- DullahanHeadInventory
- PlayerHealthSystem
- AudioSource
```

#### **1.3 Create Door GameObjects**
```
GameObject: exit door
- doorscript component
- Required Key ID: 999
- Is Locked: true

GameObject: exit door2  
- doorscript component
- Required Key ID: 999
- Is Locked: true

GameObject: RealHeadDoor
- doorscript component
- Required Key ID: 100
- Is Locked: true
```

#### **1.4 Create Head Pickables**
```
GameObject: dullahanHead_Real
- DullahanHeadPickable component
- Assign Real Head ScriptableObject

GameObject: dullahanHead_Fake1
- DullahanHeadPickable component
- Assign Fake Head 1 ScriptableObject

GameObject: dullahanHead_Fake2
- DullahanHeadPickable component
- Assign Fake Head 2 ScriptableObject
```

#### **1.5 Create Dullahan Body**
```
GameObject: DullahanBody
- DullahanBody component
- Required Head ID: [Real Head ID]
- Collider for head attachment
```

### **Step 2: Configure DullahanChaseEventManager**

#### **2.1 Basic Settings**
```
Proximity Settings:
- Proximity Radius: 10
- Proximity Center: [Dullahan Transform]

Chase Settings:
- Chase Duration: 60
- Head Spawn Delay: 2
- Head Collection Time: 90

Player Choice:
- Choice Time Limit: 30
```

#### **2.2 Door Assignments**
```
Doors:
- Exit Doors: [exit door, exit door2]
- Real Head Door: [RealHeadDoor]
- Exit Doors Linked: true
- Exit Door Key ID: 999
- Real Head Door Key ID: 100
```

#### **2.3 Integration References**
```
Integration:
- Dullahan Chase System: [Dullahan's DullahanChaseSystem]
- Audio Manager: [Your Audio Manager]
- Head Inventory: [Player's DullahanHeadInventory]
- Dullahan Body: [DullahanBody GameObject]
- Puzzle Manager: [DullahanPuzzleManager]
```

#### **2.4 Scene Management**
```
Scene Management:
- Next Scene Name: "NextLevel"
- Bad Ending Scene Name: "BadEnding"
```

### **Step 3: Configure DullahanPuzzleManager**

#### **3.1 Head Pickables**
```
Puzzle Components:
- Head Pickables: [dullahanHead_Real, dullahanHead_Fake1, dullahanHead_Fake2]
- Dullahan Body: [DullahanBody]
- Dullahan Chase: [Dullahan's DullahanChaseSystem]
- Puzzle Doors: [Any additional puzzle doors]
- Real Head Door: [RealHeadDoor]
```

#### **3.2 Manager References**
```
Managers:
- Head Inventory: [Player's DullahanHeadInventory]
- Effect Manager: [DullahanHeadEffectManager]
- Audio Manager: [Your Audio Manager]
```

### **Step 4: Configure Scriptable Objects**

#### **4.1 Real Head ScriptableObject**
```
DullahanHeadSO: RealHead
- Head ID: 1
- Head Name: "Real Dullahan Head"
- Head Type: Real
- Head Icon: [Real head sprite]
- Has Effect: false
- Pickup Sound: [Head pickup audio]
- Drop Sound: [Head drop audio]
```

#### **4.2 Fake Head 1 ScriptableObject**
```
DullahanHeadSO: FakeHead1
- Head ID: 2
- Head Name: "Fake Head 1"
- Head Type: Fake1
- Head Icon: [Fake head 1 sprite]
- Has Effect: true
- Effect Type: Buff
- Effect Description: "Speed Boost"
- Pickup Sound: [Head pickup audio]
- Drop Sound: [Head drop audio]
```

#### **4.3 Fake Head 2 ScriptableObject**
```
DullahanHeadSO: FakeHead2
- Head ID: 3
- Head Name: "Fake Head 2"
- Head Type: Fake2
- Head Icon: [Fake head 2 sprite]
- Has Effect: true
- Effect Type: Debuff
- Effect Description: "Reduced Vision"
- Pickup Sound: [Head pickup audio]
- Drop Sound: [Head drop audio]
```

### **Step 5: Configure Player Systems**

#### **5.1 DullahanHeadInventory**
```
Inventory Settings:
- Max Inventory Size: 3
- Player Reach: 3
- Selected Item: 0

Player Item GameObjects:
- Real Head Item: [Player's real head model]
- Fake Head 1 Item: [Player's fake head 1 model]
- Fake Head 2 Item: [Player's fake head 2 model]
- Lantern Item: [Player's lantern model]

Camera and UI:
- Camera: [Player's main camera]
- Press To Pickup GameObject: [UI prompt]
- Inventory Slot Images: [3 UI images]
- Inventory Background Images: [3 UI images]
```

#### **5.2 PlayerHealthSystem**
```
Health Settings:
- Max Health: 3
- Invulnerability Time: 1
- Current Health: 3

Health UI:
- Health UI: [Health UI GameObject]
- Health Bars: [3 UI images]
- Health Text: [TextMeshPro component]

Camera Effects:
- Player Camera: [Player's main camera]
- Shake Intensity: 0.5
- Shake Duration: 0.3
- Damage Flash Duration: 0.2
- Damage Flash Color: Red

Player Controller:
- Player Controller: [FirstPersonController]
```

### **Step 6: Configure Dullahan Systems**

#### **6.1 DullahanChaseSystem**
```
Chase Settings:
- Chase Speed: 6
- Patrol Speed: 3
- Detection Range: 15
- Attack Range: 2

Patrol Settings:
- Patrol Points: [Array of patrol waypoints]
- Patrol Radius: 10
- Patrol Wait Time: 2
```

#### **6.2 DullahanMeleeAttack**
```
Attack Settings:
- Attack Range: 2
- Attack Damage: 1
- Attack Cooldown: 2
- Attack Duration: 0.5

Integration:
- Player Health System: [Player's PlayerHealthSystem]
- Chase System: [Dullahan's DullahanChaseSystem]
```

### **Step 7: UI Setup**

#### **7.1 Timer UI**
```
GameObject: TimerUI
- Timer Text: [TextMeshPro component]
- Timer Fill Image: [UI Image with fill type]
- Timer Normal Color: White
- Timer Warning Color: Red
- Warning Threshold: 10
```

#### **7.2 Choice UI**
```
GameObject: ChoiceUI
- Choice Text: [TextMeshPro component]
- Help Button: [UI Button]
- Leave Button: [UI Button]
- Help Text: "Help Dullahan find its head?"
- Leave Text: "Leave through exit door"
```

#### **7.3 Proximity UI**
```
GameObject: ProximityUI
- Proximity Text: [TextMeshPro component]
- Proximity Warning Text: "Dullahan's territory..."
```

## 🎮 **Puzzle Flow Verification**

### **Phase 1: Proximity Trigger**
- ✅ Player enters proximity radius (10 units)
- ✅ Proximity warning appears
- ✅ 2-second delay, then chase starts

### **Phase 2: 60-Second Chase**
- ✅ Dullahan chases player for exactly 60 seconds
- ✅ Timer UI shows countdown
- ✅ Warning at 10 seconds remaining
- ✅ Chase ends, Dullahan returns to patrol

### **Phase 3: Exit Doors Open**
- ✅ Both exit doors open simultaneously
- ✅ Doors serve as "bait" for player
- ✅ Choice UI appears with 30-second timer

### **Phase 4: Player Choice**
- ✅ Player can choose to help or leave
- ✅ Auto-choice to leave if no decision in 30 seconds
- ✅ If leaving → Bad ending (Dullahan kills player)

### **Phase 5: Head Collection (If Helping)**
- ✅ All three heads spawn in world
- ✅ Real head door opens
- ✅ 90-second timer to find and attach real head
- ✅ Player can collect heads in inventory

### **Phase 6: Head Attachment**
- ✅ Player can attach heads to Dullahan body
- ✅ Fake heads apply buffs/debuffs to player
- ✅ Real head attachment triggers good ending

### **Phase 7: Victory/Defeat**
- ✅ Real head attached → Exit doors become safe
- ✅ Player can proceed to next scene
- ✅ No real head → Bad ending if time expires

## 🔧 **Testing Checklist**

### **Pre-Test Setup**
- [ ] All GameObjects have correct components
- [ ] All references are assigned in inspectors
- [ ] ScriptableObjects are configured
- [ ] UI elements are properly set up
- [ ] Audio clips are assigned
- [ ] Scene names are correct

### **Functionality Tests**
- [ ] Proximity detection works
- [ ] 60-second chase timer functions
- [ ] Exit doors open simultaneously
- [ ] Choice system responds correctly
- [ ] Head spawning works
- [ ] Head collection works
- [ ] Head attachment works
- [ ] Fake head effects apply
- [ ] Real head triggers victory
- [ ] Bad ending triggers correctly

### **Integration Tests**
- [ ] Dullahan AI responds to chase/patrol
- [ ] Player health system works with attacks
- [ ] Inventory system manages heads correctly
- [ ] Door system opens/closes properly
- [ ] Audio system plays appropriate sounds
- [ ] UI updates correctly throughout

## 🚨 **Common Issues & Solutions**

### **Doors Not Opening**
- Check door assignments in DullahanChaseEventManager
- Verify door colliders are set to "Is Trigger"
- Ensure doors are not locked

### **Heads Not Spawning**
- Check head pickable assignments in DullahanPuzzleManager
- Verify ScriptableObjects are assigned
- Ensure head GameObjects are active

### **Chase Not Starting**
- Check proximity radius and center
- Verify DullahanChaseSystem is assigned
- Ensure player has "Player" tag

### **Effects Not Applying**
- Check DullahanHeadEffectManager is assigned
- Verify ScriptableObject effect settings
- Ensure PlayerHealthSystem is connected

### **UI Not Showing**
- Check UI GameObject assignments
- Verify TextMeshPro components
- Ensure UI is in correct canvas

## 🎯 **Performance Optimization**

### **Recommended Settings**
- Limit proximity checks to every 0.1 seconds
- Use object pooling for multiple doors
- Optimize head spawning with delays
- Cache component references in Start()

### **Memory Management**
- Destroy unused head GameObjects
- Clear event subscriptions on destroy
- Use coroutines for timed events
- Optimize audio clip loading

This setup creates a complete, professional Dullahan puzzle system that meets all your requirements with proper integration between all components.
