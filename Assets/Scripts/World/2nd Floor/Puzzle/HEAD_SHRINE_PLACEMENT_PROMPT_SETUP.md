# 💬 Head Shrine Puzzle - Placement Prompt Setup

## 🎯 **Overview**
The Head Shrine Puzzle now includes a **placement prompt system** that shows players when they can interact with the altar and what actions are available.

## 🎮 **How It Works**

### **Prompt States:**
1. **"Press F to place head"** - Player has a head and can place it
2. **"No head in inventory"** - Player doesn't have a head
3. **"All placements are full"** - All placement points already have heads
4. **Hidden** - Player is not in range of any placement point

### **Visual Flow:**
```
Player approaches shrine → Prompt appears → Shows appropriate message
Player moves away → Prompt disappears
Player places head → Prompt updates to show new state
```

## 🛠️ **Setup Guide**

### **Step 1: Create UI Elements**

#### **Create Placement Prompt UI:**
1. **Create Canvas** (if you don't have one)
2. **Create UI Panel** as child of Canvas
3. **Name it** `"PlacementPromptUI"`
4. **Add TextMeshProUGUI** as child of the panel
5. **Name it** `"PlacementPromptText"`

#### **UI Hierarchy:**
```
Canvas
└── PlacementPromptUI (Panel)
    └── PlacementPromptText (TextMeshProUGUI)
```

### **Step 2: Configure UI Elements**

#### **PlacementPromptUI Panel:**
- **Position**: Center of screen or wherever you want the prompt
- **Size**: Small panel (e.g., 300x100)
- **Background**: Semi-transparent or solid color
- **Initially**: Set to inactive

#### **PlacementPromptText:**
- **Text**: "Press F to place head" (default)
- **Font Size**: 24-32
- **Color**: White or contrasting color
- **Alignment**: Center

### **Step 3: Assign to Head Shrine Puzzle**

In the Head Shrine Puzzle inspector:

```csharp
[Header("💬 Placement Prompt")]
placementPromptUI = [Drag your PlacementPromptUI here];
placementPromptText = [Drag your PlacementPromptText here];
placeHeadText = "Press F to place head";
noHeadText = "No head in inventory";
allFullText = "All placements are full";
```

### **Step 4: Customize Prompt Text**

You can customize the prompt messages:

```csharp
placeHeadText = "Press F to place head";
noHeadText = "You need to find a head first";
allFullText = "All altar placements are occupied";
```

## 🎨 **Visual Design Tips**

### **UI Styling:**
- **Background**: Dark semi-transparent panel
- **Text**: Bright, readable color
- **Position**: Center-bottom of screen
- **Size**: Not too large, not too small

### **Animation (Optional):**
- **Fade in/out** when appearing/disappearing
- **Pulse effect** to draw attention
- **Scale animation** for emphasis

## 🎮 **Player Experience**

### **Interaction Flow:**
```
1. Player approaches shrine → Prompt appears
2. Player sees "Press F to place head" → Knows they can interact
3. Player presses F → Head gets placed
4. Prompt updates → Shows new state
5. Player moves away → Prompt disappears
```

### **Clear Feedback:**
- **Always shows current state** - no guessing
- **Updates in real-time** - immediate feedback
- **Contextual messages** - different text for different situations

## 🔧 **Advanced Features**

### **Customizable Text:**
- **Change messages** in inspector
- **Different languages** support
- **Contextual hints** (e.g., "Find the real head first")

### **Visual Enhancements:**
- **Icons** alongside text
- **Color coding** for different states
- **Progress indicators** (e.g., "2/3 heads placed")

## 🎯 **Perfect Integration**

The placement prompt system works seamlessly with:
- **Existing inventory system** - checks for heads
- **Placement system** - shows current state
- **Reward system** - updates as heads are placed
- **Audio system** - works with existing sounds

## 🚀 **Quick Setup Checklist**

- [ ] Create Canvas (if needed)
- [ ] Create PlacementPromptUI Panel
- [ ] Create PlacementPromptText
- [ ] Style the UI elements
- [ ] Assign to Head Shrine Puzzle inspector
- [ ] Customize prompt text
- [ ] Test in play mode

## ✨ **Benefits**

- **Clear communication** - players know what to do
- **Better UX** - no confusion about interactions
- **Professional feel** - polished user experience
- **Easy to implement** - simple UI setup
- **Highly customizable** - change text and styling

The placement prompt system makes the Head Shrine Puzzle much more user-friendly and professional!
