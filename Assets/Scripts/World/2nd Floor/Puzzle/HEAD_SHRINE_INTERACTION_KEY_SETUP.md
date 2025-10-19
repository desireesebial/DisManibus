# ⌨️ Head Shrine Puzzle - Configurable Interaction Key

## 🎯 **Overview**
The Head Shrine Puzzle now includes a **configurable interaction key** so developers can set their preferred key for interacting with the shrine instead of being locked to the F key.

## 🎮 **How It Works**

### **Configurable Key System:**
- **Default Key**: F (KeyCode.F)
- **Customizable**: Change to any key in the inspector
- **Dynamic Prompts**: UI text updates automatically to show the correct key
- **Flexible**: Works with any key combination

### **Key Options Available:**
- **Letter Keys**: A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z
- **Number Keys**: 0, 1, 2, 3, 4, 5, 6, 7, 8, 9
- **Function Keys**: F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12
- **Special Keys**: Space, Enter, Escape, Tab, Shift, Ctrl, Alt
- **Arrow Keys**: UpArrow, DownArrow, LeftArrow, RightArrow
- **Mouse Buttons**: Mouse0, Mouse1, Mouse2, Mouse3, Mouse4, Mouse5, Mouse6

## 🛠️ **Setup Guide**

### **Step 1: Configure Interaction Key**

#### **In the Head Shrine Puzzle Inspector:**
1. **Find the "Shrine Settings" section**
2. **Set "Interaction Key"** to your preferred key
3. **Default is F** - change to any key you want

#### **Common Key Choices:**
```csharp
// Popular interaction keys
interactionKey = KeyCode.E;        // E for "Examine/Enter"
interactionKey = KeyCode.Space;    // Space for "Activate"
interactionKey = KeyCode.Return;   // Enter for "Confirm"
interactionKey = KeyCode.Mouse0;   // Left mouse button
interactionKey = KeyCode.F;        // F for "Furnish/Place"
```

### **Step 2: Customize Prompt Text**

#### **Update the Place Head Text:**
```csharp
// In the "Placement Prompt" section
placeHeadText = "Press {0} to place head";
```

#### **The {0} placeholder will be replaced with your key:**
- **If key is E**: "Press E to place head"
- **If key is Space**: "Press Space to place head"
- **If key is Mouse0**: "Press Mouse0 to place head"

### **Step 3: Test the Configuration**

#### **In Play Mode:**
1. **Approach the shrine** with a head
2. **Check the prompt** - should show your custom key
3. **Press your custom key** - should place the head
4. **Verify the interaction** works correctly

## 🎨 **Customization Options**

### **Different Keys for Different Scenarios:**

#### **For PC Games:**
```csharp
interactionKey = KeyCode.E;        // Standard "Use" key
interactionKey = KeyCode.F;        // Standard "Interact" key
interactionKey = KeyCode.Space;    // Standard "Activate" key
```

#### **For Console-Style Games:**
```csharp
interactionKey = KeyCode.Return;    // Enter key
interactionKey = KeyCode.Space;    // Space bar
interactionKey = KeyCode.Mouse0;   // Left mouse button
```

#### **For Mobile/Touch Games:**
```csharp
interactionKey = KeyCode.Mouse0;    // Touch/click
interactionKey = KeyCode.Space;    // Space bar
```

### **Custom Prompt Text:**

#### **Different Text Styles:**
```csharp
// Standard style
placeHeadText = "Press {0} to place head";

// Action-focused style
placeHeadText = "Press {0} to place head on altar";

// Instruction style
placeHeadText = "Press {0} to interact with shrine";

// Short style
placeHeadText = "Press {0} to place";

// Detailed style
placeHeadText = "Press {0} to place the head on the shrine";
```

## 🔧 **Advanced Configuration**

### **Multiple Interaction Keys:**

#### **If You Want Multiple Keys:**
```csharp
// Add this to the HeadShrinePuzzle class
[Header("🔧 Advanced Interaction")]
public KeyCode[] alternativeKeys = { KeyCode.E, KeyCode.Space };

// In the Update method, check multiple keys
if (Input.GetKeyDown(interactionKey) || 
    (alternativeKeys != null && alternativeKeys.Any(key => Input.GetKeyDown(key))))
{
    TryPlaceHeadOnPlacement(placement);
}
```

### **Key Combination Support:**

#### **For Complex Interactions:**
```csharp
// Check for key combinations
if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(interactionKey))
{
    // Special interaction with Shift + Key
    TryPlaceHeadOnPlacement(placement);
}
```

## 🎮 **Player Experience**

### **Clear Communication:**
- **Dynamic prompts** - Always shows the correct key
- **Consistent interaction** - Same key throughout the game
- **Intuitive controls** - Easy to remember and use

### **Accessibility:**
- **Customizable keys** - Players can use their preferred keys
- **Clear feedback** - Always know which key to press
- **Consistent behavior** - Predictable interaction system

## ✨ **Benefits**

### **For Developers:**
- **Flexible configuration** - Use any key you want
- **Easy setup** - Just change one field in inspector
- **Consistent system** - Works with existing code
- **Professional feel** - Polished interaction system

### **For Players:**
- **Clear instructions** - Always know what to press
- **Intuitive controls** - Easy to understand
- **Consistent experience** - Same interaction throughout
- **Accessible** - Can use preferred keys

## 🚀 **Quick Setup Checklist**

- [ ] Open Head Shrine Puzzle inspector
- [ ] Find "Shrine Settings" section
- [ ] Set "Interaction Key" to your preferred key
- [ ] Update "Place Head Text" if needed
- [ ] Test in play mode
- [ ] Verify prompt shows correct key
- [ ] Confirm interaction works with custom key

## 🎯 **Perfect Integration**

The configurable interaction key system works seamlessly with:
- **Existing placement system** - No code changes needed
- **Prompt system** - Automatically updates text
- **Audio system** - Works with existing sounds
- **Reward system** - No impact on functionality

## 🔧 **Troubleshooting**

### **Common Issues:**

#### **1. Key Not Working:**
- **Check**: Is the key set correctly in inspector?
- **Check**: Are there any conflicting input systems?
- **Check**: Is the key being used elsewhere in the game?

#### **2. Prompt Not Updating:**
- **Check**: Is the prompt text using {0} placeholder?
- **Check**: Is the TextMeshProUGUI assigned correctly?
- **Check**: Are there any console errors?

#### **3. Multiple Keys Not Working:**
- **Check**: Is the alternative keys array set up correctly?
- **Check**: Are the keys valid KeyCode values?
- **Check**: Is the input detection code working?

The configurable interaction key system makes the Head Shrine Puzzle much more flexible and professional!
