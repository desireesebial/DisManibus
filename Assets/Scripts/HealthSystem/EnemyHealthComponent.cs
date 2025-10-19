using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Simple enemy health component inspired by Ilumisoft Health System.
/// Can be attached to any enemy for basic health management.
/// </summary>
[AddComponentMenu("Enemy System/Enemy Health")]
public class EnemyHealthComponent : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField, Range(0, 1)] private float initialHealthRatio = 1.0f;
    
    [Header("Events")]
    public UnityEvent<int> OnHealthChanged;
    public UnityEvent OnHealthEmpty;
    public UnityEvent OnEnemyDeath;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private AudioClip deathSound;
    
    [Header("Effects")]
    [SerializeField] private GameObject deathEffect;
    [SerializeField] private GameObject damageEffect;
    
    // Properties (Ilumisoft pattern)
    public int MaxHealth { get => maxHealth; set => maxHealth = value; }
    public int CurrentHealth { get; private set; }
    public bool IsAlive => CurrentHealth > 0;
    
    // Private variables
    private bool isDead = false;
    
    private void Awake()
    {
        // Initialize health based on ratio
        SetHealth(Mathf.RoundToInt(maxHealth * initialHealthRatio));
        
        // Get audio source if not assigned
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }
    
    /// <summary>
    /// Sets the given amount of health
    /// </summary>
    /// <param name="health">Health amount to set</param>
    public void SetHealth(int health)
    {
        int previousHealth = CurrentHealth;
        CurrentHealth = Mathf.Clamp(health, 0, maxHealth);
        
        int healthChange = CurrentHealth - previousHealth;
        
        if (Mathf.Abs(healthChange) > 0)
        {
            OnHealthChanged?.Invoke(CurrentHealth);
            
            if (CurrentHealth <= 0 && !isDead)
            {
                OnHealthEmpty?.Invoke();
                HandleDeath();
            }
        }
    }
    
    /// <summary>
    /// Adds the given amount of health
    /// </summary>
    /// <param name="amount">Amount to heal</param>
    public void AddHealth(int amount)
    {
        if (!IsAlive) return;
        
        int previousHealth = CurrentHealth;
        CurrentHealth = Mathf.Clamp(CurrentHealth + amount, 0, maxHealth);
        
        int healthChange = CurrentHealth - previousHealth;
        
        if (healthChange > 0)
        {
            OnHealthChanged?.Invoke(CurrentHealth);
        }
    }
    
    /// <summary>
    /// Applies the given amount of damage
    /// </summary>
    /// <param name="damage">Damage amount</param>
    public void ApplyDamage(int damage)
    {
        if (!IsAlive) return;
        
        int previousHealth = CurrentHealth;
        CurrentHealth = Mathf.Clamp(CurrentHealth - damage, 0, maxHealth);
        
        int healthChange = CurrentHealth - previousHealth;
        
        if (Mathf.Abs(healthChange) > 0)
        {
            OnHealthChanged?.Invoke(CurrentHealth);
            
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
            
            if (CurrentHealth <= 0)
            {
                OnHealthEmpty?.Invoke();
                HandleDeath();
            }
        }
    }
    
    private void HandleDeath()
    {
        if (isDead) return;
        
        isDead = true;
        
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
    
    // Public getters
    public bool IsDead() => isDead;
    public float GetHealthPercentage() => (float)CurrentHealth / maxHealth;
    
    // Debug methods
    [ContextMenu("Take 10 Damage")]
    private void DebugTakeDamage()
    {
        ApplyDamage(10);
    }
    
    [ContextMenu("Heal 10 Health")]
    private void DebugHeal()
    {
        AddHealth(10);
    }
    
    [ContextMenu("Kill Enemy")]
    private void DebugKill()
    {
        SetHealth(0);
    }
}
