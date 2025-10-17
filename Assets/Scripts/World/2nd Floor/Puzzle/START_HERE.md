# 🚀 START HERE - Dullahan Head Placement Puzzle Setup

## Quick 5-Minute Setup Guide

This guide will get your Dullahan head placement puzzle working in under 5 minutes!

---

## 📋 Prerequisites

Before starting, make sure you have:
- ✅ Unity project with DullahanHeadInventory system
- ✅ DullahanHeadSO ScriptableObjects created
- ✅ Player with "Player" tag
- ✅ Dullahan GameObject with "Dullahan" tag

---

## 🎯 Step 1: Create the Puzzle Object (1 minute)

1. **Create Empty GameObject:**
   ```
   Right-click in Hierarchy → Create Empty
   Name: "DullahanHeadPuzzle"
   ```

2. **Add the Main Script:**
   ```
   Select "DullahanHeadPuzzle"
   Add Component → SimpleHeadPlacement
   ```

3. **Position the Puzzle:**
   ```
   Transform → Position: (0, 0, 0)  // Or wherever you want the puzzle
   ```

---

## 🎨 Step 2: Create the Head Attachment Point (1 minute)

1. **Create Attachment Point:**
   ```
   Right-click "DullahanHeadPuzzle" → Create Empty
   Name: "HeadAttachmentPoint"
   Position: (0, 1.5, 0)  // Adjust height as needed
   ```

2. **Assign to Script:**
   ```
   Select "DullahanHeadPuzzle"
   SimpleHeadPlacement → Head Attach Point: [HeadAttachmentPoint]
   ```

---

## ⚙️ Step 3: Configure Basic Settings (1 minute)

1. **Set Required Head ID:**
   ```
   SimpleHeadPlacement → Correct Head ID: 1  // Usually 1 for real head
   ```

2. **Set Interaction Distance:**
   ```
   SimpleHeadPlacement → Interaction Distance: 5.0
   ```

3. **Enable Dullahan Freeze (Optional):**
   ```
   SimpleHeadPlacement → Freeze Dullahan With Head: ✓
   ```

---

## 🎵 Step 4: Add Audio (Optional - 1 minute)

1. **Add AudioSource:**
   ```
   Select "DullahanHeadPuzzle"
   Add Component → Audio Source
   ```

2. **Assign Audio Clips:**
   ```
   SimpleHeadPlacement → Correct Head Sound: [YourSuccessSound]
   SimpleHeadPlacement → Wrong Head Sound: [YourErrorSound]
   ```

---

## 🎁 Step 5: Connect Rewards (Optional - 1 minute)

1. **Connect Door (if you have one):**
   ```
   SimpleHeadPlacement → Reward Door: [YourDoorObject]
   ```

2. **Add Reward Items (if any):**
   ```
   SimpleHeadPlacement → Reward Items: [YourItemPrefabs]
   ```

---

## ✅ Step 6: Test the Puzzle

1. **Play the Scene**
2. **Pick up a head** (if you have head pickups)
3. **Walk near the puzzle object**
4. **Press F** to place the head
5. **Check console** for debug messages

**Expected Console Output:**
```
[SimpleHeadPlacement] Initialized successfully
[SimpleHeadPlacement] Trying to place head: Real Head (ID: 1)
[SimpleHeadPlacement] ✓ CORRECT HEAD PLACED!
[SimpleHeadPlacement] Puzzle completed successfully!
```

---

## 🐛 Troubleshooting

### Common Issues:

| Problem | Solution |
|---------|----------|
| "No head selected" | Make sure player has a head in inventory |
| "No head in inventory" | Pick up a head first |
| Can't interact | Check interaction distance (try 10.0) |
| No visual feedback | Assign Head Attach Point |
| Dullahan not freezing | Check Dullahan has NavMeshAgent component |

### Debug Tips:
- Check console for error messages
- Make sure player has "Player" tag
- Make sure Dullahan has "Dullahan" tag
- Verify head IDs match between inventory and puzzle

---

## 🎯 Advanced Configuration

### Custom Head Requirements:
```csharp
// In inspector, set Correct Head ID:
// 1 = Real head
// 2 = Fake head 1  
// 3 = Fake head 2
```

### Interaction Settings:
```csharp
Interaction Distance: 5.0    // How close player needs to be
Freeze Dullahan With Head: ✓ // Freeze Dullahan when holding head
Start Frozen: ☐             // Start with Dullahan frozen
```

### Visual Settings:
```csharp
Show Wrong Head Briefly: ✓  // Show wrong head before removing
Wrong Head Duration: 2.0    // How long to show wrong head
```

---

## 🔗 Integration with Existing Systems

The puzzle automatically integrates with:
- ✅ **DullahanHeadInventory** - Detects current head
- ✅ **DullahanChaseSystem** - Freezes/unfreezes Dullahan
- ✅ **Floor2EndingEventManager** - Notifies on completion
- ✅ **Door System** - Unlocks reward doors

---

## 📚 Next Steps

Once basic setup is working:

1. **Add Visual Polish** - Better materials, lighting
2. **Add Audio** - Sound effects, music
3. **Connect More Systems** - Quests, achievements
4. **Test Thoroughly** - Try all head types
5. **Iterate** - Adjust difficulty, timing

---

## 🆘 Need Help?

- **Detailed Setup**: See `DULLAHAN_HEAD_PLACEMENT_SETUP.md`
- **Architecture**: See `SYSTEM_ARCHITECTURE.md`
- **Examples**: See `ExamplePuzzleIntegration.cs`
- **Quick Reference**: See `QUICK_REFERENCE.md`

---

**🎉 Congratulations! Your Dullahan head placement puzzle is now ready to use!**

The puzzle will work with your existing inventory system and automatically handle all the complex interactions. Players can now approach the Dullahan, place heads, and complete the puzzle with clear visual and audio feedback.
