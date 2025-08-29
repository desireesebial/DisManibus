using UnityEngine;

/// <summary>
/// Integration script that shows how Dullahan attacks connect to Player Health System
/// This script demonstrates the connection between the two systems
/// </summary>
public class DullahanAttackIntegration : MonoBehaviour
{
    [Header("System References")]
    public PlayerHealthSystem playerHealthSystem;
    public DullahanMeleeAttack dullahanAttack;
    public DullahanChaseSystem dullahanChase;

    [Header("Debug Settings")]
    public bool showDebugInfo = true;
    public bool enableDebugDamage = false;

    void Start()
    {
        SetupIntegration();
    }

    void Update()
    {
        if (showDebugInfo)
        {
            DisplayDebugInfo();
        }

        if (enableDebugDamage)
        {
            HandleDebugDamage();
        }
    }

    private void SetupIntegration()
    {
        // Find systems if not assigned
        if (playerHealthSystem == null)
        {
            playerHealthSystem = FindObjectOfType<PlayerHealthSystem>();
        }

        if (dullahanAttack == null)
        {
            dullahanAttack = FindObjectOfType<DullahanMeleeAttack>();
        }

        if (dullahanChase == null)
        {
            dullahanChase = FindObjectOfType<DullahanChaseSystem>();
        }

        // Subscribe to health events
        if (playerHealthSystem != null)
        {
            playerHealthSystem.OnHealthChanged += OnPlayerHealthChanged;
            playerHealthSystem.OnCriticalHealth += OnPlayerCriticalHealth;
            playerHealthSystem.OnPlayerDeath += OnPlayerDeath;
        }

        Debug.Log("Dullahan Attack Integration initialized");
    }

    private void OnPlayerHealthChanged(int newHealth)
    {
        Debug.Log($"Player health changed to: {newHealth}");

        // You can add logic here based on health changes
        // For example, adjust Dullahan behavior based on player health
        if (dullahanChase != null)
        {
            // Make Dullahan more aggressive when player is low on health
            if (newHealth == 1)
            {
                Debug.Log("Player is critically injured - Dullahan becomes more aggressive!");
                // You can adjust chase intensity or attack patterns here
            }
        }
    }

    private void OnPlayerCriticalHealth()
    {
        Debug.Log("Player is critically injured!");

        // You can add special effects or logic here
        // For example, make the Dullahan more dangerous
        if (dullahanAttack != null)
        {
            // Use more aggressive attack patterns
            dullahanAttack.SetAttackPattern(1); // Heavy Attack
        }
    }

    private void OnPlayerDeath()
    {
        Debug.Log("Player has died! Game Over.");

        // Stop Dullahan from attacking
        if (dullahanAttack != null)
        {
            dullahanAttack.SetCanAttack(false);
        }

        // You can add game over logic here
        // For example, show death screen, restart level, etc.
    }

    private void DisplayDebugInfo()
    {
        if (playerHealthSystem != null)
        {
            Debug.Log($"Player Health: {playerHealthSystem.GetCurrentHealth()}/{playerHealthSystem.GetMaxHealth()}");
            Debug.Log($"Player Invulnerable: {playerHealthSystem.IsInvulnerable()}");
            Debug.Log($"Player Critical: {playerHealthSystem.IsCriticalHealth()}");
        }

        if (dullahanAttack != null)
        {
            Debug.Log($"Dullahan Attacking: {dullahanAttack.IsAttacking()}");
            Debug.Log($"Player In Range: {dullahanAttack.IsPlayerInRange()}");
            Debug.Log($"Time Until Next Attack: {dullahanAttack.GetTimeUntilNextAttack():F1}s");
        }
    }

    private void HandleDebugDamage()
    {
        // Debug keys for testing the integration
        if (Input.GetKeyDown(KeyCode.H))
        {
            if (playerHealthSystem != null)
            {
                playerHealthSystem.TakeDamage(1);
            }
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            if (playerHealthSystem != null)
            {
                playerHealthSystem.Heal(1);
            }
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            if (playerHealthSystem != null)
            {
                playerHealthSystem.RestoreFullHealth();
            }
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            if (dullahanAttack != null)
            {
                dullahanAttack.ForceAttack();
            }
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        if (playerHealthSystem != null)
        {
            playerHealthSystem.OnHealthChanged -= OnPlayerHealthChanged;
            playerHealthSystem.OnCriticalHealth -= OnPlayerCriticalHealth;
            playerHealthSystem.OnPlayerDeath -= OnPlayerDeath;
        }
    }
}
