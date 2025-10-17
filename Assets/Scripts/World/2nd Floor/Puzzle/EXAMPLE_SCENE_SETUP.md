# 🎮 Example Scene Setup - Head Collection Puzzle

## Quick 5-Minute Setup

### Step 1: Create the Pedestal (2 minutes)

1. **Create Base Pedestal:**
   ```
   GameObject → 3D Object → Cylinder
   Name: "HeadCollectionPedestal"
   Scale: (2, 1, 2) - Makes it wider and shorter
   Position: (0, 0, 0) - Center of your scene
   ```

2. **Add the Script:**
   ```
   Select "HeadCollectionPedestal"
   Add Component → SimpleHeadCollectionPuzzle
   ```

3. **Configure Basic Settings:**
   ```
   Required Head IDs: [1, 2, 3]
   Interaction Distance: 3.0
   ```

### Step 2: Create Head Slots (2 minutes)

**Option A: Automatic (Recommended for beginners)**
- The script will create slots automatically
- Just run the game and they'll appear

**Option B: Manual (For custom positioning)**
1. Create 3 child objects under the pedestal:
   ```
   Right-click "HeadCollectionPedestal" → Create Empty
   Name: "Slot1"
   Position: (-1, 1, 0)
   
   Right-click "HeadCollectionPedestal" → Create Empty  
   Name: "Slot2"
   Position: (0, 1, 0)
   
   Right-click "HeadCollectionPedestal" → Create Empty
   Name: "Slot3" 
   Position: (1, 1, 0)
   ```

2. Add visual indicators (optional):
   ```
   For each slot:
   - Add Component → Mesh Renderer
   - Add Component → Mesh Filter
   - Mesh Filter → Mesh: Cube
   - Scale: (0.5, 0.1, 0.5) - Flat platform
   ```

### Step 3: Add Visual Materials (1 minute)

1. **Create Materials:**
   ```
   Right-click in Project → Create → Material
   Name: "EmptySlotMaterial"
   Color: Light Gray (0.8, 0.8, 0.8)
   
   Right-click in Project → Create → Material  
   Name: "FilledSlotMaterial"
   Color: Blue (0.2, 0.5, 1.0)
   
   Right-click in Project → Create → Material
   Name: "CorrectHeadMaterial" 
   Color: Gold (1.0, 0.8, 0.2)
   ```

2. **Assign to Script:**
   ```
   Select "HeadCollectionPedestal"
   SimpleHeadCollectionPuzzle:
   - Empty Slot Material: [EmptySlotMaterial]
   - Filled Slot Material: [FilledSlotMaterial]  
   - Correct Head Material: [CorrectHeadMaterial]
   ```

## 🎯 Complete Inspector Setup

```
HeadCollectionPedestal (GameObject)
├─ Transform
│  ├─ Position: (0, 0, 0)
│  ├─ Rotation: (0, 0, 0)
│  └─ Scale: (2, 1, 2)
│
├─ Mesh Renderer
│  └─ Materials: [PedestalMaterial]
│
├─ Mesh Filter
│  └─ Mesh: Cylinder
│
└─ SimpleHeadCollectionPuzzle
   ├─ 🎯 Puzzle Settings
   │  ├─ Required Head IDs: [1, 2, 3]
   │  └─ Interaction Distance: 3.0
   │
   ├─ 🎨 Visual Settings
   │  ├─ Empty Slot Material: [EmptySlotMaterial]
   │  ├─ Filled Slot Material: [FilledSlotMaterial]
   │  └─ Correct Head Material: [CorrectHeadMaterial]
   │
   ├─ 🎵 Audio (Optional)
   │  ├─ Head Placed Sound: [None]
   │  ├─ Puzzle Complete Sound: [None]
   │  └─ Wrong Head Sound: [None]
   │
   └─ 🎁 Rewards (Optional)
      ├─ Reward Door: [None]
      └─ Reward Items: [None]
```

## 🎮 Testing the Setup

### Test Steps:
1. **Play the scene**
2. **Walk up to the pedestal** - Should see interaction distance gizmo
3. **Press F** - Should see "No head in inventory" in console
4. **Pick up a head** (if you have head pickups in scene)
5. **Walk to pedestal and press F** - Should place head in slot
6. **Repeat until all slots filled** - Should complete puzzle

### Expected Console Output:
```
[SimpleHeadCollectionPuzzle] Setup complete! 3 slots created.
[SimpleHeadCollectionPuzzle] Created slot: Slot1 for head ID 1
[SimpleHeadCollectionPuzzle] Created slot: Slot2 for head ID 2  
[SimpleHeadCollectionPuzzle] Created slot: Slot3 for head ID 3
[SimpleHeadCollectionPuzzle] Trying to place head: Real Head (ID: 1)
[SimpleHeadCollectionPuzzle] ✓ Placing Real Head in slot!
[SimpleHeadCollectionPuzzle] 🎉 PUZZLE COMPLETED!
```

## 🎨 Visual Polish Ideas

### Enhanced Pedestal:
```
1. Add a base platform:
   - Create → 3D Object → Cube
   - Scale: (4, 0.2, 4)
   - Position: (0, -0.6, 0)
   - Material: Stone/Dark Gray

2. Add decorative elements:
   - Torches around the pedestal
   - Runes or symbols on the base
   - Glowing effects on slots
```

### Slot Visuals:
```
1. Make slots more obvious:
   - Use a different shape (Sphere, Pyramid)
   - Add glow effects
   - Use emissive materials

2. Add progress indicators:
   - Numbers above each slot
   - Progress bar
   - Completion percentage
```

## 🔧 Advanced Configuration

### Custom Head Requirements:
```csharp
// Example: Need 2 real heads and 1 fake head
Required Head IDs: [1, 1, 2]

// Example: Specific order required  
Required Head IDs: [2, 1, 3]

// Example: Only need 1 head
Required Head IDs: [1]
```

### Integration with Existing Systems:
```csharp
// Connect to door system
Reward Door: [YourDoorObject]

// Connect to item spawning
Reward Items: [KeyPrefab, HealthPotionPrefab]

// Connect to event system (automatic)
// Script will find Floor2EndingEventManager automatically
```

## 🐛 Common Setup Issues

### Issue: Slots not appearing
**Solution:** Check that child objects are named "Slot1", "Slot2", etc.

### Issue: Can't interact
**Solution:** Ensure player has DullahanHeadInventory component

### Issue: Wrong head placement
**Solution:** Check Required Head IDs array matches your head IDs

### Issue: No visual feedback
**Solution:** Assign materials in the Visual Settings section

## 🎯 Pro Tips

1. **Start Simple** - Get basic functionality working first
2. **Test Frequently** - Check each step as you build
3. **Use Console** - Watch for debug messages
4. **Visual Feedback** - Make sure players can see what's happening
5. **Audio Polish** - Add sounds for better player experience

## 🚀 Next Steps

Once basic setup is working:

1. **Add Audio** - Find or create sound effects
2. **Polish Visuals** - Better materials, lighting, effects
3. **Connect Rewards** - Link to doors, items, story progression
4. **Add Hints** - UI text, visual cues for players
5. **Test with Players** - Get feedback and iterate

---

**This setup should take less than 5 minutes and give you a fully functional head collection puzzle!**
