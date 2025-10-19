# 🎭 Single Head Model Setup Guide

## 🎯 **Overview**
This guide shows you how to set up the Head Shrine Puzzle when you're using **one head model/prefab** for all Dullahan heads instead of separate prefabs for each head type.

## 🛠️ **Setup with One Head Model**

### **Step 1: Create One Head Prefab**

#### **Create a Single Head Prefab:**
1. **Find your head model** in the project
2. **Create one prefab** called `DullahanHead.prefab`
3. **Use this same prefab** for all head types

#### **Head Prefab Structure:**
```
DullahanHead.prefab
├── Head Model (3D mesh)
├── Materials (textures)
├── Collider (for interaction)
├── Rigidbody (for physics)
└── DullahanHeadPickable (script)
```

### **Step 2: Assign Same Prefab to All ScriptableObjects**

#### **For Each DullahanHeadSO:**
1. **Real Head SO** → Assign `DullahanHead.prefab` to `headPrefab` field
2. **Wrong Head 1 SO** → Assign `DullahanHead.prefab` to `headPrefab` field  
3. **Wrong Head 2 SO** → Assign `DullahanHead.prefab` to `headPrefab` field

#### **All ScriptableObjects Use Same Prefab:**
```csharp
// Real Head SO
headPrefab = DullahanHead.prefab

// Wrong Head 1 SO  
headPrefab = DullahanHead.prefab

// Wrong Head 2 SO
headPrefab = DullahanHead.prefab
```

### **Step 3: Test Head Placement**

#### **In Play Mode:**
1. **Pick up any head** from the world
2. **Approach the shrine** - you should see the placement prompt
3. **Press F** to place the head
4. **Check the altar** - the same head model should appear on the placement point

#### **What You Should See:**
- **Same head model** appears on all placement points
- **Head is positioned** correctly on the altar
- **Head is scaled** appropriately
- **Head is non-interactive** (can't be picked up again)

## 🎨 **Visual Differentiation (Optional)**

### **If You Want Different Visuals for Each Head:**

#### **Option 1: Different Materials**
- **Keep the same head model** but use different materials
- **Assign different materials** to each ScriptableObject
- **Modify the placement code** to apply the material when spawning

#### **Option 2: Different Colors**
- **Use the same head model** but change colors
- **Apply different colors** when spawning heads
- **Use material property blocks** for color variation

#### **Option 3: Different Scales**
- **Use the same head model** but different scales
- **Modify the scale** based on head type
- **Adjust positioning** accordingly

## 🔧 **Code Modifications (If Needed)**

### **If You Want Different Visuals:**

#### **Modify PlaceHeadOnPlacement Method:**
```csharp
void PlaceHeadOnPlacement(DullahanHeadSO head, ShrinePlacement placement)
{
    // Spawn the head model
    if (head.headPrefab != null)
    {
        GameObject headModel = Instantiate(head.headPrefab, placement.placementTransform);
        headModel.transform.localPosition = new Vector3(0, 0.3f, 0);
        headModel.transform.localRotation = Quaternion.identity;
        headModel.transform.localScale = Vector3.one * 0.8f;
        
        // Apply different visuals based on head type
        ApplyHeadVisuals(headModel, head);
        
        // Store reference
        placement.placedHead = headModel;
    }
}

void ApplyHeadVisuals(GameObject headModel, DullahanHeadSO head)
{
    // Example: Different colors for different heads
    Renderer renderer = headModel.GetComponent<Renderer>();
    if (renderer != null)
    {
        switch (head.headID)
        {
            case 1: // Real head
                renderer.material.color = Color.gold;
                break;
            case 2: // Wrong head 1
                renderer.material.color = Color.red;
                break;
            case 3: // Wrong head 2
                renderer.material.color = Color.blue;
                break;
        }
    }
}
```

## 🎮 **Testing with One Head Model**

### **What to Expect:**
- **Same visual appearance** for all heads on the altar
- **Different behavior** based on head type (rewards, effects)
- **Consistent positioning** and scaling
- **Proper cleanup** of interactive components

### **Testing Checklist:**
- [ ] One head prefab created
- [ ] Same prefab assigned to all ScriptableObjects
- [ ] Head Shrine Puzzle configured
- [ ] Test placing different head types
- [ ] Verify all heads appear correctly
- [ ] Check that heads can't be picked up again

## ✨ **Benefits of One Head Model**

### **Advantages:**
- **Simpler setup** - Only one prefab to manage
- **Consistent visuals** - All heads look the same
- **Easier maintenance** - Only one model to update
- **Better performance** - Less memory usage
- **Faster development** - No need to create multiple models

### **When to Use:**
- **Prototype phase** - Quick setup for testing
- **Consistent design** - All heads should look the same
- **Performance focus** - Minimize memory usage
- **Simple puzzle** - Visual differentiation not needed

## 🎯 **Alternative: Multiple Head Models**

### **If You Want Different Head Models Later:**
1. **Create separate prefabs** for each head type
2. **Assign different prefabs** to each ScriptableObject
3. **Keep the same placement logic** - no code changes needed

### **Hybrid Approach:**
- **Use one base head model** for all heads
- **Apply different materials/colors** for visual differentiation
- **Keep the same prefab** but vary the appearance

## 🚀 **Quick Setup Summary**

### **For One Head Model:**
1. **Create one head prefab** (`DullahanHead.prefab`)
2. **Assign it to all ScriptableObjects** (same prefab for all)
3. **Test in play mode** - all heads should appear the same
4. **Optional**: Add visual differentiation if needed

### **Result:**
- **All heads look the same** on the altar
- **Different behavior** based on head type
- **Consistent visual experience** for players
- **Simple and efficient** setup

The single head model approach is perfect for many games and provides a clean, consistent visual experience!
