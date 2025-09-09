using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

/// <summary>
/// Jenglot behavior:
/// - Detects and follows the Player within a radius.
/// - If the Player is using a flashlight (selected in PlayerInventory), Jenglot freezes and does not move.
/// - If not frozen and in attack range, chants a dark magic attack (event/animation/audio hook).
///
/// Setup:
/// - Add this component to the Jenglot GameObject.
/// - Add/assign a NavMeshAgent to the Jenglot.
/// - Assign the Player (or leave null to auto-find by tag "Player").
/// - Optionally assign Animator and AudioSource and hook up the OnChantAttack event.
/// - Ensure the Player has the PlayerInventory component provided in this project.
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
    [SerializeField] float attackRange = 2.2f;
    [SerializeField] float attackCooldown = 2.5f;
    [SerializeField] int attackDamage = 10; // used with SendMessage if receiver implements TakeDamage(int)
    [SerializeField] UnityEvent onChantAttack; // hook VFX/SFX or gameplay here

    // Animator parameter names (optional)
    [SerializeField] string animParamIsMoving = "IsMoving";
    [SerializeField] string animTriggerChant = "Chant";

    PlayerInventory playerInventory;
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
            playerInventory = player.GetComponent<PlayerInventory>();
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
        if (playerInventory == null)
            return false;

        // Frozen if the currently selected inventory item is a Flashlight.
        // This uses your PlayerInventory, no dependence on other inventory systems.
        var currentItem = playerInventory.GetCurrentItem();
        if (currentItem == null)
            return false;

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

        // Optional generic damage call to player, if they implement a compatible method.
        // The receiver may implement: void TakeDamage(int amount)
        player.SendMessage("TakeDamage", attackDamage, SendMessageOptions.DontRequireReceiver);
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


