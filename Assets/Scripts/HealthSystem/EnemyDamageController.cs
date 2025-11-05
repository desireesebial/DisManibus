using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Simple enemy damage controller that can be attached to any enemy mob.
/// Handles damage dealing to the player and basic enemy health management.
/// </summary>
[RequireComponent(typeof(Collider))]
public class EnemyDamageController : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;
    [SerializeField] private int damageToPlayer = 1;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private float attackRange = 2f;
    
    [Header("Enemy Type")]
    [SerializeField] private EnemyType enemyType = EnemyType.Jenglot;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip damageSound;
    
    [Header("Effects")]
    [SerializeField] private GameObject deathEffect;
    [SerializeField] private GameObject damageEffect;
    
    [Header("Events")]
    public UnityEvent OnEnemyDeath;
    public UnityEvent OnEnemyAttack;
    public UnityEvent<int> OnEnemyHealthChanged;
    
    // Properties
    public int MaxHealth { get => maxHealth; set => maxHealth = value; }
    public int CurrentHealth { get => currentHealth; set => currentHealth = value; }
    public int DamageToPlayer { get => damageToPlayer; set => damageToPlayer = value; }
    public bool IsAlive => currentHealth > 0;
    public EnemyType Type => enemyType;
    
    // Private variables
    private float lastAttackTime;
    private PlayerHealthSystem playerHealthSystem;
    private Transform playerTransform;
    private bool isDead = false;
    
    public enum EnemyType
    {
        Jenglot,
        Kamatayan,
        Dullahan,
        KuchisakeOnna,
        Generic
    }
    
    void Start()
    {
        InitializeEnemy();
    }
    
    void Update()
    {
        if (!IsAlive || isDead) return;
        
        CheckForPlayerAndAttack();
    }
    
    private void InitializeEnemy()
    {
        // Set initial health
        currentHealth = maxHealth;
        
        // Find player components
        if (playerHealthSystem == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerHealthSystem = player.GetComponent<PlayerHealthSystem>();
                playerTransform = player.transform;
            }
        }
        
        // Get audio source if not assigned
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        // Configure AudioSource to ensure attack sounds only play when triggered
        if (audioSource != null)
        {
            audioSource.playOnAwake = false; // Prevent auto-play on start
            audioSource.loop = false; // Ensure sounds don't loop
            audioSource.Stop(); // Stop any audio that might be playing
            Debug.Log($"[EnemyDamageController] {gameObject.name} - AudioSource configured: playOnAwake=false, loop=false");
        }
        else
        {
            Debug.LogWarning($"[EnemyDamageController] {gameObject.name} - No AudioSource found! Add AudioSource component to play attack sounds.");
        }

        // Set enemy tag based on type
        SetEnemyTag();

        Debug.Log($"[EnemyDamageController] {gameObject.name} initialized. Health: {currentHealth}/{maxHealth}, Type: {enemyType}");
    }
    
    private void SetEnemyTag()
    {
        switch (enemyType)
        {
            case EnemyType.Jenglot:
                gameObject.tag = "Jenglot";
                break;
            case EnemyType.Kamatayan:
                gameObject.tag = "Kamatayan";
                break;
            case EnemyType.Dullahan:
                gameObject.tag = "Dullahan";
                break;
            case EnemyType.KuchisakeOnna:
                gameObject.tag = "Enemy"; // Use generic tag for Kuchisake Onna
                break;
            default:
                gameObject.tag = "Enemy";
                break;
        }
    }
    
    private void CheckForPlayerAndAttack()
    {
        if (playerTransform == null || playerHealthSystem == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= attackRange)
        {
            float timeSinceLastAttack = Time.time - lastAttackTime;

            if (timeSinceLastAttack >= attackCooldown)
            {
                Debug.Log($"[EnemyDamageController] {gameObject.name} is in attack range ({distanceToPlayer:F2}m <= {attackRange}m). Attacking!");
                AttackPlayer();
            }
        }
    }
    
    public void AttackPlayer()
    {
        if (playerHealthSystem == null || !IsAlive)
        {
            Debug.LogWarning($"[EnemyDamageController] {gameObject.name} cannot attack: playerHealthSystem={playerHealthSystem != null}, IsAlive={IsAlive}");
            return;
        }

        Debug.Log($"[EnemyDamageController] {gameObject.name} ({enemyType}) is attacking player for {damageToPlayer} damage");

        // Deal damage to player FIRST
        playerHealthSystem.ApplyDamage(damageToPlayer);

        // ONLY play attack sound AFTER damage is successfully applied to player
        if (audioSource != null && attackSound != null)
        {
            // Safety check: Ensure AudioSource isn't set to loop (should never loop attack sounds)
            if (audioSource.loop)
            {
                Debug.LogWarning($"[EnemyDamageController] {gameObject.name} - AudioSource loop is enabled! Disabling loop for attack sounds.");
                audioSource.loop = false;
            }

            // Play attack sound using PlayOneShot (doesn't interrupt other sounds, plays once)
            audioSource.PlayOneShot(attackSound);
            Debug.Log($"[EnemyDamageController] ✓ Playing attack sound for {gameObject.name} - Sound will play once and stop automatically");
        }
        else
        {
            if (audioSource == null)
                Debug.LogWarning($"[EnemyDamageController] {gameObject.name} - AudioSource is not assigned! Cannot play attack sound.");
            if (attackSound == null)
                Debug.LogWarning($"[EnemyDamageController] {gameObject.name} - Attack Sound AudioClip is not assigned! Assign it in Inspector.");
        }

        // Trigger attack event
        OnEnemyAttack?.Invoke();

        // Update last attack time
        lastAttackTime = Time.time;
    }
    
    public void TakeDamage(int damage)
    {
        if (!IsAlive || isDead) return;
        
        currentHealth = Mathf.Max(0, currentHealth - damage);
        
        // Trigger health changed event
        OnEnemyHealthChanged?.Invoke(currentHealth);
        
        // Play damage sound
        if (audioSource != null && damageSound != null)
        {
            audioSource.PlayOneShot(damageSound);
        }
        
        // Show damage effect
        if (damageEffect != null)
        {
            GameObject effect = Instantiate(damageEffect, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }
        
        Debug.Log($"Enemy {gameObject.name} took {damage} damage. Health: {currentHealth}/{maxHealth}");
        
        // Check for death
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    public void Heal(int healAmount)
    {
        if (!IsAlive) return;
        
        int previousHealth = currentHealth;
        currentHealth = Mathf.Min(maxHealth, currentHealth + healAmount);
        
        int healthChange = currentHealth - previousHealth;
        
        if (healthChange > 0)
        {
            OnEnemyHealthChanged?.Invoke(currentHealth);
            Debug.Log($"Enemy {gameObject.name} healed {healAmount}. Health: {currentHealth}/{maxHealth}");
        }
    }
    
    public void SetHealth(int health)
    {
        int previousHealth = currentHealth;
        currentHealth = Mathf.Clamp(health, 0, maxHealth);
        
        int healthChange = currentHealth - previousHealth;
        
        if (Mathf.Abs(healthChange) > 0)
        {
            OnEnemyHealthChanged?.Invoke(currentHealth);
            
            if (currentHealth <= 0 && !isDead)
            {
                Die();
            }
        }
    }
    
    private void Die()
    {
        if (isDead) return;
        
        isDead = true;
        currentHealth = 0;
        
        // Play death sound
        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
        
        // Show death effect
        if (deathEffect != null)
        {
            GameObject effect = Instantiate(deathEffect, transform.position, Quaternion.identity);
            Destroy(effect, 3f);
        }
        
        // Trigger death event
        OnEnemyDeath?.Invoke();
        
        // Disable enemy components
        DisableEnemy();
        
        Debug.Log($"Enemy {gameObject.name} has died!");
    }
    
    private void DisableEnemy()
    {
        // Disable collider to prevent further interactions
        Collider enemyCollider = GetComponent<Collider>();
        if (enemyCollider != null)
            enemyCollider.enabled = false;
        
        // Disable any AI or movement scripts
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            if (script != this)
                script.enabled = false;
        }
        
        // Optionally destroy the game object after a delay
        Destroy(gameObject, 5f);
    }
    
    // Collision detection for player contact
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[EnemyDamageController] {gameObject.name} OnTriggerEnter with {other.gameObject.name} (tag: {other.tag})");

        if (other.CompareTag("Player") && IsAlive)
        {
            float timeSinceLastAttack = Time.time - lastAttackTime;
            Debug.Log($"[EnemyDamageController] Player contact detected! Time since last attack: {timeSinceLastAttack:F2}s, Cooldown: {attackCooldown}s");

            // Check if cooldown has passed
            if (timeSinceLastAttack >= attackCooldown)
            {
                // Deal contact damage
                AttackPlayer();
            }
            else
            {
                Debug.Log($"[EnemyDamageController] Attack on cooldown. Wait {(attackCooldown - timeSinceLastAttack):F2}s more.");
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"[EnemyDamageController] {gameObject.name} OnCollisionEnter with {collision.gameObject.name} (tag: {collision.gameObject.tag})");

        if (collision.gameObject.CompareTag("Player") && IsAlive)
        {
            float timeSinceLastAttack = Time.time - lastAttackTime;
            Debug.Log($"[EnemyDamageController] Player contact detected! Time since last attack: {timeSinceLastAttack:F2}s, Cooldown: {attackCooldown}s");

            // Check if cooldown has passed
            if (timeSinceLastAttack >= attackCooldown)
            {
                // Deal contact damage
                AttackPlayer();
            }
            else
            {
                Debug.Log($"[EnemyDamageController] Attack on cooldown. Wait {(attackCooldown - timeSinceLastAttack):F2}s more.");
            }
        }
    }
    
    // Public getters
    public bool IsDead() => isDead;
    public float GetDistanceToPlayer()
    {
        if (playerTransform == null) return float.MaxValue;
        return Vector3.Distance(transform.position, playerTransform.position);
    }
    
    // Debug methods
    [ContextMenu("Take 10 Damage")]
    private void DebugTakeDamage()
    {
        TakeDamage(10);
    }
    
    [ContextMenu("Heal 10 Health")]
    private void DebugHeal()
    {
        Heal(10);
    }
    
    [ContextMenu("Kill Enemy")]
    private void DebugKill()
    {
        SetHealth(0);
    }
}
