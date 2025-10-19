# Health System Package

A comprehensive health and damage system inspired by the Ilumisoft Health System, designed for the DisManibus project.

## 📁 Package Contents

### Core Scripts
- **`PlayerHealthSystem.cs`** - Enhanced player health management with Ilumisoft-inspired structure
- **`EnemyDamageController.cs`** - Complete enemy system with health, damage dealing, and AI
- **`EnemyHealthComponent.cs`** - Simple health management for enemies (Ilumisoft pattern)
- **`PlayerDamageDealer.cs`** - Weapon/projectile damage dealing system

### Documentation
- **`ENEMY_DAMAGE_SYSTEM_SETUP_GUIDE.md`** - Complete setup and usage guide
- **`README.md`** - This overview document

## 🚀 Quick Start

### For Players
1. Attach `PlayerHealthSystem` to your player GameObject
2. Configure health settings, UI elements, and effects in the inspector
3. The system automatically handles damage from enemies

### For Enemies
**Option A - Full System:**
1. Attach `EnemyDamageController` to enemy GameObject
2. Configure enemy stats, attack range, and cooldown
3. Enemy will automatically attack player when in range

**Option B - Health Only:**
1. Attach `EnemyHealthComponent` to enemy GameObject
2. Use with your existing AI system
3. Handle damage dealing separately

### For Weapons
1. Attach `PlayerDamageDealer` to weapon/projectile
2. Set damage amount in inspector
3. Weapon automatically damages enemies on contact

## 🎯 Key Features

### Player Health System
- **Ilumisoft Pattern**: Properties and methods follow Ilumisoft structure
- **Unity Events**: Better integration with Unity's event system
- **Backward Compatibility**: Old methods still work
- **Visual Effects**: Camera shake, damage flash, critical health blur
- **Audio Feedback**: Damage, heal, and critical health sounds
- **UI Integration**: Health bars, status text, death screen

### Enemy System
- **Flexible Options**: Choose between full system or health-only component
- **Smart AI**: Automatic player detection and attack within range
- **Enemy Types**: Support for Jenglot, Kamatayan, Dullahan, and generic enemies
- **Audio/Visual Effects**: Optional sound and particle effects
- **Event System**: Easy integration with other scripts

### Weapon System
- **Simple Setup**: Just attach and configure
- **Automatic Detection**: Handles collision with enemies
- **Configurable**: Damage amount, hit effects, destroy on hit
- **Multi-Enemy Support**: Works with all enemy types

## 🔧 Configuration

### Player Health Settings
```csharp
// Health values
MaxHealth = 3;
CurrentHealth = 3;

// Visual effects
shakeIntensity = 0.5f;
shakeDuration = 0.3f;
damageFlashDuration = 0.2f;

// Audio
damageSound, healSound, criticalHealthSound
```

### Enemy Settings
```csharp
// Enemy stats
maxHealth = 100;
damageToPlayer = 1;
attackCooldown = 1f;
attackRange = 2f;

// Enemy type
enemyType = EnemyType.Jenglot;
```

### Weapon Settings
```csharp
// Damage settings
damageAmount = 10;
destroyOnHit = true;
destroyDelay = 0.1f;

// Effects
hitEffect, hitSound
```

## 📊 Event System

### Player Events
- `OnHealthChanged(int currentHealth)` - Health value changed
- `OnCriticalHealth()` - Player reached 1 health
- `OnPlayerDeath()` - Player died

### Enemy Events
- `OnEnemyHealthChanged(int currentHealth)` - Enemy health changed
- `OnHealthEmpty()` - Enemy health reached 0
- `OnEnemyDeath()` - Enemy died
- `OnEnemyAttack()` - Enemy attacked player

## 🎮 Usage Examples

### Basic Health Management
```csharp
// Player health
PlayerHealthSystem playerHealth = player.GetComponent<PlayerHealthSystem>();
playerHealth.ApplyDamage(1);  // Damage player
playerHealth.AddHealth(1);     // Heal player
playerHealth.SetHealth(3);     // Set specific health

// Enemy health
EnemyHealthComponent enemyHealth = enemy.GetComponent<EnemyHealthComponent>();
enemyHealth.ApplyDamage(10);   // Damage enemy
enemyHealth.AddHealth(5);      // Heal enemy
enemyHealth.SetHealth(50);     // Set specific health
```

### Event Handling
```csharp
// Player events
playerHealth.OnHealthChanged.AddListener(OnPlayerHealthChanged);
playerHealth.OnPlayerDeath.AddListener(OnPlayerDied);

// Enemy events
enemyHealth.OnEnemyDeath.AddListener(OnEnemyDied);
enemyController.OnEnemyAttack.AddListener(OnEnemyAttacked);
```

## 🐛 Debug Features

All components include debug methods accessible via context menu:
- **"Take 10 Damage"** - Test damage
- **"Heal 10 Health"** - Test healing
- **"Kill Enemy"** - Test death
- **"Test Damage"** - Test weapon damage

## 🔄 Integration

### With Existing Systems
- **Backward Compatible**: Won't break existing code
- **Flexible**: Mix with existing AI systems
- **Event-Driven**: Easy integration with other scripts
- **Performance Optimized**: Lightweight and efficient

### With Ilumisoft Health System
- **Same Patterns**: Follows Ilumisoft structure and naming
- **Unity Events**: Better integration than System.Action
- **Properties**: Clean getter/setter pattern
- **Method Names**: `ApplyDamage()`, `AddHealth()`, `SetHealth()`

## 📈 Performance Notes

- **Unity Events**: Better performance than System.Action
- **Lightweight**: Minimal overhead
- **Optional Effects**: Audio/visual effects can be disabled
- **Debug Methods**: Only available in editor
- **Efficient**: Optimized for real-time gameplay

## 🛠️ Troubleshooting

### Common Issues
1. **Enemy not attacking**: Check attack range and cooldown settings
2. **Weapon not damaging**: Ensure enemy has proper tag or component
3. **Events not firing**: Check Unity Event connections in inspector
4. **Performance issues**: Disable optional audio/visual effects

### Debug Steps
1. Use context menu debug methods to test functionality
2. Check console for debug messages
3. Verify component references in inspector
4. Test with simple setup first

## 📚 Additional Resources

- **Setup Guide**: See `ENEMY_DAMAGE_SYSTEM_SETUP_GUIDE.md` for detailed instructions
- **Unity Documentation**: Unity Events, Colliders, AudioSource
- **Ilumisoft Health System**: Reference for pattern consistency

## 🔄 Version History

- **v1.0** - Initial release with Ilumisoft-inspired structure
- **v1.1** - Added comprehensive enemy system
- **v1.2** - Enhanced with weapon damage system
- **v1.3** - Organized into HealthSystem package

## 📝 License

This package is part of the DisManibus project and follows the same licensing terms.

---

**Need Help?** Check the setup guide or use the debug features to test your implementation!
