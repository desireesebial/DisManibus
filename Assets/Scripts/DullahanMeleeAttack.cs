using UnityEngine;
using System.Collections;

public class DullahanMeleeAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackRange = 2f;
    public float attackDamage = 25f;
    public float attackCooldown = 2f;
    public float attackDuration = 0.5f;

    [Header("Attack Detection")]
    public LayerMask playerLayer = 1;
    public Transform attackPoint;
    public float attackRadius = 1f;

    [Header("Visual Effects")]
    public GameObject attackEffect;
    public ParticleSystem attackParticles;
    public Light attackLight;
    public Material attackMaterial;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip attackSound;
    public AudioClip hitSound;
    public AudioClip missSound;

    [Header("Animation")]
    public Animator animator;
    public string attackAnimationTrigger = "Attack";
    public string idleAnimationTrigger = "Idle";
    
    [Header("State Management")]
    public bool canAttack = true;
    public bool isAttacking = false;
    public bool playerInRange = false;
    
    [Header("Debug References")]
    public PlayerHealthSystem playerHealthSystem;
    public string[] attackPatterns = { "Basic Attack", "Heavy Attack", "Combo Attack" };

    // Private variables
    private Transform player;
    private float lastAttackTime = 0f;
    private Renderer dullahanRenderer;
    private Material originalMaterial;
    private Coroutine attackCoroutine;
    private DullahanChaseSystem chaseSystem;
    private bool isMovementPaused = false;

    void Start()
    {
        InitializeComponents();
        SetupAudio();
    }

    void Update()
    {
        if (isMovementPaused) return;

        CheckPlayerDistance();
        HandleAttack();
    }
    
    void InitializeComponents()
    {
        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerHealthSystem = playerObj.GetComponent<PlayerHealthSystem>();
        }

        // Get DullahanChaseSystem for movement pause
        chaseSystem = GetComponent<DullahanChaseSystem>();

        // Get components
        if (animator == null)
            animator = GetComponent<Animator>();

        if (attackPoint == null)
            attackPoint = transform;

        dullahanRenderer = GetComponent<Renderer>();
        if (dullahanRenderer != null)
            originalMaterial = dullahanRenderer.material;
    }
    
    void SetupAudio()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }
    
    void CheckPlayerDistance()
    {
        if (player == null) return;
        
        float distance = Vector3.Distance(transform.position, player.position);
        playerInRange = distance <= attackRange;
    }
    
    void HandleAttack()
    {
        if (isMovementPaused) return;
        if (!canAttack || isAttacking) return;
        
        if (playerInRange && Time.time - lastAttackTime >= attackCooldown)
        {
            StartAttack();
        }
    }

    public void StartAttack()
    {
        if (isMovementPaused) return;
        if (isAttacking) return;
        
        Debug.Log("[DullahanMeleeAttack] Starting attack");

        isAttacking = true;
        canAttack = false;
        lastAttackTime = Time.time;

        // Start attack coroutine
        if (attackCoroutine != null)
            StopCoroutine(attackCoroutine);
        
        attackCoroutine = StartCoroutine(PerformAttack());
    }
    
    IEnumerator PerformAttack()
    {
        if (isMovementPaused) yield break;

        // Play attack animation
        if (animator != null)
            animator.SetTrigger(attackAnimationTrigger);
        
        // Start visual effects
        StartAttackEffects();

        // Play attack sound
        if (audioSource != null && attackSound != null)
            audioSource.PlayOneShot(attackSound);
        
        // Wait for attack duration
        yield return new WaitForSeconds(attackDuration);
        
        // Perform attack damage
        PerformAttackDamage();
        
        // Stop visual effects
        StopAttackEffects();
        
        // Reset attack state
        isAttacking = false;
        canAttack = true;
        
        // Return to idle animation
        if (animator != null)
            animator.SetTrigger(idleAnimationTrigger);
    }
    
    void PerformAttackDamage()
    {
        if (isMovementPaused) return;

        // Check if player is in attack range
        Collider[] hitColliders = Physics.OverlapSphere(attackPoint.position, attackRadius, playerLayer);
        
        bool hitPlayer = false;
        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Player"))
            {
                // Deal damage to player
                PlayerHealthSystem playerHealth = hitCollider.GetComponent<PlayerHealthSystem>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage((int)attackDamage);
                    Debug.Log($"[DullahanMeleeAttack] Hit player for {attackDamage} damage");
                    hitPlayer = true;

                    // IMMEDIATELY pause this component
                    isMovementPaused = true;
                    Debug.Log($"[DullahanMeleeAttack] PAUSED for {attackCooldown} seconds");

                    // Pause chase system movement
                    if (chaseSystem != null)
                    {
                        chaseSystem.PauseMovementAfterAttack(attackCooldown);
                    }

                    // Start coroutine to unpause after cooldown
                    StartCoroutine(UnpauseAfterCooldown(attackCooldown));
                }

                // Play hit sound
                if (audioSource != null && hitSound != null)
                    audioSource.PlayOneShot(hitSound);

                break;
            }
        }
        
        // Play miss sound if no hit
        if (!hitPlayer && audioSource != null && missSound != null)
        {
            audioSource.PlayOneShot(missSound);
            Debug.Log("[DullahanMeleeAttack] Attack missed");
        }
    }
    
    void StartAttackEffects()
    {
        // Start particle effects
        if (attackParticles != null)
            attackParticles.Play();
        
        // Start light effects
        if (attackLight != null)
        {
            attackLight.enabled = true;
            attackLight.color = Color.red;
            attackLight.intensity = 2f;
        }
        
        // Apply attack material
        if (attackMaterial != null && dullahanRenderer != null)
            dullahanRenderer.material = attackMaterial;
        
        // Instantiate attack effect
        if (attackEffect != null)
        {
            GameObject effect = Instantiate(attackEffect, attackPoint.position, attackPoint.rotation);
            Destroy(effect, 2f);
        }
    }
    
    void StopAttackEffects()
    {
        // Stop particle effects
        if (attackParticles != null)
            attackParticles.Stop();
        
        // Stop light effects
        if (attackLight != null)
            attackLight.enabled = false;
        
        // Restore original material
        if (originalMaterial != null && dullahanRenderer != null)
            dullahanRenderer.material = originalMaterial;
    }
    
    public void SetCanAttack(bool canAttack)
    {
        this.canAttack = canAttack;
    }
    
    public void SetAttackDamage(float damage)
    {
        attackDamage = damage;
    }
    
    public void SetAttackRange(float range)
    {
        attackRange = range;
    }
    
    public void SetAttackCooldown(float cooldown)
    {
        attackCooldown = cooldown;
    }
    
    public bool IsAttacking()
    {
        return isAttacking;
    }
    
    public bool CanAttack()
    {
        return canAttack && !isAttacking && Time.time - lastAttackTime >= attackCooldown;
    }
    
    public float GetAttackRange()
    {
        return attackRange;
    }
    
    public float GetAttackDamage()
    {
        return attackDamage;
    }
    
    public float GetAttackCooldown()
    {
        return attackCooldown;
    }
    
    public float GetTimeUntilNextAttack()
    {
        float timeSinceLastAttack = Time.time - lastAttackTime;
        return Mathf.Max(0f, attackCooldown - timeSinceLastAttack);
    }

    public bool IsPlayerInRange()
    {
        return playerInRange;
    }

    public void SetMovementPaused(bool paused)
    {
        isMovementPaused = paused;
    }

    private IEnumerator UnpauseAfterCooldown(float duration)
    {
        yield return new WaitForSeconds(duration);
        isMovementPaused = false;
        Debug.Log($"[DullahanMeleeAttack] UNPAUSED after {duration}s cooldown");
    }

    // Debug methods
    public void TestAttack()
    {
        if (CanAttack())
        {
            StartAttack();
        }
        else
        {
            Debug.Log($"[DullahanMeleeAttack] Cannot attack. Time until next attack: {GetTimeUntilNextAttack():F1}s");
        }
    }

    public void ForceAttack()
    {
        Debug.Log("[DullahanMeleeAttack] Force attacking");
        canAttack = true;
        isAttacking = false;
        lastAttackTime = 0f;
            StartAttack();
    }

    // Debug visualization
    void OnDrawGizmosSelected()
    {
        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Draw attack point
        if (attackPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
    }

    void OnDestroy()
    {
        // Stop any running coroutines
        if (attackCoroutine != null)
            StopCoroutine(attackCoroutine);
    }
}
