using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject enemyPrefab;
    public Transform[] spawnPoints;
    public int maxEnemies = 5;
    public float spawnDelay = 5f;
    
    [Header("Enemy Settings")]
    public float enemyHealth = 100f;
    public float enemyDamage = 20f;
    public float enemySpeed = 3f;
    
    private float spawnTimer = 0f;
    private int currentEnemyCount = 0;
    
    void Start()
    {
        // Spawn initial enemies
        for (int i = 0; i < Mathf.Min(maxEnemies, spawnPoints.Length); i++)
        {
            SpawnEnemy();
        }
    }
    
    void Update()
    {
        // Count current enemies
        currentEnemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;
        
        // Spawn new enemies if needed
        if (currentEnemyCount < maxEnemies)
        {
            spawnTimer += Time.deltaTime;
            if (spawnTimer >= spawnDelay)
            {
                SpawnEnemy();
                spawnTimer = 0f;
            }
        }
    }
    
    void SpawnEnemy()
    {
        if (enemyPrefab == null || spawnPoints.Length == 0) return;
        
        // Select random spawn point
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        
        // Spawn enemy
        GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        enemy.tag = "Enemy";
        
        // Setup enemy components
        EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
        if (enemyAI != null)
        {
            // You can customize enemy stats here
            enemyAI.attackDamage = enemyDamage;
            enemyAI.patrolSpeed = enemySpeed;
            enemyAI.chaseSpeed = enemySpeed * 2f;
        }
        
        // Add NavMeshAgent if not present
        UnityEngine.AI.NavMeshAgent agent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent == null)
        {
            agent = enemy.AddComponent<UnityEngine.AI.NavMeshAgent>();
        }
        
        Debug.Log($"Enemy spawned at {spawnPoint.position}");
    }
    
    // Public method to spawn enemy at specific position
    public void SpawnEnemyAtPosition(Vector3 position)
    {
        if (enemyPrefab == null) return;
        
        GameObject enemy = Instantiate(enemyPrefab, position, Quaternion.identity);
        enemy.tag = "Enemy";
        
        EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
        if (enemyAI != null)
        {
            enemyAI.attackDamage = enemyDamage;
            enemyAI.patrolSpeed = enemySpeed;
            enemyAI.chaseSpeed = enemySpeed * 2f;
        }
        
        UnityEngine.AI.NavMeshAgent agent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent == null)
        {
            agent = enemy.AddComponent<UnityEngine.AI.NavMeshAgent>();
        }
    }
} 