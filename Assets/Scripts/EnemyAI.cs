using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Kamatayan AI Settings")]
    public float searchRange = 15f;        // How far can Kamatayan see the player
    public float attackRange = 2f;         // How close to attack
    public float moveSpeed = 4f;           // How fast Kamatayan moves
    public float searchSpeed = 2f;         // How fast Kamatayan searches
    public float attackDamage;
    public float patrolSpeed;
    public float chaseSpeed;
    public float detectionRange;
    public Transform[] patrolPoints;
    public float patrolWaitTime;
    public float fieldOfView;
    
    [Header("Search Behavior")]
    public Transform[] searchPoints;       // Points where Kamatayan searches
    public float waitAtPoint = 3f;         // How long to wait at each search point
    
    private Transform player;              // The player to hunt
    private int currentSearchPoint = 0;    // Which search point we're going to
    private float waitTimer = 0f;          // Timer for waiting
    private bool isWaiting = false;        // Are we waiting at a point?
    private bool foundPlayer = false;      // Did we find the player?
    
    void Start()
    {
        // Find the player to hunt
        player = GameObject.FindGameObjectWithTag("Player").transform;
        
        // Start at first search point
        if (searchPoints.Length > 0)
            transform.position = searchPoints[0].position;
    }
    
    void Update()
    {
        // Always check if we can see the player
        CheckForPlayer();
        
        // If we found the player, hunt them!
        if (foundPlayer)
        {
            HuntPlayer();
        }
        else
        {
            // If no player found, search the area
            SearchArea();
        }
    }
    
    void CheckForPlayer()
    {
        if (player == null) return;
        
        // Calculate distance to player
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // Can we see the player?
        if (distanceToPlayer <= searchRange)
        {
            foundPlayer = true;
            Debug.Log("Kamatayan found the player!");
        }
        else
        {
            // Player is too far, stop hunting
            foundPlayer = false;
        }
    }
    
    void HuntPlayer()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // Are we close enough to attack?
        if (distanceToPlayer <= attackRange)
        {
            AttackPlayer();
        }
        else
        {
            // Move towards the player
            ChasePlayer();
        }
    }
    
    void ChasePlayer()
    {
        // Calculate direction to player
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        
        // Move towards player
        transform.position += directionToPlayer * moveSpeed * Time.deltaTime;
        
        // Face the player
        transform.LookAt(player);
        
        Debug.Log("Kamatayan is chasing the player!");
    }
    
    void AttackPlayer()
    {
        // Face the player
        transform.LookAt(player);
        
        // Attack the player
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(10f);
            Debug.Log("Kamatayan attacked the player!");
        }
    }
    
    void SearchArea()
    {
        // If no search points, just stand still
        if (searchPoints.Length == 0) return;
        
        if (isWaiting)
        {
            // Wait at current search point
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0)
            {
                isWaiting = false;
                // Go to next search point
                currentSearchPoint = (currentSearchPoint + 1) % searchPoints.Length;
            }
        }
        else
        {
            // Move towards current search point
            Vector3 targetPoint = searchPoints[currentSearchPoint].position;
            Vector3 direction = (targetPoint - transform.position).normalized;
            
            // Move towards search point
            transform.position += direction * searchSpeed * Time.deltaTime;
            
            // Face the direction we're moving
            transform.LookAt(targetPoint);
            
            // Check if we reached the search point
            if (Vector3.Distance(transform.position, targetPoint) < 0.5f)
            {
                isWaiting = true;
                waitTimer = waitAtPoint;
                Debug.Log("Kamatayan is searching at point " + (currentSearchPoint + 1));
            }
        }
    }
    
    // Draw gizmos to see search range and points
    void OnDrawGizmosSelected()
    {
        // Search range (yellow)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, searchRange);
        
        // Attack range (red)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Search points (green)
        Gizmos.color = Color.green;
        for (int i = 0; i < searchPoints.Length; i++)
        {
            if (searchPoints[i] != null)
            {
                Gizmos.DrawWireSphere(searchPoints[i].position, 0.5f);
                // Draw line to next point
                if (i < searchPoints.Length - 1 && searchPoints[i + 1] != null)
                {
                    Gizmos.DrawLine(searchPoints[i].position, searchPoints[i + 1].position);
                }
            }
        }
    }
} 