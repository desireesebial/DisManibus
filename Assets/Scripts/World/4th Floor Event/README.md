# 4th Floor Kamatayan Hallucination Event 👻

## Quick Setup Guide

This script creates a creepy hallucination effect where Kamatayan stands still, waiting for the player. When the player sees it (gets close or looks at it), it disappears like a hallucination - perfect for psychological horror!

### Setup Steps:

1. **Place Kamatayan:**
   - Position the Kamatayan GameObject where you want it to stand and wait
   - This will be the hallucination spot

2. **Add the Script:**
   - Add the `KamatayanHallucinationEffect` component directly to the Kamatayan GameObject

3. **Configure Detection (Auto-finds Player):**
   - **Detection Range:** How close the player needs to be (default: 15m)
   - **Require Line of Sight:** Check if player needs to actually look at Kamatayan to make it disappear
   - **Player View Angle:** Field of view angle for detection (default: 45°)

4. **Configure Disappear Effect:**
   - **Fade Out Time:** How long it takes to fade out (0.5s recommended for creepy effect)
   - **Initial Delay:** Wait time before detection starts (0.5s)

5. **What Happens After:**
   - **Permanent Disappear:** Check to make it disappear forever (recommended for one-time scares)
   - **Reappear Delay:** If not permanent, how long before it shows up again
   - **Enable Roaming After Reappear:** Should it start roaming with AI after reappearing?

6. **Optional - Add Sound Effects:**
   - Use the **OnHallucinationFade** event to trigger creepy sounds when it starts disappearing
   - Use the **OnHallucinationGone** event to trigger sounds when fully gone
   - Example: Play whisper sounds, heartbeat, or unsettling music

### How It Works:

- Kamatayan will stand completely still at the position you placed it
- When the player gets within detection range, it starts checking
- If "Require Line of Sight" is enabled, it only triggers when player looks at it
- When detected, it fades out like a ghost and disappears
- Perfect for "did I just see that?" moments! 😱

### Tips for Maximum Creepiness:

- Place it at the end of a hallway where player will naturally look
- Set detection range to 10-15m so it disappears just as player notices it
- Use "Permanent Disappear" so player questions if it was real
- Add a subtle sound effect when it fades (breathing, whisper, etc.)
- Place it in dim lighting for that extra horror factor

### Debug Visualization:

When selected in the editor:
- Red wireframe sphere = detection range
- Yellow cone = player's field of view (if line of sight enabled)
- Red line = shows when player is in range

### That's it! 👻

No triggers needed, no complex setup. Just place it, add the script, and watch players freak out!


