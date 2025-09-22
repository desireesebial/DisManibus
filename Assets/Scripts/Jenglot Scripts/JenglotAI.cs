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
    [Tooltip("Animator trigger for spell casting animation - matches your AttackAnimation state")]
    [SerializeField] string animTriggerSpellCast = "Attack";

	PlayerInventory playerInventory;
	FlashlightController flashlightController;
    float lastAttackTime;
	bool hasDetectedPlayer;
    JenglotAnimationState currentAnimationState = JenglotAnimationState.Idle;

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
        bool hasAttack = HasParameter(animTriggerSpellCast);

        Debug.Log($"[{name}] Animation Parameters: Idle={hasIdle}, Attack={hasAttack}");
        
        if (!hasIdle || !hasAttack)
        {
            Debug.LogWarning($"[{name}] Missing animation parameters! Add 'Idle' (Bool) and 'Attack' (Trigger) to your Animator Controller.");
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
		// Prefer geometric check against actual flashlight light cone if available
		if (flashlightController != null)
		{
			// Ignore self so our own collider does not count as an obstruction
			bool illuminated = flashlightController.IsIlluminating(transform, true, lineOfSightObstructionMask);
			if (illuminated)
			{
				Debug.Log($"[{name}] Illuminated by flashlight - FREEZING!");
			}
			return illuminated;
		}

		// Fallback: use inventory selection if no controller found
		if (playerInventory == null)
			return false;
		var currentItem = playerInventory.GetCurrentItem();
		if (currentItem == null) return false;
		bool hasFlashlight = currentItem.item_type == itemType.Flashlight;
		if (hasFlashlight)
		{
			Debug.Log($"[{name}] Player has flashlight selected - FREEZING!");
		}
		return hasFlashlight;
    }

	void TryAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown)
            return;

        lastAttackTime = Time.time;

        if (animator != null && !string.IsNullOrEmpty(animTriggerSpellCast))
            animator.SetTrigger(animTriggerSpellCast);

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
        
        // Handle different states properly
        switch (newState)
        {
            case JenglotAnimationState.Idle:
            case JenglotAnimationState.Walking:
            case JenglotAnimationState.Frozen:
                // All use the sitting idle animation
                if (!string.IsNullOrEmpty(animParamIsIdle))
                {
                    animator.SetBool(animParamIsIdle, true);
                    Debug.Log($"[{name}] Set {animParamIsIdle} = true (State: {newState})");
                }
                break;
                
            case JenglotAnimationState.SpellCasting:
                // Attack trigger will be fired in TryAttack(), keep idle as base
                if (!string.IsNullOrEmpty(animParamIsIdle))
                {
                    animator.SetBool(animParamIsIdle, true);
                    Debug.Log($"[{name}] Set {animParamIsIdle} = true (State: {newState} - ready for attack trigger)");
                }
                break;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}


