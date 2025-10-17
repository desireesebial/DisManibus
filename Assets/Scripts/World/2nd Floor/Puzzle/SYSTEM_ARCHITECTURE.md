# Dullahan Head Placement Puzzle - System Architecture

## Overview Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                    Dullahan Head Placement System                │
└─────────────────────────────────────────────────────────────────┘

┌──────────────────────────┐         ┌──────────────────────────┐
│   DullahanHeadInventory  │◄────────┤      Player              │
│   (Existing System)      │         │   - PlayerController     │
│                          │         │   - Camera               │
│   - GetCurrentHead()     │         │   - Input                │
│   - HasHeads()           │         └──────────────────────────┘
│   - RemoveSelectedHead() │                     │
└──────────────────────────┘                     │
            ▲                                    │
            │                                    │
            │                                    ▼
            │                    ┌─────────────────────────────┐
            │                    │ DullahanHeadPlacementPuzzle │
            │                    │   (Main Controller)         │
            │                    ├─────────────────────────────┤
            │                    │ • Check player distance     │
            │                    │ • Raycast interaction       │
            │                    │ • Get selected head         │
            │                    │ • Validate head type        │
            │                    │ • Handle placement          │
            │                    │ • Complete puzzle           │
            │                    │ • Grant rewards             │
            │                    └─────────────────────────────┘
            │                        │         │         │
            └────────────────────┘         │         └────────────────┐
                    ▼                          ▼                          ▼
        ┌────────────────────┐   ┌──────────────────────┐   ┌────────────────────┐
        │ HeadPlaceholder    │   │  Visual Effects      │   │  Event Managers    │
        │  (Visual Control)  │   │                      │   │                    │
        ├────────────────────┤   ├──────────────────────┤   ├────────────────────┤
        │ • Show/Hide        │   │ • Lights             │   │ • Floor2EventMgr   │
        │ • Fade effects     │   │ • Particles          │   │ • ChaseEventMgr    │
        │ • Pulsing          │   │ • Audio              │   │ • OnCompletion()   │
        │ • Material switch  │   │ • Animations         │   └────────────────────┘
        │ • State management │   └──────────────────────┘
        └────────────────────┘
                    │
                    ▼
        ┌────────────────────┐
        │   Materials        │
        │                    │
        │ • Empty (white)    │
        │ • Valid (green)    │
        │ • Invalid (red)    │
        └────────────────────┘
```

## Component Relationships

### Core Components

```
DullahanHeadPlacementPuzzle
├─ Has Reference To
│  ├─ HeadPlaceholder (GameObject)
│  ├─ HeadAttachmentPoint (Transform)
│  ├─ AttachedHeadVisual (GameObject)
│  ├─ CompletionLight (Light)
│  ├─ ParticleSystems (ParticleSystem[])
│  ├─ AudioSource (AudioSource)
│  ├─ InteractionUI (GameObject)
│  └─ RewardDoor (Door)
│
├─ Finds Automatically
│  ├─ Player (GameObject with tag "Player")
│  ├─ PlayerCamera (Camera)
│  ├─ DullahanHeadInventory (FindObjectOfType)
│  ├─ Floor2EndingEventManager (FindObjectOfType)
│  └─ DullahanChaseEventManager (FindObjectOfType)
│
└─ Manages
   ├─ Interaction state
   ├─ Puzzle completion
   ├─ Head placement
   └─ Reward distribution
```

### HeadPlaceholder Component

```
HeadPlaceholder
├─ Has Reference To
│  ├─ PlaceholderRenderer (Renderer)
│  ├─ GlowLight (Light)
│  └─ AmbientParticles (ParticleSystem)
│
├─ Manages
│  ├─ Visibility (fade in/out)
│  ├─ Visual state (empty/valid/invalid)
│  ├─ Pulsing animation
│  └─ Material switching
│
└─ States
   ├─ Empty (no head in hand)
   ├─ ValidHover (correct head)
   └─ InvalidHover (wrong head)
```

## Data Flow

### Player Approach Flow

```
1. Player Movement
   ↓
2. DullahanHeadPlacementPuzzle.Update()
   ↓
3. Check Interaction (raycast or distance)
   ↓
4. Player in range?
   ├─ YES → OnPlayerEnterRange()
   │        ├─ Show HeadPlaceholder
   │        ├─ Show UI Prompt
   │        └─ Begin monitoring head selection
   │
   └─ NO  → OnPlayerExitRange()
            ├─ Hide HeadPlaceholder
            └─ Hide UI Prompt
```

### Head Selection Flow

```
1. Player has head in inventory
   ↓
2. DullahanHeadInventory.GetCurrentHead()
   ↓
3. DullahanHeadPlacementPuzzle checks head type
   ↓
4. Update placeholder appearance
   ├─ headID == requiredHeadID?
   │  ├─ YES → Set ValidMaterial (green)
   │  └─ NO  → Set InvalidMaterial (red)
   │
   └─ No head?
      └─ Set EmptyMaterial (white)
```

### Head Placement Flow

```
1. Player presses interaction key (F)
   ↓
2. TryPlaceHead()
   ↓
3. Check if player has head
   ├─ NO → Show "No head" message
   │       └─ Return
   │
   └─ YES → Continue
          ↓
4. Get selected head from inventory
   ↓
5. Check head ID
   ├─ CORRECT HEAD (headID == requiredHeadID)
   │  ├─ Remove from inventory
   │  ├─ Hide placeholder
   │  ├─ Show attached head visual
   │  ├─ Play correct sound
   │  ├─ Play particles
   │  ├─ Complete puzzle
   │  ├─ Enable completion light
   │  ├─ Grant rewards
   │  └─ Notify event managers
   │
   └─ WRONG HEAD (headID != requiredHeadID)
      ├─ Remove from inventory
      ├─ Hide placeholder
      ├─ Show fake head temporarily
      ├─ Play wrong sound
      ├─ Show error message
      ├─ Apply negative effects
      ├─ Wait (fakeHeadDisplayDuration)
      ├─ Remove fake head
      └─ Show placeholder again
```

## Integration Points

### Existing Systems

```
┌──────────────────────────┐
│  DullahanHeadInventory   │  ← Inventory system
└──────────────────────────┘
            │
            ├─ Query: GetCurrentHead()
            ├─ Query: HasHeads()
            └─ Command: RemoveSelectedHeadIfHead()

┌──────────────────────────┐
│  DullahanHeadSO          │  ← Head data
└──────────────────────────┘
            │
            ├─ Data: headID
            ├─ Data: headName
            ├─ Data: headType
            ├─ Data: headPrefab
            └─ Data: effects

┌──────────────────────────┐
│  Event Managers          │  ← Game events
└──────────────────────────┘
            │
            ├─ Floor2EndingEventManager.OnRealHeadAttachedToBody()
            └─ DullahanChaseEventManager.OnRealHeadAttachedToBody()

┌──────────────────────────┐
│  Door System             │  ← Rewards
└──────────────────────────┘
            │
            └─ Door.UnlockDoor()
```

### Custom Integration (Optional)

```
┌──────────────────────────────────┐
│  ExamplePuzzleIntegration        │
│  (Your custom logic here)        │
└──────────────────────────────────┘
            │
            ├─ Monitor: puzzle.IsPuzzleCompleted()
            ├─ Control: placeholder.Show()
            ├─ Control: placeholder.Hide()
            ├─ Control: puzzle.ResetPuzzle()
            └─ Custom: Your event handlers
```

## State Machine

### Puzzle States

```
┌─────────────┐
│  Idle       │  Initial state, waiting for player
└─────────────┘
      │
      │ Player approaches
      ▼
┌─────────────┐
│  Showing    │  Placeholder visible, monitoring player
└─────────────┘
      │
      │ Player presses F with head
      ▼
┌─────────────┐
│  Validating │  Check if correct/wrong head
└─────────────┘
      │
      ├─ Correct head
      │  ▼
      │ ┌─────────────┐
      │ │  Completed  │  Puzzle solved (terminal state)
      │ └─────────────┘
      │
      └─ Wrong head
         ▼
      ┌─────────────┐
      │  Processing │  Show fake head temporarily
      └─────────────┘
         │
         │ After duration
         ▼
      ┌─────────────┐
      │  Showing    │  Return to monitoring state
      └─────────────┘
```

### Placeholder States

```
┌─────────────┐
│  Hidden     │  Not visible (initial state)
└─────────────┘
      │
      ▼
┌─────────────┐
│  FadingIn   │  Transitioning to visible
└─────────────┘
      │
      ▼
┌─────────────┐
│  Visible    │  Fully visible, may pulse
└─────────────┘
      │
      ├─────────────┬─────────────┐
      ▼             ▼             ▼
┌─────────┐   ┌───────────┐  ┌─────────────┐
│  Empty  │   │  Valid    │  │  Invalid    │
│ (white) │   │ (green)   │  │  (red)      │
└─────────┘   └───────────┘  └─────────────┘
      │             │             │
      └─────────────┴─────────────┘
                    │
                    ▼
              ┌─────────────┐
              │  FadingOut  │  Transitioning to hidden
              └─────────────┘
                    │
                    ▼
              ┌─────────────┐
              │  Hidden     │  Not visible
              └─────────────┘
```

## Event Timeline

### Correct Head Placement Timeline

```
T=0.0s  │ Player presses F
        │ ├─ TryPlaceHead()
        │ └─ Validation starts
        │
T=0.1s  │ Head validated as correct
        │ ├─ Remove from inventory
        │ ├─ Hide placeholder
        │ └─ Play correct sound
        │
T=0.2s  │ Visual effects start
        │ ├─ Show attached head
        │ ├─ Play particles
        │ └─ Enable completion light
        │
T=0.3s  │ Puzzle completion
        │ ├─ CompletePuzzle()
        │ ├─ Grant rewards (door unlock)
        │ └─ Spawn reward items
        │
T=0.4s  │ Event notifications
        │ ├─ Notify Floor2EventManager
        │ └─ Notify ChaseEventManager
        │
T=0.5s  │ Play completion sound
        │ └─ Puzzle complete!
```

### Wrong Head Placement Timeline

```
T=0.0s  │ Player presses F
        │ ├─ TryPlaceHead()
        │ └─ Validation starts
        │
T=0.1s  │ Head validated as wrong
        │ ├─ Remove from inventory
        │ ├─ Hide placeholder
        │ └─ Play wrong sound
        │
T=0.2s  │ Show fake head
        │ ├─ Instantiate head prefab
        │ └─ Position at attachment point
        │
T=0.3s  │ Apply effects
        │ └─ DullahanHeadEffectManager.ApplyEffect()
        │
T=2.0s  │ (After fakeHeadDisplayDuration)
        │ ├─ Remove fake head
        │ ├─ Destroy temporary instance
        │ └─ Show placeholder again
        │
T=2.1s  │ Ready for next attempt
        │ └─ Placeholder visible, waiting
```

## Performance Considerations

### Per-Frame Operations

```
Update Loop (Every Frame)
├─ Check player distance OR
│  └─ Single raycast (Screen center → interactionRange)
│
├─ Update placeholder material (only if in range)
│  └─ Single material assignment
│
└─ Check input (GetKeyDown)
   └─ Only if in range
```

### Expensive Operations (Infrequent)

```
One-Time (Start)
├─ FindObjectOfType (player, inventory, event managers)
├─ GetComponent (renderers, lights, etc.)
└─ Initialize materials

Triggered (User action)
├─ Instantiate head prefab
├─ Play particle systems
├─ Coroutines (fade, temporary effects)
└─ Event notifications
```

### Memory Usage

```
Static Memory
├─ Component references (~200 bytes)
├─ Material references (~50 bytes)
└─ State variables (~100 bytes)

Dynamic Memory
├─ Temporary head instances (when wrong head placed)
└─ Coroutine allocations (minimal)

Total: < 1 KB per puzzle instance
```

## Extension Points

### For Custom Implementations

```
1. Inherit from DullahanHeadPlacementPuzzle
   └─ Override: CompletePuzzle(), ApplyWrongHeadEffects()

2. Use ExamplePuzzleIntegration pattern
   └─ Monitor: IsPuzzleCompleted()
   └─ React: Custom event handlers

3. Create custom placeholder states
   └─ Extend: HeadPlaceholder.PlaceholderState enum
   └─ Add: Custom materials and transitions

4. Add custom rewards
   └─ Override: GrantRewards()
   └─ Implement: Custom reward logic
```

## Debugging

### Debug Log Flow

```
[Puzzle] Placeholder initialized. Visible: false
[Puzzle] Player entered interaction range (distance: 2.5)
[Puzzle] Attempting to place head: Real Head (ID: 1)
[Puzzle] ✓ CORRECT HEAD PLACED: Real Head
[Puzzle] ★★★ PUZZLE COMPLETED! ★★★
[Puzzle] Reward door unlocked
[Puzzle] Notified Floor2EndingEventManager
```

### Common Debug Points

```
1. Interaction Detection
   └─ Log: "Player entered/exited range"
   └─ Check: interactionRange, raycast hit

2. Head Selection
   └─ Log: "Current head: [name] (ID: [id])"
   └─ Check: DullahanHeadInventory

3. Placement Validation
   └─ Log: "Correct/Wrong head placed"
   └─ Check: headID vs requiredHeadID

4. Visual Updates
   └─ Log: "Placeholder state: [state]"
   └─ Check: Material assignments
```

---

**This architecture diagram provides a comprehensive overview of how all components work together. Use it as a reference when extending or debugging the system.**
