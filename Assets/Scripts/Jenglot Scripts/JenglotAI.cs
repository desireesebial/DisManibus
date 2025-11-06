using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

/// <summary>
/// Jenglot animation states corresponding to available animations
/// </summary>
public enum JenglotAnimationState
{
    Idle,           // Sitting cross-legged (idle state)
    Walking,        // Moving toward player
    SpellCasting,   // Performing ranged attack
    Frozen          // Illuminated by flashlight (uses idle pose)
}

/// <summary>
/// Jenglot behavior:
/// - Detects and follows the Player within a radius.
/// - Freezes only when actually illuminated by the player's flashlight cone.
/// - Performs a long-range magic attack while not illuminated.
///
/// Setup:
/// - Add this component to the Jenglot GameObject.
/// - Add/assign a NavMeshAgent to the Jenglot.
/// - Assign the Player (or leave null to auto-find by tag "Player").
/// - Optionally assign Animator and AudioSource and hook up the OnChantAttack event.
/// - Ensure the Player has a FlashlightController (or inventory that manages it) on the player or camera root.
/// </summary>
public class JenglotAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform player;
    [SerializeField] NavMeshAgent navMeshAgent;
    [SerializeField] Animator animator; // optional
    [SerializeField] AudioSource chantAudio; // optional

    [Header("Detection/Behavior")]
    [SerializeField] float detectionRadius = 12f;
    [SerializeField] bool requireLineOfSight = true;
    [SerializeField] LayerMask lineOfSightObstructionMask = ~0; // what blocks LOS
    [SerializeField] float rotationSpeed = 10f;
	[SerializeField, Tooltip("If enabled, once the player is detected, the Jenglot will keep chasing even if LOS is blocked, until the player exceeds loseChaseDistance.")]
	bool persistentChaseAfterDetection = true;
	[SerializeField, Tooltip("Distance at which the Jenglot will give up the chase once already chasing. Should be >= detectionRadius.")]
	float loseChaseDistance = 30f;
	[SerializeField, Tooltip("If true, the Jenglot will never drop the chase within this scene once detected (ignores loseChaseDistance). It will reset only on scene unload/destroy.")]
	bool neverDropChaseInScene = true;

    [Header("Flashlight Behavior")]
    [SerializeField, Tooltip("If true, Jenglot only freezes when directly illuminated. If false, Jenglot freezes whenever flashlight is ON (regardless of direction).")]
    bool requireDirectIllumination = true;
    [SerializeField, Tooltip("If true, once frozen by flashlight, Jenglot stays frozen until flashlight is turned OFF completely.")]
    bool stayFrozenUntilFlashlightOff = true;

    [Header("Attack")]
	[Tooltip("Maximum distance for ranged attack.")]
	[SerializeField] float attackRange = 12f;
	[Tooltip("Cooldown between ranged attacks.")]
	[SerializeField] float attackCooldown = 3.5f;
	[Tooltip("Projectile or raycast damage applied to player.")]
	[SerializeField] int attackDamage = 10; // used with SendMessage if receiver implements TakeDamage(int)
    [SerializeField] UnityEvent onChantAttack; // hook VFX/SFX or gameplay here
	[Tooltip("Layer mask for line-of-sight of ranged attack.")]
	[SerializeField] LayerMask attackObstructionMask = ~0;
	[Tooltip("If true, uses raycast for instant hit. Otherwise, use projectile prefab if assigned.")]
	[SerializeField] bool useHitscan = true;
	[SerializeField] GameObject projectilePrefab; // optional projectile
	[SerializeField] float projectileSpeed = 20f;

    [Header("Animation Parameters")]
    [Tooltip("Animator parameter for idle/sitting animation (bool) - matches your SittingIdle state")]
    [SerializeField] string animParamIsIdle = "Idle";
    [Tooltip("Animator parameter for walking animation (bool) - matches your Walking state")]
    [SerializeField] string animParamIsWalking = "Walking";
    [Tooltip("Animator parameter for attack animation (bool) - matches your AttackAnimation state")]
    [SerializeField] string animParamIsAttacking = "Attacking";
    [Tooltip("Animator trigger for spell casting animation - matches your AttackAnimation state")]
    [SerializeField] string animTriggerSpellCast = "Attack";

	PlayerInventory playerInventory;
	FlashlightController flashlightController;
    float lastAttackTime;
	bool hasDetectedPlayer;
    JenglotAnimationState currentAnimationState = JenglotAnimationState.Idle;
    bool wasFrozenByFlashlight = false; // Track if we were frozen to maintain freeze state
    bool isMovementPaused = false; // Track if movement is paused after attack

    void Reset()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    void Awake()
    {
        if (navMeshAgent == null)
            navMeshAgent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        // Auto-find player if not assigned
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

		if (player != null)
		{
			playerInventory = player.GetComponent<PlayerInventory>();
			flashlightController = FindObjectOfType<FlashlightController>();
		}

        // Debug animator setup
        DebugAnimatorSetup();
    }

    void DebugAnimatorSetup()
    {
        if (animator == null)
        {
            Debug.LogWarning($"[{name}] No Animator assigned! Please assign the Animator component.");
            return;
        }

        if (animator.runtimeAnimatorController == null)
        {
            Debug.LogWarning($"[{name}] No Animator Controller assigned! Please assign your Controller.controller.");
            return;
        }

        Debug.Log($"[{name}] Animator setup OK. Controller: {animator.runtimeAnimatorController.name}");
        
        // Check if our parameters exist
        bool hasIdle = HasParameter(animParamIsIdle);
        bool hasWalking = HasParameter(animParamIsWalking);
        bool hasAttacking = HasParameter(animParamIsAttacking);
        bool hasAttackTrigger = HasParameter(animTriggerSpellCast);

        Debug.Log($"[{name}] Animation Parameters: Idle={hasIdle}, Walking={hasWalking}, Attacking={hasAttacking}, AttackTrigger={hasAttackTrigger}");
        
        if (!hasIdle || !hasWalking || !hasAttacking)
        {
            Debug.LogWarning($"[{name}] Missing animation parameters! Add 'Idle' (Bool), 'Walking' (Bool), and 'Attacking' (Bool) to your Animator Controller.");
        }

        // Debug current animator state
        var currentState = animator.GetCurrentAnimatorStateInfo(0);
        Debug.Log($"[{name}] Current Animation State: {currentState.shortNameHash} (normalized time: {currentState.normalizedTime})");
        
        // Debug all available states
        Debug.Log($"[{name}] Available states in controller:");
        for (int i = 0; i < animator.layerCount; i++)
        {
            var stateMachine = animator.GetBehaviour<UnityEngine.StateMachineBehaviour>();
            Debug.Log($"  Layer {i}: {animator.GetLayerName(i)}");
        }
    }

    bool HasParameter(string paramName)
    {
        if (animator == null || string.IsNullOrEmpty(paramName)) return false;
        
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName) return true;
        }
        return false;
    }

    void Update()
    {
        if (player == null || navMeshAgent == null)
            return;

        // Don't update if movement is paused (after attack)
        if (isMovementPaused)
        {
            return;
        }

		bool playerDetected = IsPlayerDetected();
		bool frozenByFlashlight = IsFrozenByPlayerFlashlight();

		if (!playerDetected)
        {
            // Idle when player not detected
            SetAnimationState(JenglotAnimationState.Idle);
            navMeshAgent.isStopped = true;
            return;
        }

        if (frozenByFlashlight)
        {
            // Freeze movement due to flashlight
            navMeshAgent.isStopped = true;
            navMeshAgent.velocity = Vector3.zero;
            navMeshAgent.ResetPath(); // Clear any existing path
            SetAnimationState(JenglotAnimationState.Frozen);
            FaceTarget(player.position);
            Debug.Log($"[{name}] FROZEN by flashlight - stopped at position {transform.position}");
            return;
        }

		float distance = Vector3.Distance(transform.position, player.position);
		if (distance <= attackRange)
        {
            // Attack behavior
            navMeshAgent.isStopped = true;
            navMeshAgent.velocity = Vector3.zero;
            SetAnimationState(JenglotAnimationState.SpellCasting);
            FaceTarget(player.position);
			TryAttack();
        }
        else
        {
            // Follow behavior
            navMeshAgent.isStopped = false;
            navMeshAgent.SetDestination(player.position);
            SetAnimationState(JenglotAnimationState.Walking);
        }
    }

	bool IsPlayerDetected()
    {
		float distance = Vector3.Distance(transform.position, player.position);

		// If already chasing and persistent mode is on, keep chasing (optionally forever in-scene)
		if (persistentChaseAfterDetection && hasDetectedPlayer)
		{
			if (neverDropChaseInScene)
				return true;

			if (loseChaseDistance <= 0f)
				return true;

			if (distance <= Mathf.Max(detectionRadius, loseChaseDistance))
				return true;

			// Drop chase only when well beyond the configured distance
			hasDetectedPlayer = false;
			return false;
		}

		// Fresh detection check
		if (distance > detectionRadius)
			return false;

		if (!requireLineOfSight)
		{
			hasDetectedPlayer = true;
			return true;
		}

		Vector3 origin = transform.position + Vector3.up * 1.6f;
		Vector3 dir = (player.position + Vector3.up * 1.6f) - origin;
		float maxDist = dir.magnitude;
		if (Physics.Raycast(origin, dir.normalized, out RaycastHit hit, maxDist, lineOfSightObstructionMask, QueryTriggerInteraction.Ignore))
		{
			// If we hit something that isn't the player, LOS is blocked
			if (hit.transform != player && !hit.transform.IsChildOf(player))
				return false;
		}

		hasDetectedPlayer = true;
		return true;
    }

	bool IsFrozenByPlayerFlashlight()
    {
		bool isFlashlightOn = false;
		bool isIlluminated = false;

		// Check if flashlight is on and optionally if we're illuminated
		if (flashlightController != null)
		{
			isFlashlightOn = flashlightController.IsFlashlightOn();
			
			if (requireDirectIllumination)
			{
				// Only freeze when directly illuminated
				isIlluminated = flashlightController.IsIlluminating(transform, true, lineOfSightObstructionMask);
			}
			else
			{
				// Freeze whenever flashlight is on (regardless of direction)
				isIlluminated = isFlashlightOn;
			}
		}
		else
		{
			// Fallback: use inventory selection if no controller found
			if (playerInventory != null)
			{
				var currentItem = playerInventory.GetCurrentItem();
				isFlashlightOn = currentItem != null && currentItem.item_type == itemType.Flashlight;
				isIlluminated = isFlashlightOn;
			}
		}

		// Handle persistent freezing logic
		if (stayFrozenUntilFlashlightOff)
		{
			if (isIlluminated && !wasFrozenByFlashlight)
			{
				// First time being illuminated - start freeze
				wasFrozenByFlashlight = true;
				Debug.Log($"[{name}] First illumination - FREEZING until flashlight turns OFF!");
			}
			else if (!isFlashlightOn && wasFrozenByFlashlight)
			{
				// Flashlight turned off - unfreeze
				wasFrozenByFlashlight = false;
				Debug.Log($"[{name}] Flashlight turned OFF - UNFREEZING!");
			}
			
			return wasFrozenByFlashlight;
		}
		else
		{
			// Standard behavior - freeze only when illuminated
			if (isIlluminated)
			{
				Debug.Log($"[{name}] Currently illuminated - FREEZING!");
			}
			return isIlluminated;
		}
    }

	void TryAttack()
    {
        if (isMovementPaused) return;

        if (Time.time - lastAttackTime < attackCooldown)
            return;

        lastAttackTime = Time.time;

        // Trigger attack animation (optional - for one-shot effects)
        if (animator != null && !string.IsNullOrEmpty(animTriggerSpellCast))
        {
            animator.SetTrigger(animTriggerSpellCast);
            Debug.Log($"[{name}] TRIGGERED ATTACK ANIMATION!");
        }

        if (chantAudio != null)
            chantAudio.Play();

        onChantAttack?.Invoke();

		// Execute ranged attack
		if (useHitscan)
		{
			PerformHitscan();
		}
		else if (projectilePrefab != null)
		{
			LaunchProjectile();
		}

        // Pause movement after attack to give player escape chance
        if (!isMovementPaused)
        {
            // CRITICAL: Set pause flag and stop agent IMMEDIATELY
            isMovementPaused = true;

            if (navMeshAgent != null && navMeshAgent.enabled && navMeshAgent.isOnNavMesh)
            {
                navMeshAgent.isStopped = true;
                navMeshAgent.ResetPath();
                navMeshAgent.velocity = UnityEngine.Vector3.zero;
                Debug.Log($"[{name}] Agent STOPPED immediately in TryAttack()");
            }

            StartCoroutine(PauseMovementAfterAttack(attackCooldown));
        }
    }

	void PerformHitscan()
	{
		Vector3 origin = transform.position + Vector3.up * 1.4f;
		Vector3 toPlayer = (player.position + Vector3.up * 1.6f) - origin;
		float maxDist = Mathf.Min(attackRange, toPlayer.magnitude + 0.5f);
		if (Physics.Raycast(origin, toPlayer.normalized, out RaycastHit hit, maxDist, attackObstructionMask, QueryTriggerInteraction.Ignore))
		{
			// Only apply if we actually hit player or one of its children
			if (hit.transform == player || hit.transform.IsChildOf(player))
			{
				player.SendMessage("TakeDamage", attackDamage, SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	void LaunchProjectile()
	{
		Vector3 origin = transform.position + Vector3.up * 1.4f;
		Quaternion rot = Quaternion.LookRotation((player.position + Vector3.up * 1.6f) - origin, Vector3.up);
		GameObject proj = Instantiate(projectilePrefab, origin, rot);
		Rigidbody rb = proj.GetComponent<Rigidbody>();
		if (rb != null)
		{
			rb.linearVelocity = proj.transform.forward * projectileSpeed;
		}
	}

    void FaceTarget(Vector3 worldPosition)
    {
        Vector3 toTarget = worldPosition - transform.position;
        toTarget.y = 0f;
        if (toTarget.sqrMagnitude < 0.0001f)
            return;
        Quaternion targetRot = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
    }

    void SetAnimationState(JenglotAnimationState newState)
    {
        if (currentAnimationState == newState) return;
        
        Debug.Log($"[{name}] Animation State: {currentAnimationState} → {newState}");
        currentAnimationState = newState;
        
        if (animator == null) return;
        
        // Reset all animation parameters first
        if (!string.IsNullOrEmpty(animParamIsIdle))
            animator.SetBool(animParamIsIdle, false);
        if (!string.IsNullOrEmpty(animParamIsWalking))
            animator.SetBool(animParamIsWalking, false);
        if (!string.IsNullOrEmpty(animParamIsAttacking))
            animator.SetBool(animParamIsAttacking, false);
        
        // Set the appropriate animation state
        switch (newState)
        {
            case JenglotAnimationState.Idle:
                if (!string.IsNullOrEmpty(animParamIsIdle))
                {
                    animator.SetBool(animParamIsIdle, true);
                    Debug.Log($"[{name}] Playing IDLE animation");
                }
                break;
                
            case JenglotAnimationState.Walking:
                if (!string.IsNullOrEmpty(animParamIsWalking))
                {
                    animator.SetBool(animParamIsWalking, true);
                    Debug.Log($"[{name}] Playing WALKING animation");
                }
                else
                {
                    // Fallback to idle if walking parameter doesn't exist
                    if (!string.IsNullOrEmpty(animParamIsIdle))
                        animator.SetBool(animParamIsIdle, true);
                }
                break;
                
            case JenglotAnimationState.SpellCasting:
                if (!string.IsNullOrEmpty(animParamIsAttacking))
                {
                    animator.SetBool(animParamIsAttacking, true);
                    Debug.Log($"[{name}] Playing ATTACKING animation (continuous)");
                }
                else
                {
                    // Fallback to idle if attacking parameter doesn't exist
                    if (!string.IsNullOrEmpty(animParamIsIdle))
                        animator.SetBool(animParamIsIdle, true);
                }
                break;
                
            case JenglotAnimationState.Frozen:
                if (!string.IsNullOrEmpty(animParamIsIdle))
                {
                    animator.SetBool(animParamIsIdle, true);
                    Debug.Log($"[{name}] Playing FROZEN animation (idle pose)");
                }
                break;
        }
        
        // Debug current animator state after parameter change
        var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        Debug.Log($"[{name}] After parameter change - IsName('SittingIdle'): {stateInfo.IsName("SittingIdle")}, IsName('AttackAnimation'): {stateInfo.IsName("AttackAnimation")}");
    }


    void ForceIdleAnimation()
    {
        if (animator != null && !string.IsNullOrEmpty(animParamIsIdle))
        {
            animator.SetBool(animParamIsIdle, true);
            Debug.Log($"[{name}] FORCED idle animation");
        }
    }

    /// <summary>
    /// Pauses enemy movement for specified duration after attacking player.
    /// Gives player a chance to escape.
    /// </summary>
    System.Collections.IEnumerator PauseMovementAfterAttack(float duration)
    {
        // Note: isMovementPaused is already set to true in TryAttack() before this coroutine starts
        Debug.Log($"[{name}] Pausing movement for {duration} seconds after attack");

        // Wait for cooldown duration
        yield return new WaitForSeconds(duration);

        // Resume movement
        if (navMeshAgent != null && navMeshAgent.enabled && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.isStopped = false;
            Debug.Log($"[{name}] Resuming movement after {duration}s pause");
        }

        isMovementPaused = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}


