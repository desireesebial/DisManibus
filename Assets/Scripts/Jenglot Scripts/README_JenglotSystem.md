# 🧿 Jenglot Enemy Behavior System

A complete behavior system for the Jenglot enemy that follows the player but stops moving when being stared at.

## 🎮 System Overview

### **Core Mechanics:**
- **Proximity Activation**: Jenglot activates when player enters its territory
- **Player Following**: Actively follows the player using NavMesh pathfinding
- **Stare Detection**: Stops moving when player is looking directly at it
- **Room-Based Triggers**: Enhanced detection using trigger colliders
- **Audio & Visual Feedback**: Complete sensory feedback system

### **Game Flow:**
1. **Idle State** - Jenglot is inactive until player approaches
2. **Activation** - Player enters the room/proximity range
3. **Following** - Jenglot follows player using NavMesh
4. **Freeze** - Stops moving when player stares directly at it
5. **Resume** - Continues following when player looks away
6. **Persistent Pursuit** - Once activated, Jenglot follows player across the entire map (if persistentFollowing = true)
7. **Deactivation** - Only stops if player goes extremely far or persistentFollowing is disabled

---

## 🌍 **Persistent Following Mode**

### **What is Persistent Following?**
When `persistentFollowing` is enabled (default), the Jenglot will follow the player **everywhere** in the map once activated. This creates a true horror game experience where:

- **Once Detected, Always Following**: The Jenglot never gives up the chase
- **Map-Wide Pursuit**: Follows player across rooms, floors, and entire scenes
- **Only Stops When Stared At**: The only way to temporarily stop it is by looking directly at it
- **Strategic Gameplay**: Players must use environment and stare mechanics to survive

### **How It Works**
1. Player enters Jenglot's detection range (activationRange)
2. Jenglot activates and plays activation sound
3. Jenglot begins following player using NavMesh pathfinding
4. **Unlike room-based mode, Jenglot stays active forever**
5. Only deactivates if player moves beyond deactivationRange (default: 100m) when persistentFollowing = false

### **Game Design Benefits**
```
✅ Creates constant tension and urgency
✅ Encourages strategic positioning and environment use
✅ Makes stare detection more critical for survival
✅ Enables complex level design with multiple Jenglots
✅ Perfect for horror game atmosphere
```

### **Configuration Options**
```
persistentFollowing: true   // Enable/disable persistent following
deactivationRange: 100f     // Distance after which Jenglot deactivates (if persistentFollowing = false)
```

---

## 🏗️ **Core Components**

### **1. JenglotBehavior.cs**
**Main behavior controller**

**Features:**
- Proximity-based activation system
- Advanced stare detection using camera raycast
- NavMesh-based pathfinding
- Animator integration
- Audio feedback system
- Visual material switching
- Debug visualization

**Key Settings:**
- `activationRange`: Distance to activate Jenglot (default: 10m)
- `followSpeed`: Movement speed when following player (default: 2m/s)
- `stareDetectionAngle`: Cone angle for stare detection (default: 45°)
- `stareMaxDistance`: Maximum distance for stare detection (default: 15m)

### **2. JenglotRoomTrigger.cs**
**Room-based detection system**

**Features:**
- Trigger collider-based room detection
- Light color changes when player enters
- Audio cues for room entry/exit
- Delayed deactivation system
- Visual room boundary indicators

**Key Settings:**
- `activateOnEnter`: Auto-activate Jenglot when player enters
- `deactivateOnExit`: Auto-deactivate when player exits
- `deactivationDelay`: Delay before deactivation (default: 2s)

### **3. JenglotSetup.cs**
**Automated setup helper**

**Features:**
- One-click Jenglot setup
- Auto-component configuration
- Model instantiation
- NavMesh validation
- Room trigger creation

---

## 📋 **Quick Setup Guide**

### **Method 1: Automatic Setup (Recommended)**

1. **Create Empty GameObject**
   ```
   GameObject → Create Empty
   Name: "Jenglot"
   ```

2. **Add Setup Component**
   ```
   Add Component → JenglotSetup
   ```

3. **Configure Settings** in JenglotSetup:
   - Set `jenglotModelPrefab` (drag Jenglot model from Assets/Assets/3rd Floor/Jenglot/)
   - Adjust `activationRange`, `followSpeed`, etc.
   - Enable `createRoomTrigger` if you want room-based detection

4. **Run Setup**
   ```
   Right-click JenglotSetup → "Setup Jenglot"
   ```

### **Method 2: Manual Setup**

1. **Create Jenglot GameObject**
   ```
   GameObject → Create Empty
   Name: "Jenglot"
   Tag: "Jenglot"
   ```

2. **Add Required Components**
   ```
   - JenglotBehavior
   - NavMeshAgent
   - AudioSource
   - Collider (CapsuleCollider recommended)
   ```

3. **Add Model as Child**
   ```
   Drag: Assets/Assets/3rd Floor/Jenglot/Character_output.fbx
   → As child of Jenglot GameObject
   ```

4. **Configure Components** (see detailed setup below)

---

## 🔧 **Detailed Configuration**

### **JenglotBehavior Configuration**

#### **Jenglot Settings**
```
activationRange: 10f        // Distance to start following player
followSpeed: 2f             // Speed when moving toward player
stareDetectionAngle: 45f    // Cone angle for stare detection
stareMinDistance: 1f        // Minimum distance for stare effect
stareMaxDistance: 15f       // Maximum distance for stare effect
persistentFollowing: true   // Once activated, follow player everywhere in the map
deactivationRange: 100f     // Only deactivate if player goes extremely far (when persistentFollowing = false)
```

#### **Movement Settings**
```
stopDistance: 2f            // How close Jenglot gets to player
rotationSpeed: 5f           // How fast Jenglot turns
canPassThroughDoors: false  // NavMesh obstacle avoidance
```

#### **Audio Settings**
```
activationSound             // Played when Jenglot activates
movementSound              // Looped while moving
freezeSound                // Played when frozen by stare
```

#### **Visual Effects**
```
normalMaterial             // Material when active
frozenMaterial             // Material when being stared at
```

### **NavMeshAgent Configuration**
```
Speed: 2f
Angular Speed: 120f
Acceleration: 8f
Stopping Distance: 2f
Radius: 0.5f
Height: 1.5f
Obstacle Avoidance: High Quality
```

### **Room Trigger Setup (Optional)**

1. **Create Trigger GameObject**
   ```
   GameObject → Create Empty
   Name: "JenglotRoomTrigger"
   ```

2. **Add Components**
   ```
   - BoxCollider (isTrigger = true)
   - JenglotRoomTrigger
   ```

3. **Configure Trigger**
   ```
   - Set BoxCollider size to cover room
   - Assign jenglotBehavior reference
   - Configure room lights array
   - Set audio clips for enter/exit
   ```

---

## 🎨 **Animation Setup**

### **Required Animator Parameters**
```
IsMoving (Bool)    - True when Jenglot is moving
IsFrozen (Bool)    - True when being stared at
```

### **Animation States**
```
Idle               - Default state when not moving
Moving             - Walking/floating toward player
Frozen             - Statue-like state when stared at
Activation         - Optional activation animation
```

### **Setup Instructions**
1. Create Animator Controller in Assets/Assets/3rd Floor/Jenglot/
2. Add the required Bool parameters
3. Create states and transitions
4. Assign animations from the Jenglot FBX files
5. Assign controller to Jenglot's Animator component

---

## 🔊 **Audio Integration**

### **Required Audio Clips**
- **Activation Sound**: Played when Jenglot first detects player
- **Movement Sound**: Looped while Jenglot is moving
- **Freeze Sound**: Played when player stares at Jenglot
- **Room Enter Sound**: Played when player enters room (optional)
- **Room Exit Sound**: Played when player leaves room (optional)

### **Audio Source Setup**
```
Spatial Blend: 1.0 (3D)
Rolloff Mode: Logarithmic
Max Distance: 20f
Volume: 0.8f
```

---

## 🎭 **Material & Visual Effects**

### **Material Setup**
1. **Normal Material**: Default appearance when active
2. **Frozen Material**: Appearance when being stared at (could be stone-like)

### **Recommended Visual Effects**
```
Normal State:
- Subtle emission glow
- Slight color variation
- Normal textures

Frozen State:
- Stone/statue material
- Reduced emission
- Desaturated colors
- Optional particle effects
```

---

## 🐛 **Testing & Debug**

### **Debug Features**
```
showDebugGizmos: true       // Show detection ranges in Scene view
enableDebugLogs: true       // Console debug messages
```

### **Debug Visualizations**
- **Yellow Circle**: Activation range (inactive)
- **Green Circle**: Activation range (active)
- **Blue Line**: Camera to Jenglot line of sight
- **Red Line**: Line of sight when being stared at
- **Cyan Circles**: Stare detection range (min/max)
- **White Line**: Current NavMesh path

### **Testing Methods**
1. **Runtime Testing**
   ```
   - Enter Play Mode
   - Walk toward Jenglot to activate
   - Look directly at it to freeze
   - Look away to resume movement
   ```

2. **Component Testing**
   ```
   Right-click JenglotBehavior → "Force Activate"
   Right-click JenglotBehavior → "Force Deactivate"
   ```

### **Common Issues & Solutions**

#### **Jenglot Not Moving**
- Check if NavMesh is baked in the scene
- Verify NavMeshAgent is enabled
- Ensure player has "Player" tag
- Check if Jenglot is being stared at

#### **Stare Detection Not Working**
- Verify player camera reference is assigned
- Check stareDetectionAngle and stareMaxDistance values
- Ensure clear line of sight (no obstacles)
- Check if player is within detection range

#### **No Audio**
- Verify AudioSource component is present
- Check if audio clips are assigned
- Ensure AudioSource volume > 0
- Check if AudioListener is present in scene

---

## 🚀 **Advanced Features**

### **Custom Events**
```csharp
// Subscribe to Jenglot events
JenglotBehavior jenglot = FindObjectOfType<JenglotBehavior>();

// Add custom behavior when Jenglot activates
jenglot.OnActivated += () => {
    Debug.Log("Jenglot is now hunting!");
    // Your custom code here
};
```

### **Performance Optimization**
```
- Use LOD system for distant Jenglots
- Reduce update frequency when far from player
- Pool audio sources for multiple Jenglots
- Use occlusion culling for line-of-sight checks
```

### **Multiple Jenglots**
```
- Each Jenglot operates independently
- Use different activation ranges to prevent clustering
- Consider shared audio management
- Use groups for coordinated behavior
```

---

## 📚 **Integration with Existing Systems**

### **Player Controller Integration**
- Works with both SimplePlayerMovement and FirstPersonController
- Automatically finds player camera
- Compatible with existing input systems

### **Audio Manager Integration**
```csharp
// Optional: Integrate with existing AudioManager
AudioManager.Instance.PlaySFX("jenglot_activation");
```

### **Game Manager Integration**
```csharp
// Optional: Register with game state system
GameManager.Instance.RegisterEnemy(jenglotBehavior);
```

---

## 📖 **Script References**

### **Public Methods**
```csharp
// JenglotBehavior
jenglot.ForceActivate()          // Manually activate
jenglot.ForceDeactivate()        // Manually deactivate
jenglot.SetFollowSpeed(float)    // Change follow speed
jenglot.IsCurrentlyActive        // Check if active
jenglot.IsCurrentlyFrozen        // Check if frozen
jenglot.DistanceToPlayer         // Get distance to player

// JenglotRoomTrigger
trigger.IsPlayerInRoom           // Check if player in room
trigger.ForcePlayerEnter()       // Simulate player entry
trigger.ForcePlayerExit()        // Simulate player exit
```

---

## 🎯 **Best Practices**

1. **Scene Setup**
   - Always bake NavMesh before testing
   - Position Jenglot on ground level
   - Use room triggers for precise control

2. **Performance**
   - Limit line-of-sight checks with reasonable max distance
   - Use object pooling for multiple Jenglots
   - Optimize audio playback

3. **Game Design**
   - Balance stare detection sensitivity
   - Provide clear audio/visual feedback
   - Test with different player speeds

4. **Debugging**
   - Enable debug gizmos during development
   - Use console logs to track behavior
   - Test edge cases (corners, obstacles, etc.)

---

## 🔄 **Version History**

**v1.0** - Initial implementation
- Basic proximity and stare detection
- NavMesh integration
- Audio and visual feedback
- Room trigger system
- Setup automation tools

---

**Happy haunting! 🧿👻**
