using UnityEngine;
using System.Collections;

/// <summary>
/// Specific debugger for Dullahan attack system issues
/// Since health system works, this focuses on attack-specific problems
/// </summary>
public class DullahanAttackDebugger : MonoBehaviour
{
    [Header("References")]
    public DullahanMeleeAttack dullahanAttack;
    public PlayerHealthSystem playerHealth;
    public DullahanChaseSystem chaseSystem;
    
    [Header("Debug Settings")]
    public bool enableRealTimeLogging = true;
    public bool showAttackRange = true;
    public bool forceAttackOnProximity = false;
    
    [Header("Override Settings")]
    public bool overrideAttackRange = false;
    public float debugAttackRange = 5f;
    public bool overrideCanAttack = false;
    public bool debugCanAttack = true;
    
    private Transform playerTransform;
    private bool lastPlayerInRange = false;
    private float lastAttackTime = 0f;
    
    void Start()
    {
        InitializeDebugger();
    }
    
    void Update()
    {
        if (dullahanAttack == null) return;
        
        ApplyDebugOverrides();
        MonitorAttackConditions();
        HandleDebugInput();
        
        if (showAttackRange)
        {
            DrawDebugRange();
        }
    }
    
    private void InitializeDebugger()
    {
        // Auto-find references
        if (dullahanAttack == null)
            dullahanAttack = FindObjectOfType<DullahanMeleeAttack>();
            
        if (playerHealth == null)
            playerHealth = FindObjectOfType<PlayerHealthSystem>();
            
        if (chaseSystem == null)
            chaseSystem = FindObjectOfType<DullahanChaseSystem>();
        
        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
        
        RunAttackDiagnostics();
    }
    
    private void RunAttackDiagnostics()
    {
        Debug.Log("⚔️ === DULLAHAN ATTACK DIAGNOSTICS ===");
        
        if (dullahanAttack == null)
        {
            Debug.LogError("❌ DullahanMeleeAttack not found!");
            return;
        }
        
        // Check basic settings
        Debug.Log($"🎯 Attack Range: {dullahanAttack.attackRange}");
        Debug.Log($"💥 Attack Damage: {dullahanAttack.attackDamage}");
        Debug.Log($"⏱️ Attack Cooldown: {dullahanAttack.attackCooldown}");
        Debug.Log($"🔄 Can Attack: {dullahanAttack.canAttack}");
        
        // Check references
        if (dullahanAttack.playerHealthSystem == null)
        {
            Debug.LogError("❌ DullahanMeleeAttack.playerHealthSystem is NULL!");
        }
        else
        {
            Debug.Log("✅ PlayerHealthSystem reference assigned");
        }
        
        // Check attack patterns
        if (dullahanAttack.attackPatterns == null || dullahanAttack.attackPatterns.Length == 0)
        {
            Debug.LogError("❌ No attack patterns assigned!");
        }
        else
        {
            Debug.Log($"✅ {dullahanAttack.attackPatterns.Length} attack patterns found:");
            for (int i = 0; i < dullahanAttack.attackPatterns.Length; i++)
            {
                var pattern = dullahanAttack.attackPatterns[i];
                Debug.Log($"  Pattern {i}: {pattern.patternName} - Damage: {pattern.damage}, Range: {pattern.range}");
            }
        }
        
        // Check player detection
        if (playerTransform == null)
        {
            Debug.LogError("❌ Player transform not found! Check Player tag.");
        }
        else
        {
            Debug.Log($"✅ Player found: {playerTransform.name}");
        }
        
        // Check chase system integration
        if (chaseSystem == null)
        {
            Debug.LogWarning("⚠️ DullahanChaseSystem not found - attack patterns may not work correctly");
        }
        else
        {
            Debug.Log("✅ DullahanChaseSystem found");
            
            // Test GetCurrentIntensity method
            try
            {
                float intensity = chaseSystem.GetCurrentIntensity();
                Debug.Log($"✅ Chase intensity: {intensity}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ GetCurrentIntensity() failed: {e.Message}");
                Debug.LogError("This might cause attack pattern selection to fail!");
            }
        }
    }
    
    private void ApplyDebugOverrides()
    {
        if (overrideAttackRange)
        {
            dullahanAttack.attackRange = debugAttackRange;
        }
        
        if (overrideCanAttack)
        {
            dullahanAttack.canAttack = debugCanAttack;
        }
    }
    
    private void MonitorAttackConditions()
    {
        if (playerTransform == null || dullahanAttack == null) return;
        
        // Calculate distance
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        bool playerInRange = distance <= dullahanAttack.attackRange;
        
        // Log range changes
        if (playerInRange != lastPlayerInRange)
        {
            if (playerInRange)
            {
                Debug.Log($"🎯 Player ENTERED attack range! Distance: {distance:F2}");
                if (forceAttackOnProximity && dullahanAttack.canAttack)
                {
                    Debug.Log("🔥 Auto-triggering attack due to proximity!");
                    dullahanAttack.StartAttack();
                }
            }
            else
            {
                Debug.Log($"🚶 Player LEFT attack range! Distance: {distance:F2}");
            }
            lastPlayerInRange = playerInRange;
        }
        
        // Log attack attempts
        if (enableRealTimeLogging && playerInRange)
        {
            bool isAttacking = dullahanAttack.IsAttacking();
            float timeUntilNext = dullahanAttack.GetTimeUntilNextAttack();
            
            if (Time.time - lastAttackTime > 2f) // Log every 2 seconds
            {
                Debug.Log($"📊 Attack Status - In Range: {playerInRange}, Can Attack: {dullahanAttack.canAttack}, Is Attacking: {isAttacking}, Cooldown: {timeUntilNext:F1}s");
                lastAttackTime = Time.time;
            }
        }
    }
    
    private void HandleDebugInput()
    {
        // Force attack
        if (Input.GetKeyDown(KeyCode.X))
        {
            Debug.Log("🔥 === FORCE ATTACK TRIGGERED ===");
            if (dullahanAttack != null)
            {
                Debug.Log($"Pre-attack state: CanAttack={dullahanAttack.canAttack}, IsAttacking={dullahanAttack.IsAttacking()}");
                dullahanAttack.StartAttack();
            }
        }
        
        // Toggle can attack
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (dullahanAttack != null)
            {
                dullahanAttack.canAttack = !dullahanAttack.canAttack;
                Debug.Log($"🔄 Toggled CanAttack to: {dullahanAttack.canAttack}");
            }
        }
        
        // Teleport player to attack range
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (playerTransform != null)
            {
                Vector3 attackPosition = transform.position + transform.forward * (dullahanAttack.attackRange - 0.5f);
                playerTransform.position = attackPosition;
                Debug.Log($"📍 Teleported player to attack range! Distance now: {Vector3.Distance(transform.position, playerTransform.position):F2}");
            }
        }
        
        // Test damage directly
        if (Input.GetKeyDown(KeyCode.D))
        {
            Debug.Log("💥 === TESTING DIRECT DAMAGE FROM DULLAHAN ===");
            if (dullahanAttack != null && dullahanAttack.playerHealthSystem != null)
            {
                Debug.Log("Calling playerHealthSystem.TakeDamage(1) directly...");
                dullahanAttack.playerHealthSystem.TakeDamage(1);
                Debug.Log("Direct damage call completed");
            }
        }
    }
    
    private void DrawDebugRange()
    {
        if (dullahanAttack == null) return;
        
        // Draw attack range using multiple lines to simulate a circle
        DrawDebugCircle(transform.position, dullahanAttack.attackRange, Color.yellow);
        
        // Draw line to player if in range
        if (playerTransform != null)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            bool inRange = distance <= dullahanAttack.attackRange;
            
            Color lineColor = inRange ? Color.red : Color.gray;
            Debug.DrawLine(transform.position, playerTransform.position, lineColor);
        }
    }
    
    private void DrawDebugCircle(Vector3 center, float radius, Color color)
    {
        int segments = 16;
        float angleStep = 360f / segments;
        
        for (int i = 0; i < segments; i++)
        {
            float angle1 = i * angleStep * Mathf.Deg2Rad;
            float angle2 = (i + 1) * angleStep * Mathf.Deg2Rad;
            
            Vector3 point1 = center + new Vector3(Mathf.Cos(angle1) * radius, 0, Mathf.Sin(angle1) * radius);
            Vector3 point2 = center + new Vector3(Mathf.Cos(angle2) * radius, 0, Mathf.Sin(angle2) * radius);
            
            Debug.DrawLine(point1, point2, color);
        }
    }
    
    // Method to simulate what happens in DullahanMeleeAttack.CheckForHit()
    public void TestAttackHitDetection()
    {
        if (dullahanAttack == null || playerTransform == null) return;
        
        Debug.Log("🎯 === TESTING HIT DETECTION ===");
        
        // Get current attack pattern (simulate what DullahanMeleeAttack does)
        var currentPattern = dullahanAttack.attackPatterns[0]; // Basic attack pattern
        
        // Check distance (same logic as CheckForHit)
        float distanceToPlayer = Vector3.Distance(dullahanAttack.attackPoint.position, playerTransform.position);
        Debug.Log($"Distance to player: {distanceToPlayer}");
        Debug.Log($"Attack pattern range: {currentPattern.range}");
        Debug.Log($"Attack point position: {dullahanAttack.attackPoint.position}");
        Debug.Log($"Player position: {playerTransform.position}");
        
        if (distanceToPlayer <= currentPattern.range)
        {
            Debug.Log("✅ Player would be HIT!");
        }
        else
        {
            Debug.Log("❌ Player would MISS!");
        }
    }
    
    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(Screen.width - 300, 10, 290, 400));
        GUILayout.Label("=== DULLAHAN ATTACK DEBUG ===", GUI.skin.box);
        
        if (dullahanAttack != null)
        {
            GUILayout.Label($"🔄 Can Attack: {dullahanAttack.canAttack}");
            GUILayout.Label($"⚔️ Is Attacking: {dullahanAttack.IsAttacking()}");
            GUILayout.Label($"📏 Attack Range: {dullahanAttack.attackRange:F1}");
            GUILayout.Label($"⏰ Next Attack: {dullahanAttack.GetTimeUntilNextAttack():F1}s");
            
            if (playerTransform != null)
            {
                float distance = Vector3.Distance(transform.position, playerTransform.position);
                bool inRange = distance <= dullahanAttack.attackRange;
                GUILayout.Label($"📍 Distance: {distance:F2}");
                GUILayout.Label($"🎯 In Range: {inRange}");
            }
        }
        
        GUILayout.Space(10);
        GUILayout.Label("=== CONTROLS ===");
        GUILayout.Label("X - Force Attack");
        GUILayout.Label("C - Toggle Can Attack");
        GUILayout.Label("T - Teleport to Range");
        GUILayout.Label("D - Test Direct Damage");
        
        GUILayout.Space(10);
        if (GUILayout.Button("Test Hit Detection"))
        {
            TestAttackHitDetection();
        }
        
        GUILayout.EndArea();
    }
}
