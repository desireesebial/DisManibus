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

    [Header("Spatial Awareness")]
    public bool enforceSameFloor = true;
    public float maxVerticalDifference = 1.5f;
    public bool requireLineOfSight = true;
    public LayerMask lineOfSightObstacles;

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

        // Horizontal distance on XZ plane
        Vector2 selfXZ = new Vector2(transform.position.x, transform.position.z);
        Vector2 playerXZ = new Vector2(playerTransform.position.x, playerTransform.position.z);
        float horizontalDistance = Vector2.Distance(selfXZ, playerXZ);

        // Floor check using vertical delta
        bool sameFloor = !enforceSameFloor || Mathf.Abs(transform.position.y - playerTransform.position.y) <= maxVerticalDifference;

        // Line of sight check
        bool hasLOS = !requireLineOfSight || HasLineOfSight(playerTransform);

        playerInRange = horizontalDistance <= attackRange && sameFloor && hasLOS;

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
        // Overlap check around attack point for any player-layer targets
        if (attackPoint == null) attackPoint = transform;

        Collider[] hits = Physics.OverlapSphere(attackPoint.position, attackRadius, playerLayer, QueryTriggerInteraction.Collide);
        if (hits == null || hits.Length == 0)
        {
            PlayMissSound();
            return;
        }

        bool appliedDamage = false;
        for (int i = 0; i < hits.Length; i++)
        {
            Collider col = hits[i];
            Transform target = col.transform;

            // Optional same-floor and LOS validation
            bool sameFloor = !enforceSameFloor || Mathf.Abs(attackPoint.position.y - target.position.y) <= maxVerticalDifference;
            bool hasLOS = !requireLineOfSight || HasLineOfSight(target);
            if (!sameFloor || !hasLOS) continue;

            // Support any object that has PlayerHealthSystem on it or its parents
            PlayerHealthSystem targetHealth = col.GetComponentInParent<PlayerHealthSystem>();
            if (targetHealth == null) continue;

            // Apply pattern damage
            int damage = Mathf.RoundToInt(currentPattern.damage);
            targetHealth.TakeDamage(damage);
            appliedDamage = true;
        }

        if (appliedDamage)
        {
            PlayHitSound();
            StartHitEffects();
        }
        else
        {
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
        Debug.Log($"Player stunned for {currentPattern.stunDuration} seconds!");

        // Disable player movement using FirstPersonController reference on the PlayerHealthSystem
        FirstPersonController controller = null;
        if (playerHealthSystem != null)
        {
            controller = playerHealthSystem.playerController;
        }
        if (controller == null)
        {
            controller = FindObjectOfType<FirstPersonController>();
        }

        bool previousCanMove = true;
        if (controller != null)
        {
            previousCanMove = controller.playerCanMove;
            controller.playerCanMove = false;
        }

        yield return new WaitForSeconds(currentPattern.stunDuration);

        if (controller != null)
        {
            controller.playerCanMove = previousCanMove;
        }

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

        // Visualize attack overlap radius
        if (attackPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
    }

    // Public getters
    public bool IsAttacking() => isAttacking;
    public bool IsPlayerInRange() => playerInRange;
    public AttackPattern GetCurrentPattern() => currentPattern;
    public float GetAttackCooldown() => currentPattern != null ? currentPattern.cooldown : attackCooldown;
    public float GetTimeUntilNextAttack() => Mathf.Max(0, (lastAttackTime + (currentPattern != null ? currentPattern.cooldown : attackCooldown)) - Time.time);

    private bool HasLineOfSight(Transform target)
    {
        if (target == null) return false;

        Vector3 origin = attackPoint != null ? attackPoint.position : transform.position;
        Vector3 targetPos = target.position;
        Vector3 dir = (targetPos - origin).normalized;
        float dist = Vector3.Distance(origin, targetPos);

        // If no obstacle mask provided, default to Physics default which may hit anything
        int mask = lineOfSightObstacles.value == 0 ? Physics.DefaultRaycastLayers : lineOfSightObstacles.value;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, dist, mask, QueryTriggerInteraction.Ignore))
        {
            // If we hit something before reaching the target that is not the target, LOS is blocked
            if (hit.transform != target && hit.transform.IsChildOf(target) == false)
            {
                return false;
            }
        }

        return true;
    }
}
