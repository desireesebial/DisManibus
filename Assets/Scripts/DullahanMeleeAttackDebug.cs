using UnityEngine;

/// <summary>
/// Temporary debug script to help diagnose Dullahan attack issues
/// Attach this to the Dullahan GameObject alongside DullahanMeleeAttack
/// </summary>
public class DullahanMeleeAttackDebug : MonoBehaviour
{
    public DullahanMeleeAttack meleeAttack;
    public PlayerHealthSystem playerHealth;
    
    [Header("Debug Settings")]
    public bool enableDebugLogging = true;
    public bool showDebugGUI = true;
    public KeyCode forceAttackKey = KeyCode.X;
    public KeyCode testDamageKey = KeyCode.Z;
    
    private void Start()
    {
        // Auto-find references
        if (meleeAttack == null)
            meleeAttack = GetComponent<DullahanMeleeAttack>();
            
        if (playerHealth == null)
            playerHealth = FindObjectOfType<PlayerHealthSystem>();
            
        // Initial diagnostics
        RunDiagnostics();
    }
    
    private void Update()
    {
        // Debug controls
        if (Input.GetKeyDown(forceAttackKey))
        {
            Debug.Log("=== FORCING DULLAHAN ATTACK ===");
            if (meleeAttack != null)
            {
                meleeAttack.StartAttack();
            }
        }
        
        if (Input.GetKeyDown(testDamageKey))
        {
            Debug.Log("=== TESTING DIRECT DAMAGE ===");
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(1);
            }
        }
    }
    
    private void RunDiagnostics()
    {
        if (!enableDebugLogging) return;
        
        Debug.Log("=== DULLAHAN ATTACK DIAGNOSTICS ===");
        
        // Check DullahanMeleeAttack
        if (meleeAttack == null)
        {
            Debug.LogError("❌ DullahanMeleeAttack component NOT FOUND!");
            return;
        }
        Debug.Log("✅ DullahanMeleeAttack component found");
        
        // Check PlayerHealthSystem reference
        if (meleeAttack.playerHealthSystem == null)
        {
            Debug.LogError("❌ DullahanMeleeAttack.playerHealthSystem is NULL!");
        }
        else
        {
            Debug.Log("✅ DullahanMeleeAttack.playerHealthSystem is assigned");
        }
        
        // Check PlayerHealthSystem exists
        if (playerHealth == null)
        {
            Debug.LogError("❌ PlayerHealthSystem NOT FOUND in scene!");
        }
        else
        {
            Debug.Log($"✅ PlayerHealthSystem found - Health: {playerHealth.GetCurrentHealth()}/{playerHealth.GetMaxHealth()}");
        }
        
        // Check Player tag
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("❌ No GameObject with 'Player' tag found!");
        }
        else
        {
            Debug.Log($"✅ Player GameObject found: {player.name}");
        }
        
        // Check attack patterns
        if (meleeAttack.attackPatterns == null || meleeAttack.attackPatterns.Length == 0)
        {
            Debug.LogError("❌ No attack patterns assigned!");
        }
        else
        {
            Debug.Log($"✅ {meleeAttack.attackPatterns.Length} attack patterns found");
        }
        
        Debug.Log("=== DIAGNOSTIC COMPLETE ===");
        Debug.Log($"Press {forceAttackKey} to force attack, {testDamageKey} to test direct damage");
    }
    
    private void OnGUI()
    {
        if (!showDebugGUI) return;
        
        GUILayout.BeginArea(new Rect(10, 100, 300, 200));
        GUILayout.Label("Dullahan Attack Debug");
        
        if (meleeAttack != null)
        {
            GUILayout.Label($"Can Attack: {meleeAttack.canAttack}");
            GUILayout.Label($"Is Attacking: {meleeAttack.IsAttacking()}");
            GUILayout.Label($"Player In Range: {meleeAttack.IsPlayerInRange()}");
            GUILayout.Label($"Time Until Next: {meleeAttack.GetTimeUntilNextAttack():F1}s");
        }
        
        if (playerHealth != null)
        {
            GUILayout.Label($"Player Health: {playerHealth.GetCurrentHealth()}/{playerHealth.GetMaxHealth()}");
            GUILayout.Label($"Invulnerable: {playerHealth.IsInvulnerable()}");
        }
        
        GUILayout.EndArea();
    }
}
