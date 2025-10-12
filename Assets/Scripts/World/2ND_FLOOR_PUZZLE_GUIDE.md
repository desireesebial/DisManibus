# 2nd Floor Dullahan Head Placement Puzzle

## 📍 Location
**Path**: `Assets/Scripts/World/2nd Floor/Puzzle/`

## 🎯 What's New
A complete puzzle system that allows the player to place the Dullahan's head onto its body. The placeholder is initially invisible and becomes visible when the player approaches, changing color based on whether they're holding the correct or wrong head.

## 🚀 Quick Start

### Where to Find It
Navigate to: `Assets/Scripts/World/2nd Floor/Puzzle/`

### What's Inside
1. **DullahanHeadPlacementPuzzle.cs** - Main puzzle controller
2. **HeadPlaceholder.cs** - Placeholder visual controller  
3. **ExamplePuzzleIntegration.cs** - Integration example
4. **Setup Guides** - Complete documentation

### Getting Started (5 Minutes)
1. Open `QUICK_REFERENCE.md` in the Puzzle folder
2. Follow the 5-minute setup guide
3. Test in your scene!

## 📖 Documentation

| File | Purpose |
|------|---------|
| **README.md** | System overview and file structure |
| **QUICK_REFERENCE.md** | 5-minute setup guide + common settings |
| **DULLAHAN_HEAD_PLACEMENT_SETUP.md** | Detailed setup with troubleshooting |
| **ExamplePuzzleIntegration.cs** | Code examples for custom integration |

## ✨ Features

### Core Mechanics
- ✅ Placeholder initially invisible, appears on player approach
- ✅ Color-coded feedback (green = correct, red = wrong, white = empty)
- ✅ Smooth fade transitions and pulsing animations
- ✅ Wrong heads consumed and displayed briefly
- ✅ Correct head completes puzzle and grants rewards

### Integration
- ✅ Automatically works with `DullahanHeadInventory`
- ✅ Notifies event managers on completion
- ✅ Supports door unlocking and item spawning
- ✅ Fully configurable through Unity Inspector

### Interaction
- ✅ Raycast-based (must look at puzzle) OR distance-based
- ✅ Customizable interaction key (default: F)
- ✅ Visual and audio feedback
- ✅ UI prompts for player guidance

## 🎮 How It Works

```
1. Dullahan is chasing/patrolling the player
2. Player picks up a Dullahan head
3. 🥶 DULLAHAN FREEZES (stops moving immediately)
4. Player can safely approach the body
5. Placeholder fades in (initially invisible → visible)
6. Placeholder changes color based on held head:
   - Green: Correct head
   - Red: Wrong head  
   - White: No head
7. Player presses F to place head
8. If correct:
   - Puzzle completes, rewards granted
   - 🔥 Dullahan unfreezes
9. If wrong:
   - Head consumed, shown briefly, then removed
   - 🔥 Dullahan unfreezes and resumes chase
```

## 📝 Minimal Setup Example

```
1. Create GameObject: "DullahanHeadPlacementPuzzle"
2. Add child Sphere: "HeadPlaceholder" (scale 0.3, 0.3, 0.3)
3. Add child Empty: "HeadAttachmentPoint"
4. Add DullahanHeadPlacementPuzzle script to parent
5. Add HeadPlaceholder script to placeholder
6. Connect references in inspector
7. Done! Test it!
```

## 🔗 Reference Video
Similar to this placement mechanic: https://youtu.be/hHPOHYZeEq0?si=VZukXpGHPZBSRnlG  
(But placing head on Dullahan's neck instead)

## 🛠️ Configuration

### Inspector Settings (DullahanHeadPlacementPuzzle)
- **Required Head ID**: ID of correct head (default: 1)
- **Interaction Range**: Distance to interact (default: 3m)
- **Interaction Key**: Key to place head (default: F)
- **Placeholder Initially Visible**: Start hidden (recommended: false)
- **Use Raycast Interaction**: Must look at puzzle (recommended: true)
- **Show Fake Head Briefly**: Display wrong heads temporarily (recommended: true)
- **Fake Head Display Duration**: How long to show wrong head (default: 2s)

### Inspector Settings (HeadPlaceholder)
- **Use Fade Transition**: Smooth fade in/out (recommended: true)
- **Enable Pulsing**: Animated breathing effect (recommended: true)
- **Enable Glow**: Light glow effect (recommended: true)
- **Materials**: Assign empty/valid/invalid materials for visual feedback

## 🎨 Optional Materials

Create three materials for best visual feedback:
1. **Mat_PlaceholderEmpty**: White transparent (RGBA: 255, 255, 255, 50)
2. **Mat_PlaceholderValid**: Green glowing (RGBA: 0, 255, 0, 100) + Emission
3. **Mat_PlaceholderInvalid**: Red warning (RGBA: 255, 0, 0, 100) + Emission

Assign these in the puzzle's inspector for color-coded feedback!

## 🐛 Troubleshooting

| Problem | Check |
|---------|-------|
| Placeholder not appearing | Player distance < Interaction Range |
| Can't place head | Player has DullahanHeadInventory component |
| No color change | Materials assigned in inspector |
| Wrong key | Check Interaction Key setting |
| No sound | Audio clips assigned in inspector |

For detailed troubleshooting, see `DULLAHAN_HEAD_PLACEMENT_SETUP.md`

## 💡 Tips

1. **Start Simple**: Get basic setup working first, then add effects
2. **Test Both Heads**: Try correct and wrong heads to verify behavior
3. **Use Materials**: Visual feedback greatly improves player experience
4. **Add Audio**: Sound effects make interactions more satisfying
5. **Check Console**: Scripts log helpful debug messages

## 🔧 Dependencies

### Required
- `DullahanHeadInventory.cs` (existing system)
- `DullahanHeadSO.cs` (existing system)
- `TextMeshPro` (Unity package)

### Optional
- `Door.cs` (for reward door unlocking)
- `Floor2EndingEventManager` (for event notifications)
- `DullahanChaseEventManager` (for event notifications)

## 📚 Next Steps

1. **Read**: Open `2nd Floor/Puzzle/QUICK_REFERENCE.md`
2. **Setup**: Follow the 5-minute guide
3. **Test**: Try in your scene with different heads
4. **Customize**: Adjust settings to match your game
5. **Integrate**: Use ExamplePuzzleIntegration.cs for custom logic

## 📁 File Structure

```
Assets/Scripts/World/
├── 2ND_FLOOR_PUZZLE_GUIDE.md (this file)
└── 2nd Floor/
    └── Puzzle/
        ├── DullahanHeadPlacementPuzzle.cs
        ├── HeadPlaceholder.cs
        ├── ExamplePuzzleIntegration.cs
        ├── DULLAHAN_HEAD_PLACEMENT_SETUP.md
        ├── QUICK_REFERENCE.md
        └── README.md
```

## 🎓 Learning Path

**Beginner**: Follow QUICK_REFERENCE.md for basic setup  
**Intermediate**: Read DULLAHAN_HEAD_PLACEMENT_SETUP.md for details  
**Advanced**: Study ExamplePuzzleIntegration.cs for custom integration

---

**Ready to Get Started?**  
👉 Navigate to: `Assets/Scripts/World/2nd Floor/Puzzle/QUICK_REFERENCE.md`

**Questions?**  
👉 Check: `Assets/Scripts/World/2nd Floor/Puzzle/DULLAHAN_HEAD_PLACEMENT_SETUP.md`

**Version**: 1.0  
**Created**: December 10, 2025  
**Compatible With**: DisManibus Project (Unity)

