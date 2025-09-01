# 🧿 Jenglot Scripts Collection

This folder contains all the scripts needed for the Jenglot enemy system in your horror game.

## 📁 Scripts Overview

### **Core Scripts**
- **`JenglotBehavior.cs`** - Main behavior system for Jenglot enemies
  - Proximity activation
  - Player following with NavMesh
  - Stare detection (freezes when looked at)
  - Audio and visual effects

- **`JenglotSetup.cs`** - One-click setup helper
  - Automatically configures all components
  - Sets up NavMeshAgent, AudioSource, etc.
  - Creates room triggers if needed

- **`JenglotRoomTrigger.cs`** - Room-based activation system
  - Precise room detection using colliders
  - Light effects and audio cues
  - Can work with or replace distance-based detection

### **Testing & Debug**
- **`JenglotTestingHelper.cs`** - Development and testing tools
  - Force activate/deactivate Jenglot
  - Real-time status information
  - Scene validation and diagnostics
  - Debug visualization

### **Documentation**
- **`README_JenglotSystem.md`** - Complete system documentation
  - Setup instructions
  - Configuration options
  - Integration examples
  - Troubleshooting guide

## 🚀 Quick Start

1. **Create a Jenglot GameObject** in your scene
2. **Add `JenglotSetup` component**
3. **Click "Setup Jenglot"** in the context menu
4. **Configure settings** as needed
5. **Test with `JenglotTestingHelper`**

## 🔧 Key Features

- **Smart AI**: Follows player but freezes when stared at
- **NavMesh Integration**: Proper pathfinding around obstacles
- **Audio System**: Activation, movement, and freeze sounds
- **Visual Effects**: Material changes and animations
- **Room Triggers**: Precise area-based activation
- **Testing Tools**: Comprehensive debugging and validation

## 📍 Usage in Scenes

These scripts are designed to work in your **3rd Floor** scenes where Jenglot enemies appear. The system automatically integrates with your existing player controller and audio systems.

## 🎯 Next Steps

1. **Test the system** using the testing helper
2. **Customize behavior** by adjusting inspector values
3. **Add multiple Jenglots** for increased horror
4. **Integrate with quest system** for story progression
