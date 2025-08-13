# Crosshair Troubleshooting Guide

If your crosshair is not appearing, follow these steps to fix it:

## 🔧 **Step-by-Step Fix**

### **Step 1: Use the Working Crosshair Script**
1. **Delete the old scripts**: Remove `Crosshair.cs` and `SimpleCrosshair.cs` from your GameObject
2. **Add the new script**: Attach `WorkingCrosshair.cs` to your player GameObject or camera
3. **Check the Console**: Look for debug messages starting with "WorkingCrosshair:"

### **Step 2: Verify Script Attachment**
1. **Select your player GameObject** in the Hierarchy
2. **Check the Inspector**: You should see the `WorkingCrosshair` component
3. **Ensure it's enabled**: The checkbox next to the component name should be checked

### **Step 3: Check Console for Debug Messages**
1. **Open Console**: Window → General → Console
2. **Look for these messages**:
   - ✅ "WorkingCrosshair: Starting initialization..."
   - ✅ "WorkingCrosshair: Crosshair created successfully!"
   - ❌ Any error messages in red

### **Step 4: Force Recreate the Crosshair**
1. **Play the scene**
2. **Press Y key** to force recreate the crosshair
3. **Check console** for recreation messages

### **Step 5: Test with the Test Script**
1. **Add CrosshairTest.cs** to the same GameObject as WorkingCrosshair
2. **Play the scene**
3. **Use these keys to test**:
   - **C** - Toggle visibility
   - **R** - Change color
   - **T** - Change size
   - **Y** - Recreate crosshair

## 🚨 **Common Issues & Solutions**

### **Issue: "No WorkingCrosshair component found"**
**Solution**: Make sure `WorkingCrosshair.cs` is attached to the same GameObject as `CrosshairTest.cs`

### **Issue: Canvas not found**
**Solution**: The script automatically creates a Canvas. Check if there are any Canvas-related errors in the console.

### **Issue: Crosshair created but not visible**
**Solution**: 
1. Check if the Canvas is active in the Hierarchy
2. Ensure the Canvas is set to "Screen Space - Overlay"
3. Check if the crosshair object is active

### **Issue: Crosshair appears in wrong position**
**Solution**: The crosshair should automatically center itself. If not, check Canvas Scaler settings.

## 📋 **Complete Setup Checklist**

- [ ] `WorkingCrosshair.cs` attached to player GameObject or camera
- [ ] `CrosshairTest.cs` attached to the same GameObject (optional, for testing)
- [ ] Console shows "Crosshair created successfully!" message
- [ ] Canvas is visible in Hierarchy
- [ ] Canvas is set to "Screen Space - Overlay"
- [ ] No error messages in Console

## 🎯 **Quick Test Method**

1. **Create a new empty GameObject** in your scene
2. **Name it "CrosshairTest"**
3. **Attach both scripts**:
   - `WorkingCrosshair.cs`
   - `CrosshairTest.cs`
4. **Play the scene**
5. **Check console** for debug messages
6. **Use test keys** to verify functionality

## 🔍 **Debug Information**

The `WorkingCrosshair` script includes extensive debug logging:
- **Startup messages**: Shows each step of creation
- **Canvas status**: Reports if using existing or creating new Canvas
- **Object status**: Shows if crosshair object is active
- **Error handling**: Catches and reports any exceptions

## 📱 **Mobile/VR Considerations**

- **Canvas Scaler**: Automatically set to "Scale With Screen Size"
- **Reference Resolution**: Set to 1920x1080 (can be adjusted)
- **Sorting Order**: Set to 100 to ensure it appears above other UI

## 🆘 **Still Not Working?**

If the crosshair still doesn't appear after following all steps:

1. **Check Unity version**: Ensure you're using Unity 2019.4 or later
2. **Verify UI package**: Make sure the UI package is installed
3. **Restart Unity**: Sometimes a restart fixes UI issues
4. **Check for conflicts**: Ensure no other scripts are interfering with UI creation

## 📞 **Get Help**

Include these details when asking for help:
- Unity version
- Console output (copy all WorkingCrosshair messages)
- Screenshot of Inspector showing WorkingCrosshair component
- Screenshot of Hierarchy showing Canvas structure
