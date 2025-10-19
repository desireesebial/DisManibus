# Enemy Damage Flow Explanation

This document explains exactly how the EnemyDamageController damages the player's health in your DisManibus survival horror game.

## 🔄 **Complete Damage Flow**

### **Step 1: Enemy Detection & Range Check**
```csharp
// EnemyDamageController.Update() - Called every frame
private void CheckForPlayerAndAttack()
{
    if (playerTransform == null || playerHealthSystem == null) return;
    
    // Calculate distance to player
    float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
    
    // Check if player is within attack range
    if (distanceToPlayer <= attackRange)
    {
        // Check if enough time has passed since last attack
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            AttackPlayer(); // Deal damage to player
        }
    }
}
```

### **Step 2: Attack Player Method**
```csharp
public void AttackPlayer()
{
    if (playerHealthSystem == null || !IsAlive) return;
    
    // 🎯 THIS IS WHERE DAMAGE HAPPENS
    playerHealthSystem.ApplyDamage(damageToPlayer);
    
    // Play attack sound
    if (audioSource != null && attackSound != null)
    {
        audioSource.PlayOneShot(attackSound);
    }
    
    // Trigger attack event
    OnEnemyAttack?.Invoke();
    
    // Update last attack time (prevents spam attacks)
    lastAttackTime = Time.time;
    
    Debug.Log($"Enemy {gameObject.name} attacked player for {damageToPlayer} damage");
}
```

### **Step 3: Player Health System Response**
```csharp
// PlayerHealthSystem.ApplyDamage() - Called by enemy
public void ApplyDamage(int damage)
{
    if (isInvulnerable || !IsAlive) return;

    int previousHealth = currentHealth;
    currentHealth = Mathf.Max(0, currentHealth - damage);
    
    int healthChange = currentHealth - previousHealth;
    
    if (Mathf.Abs(healthChange) > 0)
    {
        OnHealthChanged?.Invoke(currentHealth);
        
        // Visual and audio feedback
        StartCoroutine(CameraShake());
        StartDamageFlash();
        PlayDamageSound();

        // Apply debuffs based on health state
        ApplyHealthDebuffs();

        // Check for critical health
        if (currentHealth == 1)
        {
            OnCriticalHealth?.Invoke();
            StartCriticalHealthBlur();
        }

        // Check for death
        if (currentHealth <= 0)
        {
            OnPlayerDeath?.Invoke();
            HandlePlayerDeath();
        }

        // Start invulnerability frames
        StartCoroutine(InvulnerabilityFrames());

        // Update UI
        UpdateHealthUI();
        UpdateStatusUI();
    }
}
```

## 🎯 **Two Attack Methods**

### **Method 1: Range-Based Attacks (Automatic)**
```csharp
// Enemy automatically attacks when player is in range
void Update()
{
    if (!IsAlive || isDead) return;
    
    CheckForPlayerAndAttack(); // Checks distance and attacks
}
```

**How it works:**
1. **Every frame**: Enemy checks distance to player
2. **Range check**: If player is within `attackRange` distance
3. **Cooldown check**: If enough time has passed since last attack
4. **Attack**: Calls `AttackPlayer()` method
5. **Damage**: Player takes `damageToPlayer` amount of damage

### **Method 2: Contact-Based Attacks (Collision)**
```csharp
// Enemy attacks when player touches/collides with enemy
private void OnTriggerEnter(Collider other)
{
    if (other.CompareTag("Player") && IsAlive)
    {
        AttackPlayer(); // Deal contact damage
    }
}

private void OnCollisionEnter(Collision collision)
{
    if (collision.gameObject.CompareTag("Player") && IsAlive)
    {
        AttackPlayer(); // Deal contact damage
    }
}
```

**How it works:**
1. **Player touches enemy**: Collision/trigger detection
2. **Immediate attack**: No range or cooldown checks
3. **Damage**: Player takes `damageToPlayer` amount of damage

## ⚙️ **Configuration Settings**

### **Enemy Stats (Inspector)**
```csharp
[Header("Enemy Stats")]
[SerializeField] private int maxHealth = 100;        // Enemy's health
[SerializeField] private int currentHealth;          // Current enemy health
[SerializeField] private int damageToPlayer = 1;     // 🎯 DAMAGE TO PLAYER
[SerializeField] private float attackCooldown = 1f;  // Time between attacks
[SerializeField] private float attackRange = 2f;    // Attack range distance
```

### **Floor-Specific Damage Settings**
| Floor | Enemy | Damage to Player | Attack Range | Attack Cooldown |
|-------|-------|------------------|--------------|-----------------|
| Floor 4 | Kamatayan | 1 | 2.0 units | 1.0 seconds |
| Floor 3 | Jenglot | 1 | 5.0 units (Long Range) | 1.0 seconds |
| Floor 2 | Dullahan | 1 | 2.5 units | 1.2 seconds |
| Floor 1 | Kuchisake Onna | 1 | 1.8 units | 1.5 seconds |

## 🔄 **Complete Damage Sequence**

### **1. Enemy Detection**
```
Enemy detects player within range
↓
Check if attack cooldown has passed
↓
Call AttackPlayer() method
```

### **2. Damage Application**
```
AttackPlayer() calls playerHealthSystem.ApplyDamage(damageToPlayer)
↓
PlayerHealthSystem reduces currentHealth by damageToPlayer
↓
Player health: 3 → 2 (if damageToPlayer = 1)
```

### **3. Player Response**
```
Health change triggers events
↓
Visual effects: Camera shake, damage flash
↓
Audio effects: Damage sound
↓
UI updates: Health bars, status text
↓
Debuffs: Movement speed reduction if injured
```

### **4. Critical Health Check**
```
If health reaches 1:
↓
Critical health effects: Screen blur
↓
OnCriticalHealth event triggered
```

### **5. Death Check**
```
If health reaches 0:
↓
Player death sequence
↓
Death screen appears
↓
Game over handling
```

## 🎮 **Player Health System Integration**

### **PlayerHealthSystem Settings**
```csharp
[Header("Enemy Tags & Damage")]
public string jenglotTag = "Jenglot";
public int jenglotDamage = 1;
public string kamatayanTag = "Kamatayan";
public int kamatayanDamage = 1;
public string dullahanTag = "Dullahan";
public int dullahanDamage = 1;
```

### **Automatic Enemy Detection**
```csharp
// PlayerHealthSystem automatically detects enemies by tag
private void TryApplyEnemyContactDamage(GameObject other)
{
    if (other == null || currentHealth <= 0) return;

    if (!isInvulnerable)
    {
        if (!string.IsNullOrEmpty(jenglotTag) && other.CompareTag(jenglotTag))
        {
            TakeDamage(Mathf.Max(1, jenglotDamage));
            return;
        }
        if (!string.IsNullOrEmpty(kamatayanTag) && other.CompareTag(kamatayanTag))
        {
            TakeDamage(Mathf.Max(1, kamatayanDamage));
            return;
        }
        if (!string.IsNullOrEmpty(dullahanTag) && other.CompareTag(dullahanTag))
        {
            TakeDamage(Mathf.Max(1, dullahanDamage));
            return;
        }
    }
}
```

## 🛡️ **Invulnerability System**

### **Invulnerability Frames**
```csharp
// After taking damage, player becomes invulnerable briefly
private IEnumerator InvulnerabilityFrames()
{
    isInvulnerable = true;
    yield return new WaitForSeconds(invulnerabilityTime); // Default: 1 second
    isInvulnerable = false;
}
```

**Purpose:**
- **Prevents spam damage**: Player can't take multiple hits instantly
- **Player reaction time**: Gives player time to escape
- **Game balance**: Prevents unfair instant death

## 🎯 **Damage Examples**

### **Example 1: Jenglot Long Range Attack**
```
1. Player enters Jenglot's 5.0 unit range
2. Jenglot waits for 1.0 second cooldown
3. Jenglot calls AttackPlayer()
4. PlayerHealthSystem.ApplyDamage(1) is called
5. Player health: 3 → 2
6. Camera shake, damage flash, audio play
7. Player becomes invulnerable for 1 second
8. Jenglot waits 1.0 second before next attack
```

### **Example 2: Dullahan Contact Attack**
```
1. Player touches Dullahan (collision)
2. Dullahan immediately calls AttackPlayer()
3. PlayerHealthSystem.ApplyDamage(1) is called
4. Player health: 2 → 1 (critical health)
5. Critical health blur effect starts
6. Player becomes invulnerable for 1 second
```

### **Example 3: Player Death**
```
1. Player at 1 health takes damage
2. PlayerHealthSystem.ApplyDamage(1) is called
3. Player health: 1 → 0
4. OnPlayerDeath event triggered
5. Death screen appears
6. Game pauses, cursor unlocked
7. Player can retry or return to main menu
```

## 🔧 **Debugging Damage**

### **Debug Methods**
```csharp
// Context menu options for testing
[ContextMenu("Take 10 Damage")] - Test enemy damage
[ContextMenu("Heal 10 Health")] - Test enemy healing
[ContextMenu("Kill Enemy")] - Test enemy death
```

### **Debug Logs**
```
"Enemy Jenglot attacked player for 1 damage"
"Player took 1 damage. Health: 2/3"
"Applied minor injury debuffs: Reduced speed and sensitivity"
```

## 🎮 **Summary**

The EnemyDamageController damages the player through a simple but effective system:

1. **Detection**: Enemy detects player within range or on contact
2. **Attack**: Enemy calls `AttackPlayer()` method
3. **Damage**: `playerHealthSystem.ApplyDamage(damageToPlayer)` is called
4. **Response**: Player health decreases, effects play, UI updates
5. **Protection**: Invulnerability frames prevent spam damage

This creates a balanced, responsive damage system perfect for your survival horror game!
