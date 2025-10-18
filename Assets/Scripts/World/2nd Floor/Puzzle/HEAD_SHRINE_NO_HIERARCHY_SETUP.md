# 🏛️ Head Shrine Puzzle - No Hierarchy Setup

## 🎯 **Problem Solved!**
No more hassle moving altar assets as children! The Head Shrine Puzzle now works with **existing assets in their current hierarchy positions**.

## 🛠️ **Super Easy Setup (2 Steps)**

### **Step 1: Create the Shrine Script**
1. Create an **empty GameObject** in your scene
2. Name it `"HeadShrine"`
3. Add the `HeadShrinePuzzle` script to it

### **Step 2: Assign Existing Assets**
In the inspector, simply **drag and drop** your existing altar assets:

```csharp
[Header("🏗️ Altar Assets (Assign Existing Objects)")]
altarBase = [Drag your TableV2 here];
leftCandle = [Drag your CandleV1 here];
centerCandle = [Drag your CandleV2 here];
rightCandle = [Drag your CandleV3 here];
```

**That's it!** No need to move anything in the hierarchy.

## 🎮 **How It Works**

### **Automatic Placement Creation:**
The script automatically creates placement points based on your existing candle positions:

- **Center Placement**: Positioned above your center candle
- **Left Placement**: Positioned above your left candle  
- **Right Placement**: Positioned above your right candle

### **Smart Positioning:**
- Uses your **existing candle positions** as reference points
- Places placement points **slightly above** each candle
- **No hierarchy changes** required

## 🔧 **Inspector Configuration**

### **🏗️ Altar Assets:**
```csharp
altarBase = TableV2;        // Your wooden table
leftCandle = CandleV1;      // Left candle holder
centerCandle = CandleV2;    // Center candle holder
rightCandle = CandleV3;     // Right candle holder
```

### **🏛️ Shrine Settings:**
```csharp
requiredHeadIDs = { 1, 2, 3 };           // Head IDs for each placement
realHeadPlacementIndex = 0;              // Center placement (0 = first)
interactionDistance = 4f;                // How close player needs to be
```

### **🎯 Head Rewards:**
```csharp
wrongHead1Door = [doorscript for wrong head 1];
wrongHead2Door = [doorscript for wrong head 2];
realHeadDoor = [doorscript for real head];
keyReward = [Key prefab];
keySpawnPoint = [Key spawn location];
```

## 🎨 **Visual Debug**

When you select the HeadShrine GameObject, you'll see:
- **Green sphere**: Interaction distance
- **Blue cube**: Altar base position
- **Cyan cubes**: Candle positions
- **Red cube**: Real head placement (center)
- **Yellow cubes**: Wrong head placements (left/right)

## ✨ **Benefits**

### **No Hierarchy Hassle:**
- **Keep existing organization** - no need to move assets
- **Works with any hierarchy** - assets can be anywhere
- **Easy to maintain** - just assign references

### **Smart Positioning:**
- **Uses existing positions** - no manual positioning needed
- **Automatic placement creation** - based on candle positions
- **Flexible setup** - works with any altar layout

### **Easy Configuration:**
- **Simple drag & drop** - just assign existing GameObjects
- **Clear visual feedback** - gizmos show all positions
- **No complex setup** - minimal configuration required

## 🎯 **Perfect for Your Scene**

This approach is perfect for your Unity scene where you have:
- **TableV2** as the main altar
- **CandleV1, CandleV2, CandleV3** as candle holders
- **All assets in their current positions**

Just drag and drop them into the inspector fields, and the script handles the rest!

## 🚀 **Quick Start**

1. **Create HeadShrine GameObject** with the script
2. **Drag TableV2** to `altarBase`
3. **Drag CandleV1** to `leftCandle`
4. **Drag CandleV2** to `centerCandle`
5. **Drag CandleV3** to `rightCandle`
6. **Configure rewards** (doors, keys, etc.)
7. **Play and test!**

No hierarchy changes, no positioning hassles - just simple drag and drop assignment!
