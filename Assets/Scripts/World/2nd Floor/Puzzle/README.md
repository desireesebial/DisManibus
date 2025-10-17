# Dullahan Head Placement Puzzle System

## 📁 Folder Contents

This folder contains the complete system for the Dullahan head placement puzzle on the 2nd floor.

### Core Scripts

1. **DullahanHeadPlacementPuzzle.cs**
   - Main puzzle controller
   - Handles player interaction
   - Manages head placement logic
   - Controls puzzle completion and rewards

2. **HeadPlaceholder.cs**
   - Visual controller for the placeholder object
   - Handles visibility, pulsing, and glow effects
   - Changes appearance based on player state

3. **ExamplePuzzleIntegration.cs** (Optional)
   - Example integration with other game systems
   - Shows how to hook into puzzle events
   - Template for custom implementations

### Documentation

1. **DULLAHAN_HEAD_PLACEMENT_SETUP.md**
   - Complete setup guide with detailed instructions
   - Troubleshooting section
   - Advanced configuration options

2. **QUICK_REFERENCE.md**
   - Quick 5-minute setup guide
   - Common settings reference table
   - Code examples and tips

3. **README.md** (This file)
   - Overview of the system
   - Quick links to documentation

## 🎮 How It Works

### Player Perspective
1. Player finds and picks up Dullahan heads
2. Player approaches the Dullahan's body
3. Placeholder appears showing where to place head
4. Placeholder changes color:
   - **Green**: Holding correct head
   - **Red**: Holding wrong head
   - **White**: No head in hand
5. Player presses F to place head
6. If correct: Puzzle completes, rewards granted
7. If wrong: Head is consumed, displayed briefly, then removed

### Technical Flow
```
Player Approaches
    ↓
Placeholder Appears (invisible → visible)
    ↓
Check Selected Head
    ↓
Update Placeholder Material (empty/valid/invalid)
    ↓
Player Presses F
    ↓
Validate Head Type
    ↓
[Correct Head]              [Wrong Head]
    ↓                           ↓
Attach Head                 Show Briefly
Complete Puzzle             Apply Effects
Grant Rewards              Remove Head
Notify Systems             Restore Placeholder
```

## 🚀 Quick Start

See **QUICK_REFERENCE.md** for a 5-minute setup guide.

### Minimal Setup (3 Steps)
1. Create puzzle GameObject with placeholder child
2. Add scripts to both objects
3. Connect references in inspector

**That's it!** The system works with existing inventory automatically.

## 📚 Documentation Guide

| Document | When to Use |
|----------|-------------|
| QUICK_REFERENCE.md | Quick setup, common settings, troubleshooting |
| DULLAHAN_HEAD_PLACEMENT_SETUP.md | Detailed setup, advanced features, integration |
| ExamplePuzzleIntegration.cs | Custom integration, event handling, save/load |

## ✨ Key Features

- ✅ **Placeholder Visibility Control**: Initially hidden, appears on approach
- ✅ **Visual Feedback**: Color-coded materials for different states
- ✅ **Smooth Transitions**: Fade in/out, pulsing animations
- ✅ **Wrong Head Handling**: Temporary display then removal
- ✅ **Automatic Integration**: Works with DullahanHeadInventory
- ✅ **Event System**: Notifies event managers automatically
- ✅ **Reward System**: Door unlocking, item spawning
- ✅ **Audio System**: Sounds for all interactions
- ✅ **Raycast or Distance**: Two interaction modes
- ✅ **Highly Configurable**: Dozens of customization options

## 🎯 Design Goals

This system was designed to:
1. **Match the reference video**: Similar placement mechanic
2. **Integrate seamlessly**: Works with existing systems
3. **Be user-friendly**: Easy setup in Unity Inspector
4. **Be flexible**: Customizable for different use cases
5. **Be maintainable**: Well-documented and organized

## 🔧 Technical Details

### Dependencies
- `DullahanHeadInventory.cs`: Player inventory system
- `DullahanHeadSO.cs`: Head data ScriptableObjects
- `TextMeshPro`: For UI text (Unity package)

### Optional Integrations
- `Floor2EndingEventManager`: Event notifications
- `DullahanChaseEventManager`: Event notifications
- `Door`: Reward door unlocking
- Various audio/visual components

### Performance
- Lightweight: Single raycast or distance check per frame
- No physics calculations during gameplay
- Minimal allocations (coroutines only for effects)

## 📝 Example Usage

### Basic Puzzle Setup
```csharp
// The puzzle is automatically set up through Unity Inspector
// Just assign references and configure settings
```

### Check Completion
```csharp
DullahanHeadPlacementPuzzle puzzle = FindObjectOfType<DullahanHeadPlacementPuzzle>();
if (puzzle.IsPuzzleCompleted())
{
    Debug.Log("Puzzle complete!");
}
```

### Control Placeholder
```csharp
HeadPlaceholder placeholder = FindObjectOfType<HeadPlaceholder>();
placeholder.Show();
placeholder.SetState(HeadPlaceholder.PlaceholderState.ValidHover);
```

## 🐛 Common Issues

| Issue | Solution |
|-------|----------|
| Placeholder not appearing | Check interactionRange and player distance |
| Can't place head | Ensure player has DullahanHeadInventory and a head |
| No visual feedback | Assign materials in inspector |
| Wrong key | Change interactionKey setting |

See **DULLAHAN_HEAD_PLACEMENT_SETUP.md** for detailed troubleshooting.

## 🔗 Related Systems

- **Inventory**: `DullahanHeadInventory` (automatically detected)
- **Pickables**: `DullahanHeadPickable` (for picking up heads)
- **Events**: `Floor2EndingEventManager`, `DullahanChaseEventManager`
- **Doors**: Standard `Door` script for reward unlocking

## 📖 Further Reading

1. Start with **QUICK_REFERENCE.md** for basic setup
2. Read **DULLAHAN_HEAD_PLACEMENT_SETUP.md** for details
3. Check **ExamplePuzzleIntegration.cs** for advanced usage
4. See existing Dullahan scripts for system context

## 🎥 Reference

Based on placement mechanic similar to: https://youtu.be/hHPOHYZeEq0?si=VZukXpGHPZBSRnlG

## 📦 File Structure

```
2nd Floor/
└── Puzzle/
    ├── DullahanHeadPlacementPuzzle.cs
    ├── HeadPlaceholder.cs
    ├── ExamplePuzzleIntegration.cs
    ├── DULLAHAN_HEAD_PLACEMENT_SETUP.md
    ├── QUICK_REFERENCE.md
    └── README.md (this file)
```

## 🎨 Inspector Preview

```
DullahanHeadPlacementPuzzle Component:
┌─────────────────────────────────────┐
│ Puzzle Settings                      │
│ ├─ Required Head ID: 1              │
│ └─ Puzzle Completed: ☐              │
├─────────────────────────────────────┤
│ Interaction Settings                 │
│ ├─ Interaction Range: 3.0           │
│ ├─ Interaction Key: F                │
│ └─ Use Raycast: ☑                   │
├─────────────────────────────────────┤
│ Placeholder Settings                 │
│ ├─ Head Placeholder: [GameObject]   │
│ ├─ Initially Visible: ☐             │
│ ├─ Empty Material: [Material]       │
│ ├─ Valid Material: [Material]       │
│ └─ Invalid Material: [Material]     │
├─────────────────────────────────────┤
│ Head Attachment                      │
│ ├─ Attachment Point: [Transform]    │
│ └─ Attached Visual: [GameObject]    │
├─────────────────────────────────────┤
│ Visual Effects                       │
│ Audio                                │
│ UI                                   │
│ Rewards                              │
└─────────────────────────────────────┘
```

---

**Version**: 1.0  
**Created**: December 10, 2025  
**Project**: DisManibus  
**Location**: `Assets/Scripts/World/2nd Floor/Puzzle/`
