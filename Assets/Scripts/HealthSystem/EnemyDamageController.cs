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
        
        // Set enemy tag based on type
        SetEnemyTag();
        
        Debug.Log($"Enemy {gameObject.name} initialized. Health: {currentHealth}/{maxHealth}, Type: {enemyType}");
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
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                AttackPlayer();
            }
        }
    }
    
    public void AttackPlayer()
    {
        if (playerHealthSystem == null || !IsAlive) return;
        
        // Deal damage to player
        playerHealthSystem.ApplyDamage(damageToPlayer);
        
        // Play attack sound
        if (audioSource != null && attackSound != null)
        {
            audioSource.PlayOneShot(attackSound);
        }
        
        // Trigger attack event
        OnEnemyAttack?.Invoke();
        
        // Update last attack time
        lastAttackTime = Time.time;
        
        Debug.Log($"Enemy {gameObject.name} attacked player for {damageToPlayer} damage");
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
        if (other.CompareTag("Player") && IsAlive)
        {
            // Deal contact damage
            AttackPlayer();
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && IsAlive)
        {
            // Deal contact damage
            AttackPlayer();
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
