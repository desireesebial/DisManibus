using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(EnemyAI))]
[RequireComponent(typeof(NavMeshAgent))]
public class EnemySetup : MonoBehaviour
{
    [Header("Enemy Configuration")]
    public string enemyName = "Enemy";
    public float health = 100f;
    public float damage = 20f;
    public float speed = 3f;
    
    [Header("Detection")]
    public float detectionRange = 10f;
    public float attackRange = 2f;
    public float fieldOfView = 120f;
    
    [Header("Patrol")]
    public Transform[] patrolPoints;
    public float patrolWaitTime = 2f;
    
    void Awake()
    {
        SetupEnemy();
    }
    
    void SetupEnemy()
    {
        // Set tag
        gameObject.tag = "Enemy";
        
        // Setup EnemyAI component
        EnemyAI enemyAI = GetComponent<EnemyAI>();
        if (enemyAI != null)
        {
            enemyAI.patrolPoints = patrolPoints;
            enemyAI.patrolWaitTime = patrolWaitTime;
            enemyAI.patrolSpeed = speed;
            enemyAI.chaseSpeed = speed * 2f;
            enemyAI.detectionRange = detectionRange;
            enemyAI.attackRange = attackRange;
            enemyAI.fieldOfView = fieldOfView;
            enemyAI.attackDamage = damage;
        }
        
        // Setup NavMeshAgent
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.speed = speed;
            agent.angularSpeed = 120f;
            agent.acceleration = 8f;
            agent.stoppingDistance = 1f;
            agent.radius = 0.5f;
            agent.height = 2f;
        }
        
        // Add collider if not present
        Collider collider = GetComponent<Collider>();
        if (collider == null)
        {
            CapsuleCollider capsuleCollider = gameObject.AddComponent<CapsuleCollider>();
            capsuleCollider.height = 2f;
            capsuleCollider.radius = 0.5f;
            capsuleCollider.center = Vector3.up;
        }
        
        // Add rigidbody if not present (for physics interactions)
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true; // Let NavMeshAgent handle movement
            rb.useGravity = false;
        }
        
        Debug.Log($"Enemy '{enemyName}' setup complete!");
    }
    
    // Method to create a basic enemy prefab
    [ContextMenu("Create Basic Enemy")]
    public static GameObject CreateBasicEnemy()
    {
        GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        enemy.name = "Enemy";
        
        // Add required components
        enemy.AddComponent<EnemyAI>();
        enemy.AddComponent<NavMeshAgent>();
        enemy.AddComponent<EnemySetup>();
        
        // Setup basic appearance
        Renderer renderer = enemy.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.red;
        }
        
        return enemy;
    }
} 