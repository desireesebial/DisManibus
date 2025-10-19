# Enemy Damage System Setup Guide

This guide explains how to use the new enemy damage system inspired by the Ilumisoft Health System.

## Components Overview

### 1. PlayerHealthSystem (Revised)
- **Location**: `Assets/Scripts/HealthSystem/PlayerHealthSystem.cs`
- **Purpose**: Manages player health with Ilumisoft-inspired structure
- **New Methods**: `ApplyDamage()`, `AddHealth()`, `SetHealth()`
- **Backward Compatibility**: Old methods `TakeDamage()` and `Heal()` still work

### 2. EnemyDamageController
- **Location**: `Assets/Scripts/HealthSystem/EnemyDamageController.cs`
- **Purpose**: Complete enemy system with health, damage dealing, and AI
- **Features**: Attack cooldown, range detection, audio/visual effects
- **Enemy Types**: Jenglot, Kamatayan, Dullahan, Kuchisake Onna, Generic

### 3. EnemyHealthComponent
- **Location**: `Assets/Scripts/HealthSystem/EnemyHealthComponent.cs`
- **Purpose**: Simple health management for enemies (Ilumisoft pattern)
- **Features**: Health events, audio/visual effects, death handling

### 4. PlayerDamageDealer
- **Location**: `Assets/Scripts/HealthSystem/PlayerDamageDealer.cs`
- **Purpose**: Allows player weapons/projectiles to damage enemies
- **Usage**: Attach to weapons, bullets, or any damage-dealing object

## Floor-Specific Enemy Setup

### **DisManibus Floor Enemy Distribution**
- **Floor 4**: Kamatayan enemies
- **Floor 3**: Jenglot enemies  
- **Floor 2**: Dullahan enemies
- **Floor 1**: Kuchisake Onna enemies

### **Enemy Tagging System**
Each floor's enemies must be properly tagged for the damage system to work:

#### **Floor 4 - Kamatayan**
```csharp
// Enemy GameObject must have tag: "Kamatayan"
GameObject.tag = "Kamatayan";
EnemyDamageController.enemyType = EnemyType.Kamatayan;
```

#### **Floor 3 - Jenglot**
```csharp
// Enemy GameObject must have tag: "Jenglot"
GameObject.tag = "Jenglot";
EnemyDamageController.enemyType = EnemyType.Jenglot;
```

#### **Floor 2 - Dullahan**
```csharp
// Enemy GameObject must have tag: "Dullahan"
GameObject.tag = "Dullahan";
EnemyDamageController.enemyType = EnemyType.Dullahan;
```

#### **Floor 1 - Kuchisake Onna**
```csharp
// Enemy GameObject must have tag: "Kuchisake Onna" or "Enemy"
GameObject.tag = "Enemy"; // Use generic enemy tag
EnemyDamageController.enemyType = EnemyType.Generic;
```

### **PlayerHealthSystem Enemy Tags**
The PlayerHealthSystem automatically detects enemies by their tags:
- `jenglotTag = "Jenglot"`
- `kamatayanTag = "Kamatayan"`
- `dullahanTag = "Dullahan"`

### **Floor-by-Floor Setup Guide**

#### **Floor 4 - Kamatayan Setup**
1. **Tag Enemy**: Set GameObject tag to "Kamatayan"
2. **Add EnemyDamageController**: Attach component to enemy
3. **Configure Settings**:
   - Enemy Type: Kamatayan
   - Max Health: 100
   - Damage to Player: 1
   - Attack Range: 2 units
   - Attack Cooldown: 1 second
4. **Test**: Enemy should attack player when in range

#### **Floor 3 - Jenglot Setup**
1. **Tag Enemy**: Set GameObject tag to "Jenglot"
2. **Add EnemyDamageController**: Attach component to enemy
3. **Configure Settings**:
   - Enemy Type: Jenglot
   - Max Health: 80
   - Damage to Player: 1
   - Attack Range: 5.0 units (Long Range)
   - Attack Cooldown: 0.8 seconds
4. **Test**: Enemy should attack player when in range

#### **Jenglot Long Range Configuration**
For long-range Jenglot attacks, use these settings:
- **Attack Range**: 5.0 - 8.0 units (Long range)
- **Attack Cooldown**: 1.0 - 1.5 seconds (Slower attacks for balance)
- **Damage to Player**: 1 (Same damage, longer range)
- **Max Health**: 80 (Same health, longer range)

#### **Floor 2 - Dullahan Setup**
1. **Tag Enemy**: Set GameObject tag to "Dullahan"
2. **Add EnemyDamageController**: Attach component to enemy
3. **Configure Settings**:
   - Enemy Type: Dullahan
   - Max Health: 120
   - Damage to Player: 1
   - Attack Range: 2.5 units
   - Attack Cooldown: 1.2 seconds
4. **Test**: Enemy should attack player when in range

#### **Floor 1 - Kuchisake Onna Setup**
1. **Tag Enemy**: Set GameObject tag to "Enemy"
2. **Add EnemyDamageController**: Attach component to enemy
3. **Configure Settings**:
   - Enemy Type: Generic
   - Max Health: 60
   - Damage to Player: 1
   - Attack Range: 1.8 units
   - Attack Cooldown: 1.5 seconds
4. **Test**: Enemy should attack player when in range

### **Floor Enemy Quick Reference**

| Floor | Enemy Type | GameObject Tag | EnemyDamageController Type | Max Health | Attack Range | Attack Cooldown |
|-------|------------|----------------|----------------------------|------------|--------------|-----------------|
| Floor 4 | Kamatayan | "Kamatayan" | Kamatayan | 100 | 2.0 units | 1.0 seconds |
| Floor 3 | Jenglot | "Jenglot" | Jenglot | 80 | **5.0 units** (Long Range) | 1.0 seconds |
| Floor 2 | Dullahan | "Dullahan" | Dullahan | 120 | 2.5 units | 1.2 seconds |
| Floor 1 | Kuchisake Onna | "Enemy" | Generic | 60 | 1.8 units | 1.5 seconds |

### **Attack Range Comparison**
- **Short Range**: 1.0 - 2.0 units (Close combat)
- **Medium Range**: 2.0 - 4.0 units (Standard range)
- **Long Range**: 5.0 - 8.0 units (Ranged attacks)
- **Very Long Range**: 8.0+ units (Sniper-like attacks)

### **Important Notes**
- **Dullahan MUST be tagged as "Dullahan"** for the system to work
- **Kuchisake Onna uses "Enemy" tag** (generic enemy)
- **Each floor has different enemy stats** for variety
- **PlayerHealthSystem automatically detects** all enemy types by tag

## 🎯 **Attack Range Configuration**

### **How to Set Long Range for Jenglot**

#### **Method 1: Inspector Settings**
1. Select your Jenglot enemy GameObject
2. Find the **EnemyDamageController** component
3. In the **Enemy Stats** section:
   - Set **Attack Range** to `5.0` (or higher for very long range)
   - Adjust **Attack Cooldown** to `1.0` seconds (slower for balance)
4. Test in play mode to verify range

#### **Method 2: Code Configuration**
```csharp
// Get the EnemyDamageController component
EnemyDamageController jenglotController = jenglot.GetComponent<EnemyDamageController>();

// Set long range attack
jenglotController.attackRange = 5.0f;  // Long range
jenglotController.attackCooldown = 1.0f;  // Slower attacks
jenglotController.damageToPlayer = 1;  // Same damage
```

### **Attack Range Examples**

#### **Short Range (Close Combat)**
- **Range**: 1.0 - 2.0 units
- **Use Case**: Melee enemies, close encounters
- **Examples**: Kuchisake Onna, basic Jenglot

#### **Medium Range (Standard)**
- **Range**: 2.0 - 4.0 units
- **Use Case**: Standard enemy attacks
- **Examples**: Kamatayan, Dullahan

#### **Long Range (Ranged Attacks)**
- **Range**: 5.0 - 8.0 units
- **Use Case**: Ranged enemies, snipers
- **Examples**: Long-range Jenglot, archer enemies

#### **Very Long Range (Sniper)**
- **Range**: 8.0+ units
- **Use Case**: Sniper enemies, long-range threats
- **Examples**: Sniper Jenglot, long-range threats

### **Balancing Long Range Attacks**

#### **Recommended Settings for Long Range Jenglot**
```csharp
// Long Range Jenglot Configuration
Attack Range: 5.0 - 8.0 units
Attack Cooldown: 1.0 - 1.5 seconds (slower for balance)
Damage to Player: 1 (same damage)
Max Health: 80 (same health)
```

#### **Why Slower Attack Cooldown?**
- **Balance**: Long range + fast attacks = too powerful
- **Player Reaction**: Gives player time to react
- **Gameplay**: Creates tension without being unfair
- **Survival Horror**: Maintains challenge without frustration

### **Testing Long Range Attacks**

#### **Step 1: Set Attack Range**
1. Open your Jenglot enemy in the scene
2. Select the GameObject
3. Find EnemyDamageController component
4. Set Attack Range to 5.0 or higher

#### **Step 2: Test in Play Mode**
1. Enter play mode
2. Move player to different distances from Jenglot
3. Verify Jenglot attacks when player is within range
4. Check that Jenglot doesn't attack when player is too far

#### **Step 3: Adjust for Balance**
- **Too Easy**: Increase attack cooldown
- **Too Hard**: Decrease attack range
- **Too Fast**: Increase attack cooldown
- **Too Slow**: Decrease attack cooldown

## Setup Instructions

### For Enemies (Choose One Approach)

#### Option A: Full Enemy System (EnemyDamageController)
1. Add `EnemyDamageController` component to enemy GameObject
2. Configure enemy stats in inspector:
   - Max Health: 100
   - Damage to Player: 1
   - Attack Cooldown: 1 second
   - Attack Range: 2 units
   - Enemy Type: Select appropriate type
3. Assign audio clips and effects (optional)
4. The enemy will automatically attack the player when in range

#### Option B: Simple Health Only (EnemyHealthComponent)
1. Add `EnemyHealthComponent` to enemy GameObject
2. Configure health settings in inspector
3. Use other scripts to handle damage dealing and AI
4. Connect to your existing enemy AI system

### For Player Weapons
1. Add `PlayerDamageDealer` component to weapon/projectile
2. Set damage amount in inspector
3. Configure hit effects and sounds (optional)
4. The weapon will automatically damage enemies on contact

### For Player Health System
1. The existing `PlayerHealthSystem` has been enhanced
2. New methods follow Ilumisoft pattern:
   - `ApplyDamage(int damage)` - Apply damage to player
   - `AddHealth(int amount)` - Heal player
   - `SetHealth(int health)` - Set specific health value
3. Old methods still work for backward compatibility

## Usage Examples

### Enemy Attacks Player
```csharp
// EnemyDamageController automatically handles this
// No additional code needed - just configure in inspector
```

### Player Attacks Enemy
```csharp
// PlayerDamageDealer automatically handles this
// No additional code needed - just attach to weapon
```

### Manual Health Management
```csharp
// Get enemy health component
EnemyHealthComponent enemyHealth = enemy.GetComponent<EnemyHealthComponent>();

// Deal damage
enemyHealth.ApplyDamage(10);

// Heal enemy
enemyHealth.AddHealth(5);

// Set specific health
enemyHealth.SetHealth(50);
```

### Player Health Management
```csharp
// Get player health system
PlayerHealthSystem playerHealth = player.GetComponent<PlayerHealthSystem>();

// Deal damage to player
playerHealth.ApplyDamage(1);

// Heal player
playerHealth.AddHealth(1);

// Set specific health
playerHealth.SetHealth(3);
```

## Event System

### Player Health Events
- `OnHealthChanged` - Triggered when player health changes
- `OnCriticalHealth` - Triggered when player reaches 1 health
- `OnPlayerDeath` - Triggered when player dies

### Enemy Health Events
- `OnHealthChanged` - Triggered when enemy health changes
- `OnHealthEmpty` - Triggered when enemy health reaches 0
- `OnEnemyDeath` - Triggered when enemy dies

### Enemy Attack Events
- `OnEnemyAttack` - Triggered when enemy attacks player

## Configuration Tips

1. **Enemy Types**: Set appropriate enemy type to match your game's enemy system
2. **Attack Range**: Adjust based on enemy size and attack method
3. **Attack Cooldown**: Balance between challenge and fairness
4. **Audio/Visual Effects**: Add for better player feedback
5. **Health Values**: Balance based on your game's difficulty

## Debug Features

All components include debug methods accessible via context menu:
- "Take 10 Damage" - Test damage
- "Heal 10 Health" - Test healing
- "Kill Enemy" - Test death

## Integration with Existing Systems

The new system is designed to work alongside your existing code:
- PlayerHealthSystem maintains all original functionality
- Enemy components can be mixed with existing AI systems
- Event system allows easy integration with other scripts
- Backward compatibility ensures no breaking changes

## Performance Notes

- Events use UnityEvents for better performance
- Components are lightweight and efficient
- Audio/visual effects are optional and can be disabled
- Debug methods are only available in editor
