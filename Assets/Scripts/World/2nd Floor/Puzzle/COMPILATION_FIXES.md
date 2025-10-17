# Compilation Fixes - Dullahan Head Placement Puzzle

## Overview

This document contains fixes for common compilation errors that may occur when setting up the Dullahan Head Placement Puzzle system.

---

## Common Compilation Errors

### Error 1: "The type or namespace name 'TMPro' could not be found"

**Problem**: TextMeshPro namespace not found
**Solution**: 
1. Install TextMeshPro package:
   ```
   Window → Package Manager → Unity Registry → TextMeshPro → Install
   ```
2. Add using statement:
   ```csharp
   using TMPro;
   ```

### Error 2: "The type or namespace name 'DullahanHeadInventory' could not be found"

**Problem**: DullahanHeadInventory class not found
**Solution**:
1. Ensure DullahanHeadInventory.cs is in your project
2. Check the script is in the correct folder
3. Verify the class name matches exactly

### Error 3: "The type or namespace name 'DullahanHeadSO' could not be found"

**Problem**: DullahanHeadSO ScriptableObject not found
**Solution**:
1. Ensure DullahanHeadSO.cs is in your project
2. Check the script is in the correct folder
3. Verify the class name matches exactly

### Error 4: "The type or namespace name 'DullahanChaseSystem' could not be found"

**Problem**: DullahanChaseSystem class not found
**Solution**:
1. Ensure DullahanChaseSystem.cs is in your project
2. Check the script is in the correct folder
3. Verify the class name matches exactly

### Error 5: "The type or namespace name 'Floor2EndingEventManager' could not be found"

**Problem**: Floor2EndingEventManager class not found
**Solution**:
1. Ensure Floor2EndingEventManager.cs is in your project
2. Check the script is in the correct folder
3. Verify the class name matches exactly

---

## Missing Dependencies

### Required Scripts
Make sure these scripts are present in your project:
- `DullahanHeadInventory.cs`
- `DullahanHeadSO.cs`
- `DullahanChaseSystem.cs`
- `Floor2EndingEventManager.cs`
- `DullahanHeadPickable.cs`

### Required Unity Packages
Install these packages if missing:
- **TextMeshPro**: For UI text display
- **AI Navigation**: For NavMeshAgent (if using chase system)

---

## Quick Fix Checklist

### Before Compiling:
- [ ] All required scripts are in the project
- [ ] TextMeshPro package is installed
- [ ] All using statements are correct
- [ ] Class names match exactly
- [ ] Scripts are in correct folders

### After Compiling:
- [ ] No compilation errors in Console
- [ ] All scripts compile successfully
- [ ] No missing references in Inspector
- [ ] All components can be added to GameObjects

---

## Troubleshooting Steps

### Step 1: Check Console
1. Open Unity Console (Window → General → Console)
2. Look for compilation errors
3. Read error messages carefully
4. Note which scripts are missing

### Step 2: Verify Scripts
1. Check that all required scripts exist
2. Verify script names match exactly
3. Ensure scripts are in correct folders
4. Check for typos in class names

### Step 3: Check Dependencies
1. Verify all using statements are correct
2. Check that required packages are installed
3. Ensure all referenced classes exist
4. Verify namespace declarations

### Step 4: Reimport Scripts
1. Right-click on script folder
2. Select "Reimport"
3. Wait for Unity to recompile
4. Check Console for errors

---

## Common Solutions

### Solution 1: Install Missing Packages
```
Window → Package Manager → Unity Registry → [Package Name] → Install
```

### Solution 2: Fix Using Statements
```csharp
// Add these using statements at the top of your script
using UnityEngine;
using System.Collections;
using TMPro;
```

### Solution 3: Check Script Locations
Ensure scripts are in the correct folders:
```
Assets/Scripts/IntventorySystem/
├─ DullahanHeadInventory.cs
├─ DullahanHeadSO.cs
├─ DullahanChaseSystem.cs
└─ Floor2EndingEventManager.cs

Assets/Scripts/World/2nd Floor/Puzzle/
└─ SimpleHeadPlacement.cs
```

### Solution 4: Verify Class Names
Make sure class names match exactly:
```csharp
public class DullahanHeadInventory : MonoBehaviour
public class DullahanHeadSO : ScriptableObject
public class DullahanChaseSystem : MonoBehaviour
public class Floor2EndingEventManager : MonoBehaviour
```

---

## Prevention Tips

### Best Practices:
1. **Keep Scripts Organized**: Use consistent folder structure
2. **Use Exact Names**: Match class names exactly
3. **Install Packages Early**: Install required packages before coding
4. **Test Frequently**: Compile often to catch errors early
5. **Use Version Control**: Track changes to avoid losing scripts

### Common Mistakes to Avoid:
- Typos in class names
- Missing using statements
- Incorrect folder structure
- Missing Unity packages
- Inconsistent naming conventions

---

## Getting Help

### If You're Still Having Issues:
1. Check Unity Console for specific error messages
2. Verify all required scripts are present
3. Ensure all Unity packages are installed
4. Check for typos in class names
5. Verify script folder structure

### Additional Resources:
- Unity Documentation: https://docs.unity3d.com/
- TextMeshPro Documentation: https://docs.unity3d.com/Manual/com.unity.textmeshpro.html
- Unity Forums: https://forum.unity.com/

---

**This document should resolve most compilation issues. If you continue to have problems, check the specific error messages in Unity Console and verify all dependencies are correctly installed.**
