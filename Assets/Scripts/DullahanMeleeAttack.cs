using UnityEngine;
using System.Collections;

public class DullahanMeleeAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackRange = 2f;
    public float attackDamage = 1;
    public float attackCooldown = 2f;
    public float attackDuration = 0.5f;
    public bool canAttack = true;

    [Header("Attack Detection")]
    public Transform attackPoint;
    public LayerMask playerLayer = 1;
    public float attackRadius = 1f;

    [Header("Visual Effects")]
    public Animator dullahanAnimator;
    public string attackTriggerName = "Attack";
    public ParticleSystem attackParticles;
    public Light attackLight;
    public Color attackLightColor = Color.red;
    public float attackLightIntensity = 2f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip attackSound;
    public AudioClip hitSound;
    public AudioClip missSound;

    [Header("Integration")]
    public PlayerHealthSystem playerHealthSystem;
    public DullahanChaseSystem chaseSystem;

    [Header("Attack Patterns")]
    public AttackPattern[] attackPatterns = new AttackPattern[]
    {
        new AttackPattern { patternName = "Basic Attack", damage = 1f, range = 2f, cooldown = 2f, animationTrigger = "Attack" },
        new AttackPattern { patternName = "Heavy Attack", damage = 2f, range = 2.5f, cooldown = 4f, animationTrigger = "HeavyAttack" },
        new AttackPattern { patternName = "Quick Attack", damage = 0.5f, range = 1.5f, cooldown = 1f, animationTrigger = "QuickAttack" }
    };

    // Private variables
    private bool isAttacking = false;
    private float lastAttackTime = 0f;
    private AttackPattern currentPattern;
    private Transform playerTransform;
    private bool playerInRange = false;

    [System.Serializable]
    public class AttackPattern
    {
        public string patternName;
        public float damage;
        public float range;
        public float cooldown;
        public string animationTrigger;
        public bool canStun = false;
        public float stunDuration = 0f;
        public AudioClip customAttackSound;
        public AudioClip customHitSound;
    }

    void Start()
    {
        InitializeAttackSystem();
    }

    void Update()
    {
        if (!canAttack) return;

        CheckPlayerInRange();
        HandleAttackLogic();
    }

    private void InitializeAttackSystem()
    {
        // Find references if not assigned
        if (attackPoint == null)
        {
            attackPoint = transform;
        }

        if (playerHealthSystem == null)
        {
            playerHealthSystem = FindObjectOfType<PlayerHealthSystem>();
        }

        if (chaseSystem == null)
        {
            chaseSystem = GetComponent<DullahanChaseSystem>();
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        // Set default attack pattern
        if (attackPatterns.Length > 0)
        {
            currentPattern = attackPatterns[0];
        }

        Debug.Log("Dullahan Melee Attack System initialized");
    }

    private void CheckPlayerInRange()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        playerInRange = distanceToPlayer <= attackRange;

        // Debug visualization
        if (playerInRange)
        {
            Debug.DrawLine(transform.position, playerTransform.position, Color.red);
        }
    }

    private void HandleAttackLogic()
    {
        if (isAttacking || !playerInRange) return;

        // Check if enough time has passed since last attack
        if (Time.time - lastAttackTime < currentPattern.cooldown) return;

        // Start attack
        StartAttack();
    }

    public void StartAttack()
    {
        if (isAttacking) return;

        isAttacking = true;
        lastAttackTime = Time.time;

        // Choose attack pattern based on situation
        ChooseAttackPattern();

        // Start attack sequence
        StartCoroutine(PerformAttack());
    }

    private void ChooseAttackPattern()
    {
        // Simple pattern selection - use basic attack for now
        // TODO: Implement intensity-based selection when DullahanChaseSystem is ready
        currentPattern = attackPatterns[0]; // Always use Basic Attack
        Debug.Log($"Dullahan using attack pattern: {currentPattern.patternName}");
    }

    private IEnumerator PerformAttack()
    {
        // Play attack animation
        if (dullahanAnimator != null && !string.IsNullOrEmpty(currentPattern.animationTrigger))
        {
            dullahanAnimator.SetTrigger(currentPattern.animationTrigger);
        }

        // Play attack sound
        PlayAttackSound();

        // Visual effects
        StartAttackEffects();

        // Wait for attack to reach player
        yield return new WaitForSeconds(attackDuration * 0.5f);

        // Check for hit
        CheckForHit();

        // Wait for attack to finish
        yield return new WaitForSeconds(attackDuration * 0.5f);

        // End attack
        EndAttack();
    }

    private void CheckForHit()
    {
        if (playerTransform == null || playerHealthSystem == null) return;

        // Check if player is still in range
        float distanceToPlayer = Vector3.Distance(attackPoint.position, playerTransform.position);
        
        if (distanceToPlayer <= currentPattern.range)
        {
            // Player is hit!
            DealDamage();
        }
        else
        {
            // Attack missed
            PlayMissSound();
        }
    }

    private void DealDamage()
    {
        if (playerHealthSystem == null) return;

        // Deal damage to player
        int damage = Mathf.RoundToInt(currentPattern.damage);
        playerHealthSystem.TakeDamage(damage);

        // Play hit sound
        PlayHitSound();

        // Visual feedback
        StartHitEffects();

        // Apply stun if pattern has it
        if (currentPattern.canStun && currentPattern.stunDuration > 0)
        {
            StartCoroutine(StunPlayer());
        }

        Debug.Log($"Dullahan hit player for {damage} damage!");
    }

    private IEnumerator StunPlayer()
    {
        // You can implement player stun logic here
        // For example, disable player movement temporarily
        Debug.Log($"Player stunned for {currentPattern.stunDuration} seconds!");
        
        yield return new WaitForSeconds(currentPattern.stunDuration);
        
        Debug.Log("Player stun ended");
    }

    private void StartAttackEffects()
    {
        // Attack light
        if (attackLight != null)
        {
            attackLight.color = attackLightColor;
            attackLight.intensity = attackLightIntensity;
            attackLight.enabled = true;
        }

        // Attack particles
        if (attackParticles != null)
        {
            attackParticles.Play();
        }
    }

    private void StartHitEffects()
    {
        // You can add additional hit effects here
        // For example, screen shake, blood particles, etc.
    }

    private void EndAttack()
    {
        isAttacking = false;

        // Turn off attack effects
        if (attackLight != null)
        {
            attackLight.enabled = false;
        }

        if (attackParticles != null)
        {
            attackParticles.Stop();
        }
    }

    private void PlayAttackSound()
    {
        if (audioSource == null) return;

        AudioClip soundToPlay = currentPattern.customAttackSound != null ? 
            currentPattern.customAttackSound : attackSound;

        if (soundToPlay != null)
        {
            audioSource.PlayOneShot(soundToPlay);
        }
    }

    private void PlayHitSound()
    {
        if (audioSource == null) return;

        AudioClip soundToPlay = currentPattern.customHitSound != null ? 
            currentPattern.customHitSound : hitSound;

        if (soundToPlay != null)
        {
            audioSource.PlayOneShot(soundToPlay);
        }
    }

    private void PlayMissSound()
    {
        if (audioSource != null && missSound != null)
        {
            audioSource.PlayOneShot(missSound);
        }
    }

    // Public methods for external control
    public void SetCanAttack(bool canAttack)
    {
        this.canAttack = canAttack;
    }

    public void SetAttackPattern(int patternIndex)
    {
        if (patternIndex >= 0 && patternIndex < attackPatterns.Length)
        {
            currentPattern = attackPatterns[patternIndex];
        }
    }

    public void ForceAttack()
    {
        if (canAttack && !isAttacking)
        {
            StartAttack();
        }
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

    void OnDrawGizmos()
    {
        // Draw current attack pattern range
        if (currentPattern != null)
        {
            Gizmos.color = playerInRange ? Color.green : Color.blue;
            Gizmos.DrawWireSphere(transform.position, currentPattern.range);
        }
    }

    // Public getters
    public bool IsAttacking() => isAttacking;
    public bool IsPlayerInRange() => playerInRange;
    public AttackPattern GetCurrentPattern() => currentPattern;
    public float GetAttackCooldown() => currentPattern != null ? currentPattern.cooldown : attackCooldown;
    public float GetTimeUntilNextAttack() => Mathf.Max(0, (lastAttackTime + (currentPattern != null ? currentPattern.cooldown : attackCooldown)) - Time.time);
}
