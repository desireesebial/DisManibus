# 🔥 Sequential Torch Puzzle Flow Diagram

## 📊 Puzzle Flow Visualization

```
┌─────────────────────────────────────────────────────────────────┐
│                    SEQUENTIAL TORCH PUZZLE FLOW                 │
└─────────────────────────────────────────────────────────────────┘

🎮 PLAYER ACTIONS                    🔥 TORCH STATES                    🎯 PUZZLE MANAGER
─────────────────                    ───────────────                    ──────────────────

Player approaches Torch 1            Torch 1: READY (yellow glow)      Manager: Initialize puzzle
     ↓                                      ↓                           ↓
Press F to light Torch 1              Torch 1: LIT (flame + light)     Manager: OnTorchLit(1)
     ↓                                      ↓                           ↓
Player approaches Torch 2            Torch 2: READY (yellow glow)      Manager: Set next torch ready
     ↓                                      ↓                           ↓
Press F to light Torch 2              Torch 2: LIT (flame + light)     Manager: OnTorchLit(2)
     ↓                                      ↓                           ↓
Player approaches Torch 3            Torch 3: READY (yellow glow)      Manager: Set next torch ready
     ↓                                      ↓                           ↓
Press F to light Torch 3              Torch 3: LIT (flame + light)     Manager: OnTorchLit(3)
     ↓                                      ↓                           ↓
...continue for all torches...        ...all torches LIT...             Manager: Check completion
     ↓                                      ↓                           ↓
All torches lit in sequence           All torches: LIT + celebration    Manager: CompletePuzzle()
     ↓                                      ↓                           ↓
🎉 PUZZLE COMPLETE!                  🎆 Visual effects + audio          🚪 Door unlocks + rewards
```

## 🔄 Wrong Sequence Flow

```
┌─────────────────────────────────────────────────────────────────┐
│                    WRONG SEQUENCE FLOW                          │
└─────────────────────────────────────────────────────────────────┘

Player approaches Torch 3            Torch 3: NOT READY (no glow)      Manager: CanLightTorch(3) = false
     ↓                                      ↓                           ↓
Press F to light Torch 3              Torch 3: RED FLASH               Manager: OnWrongSequenceAttempted(3)
     ↓                                      ↓                           ↓
❌ WRONG SEQUENCE!                   🔴 Visual feedback + sound         Manager: Start reset timer
     ↓                                      ↓                           ↓
Wait 3 seconds...                    All torches: RESET to unlit        Manager: ResetPuzzle()
     ↓                                      ↓                           ↓
Puzzle resets                         Torch 1: READY again              Manager: Initialize puzzle
     ↓                                      ↓                           ↓
Start over from Torch 1              Player must light in correct order Manager: Ready for new attempt
```

## 🎨 Visual State Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                    TORCH VISUAL STATES                          │
└─────────────────────────────────────────────────────────────────┘

UNLIT STATE                         READY STATE                        LIT STATE
────────────                        ───────────                        ──────────

🔘 Dark torch base                  🟡 Yellow glow                     🔥 Orange flame
🔘 No flame visible                 🟡 Ready particles                 🔥 Fire particles
🔘 No light emission                🟡 Subtle pulsing                  🔥 Bright light
🔘 No interaction prompt            🟡 "Press F to light torch"        🔥 No interaction needed
🔘 Cannot be lit                    🟡 Can be lit                      🔥 Already lit
```

## 🎵 Audio Flow

```
┌─────────────────────────────────────────────────────────────────┐
│                        AUDIO SEQUENCE                           │
└─────────────────────────────────────────────────────────────────┘

Torch becomes ready:                🔔 Ready sound (subtle chime)
     ↓
Player lights torch:                🔥 Light sound (whoosh + crackle)
     ↓
Wrong sequence:                     ❌ Wrong sound (buzz + error)
     ↓
Puzzle resets:                      🔄 Reset sound (mechanical)
     ↓
Puzzle completes:                   🎉 Completion sound (triumphant)
     ↓
Celebration:                        🎆 Victory music + effects
```

## 🎯 Manager State Machine

```
┌─────────────────────────────────────────────────────────────────┐
│                    PUZZLE MANAGER STATES                        │
└─────────────────────────────────────────────────────────────────┘

INITIALIZE                          IN_PROGRESS                        COMPLETED
──────────                          ───────────                        ──────────

• Sort torches by sequence          • Track current sequence index     • All torches lit
• Reset all torches to unlit        • Enable next torch when ready     • Play completion effects
• Make first torch ready            • Handle wrong sequence attempts   • Unlock door
• Clear lit torches list            • Update visual feedback           • Spawn rewards
• Set puzzle incomplete             • Check for completion             • Start celebration
• Hide completion effects           • Manage reset timer               • Mark puzzle complete
```

## 🔧 Component Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                    COMPONENT RELATIONSHIPS                      │
└─────────────────────────────────────────────────────────────────┘

SequentialTorchManager (1)          SequentialTorch (N)
─────────────────────────           ──────────────────
• Manages entire puzzle             • Individual torch behavior
• Tracks sequence progress          • Handles player interaction
• Controls torch readiness          • Visual/audio feedback
• Handles completion                • Notifies manager of events
• Manages rewards                   • Manages torch state
• Debug tools                       • Distance checking
```

## 🎮 Player Experience Flow

```
┌─────────────────────────────────────────────────────────────────┐
│                    PLAYER EXPERIENCE JOURNEY                    │
└─────────────────────────────────────────────────────────────────┘

1. DISCOVERY                       2. UNDERSTANDING                 3. EXECUTION
   ─────────                          ────────────                     ──────────
   
   • Enter puzzle room               • Notice first torch glows         • Light torch 1
   • See multiple torches            • Try to light other torches       • See torch 2 become ready
   • Notice some are dark            • Realize sequence is required     • Light torch 2
   • Some have yellow glow           • Understand lighting order        • Continue sequence
   • See interaction prompts         • Plan lighting strategy           • Light all torches
   
4. COMPLETION                       5. REWARD
   ──────────                         ────────
   
   • All torches lit                 • Door unlocks
   • Visual celebration              • Rewards spawn
   • Audio fanfare                   • New area accessible
   • Sense of achievement            • Progress to next challenge
```

## 🐛 Debug Flow

```
┌─────────────────────────────────────────────────────────────────┐
│                        DEBUG TOOLS                              │
└─────────────────────────────────────────────────────────────────┘

Context Menu Commands:              Console Logs:                      Gizmos:
─────────────────────               ──────────────                     ───────
• Reset Puzzle                      • [SequentialTorch] messages       • Interaction spheres
• Complete Puzzle                   • [SequentialTorchManager] logs    • Sequence connections
                                   • State changes                     • Reward spawn points
                                   • Error messages                    • Visual debugging
```

This flow diagram shows the complete lifecycle of the sequential torch lighting puzzle, from initialization through completion, including error handling and player feedback systems.
