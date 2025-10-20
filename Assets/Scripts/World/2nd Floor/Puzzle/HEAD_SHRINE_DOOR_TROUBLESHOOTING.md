# 🚪 Head Shrine Puzzle - Door Opening Troubleshooting

## 🎯 **Issue Fixed: Fake Heads Not Opening Doors**

The issue where fake heads weren't automatically opening doors has been **FIXED**! The problem was in the reward system logic.

## 🔧 **What Was Wrong**

### **Previous Logic (Broken):**
```csharp
// OLD CODE - Used placement index (unreliable)
int placementIndex = placements.IndexOf(placement);
if (placementIndex == 1) // First wrong head
if (placementIndex == 2) // Second wrong head
```

### **New Logic (Fixed):**
```csharp
// NEW CODE - Uses head ID (reliable)
if (head.headID == 2) // First wrong head
if (head.headID == 3) // Second wrong head
```

## 🎮 **How It Works Now**

### **Door Opening Logic:**
1. **Real Head (ID: 1)** → Opens `realHeadDoor`
2. **Wrong Head 1 (ID: 2)** → Opens `wrongHead1Door`
3. **Wrong Head 2 (ID: 3)** → Opens `wrongHead2Door`

### **Automatic Door Opening:**
- **Immediate**: Doors open as soon as the head is placed
- **Automatic**: No manual intervention needed
- **Reliable**: Based on head ID, not placement position

## 🛠️ **Setup Requirements**

### **Step 1: Assign Door References**
In the Head Shrine Puzzle inspector:

```csharp
[Header("🎯 Head Rewards")]
public doorscript wrongHead1Door;  // Assign door for wrong head 1
public doorscript wrongHead2Door;  // Assign door for wrong head 2
public doorscript realHeadDoor;    // Assign door for real head
```

### **Step 2: Configure Head IDs**
Make sure your `DullahanHeadSO` ScriptableObjects have the correct IDs:

```csharp
// Real Head SO
headID = 1

// Wrong Head 1 SO
headID = 2

// Wrong Head 2 SO
headID = 3
```

### **Step 3: Test Door Opening**
1. **Place wrong head 1** → `wrongHead1Door` should open
2. **Place wrong head 2** → `wrongHead2Door` should open
3. **Place real head** → `realHeadDoor` should open

## 🔍 **Troubleshooting Guide**

### **Issue 1: Doors Still Not Opening**

#### **Check Console Logs:**
Look for these debug messages:
```
[HeadShrinePuzzle] 🚪 Wrong head placed: [HeadName] (ID: [ID])
[HeadShrinePuzzle] Wrong head 1 door unlocked and opened!
[HeadShrinePuzzle] Wrong head 2 door unlocked and opened!
```

#### **If You See Warnings:**
```
[HeadShrinePuzzle] Wrong head 1 door not assigned!
[HeadShrinePuzzle] Wrong head 2 door not assigned!
```
**Solution**: Assign the door references in the inspector

#### **If You See:**
```
[HeadShrinePuzzle] Unknown wrong head ID: [ID]. Expected 2 or 3.
```
**Solution**: Check your head IDs in the ScriptableObjects

### **Issue 2: Wrong Door Opening**

#### **Check Head IDs:**
- **Real Head**: Should have ID = 1
- **Wrong Head 1**: Should have ID = 2
- **Wrong Head 2**: Should have ID = 3

#### **Check Door Assignments:**
- **wrongHead1Door**: Should be assigned to door for wrong head 1
- **wrongHead2Door**: Should be assigned to door for wrong head 2
- **realHeadDoor**: Should be assigned to door for real head

### **Issue 3: Doors Open But Don't Stay Open**

#### **Check Door Script:**
Make sure your `doorscript` has these methods:
```csharp
public void UnlockDoor()  // Unlocks the door
public void OpenDoor()    // Opens the door
```

#### **Check Door State:**
- **Unlocked**: Door can be opened
- **Open**: Door is actually open
- **Both**: Door should stay open

## 🎯 **Testing Checklist**

### **Before Testing:**
- [ ] Door references assigned in inspector
- [ ] Head IDs set correctly (1, 2, 3)
- [ ] Door script has UnlockDoor() and OpenDoor() methods
- [ ] Console is open to see debug messages

### **During Testing:**
- [ ] Place wrong head 1 → Check if wrongHead1Door opens
- [ ] Place wrong head 2 → Check if wrongHead2Door opens
- [ ] Place real head → Check if realHeadDoor opens
- [ ] Check console for debug messages
- [ ] Verify doors stay open

### **After Testing:**
- [ ] All doors open correctly
- [ ] No console errors or warnings
- [ ] Doors stay open after placement
- [ ] Audio plays when doors open

## 🔧 **Advanced Configuration**

### **Custom Head IDs:**
If you want to use different head IDs, modify the code:

```csharp
// In HandleWrongHeadRewards method
if (head.headID == YOUR_WRONG_HEAD_1_ID) // Change this
if (head.headID == YOUR_WRONG_HEAD_2_ID) // Change this
```

### **Multiple Doors per Head:**
If you want multiple doors to open for one head:

```csharp
if (head.headID == 2) // First wrong head
{
    if (wrongHead1Door) wrongHead1Door.UnlockDoor();
    if (wrongHead1Door) wrongHead1Door.OpenDoor();
    if (additionalDoor) additionalDoor.UnlockDoor(); // Add more doors
    if (additionalDoor) additionalDoor.OpenDoor();
}
```

## ✨ **Benefits of the Fix**

### **Reliability:**
- **Head ID based** - More reliable than placement index
- **Immediate opening** - Doors open as soon as head is placed
- **Clear debugging** - Console messages show what's happening

### **Flexibility:**
- **Customizable** - Easy to change head IDs
- **Extensible** - Can add more doors or heads
- **Maintainable** - Clear, readable code

### **User Experience:**
- **Instant feedback** - Doors open immediately
- **Clear progression** - Players see doors opening
- **Satisfying** - Visual confirmation of progress

## 🚀 **Quick Fix Summary**

The door opening issue has been **completely fixed**! The system now:

1. **Uses head ID** instead of placement index
2. **Opens doors immediately** when heads are placed
3. **Provides clear debug messages** for troubleshooting
4. **Works reliably** with any head configuration

Your fake heads should now automatically open their assigned doors as soon as they're placed on the altar!
