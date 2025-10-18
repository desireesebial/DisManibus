# Key Pickup System Setup Guide

## Problem Identified
Your key pickup system wasn't working because **no key GameObjects in your scene have the `KeyPickable` component attached**.

## Solution Steps

### Step 1: Add KeyPickable Component to Key GameObjects

For each key GameObject in your scene (like the "Right Key" shown in your image):

1. **Select the key GameObject** in the scene hierarchy
2. **Add the KeyPickable component**:
   - In the Inspector, click "Add Component"
   - Search for "KeyPickable" and add it
3. **Assign the KeySO ScriptableObject**:
   - In the KeyPickable component, drag your `RightKeySO` asset to the "Key Data" field
4. **Verify the GameObject has a Collider**:
   - The key should have a Box Collider (which it already has from your image)
   - Make sure the collider is enabled and not set to "Trigger"

### Step 2: Verify DullahanHeadInventory Setup

Check that your `DullahanHeadInventory` component has:

1. **Camera Reference**: 
   - The `cam` field should be assigned to your player's camera
   - If null, the system will try to auto-find it

2. **Player Reach**: 
   - Default is 3 units, adjust if needed
   - This determines how far the player can reach to pick up items

3. **Pickup Key**: 
   - Default is E key
   - Make sure this doesn't conflict with other systems

### Step 3: Key Visual Mapping (Optional)

If you want keys to show in the player's hand when selected:

1. In `DullahanHeadInventory`, find the "Key Visuals" section
2. Add entries to the `keyVisuals` list:
   - `key`: Reference to your KeySO asset
   - `keyObject`: GameObject to show in player's hand

### Step 4: Test the System

1. **Play the scene**
2. **Look at a key** (crosshair should be on it)
3. **Press E** to pick it up
4. **Check the Console** for debug messages

## Debug Messages

The enhanced system now provides detailed debug messages:

- **KeyPickable**: Logs when keys are ready and when picked up
- **DullahanHeadInventory**: Logs pickup attempts and inventory operations
- **Error Messages**: Clear error messages for missing components or data

## Common Issues and Solutions

### Issue: "No KeySO assigned"
**Solution**: Make sure you've dragged the KeySO asset to the "Key Data" field in the KeyPickable component.

### Issue: "Camera reference is null"
**Solution**: Assign the player's camera to the `cam` field in DullahanHeadInventory, or ensure the player GameObject has the "Player" tag.

### Issue: "Inventory is full"
**Solution**: Either increase `maxInventorySize` or remove some items from inventory.

### Issue: "No Collider found"
**Solution**: Add a Box Collider to the key GameObject if it doesn't have one.

### Issue: Keys not showing in hand
**Solution**: Set up the key visual mapping in DullahanHeadInventory's "Key Visuals" section.

## Example Setup

For the "Right Key" in your image:

1. Select the "Right Key" GameObject
2. Add Component → KeyPickable
3. Drag "RightKeySO" to the "Key Data" field
4. Ensure the Box Collider is enabled
5. Test in play mode

## Testing Checklist

- [ ] KeyPickable component added to key GameObject
- [ ] KeySO assigned to KeyPickable.keyData
- [ ] Key GameObject has enabled Collider
- [ ] DullahanHeadInventory has camera reference
- [ ] Player can see pickup prompt when looking at key
- [ ] Pressing E picks up the key
- [ ] Key appears in inventory UI
- [ ] Key can be selected and used

## Additional Notes

- The system uses raycast from screen center (crosshair), not mouse position
- Keys are destroyed when picked up (this is intentional)
- The system supports up to 3 items by default (configurable)
- Keys can be used with doors and puzzles that check for specific key IDs
