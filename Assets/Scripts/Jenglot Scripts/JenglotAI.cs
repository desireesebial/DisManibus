using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

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

    // Animator parameter names (optional)
    [SerializeField] string animParamIsMoving = "IsMoving";
    [SerializeField] string animTriggerChant = "Chant";

	PlayerInventory playerInventory;
	FlashlightController flashlightController;
    float lastAttackTime;

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
            SetMoving(false);
            navMeshAgent.isStopped = true;
            return;
        }

        if (frozenByFlashlight)
        {
            // Freeze movement due to flashlight
            navMeshAgent.isStopped = true;
            navMeshAgent.velocity = Vector3.zero;
            SetMoving(false);
            FaceTarget(player.position);
            return;
        }

		float distance = Vector3.Distance(transform.position, player.position);
		if (distance <= attackRange)
        {
            // Attack behavior
            navMeshAgent.isStopped = true;
            navMeshAgent.velocity = Vector3.zero;
            SetMoving(false);
            FaceTarget(player.position);
			TryAttack();
        }
        else
        {
            // Follow behavior
            navMeshAgent.isStopped = false;
            navMeshAgent.SetDestination(player.position);
            SetMoving(true);
        }
    }

    bool IsPlayerDetected()
    {
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance > detectionRadius)
            return false;

        if (!requireLineOfSight)
            return true;

        Vector3 origin = transform.position + Vector3.up * 1.6f;
        Vector3 dir = (player.position + Vector3.up * 1.6f) - origin;
        float maxDist = dir.magnitude;
        if (Physics.Raycast(origin, dir.normalized, out RaycastHit hit, maxDist, lineOfSightObstructionMask, QueryTriggerInteraction.Ignore))
        {
            // If we hit something that isn't the player, LOS is blocked
            if (hit.transform != player)
                return false;
        }
        return true;
    }

	bool IsFrozenByPlayerFlashlight()
    {
		// Prefer geometric check against actual flashlight light cone if available
		if (flashlightController != null)
		{
			// Ignore self so our own collider does not count as an obstruction
			return flashlightController.IsIlluminating(transform, true, lineOfSightObstructionMask);
		}

		// Fallback: use inventory selection if no controller found
		if (playerInventory == null)
			return false;
		var currentItem = playerInventory.GetCurrentItem();
		if (currentItem == null) return false;
		return currentItem.item_type == itemType.Flashlight;
    }

	void TryAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown)
            return;

        lastAttackTime = Time.time;

        if (animator != null && !string.IsNullOrEmpty(animTriggerChant))
            animator.SetTrigger(animTriggerChant);

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
			rb.velocity = proj.transform.forward * projectileSpeed;
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

    void SetMoving(bool isMoving)
    {
        if (animator != null && !string.IsNullOrEmpty(animParamIsMoving))
            animator.SetBool(animParamIsMoving, isMoving);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}


