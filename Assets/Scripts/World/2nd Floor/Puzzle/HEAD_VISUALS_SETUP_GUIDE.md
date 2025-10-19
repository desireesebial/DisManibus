# 🎭 Head Visuals Setup Guide

## 🎯 **Overview**
This guide shows you how to set up the head visuals for your Head Shrine Puzzle so players can see the actual head models when they place them on the altar.

## 🛠️ **Step-by-Step Setup**

### **Step 1: Create Head Prefabs**

#### **For Each Head Type:**
1. **Find your head models** in the project
2. **Create prefabs** for each head type:
   - `RealHead.prefab`
   - `WrongHead1.prefab` 
   - `WrongHead2.prefab`

#### **Head Prefab Setup:**
```
Head Prefab Structure:
├── Head Model (3D mesh)
├── Materials (textures)
├── Collider (for interaction)
├── Rigidbody (for physics)
└── DullahanHeadPickable (script)
```

### **Step 2: Configure Head Prefabs**

#### **Required Components:**
- **Mesh Renderer** - Shows the head model
- **Collider** - For interaction (will be removed when placed)
- **Rigidbody** - For physics (will be removed when placed)
- **DullahanHeadPickable** - For pickup logic (will be removed when placed)

#### **Optional Components:**
- **Audio Source** - For head sounds
- **Particle System** - For head effects
- **Light** - For head glow

### **Step 3: Assign Head Prefabs to ScriptableObjects**

#### **Open Each DullahanHeadSO:**
1. **Real Head SO** → Assign `RealHead.prefab` to `headPrefab` field
2. **Wrong Head 1 SO** → Assign `WrongHead1.prefab` to `headPrefab` field  
3. **Wrong Head 2 SO** → Assign `WrongHead2.prefab` to `headPrefab` field

#### **ScriptableObject Setup:**
```csharp
[CreateAssetMenu(fileName = "New Dullahan Head", menuName = "Dullahan/Head")]
public class DullahanHeadSO : ScriptableObject
{
    public int headID;
    public string headName;
    public GameObject headPrefab; // ← Assign your head prefab here
    public DullahanHeadEffect[] effects;
}
```

### **Step 4: Test Head Placement**

#### **In Play Mode:**
1. **Pick up a head** from the world
2. **Approach the shrine** - you should see the placement prompt
3. **Press F** to place the head
4. **Check the altar** - the head model should appear on the placement point

#### **What You Should See:**
- **Head model spawns** on the correct placement point
- **Head is positioned** slightly above the placement (0.3f units up)
- **Head is scaled** to 80% of original size
- **Head is non-interactive** (can't be picked up again)

## 🎨 **Visual Customization**

### **Head Model Positioning:**
```csharp
// In HeadShrinePuzzle.cs - PlaceHeadOnPlacement method
headModel.transform.localPosition = new Vector3(0, 0.3f, 0); // Height above placement
headModel.transform.localRotation = Quaternion.identity;    // Rotation
headModel.transform.localScale = Vector3.one * 0.8f;       // Scale (80%)
```

### **Adjust Head Position:**
- **Height**: Change `0.3f` to adjust how high the head sits
- **Scale**: Change `0.8f` to make heads bigger/smaller
- **Rotation**: Modify if heads need to face a specific direction

### **Head Model Requirements:**
- **3D Mesh**: The actual head geometry
- **Materials**: Proper textures and materials
- **Appropriate Size**: Not too big or small for the altar
- **Good Proportions**: Looks natural on the placement point

## 🔧 **Troubleshooting**

### **Common Issues:**

#### **1. Head Model Not Appearing:**
- **Check**: Is `headPrefab` assigned in the ScriptableObject?
- **Check**: Does the head prefab have a Mesh Renderer?
- **Check**: Are there any errors in the console?

#### **2. Head Model in Wrong Position:**
- **Adjust**: `localPosition` in the code
- **Adjust**: `localScale` for size
- **Adjust**: `localRotation` for orientation

#### **3. Head Model Too Big/Small:**
- **Change**: `localScale = Vector3.one * 0.8f` (adjust the 0.8f value)
- **Or**: Resize the head prefab itself

#### **4. Head Model Can Still Be Picked Up:**
- **Check**: `CleanupHeadComponents` method is working
- **Check**: DullahanHeadPickable component is being removed

## 🎮 **Testing Checklist**

### **Before Testing:**
- [ ] Head prefabs created
- [ ] Head prefabs assigned to ScriptableObjects
- [ ] Head Shrine Puzzle configured
- [ ] Placement points set up

### **During Testing:**
- [ ] Pick up a head
- [ ] Approach shrine
- [ ] See placement prompt
- [ ] Press F to place head
- [ ] See head model appear on altar
- [ ] Verify head can't be picked up again
- [ ] Check head positioning looks good

### **After Testing:**
- [ ] All head types work correctly
- [ ] Head models appear in correct positions
- [ ] Head models are properly scaled
- [ ] Head models are non-interactive
- [ ] Visual feedback is clear

## ✨ **Pro Tips**

### **Head Model Design:**
- **Keep it simple** - Don't over-complicate the head models
- **Good materials** - Use appropriate textures and materials
- **Proper scale** - Make sure heads fit well on the altar
- **Clear visibility** - Players should easily see what they placed

### **Performance:**
- **Optimize meshes** - Don't use overly complex head models
- **Efficient materials** - Use mobile-friendly shaders if needed
- **LOD groups** - Consider level-of-detail for complex heads

### **Visual Polish:**
- **Consistent style** - All heads should have similar visual style
- **Good lighting** - Make sure heads are visible in the scene
- **Proper positioning** - Heads should sit naturally on the altar

## 🎯 **Final Result**

When everything is set up correctly:
- **Players can see** the exact head they placed
- **Visual confirmation** of their actions
- **Progress tracking** through visible head models
- **Professional feel** with polished visuals

The head visuals system makes the Head Shrine Puzzle much more engaging and satisfying for players!
