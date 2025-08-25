# Dullahan Chase System & Three Heads Puzzle

This system implements an intensity-based chase mechanic for the Dullahan enemy and a puzzle involving three heads that the player must find and carry to complete the game.

## Overview

The system consists of several interconnected components:

1. **DullahanChaseSystem** - Enhanced chase mechanics with dynamic intensity
2. **DullahanHeadSO** - Scriptable objects for head items
3. **DullahanHeadPickable** - Pickable head objects with effects
4. **DullahanHeadEffectManager** - Manages head effects on the player
5. **DullahanBody** - The body that receives the real head
6. **DullahanPuzzleManager** - Coordinates the entire puzzle system

## Setup Instructions

### 1. Dullahan Chase System Setup

1. **Add DullahanChaseSystem to Dullahan GameObject:**
   - Select the Dullahan enemy GameObject
   - Add the `DullahanChaseSystem` script
   - Configure the chase settings in the inspector

2. **Configure Chase Settings:**
   ```
   Chase Intensity Settings:
   - Max Chase Speed: 8f (maximum speed when very close)
   - Min Chase Speed: 3f (minimum speed when far away)
   - Max Intensity Distance: 2f (distance for maximum intensity)
   - Min Intensity Distance: 15f (distance to start chase)
   - Intensity Multiplier: 1.5f (curve of intensity increase)
   ```

3. **Setup Audio:**
   - Assign AudioSource component
   - Add chase intensity clips (different audio for different intensity levels)
   - Add heartbeat clips for player heartbeat sounds

4. **Setup Visual Effects:**
   - Assign the Dullahan's light component
   - Configure light intensity and color changes
   - Set up flicker effects

5. **Setup Player Effects:**
   - Assign the FirstPersonController
   - Configure screen shake and FOV effects

### 2. Three Heads Puzzle Setup

#### Create Head Scriptable Objects

1. **Right-click in Project window → Create → Scriptable Objects → DullahanHead**

2. **Create three head assets:**
   - **Real Head** (ID: 1, Type: Real, No effects)
   - **Fake Head 1** (ID: 2, Type: Fake1, Effect: SpeedDebuff)
   - **Fake Head 2** (ID: 3, Type: Fake2, Effect: SpeedBoost)

3. **Configure each head:**
   ```
   Head Properties:
   - Head Name: "Dullahan's Real Head" / "Fake Head 1" / "Fake Head 2"
   - Head ID: 1 / 2 / 3
   - Head Type: Real / Fake1 / Fake2
   - Description: "The real head that opens the door" / etc.
   
   Effects (for fake heads):
   - Has Effect: true
   - Effect Type: SpeedDebuff / SpeedBoost
   - Effect Strength: 0.3f
   - Effect Duration: 15f
   ```

#### Setup Head Pickable Objects

1. **Create head prefabs:**
   - Create 3D objects for each head
   - Add `DullahanHeadPickable` script
   - Add collider with "Is Trigger" enabled
   - Add visual effects (lights, materials)

2. **Configure head pickables:**
   ```
   Head Settings:
   - Head Data: Assign corresponding DullahanHeadSO
   - Interaction Range: 3f
   - Interaction Key: E
   
   Visual Effects:
   - Head Light: Add light component for glow
   - Head Renderer: Assign renderer for material changes
   - Has Glow Effect: true (for visual appeal)
   ```

#### Setup Dullahan Body

1. **Create body GameObject:**
   - Add `DullahanBody` script
   - Add collider with "Is Trigger" enabled
   - Add visual components (renderer, light)

2. **Configure body:**
   ```
   Body Settings:
   - Required Head ID: 1 (matches real head)
   - Body Name: "Dullahan Body"
   
   Door Settings:
   - Final Door: Assign the door that opens when puzzle is complete
   - Door Key ID: 999 (special key ID)
   
   Visual Effects:
   - Head Attachment Point: Transform where head appears
   - Head Visual: GameObject that shows attached head
   - Body Light: Light component for activation effects
   - Body Material: Normal material
   - Active Body Material: Material when head is attached
   ```

#### Setup Effect Manager

1. **Create Effect Manager GameObject:**
   - Add `DullahanHeadEffectManager` script
   - Assign player references (FirstPersonController, PlayerInventory)
   - Assign DullahanChaseSystem reference

2. **Configure effects:**
   ```
   Effect Settings:
   - Effect Fade Time: 1f
   - Show Effect Notifications: true
   
   UI:
   - Effect Notification UI: Canvas with notification text
   - Effect Notification Text: TextMeshPro component
   ```

#### Setup Puzzle Manager

1. **Create Puzzle Manager GameObject:**
   - Add `DullahanPuzzleManager` script
   - Assign all puzzle components

2. **Configure puzzle:**
   ```
   Puzzle Components:
   - Head Pickables: Array of DullahanHeadPickable objects
   - Dullahan Body: Reference to DullahanBody
   - Dullahan Chase: Reference to DullahanChaseSystem
   - Final Door: Reference to the final door
   
   Head Spawn Settings:
   - Head Spawn Points: Array of Transform positions
   - Head Prefabs: Array of head GameObject prefabs
   - Head Data: Array of DullahanHeadSO assets
   
   Puzzle Settings:
   - Puzzle Active: true
   - Puzzle Start Delay: 5f
   ```

## How It Works

### Chase System

1. **Distance-Based Intensity:** The closer the Dullahan gets to the player, the higher the chase intensity
2. **Dynamic Effects:** Speed, audio, visual effects, and player effects all scale with intensity
3. **Audio Progression:** Different audio clips play at different intensity levels
4. **Visual Feedback:** Light intensity and color change based on proximity
5. **Player Effects:** Screen shake and FOV changes create tension

### Three Heads Puzzle

1. **Head Collection:** Player must find and pick up three heads scattered around the level
2. **Effect Application:** Fake heads apply buffs/debuffs to the player
3. **Real Head Identification:** Only the real head (ID: 1) can be attached to the body
4. **Puzzle Completion:** Attaching the real head unlocks the final door and completes the game

### Head Effects

- **SpeedBoost:** Increases player movement speed
- **SpeedDebuff:** Decreases player movement speed
- **VisionBoost:** Increases FOV
- **VisionDebuff:** Decreases FOV
- **StaminaBoost:** Increases sprint duration
- **StaminaDebuff:** Decreases sprint duration
- **FearEffect:** Increases Dullahan chase intensity
- **CalmEffect:** Decreases Dullahan chase intensity

## Integration with Existing Systems

### Inventory System
- Heads are converted to KeyItemsSO for inventory compatibility
- Uses existing inventory UI and management
- Maintains inventory size limits

### Door System
- Final door uses special key ID (999)
- Integrates with existing door locking mechanism
- Maintains audio and visual feedback

### First Person Controller
- Effects modify player movement and vision
- Screen shake and FOV changes enhance immersion
- All effects are temporary and fade over time

## Customization

### Adding New Head Types
1. Create new DullahanHeadSO asset
2. Configure head properties and effects
3. Create corresponding prefab with DullahanHeadPickable script
4. Add to puzzle manager's head arrays

### Modifying Chase Intensity
1. Adjust distance thresholds in DullahanChaseSystem
2. Modify intensity calculation curve
3. Add new audio clips for different intensity levels
4. Customize visual effects and player feedback

### Adding New Effects
1. Add new effect type to EffectType enum
2. Implement effect logic in DullahanHeadEffectManager
3. Add effect description in GetEffectDescription method
4. Create corresponding audio and visual assets

## Troubleshooting

### Common Issues

1. **Heads not spawning:**
   - Check head spawn points are assigned
   - Verify head prefabs have DullahanHeadPickable script
   - Ensure head data arrays match in length

2. **Chase not working:**
   - Verify player has "Player" tag
   - Check NavMeshAgent component on Dullahan
   - Ensure DullahanChaseSystem is properly configured

3. **Effects not applying:**
   - Check DullahanHeadEffectManager references
   - Verify effect strength and duration values
   - Ensure player components are assigned

4. **Body not accepting head:**
   - Verify head ID matches required head ID
   - Check inventory system integration
   - Ensure DullahanBody script is properly configured

### Debug Information

All scripts include debug logging to help identify issues:
- Chase start/end events
- Head pickup events
- Effect application/removal
- Puzzle progress updates

## Performance Considerations

- Limit active effects to prevent performance issues
- Use object pooling for particle effects
- Optimize audio sources and light components
- Consider LOD systems for complex visual effects

## Future Enhancements

- Add more head types and effects
- Implement head-specific animations
- Add environmental storytelling elements
- Create multiple puzzle variations
- Add save/load functionality for puzzle state
