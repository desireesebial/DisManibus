# Quick Debug Guide - Keypad Freeze Issues

## If the Game Still Freezes

Follow these steps in order:

### Step 1: Check Unity Console
Look for these messages in order:
```
[KeyPad] Initialized with code length: 4, Correct code: 1234
[KeyPad] Correct code entered! Processing...
[KeyPad] Unlocking door: Elevator_Door
[KeyPad] Starting door open coroutine...
[KeyPad] OpenDoorAfterDelay started. Waiting 0.5 seconds...
[KeyPad] Delay complete. Attempting to open door...
[KeyPad] Toggling door Elevator_Door...
[Door] Door Elevator_Door opened!
```

**If you see "Animation timeout" message:**
- Your `openSpeed` is too low
- Increase it to 5 or higher in the Inspector

**If you see "Invalid CodeLength" error:**
- Fix the `Code Length` field in KeyPad Inspector
- Must be a number like "4" not "four"

**If logging stops at a certain point:**
- The line before it is where the freeze happens
- Report which line it stopped at

### Step 2: Verify Inspector Settings

#### On KeyPadScript Component:
```
Code Length: "4"  (or your preferred length)
Correct: "1234"   (must match length)
Target Door: [Assigned to door GameObject]
✓ Unlock Door On Success
✓ Open Door After Unlock
```

#### On doorscript Component (on the door):
```
Open Angle: 90
Open Speed: 2 (or higher)
Is Locked: ✓ (checked)
Required Key ID: -1 (if using keypad)
```

### Step 3: Check GameObject States

1. Find your door GameObject (e.g., "Elevator_Door")
2. Check if it's **active** (checkbox next to name should be checked)
3. If inactive, either:
   - Activate it in the scene, OR
   - Let the script activate it automatically (it will log this)

### Step 4: Test on Original PC First

Before testing on other PCs:
1. Enter WRONG code first - should reset
2. Enter CORRECT code - should unlock and open
3. Check Console for all expected log messages
4. No errors should appear

### Step 5: When Testing on Other PC

If it works on your PC but freezes on another:

**Possible causes:**
- Different Unity version
- Different frame rate (very fast/slow PC)
- Different Time.deltaTime behavior
- Corrupted build

**Solutions:**
1. Build from that PC directly
2. Check the `openSpeed` value - may need adjustment per PC
3. Enable VSync in Player Settings
4. Check the Player.log file on the frozen PC

### Quick Fixes

#### Fix 1: Force Door Active
In Unity hierarchy, make sure Elevator_Door (or your door) GameObject is **ACTIVE** before playing.

#### Fix 2: Increase openSpeed
If animation is slow or freezing:
- Select door GameObject
- Find doorscript component
- Set `Open Speed` to 5 or 10

#### Fix 3: Remove Linked Doors
If you have linked doors:
- Temporarily set `Linked Doors` array size to 0
- Test if freeze still happens
- If fixed, issue is with linked doors setup

#### Fix 4: Check for Circular References
Make sure:
- Door A doesn't link to Door B
- While Door B also links to Door A
- This creates infinite recursion

### Emergency Bypass

If nothing works, use this temporary code to test:

In KeyPadScript.cs, replace OnCorrectCodeEntered() with:
```csharp
private void OnCorrectCodeEntered()
{
    Debug.Log("[EMERGENCY] Correct code entered!");
    
    if (targetDoor != null)
    {
        targetDoor.gameObject.SetActive(true);
        targetDoor.isLocked = false;
        targetDoor.ForceOpen(); // Instant open, no animation
    }
}
```

This will:
- Skip all animations
- Skip all coroutines
- Instantly open the door

If this works but normal code doesn't, the issue is in the animation system.

## Log File Locations

### In Unity Editor:
```
Windows: C:\Users\[Username]\AppData\Local\Unity\Editor\Editor.log
```

### In Built Game:
```
Windows: C:\Users\[Username]\AppData\LocalLow\[CompanyName]\[GameName]\Player.log
```

Look for error messages or stack traces.

## Contact Information

If still having issues, provide:
1. Unity Console output (all [KeyPad] and [Door] messages)
2. Inspector screenshot of KeyPadScript
3. Inspector screenshot of doorscript
4. Which PC it works on vs which freezes
5. Where in the log sequence it stops

## Common Error Messages

| Error Message | Meaning | Fix |
|--------------|---------|-----|
| "Coroutine couldn't be started" | GameObject is inactive | Should auto-fix now; check logs |
| "Invalid CodeLength" | Bad code length value | Set to valid number in Inspector |
| "Animation timeout" | Animation stuck | Increase openSpeed |
| "Target door is null" | Door not assigned | Assign door in Inspector |
| "Zero deltaTime detected" | Frame timing issue | Should auto-handle; if persists, enable VSync |

## Performance Monitoring

To check if it's a performance freeze vs. infinite loop:

1. Open Unity Profiler (Window > Analysis > Profiler)
2. Run game
3. Enter correct code
4. Watch CPU usage
5. If CPU spikes to 100% on one core = infinite loop
6. If CPU normal but game frozen = deadlock

Report findings for further debugging.

