using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Comprehensive Health System Debugger
/// Attach this to any GameObject to debug health-related issues
/// </summary>
public class HealthSystemDebugger : MonoBehaviour
{
    [Header("System References")]
    public PlayerHealthSystem playerHealthSystem;
    public DullahanMeleeAttack dullahanAttack;
    
    [Header("Debug Settings")]
    public bool enableDebugLogging = true;
    public bool showDebugGUI = true;
    public bool enableDebugControls = true;
    
    [Header("Debug Controls")]
    public KeyCode damageKey = KeyCode.H;
    public KeyCode healKey = KeyCode.J;
    public KeyCode fullHealKey = KeyCode.K;
    public KeyCode forceAttackKey = KeyCode.X;
    public KeyCode toggleInvulnerabilityKey = KeyCode.I;
    
    [Header("Debug Display")]
    public bool showHealthBars = true;
    public bool showReferences = true;
    public bool showTimers = true;
    
    private bool manualInvulnerability = false;
    
    void Start()
    {
        InitializeDebugger();
    }
    
    void Update()
    {
        if (enableDebugControls)
        {
            HandleDebugInput();
        }
        
        if (enableDebugLogging)
        {
            MonitorHealthChanges();
        }
    }
    
    private void InitializeDebugger()
    {
        // Auto-find systems if not assigned
        if (playerHealthSystem == null)
        {
            playerHealthSystem = FindObjectOfType<PlayerHealthSystem>();
        }
        
        if (dullahanAttack == null)
        {
            dullahanAttack = FindObjectOfType<DullahanMeleeAttack>();
        }
        
        // Run initial diagnostics
        RunHealthSystemDiagnostics();
        
        Debug.Log("=== HEALTH SYSTEM DEBUGGER INITIALIZED ===");
        LogDebugControls();
    }
    
    private void RunHealthSystemDiagnostics()
    {
        Debug.Log("🔍 === HEALTH SYSTEM DIAGNOSTICS ===");
        
        // 1. Check PlayerHealthSystem
        if (playerHealthSystem == null)
        {
            Debug.LogError("❌ PlayerHealthSystem NOT FOUND!");
            return;
        }
        Debug.Log("✅ PlayerHealthSystem found");
        
        // 2. Check Health Values
        Debug.Log($"💖 Current Health: {playerHealthSystem.GetCurrentHealth()}/{playerHealthSystem.GetMaxHealth()}");
        Debug.Log($"🛡️ Invulnerable: {playerHealthSystem.IsInvulnerable()}");
        Debug.Log($"💀 Is Dead: {playerHealthSystem.IsDead()}");
        Debug.Log($"⚠️ Critical Health: {playerHealthSystem.IsCriticalHealth()}");
        
        // 3. Check UI Components
        CheckHealthUI();
        
        // 4. Check Attack System
        CheckAttackSystem();
        
        // 5. Check Player GameObject
        CheckPlayerGameObject();
        
        Debug.Log("🔍 === DIAGNOSTICS COMPLETE ===");
    }
    
    private void CheckHealthUI()
    {
        Debug.Log("🎨 === CHECKING HEALTH UI ===");
        
        if (playerHealthSystem.healthUI == null)
        {
            Debug.LogWarning("⚠️ Health UI GameObject not assigned");
        }
        else
        {
            Debug.Log($"✅ Health UI assigned: {playerHealthSystem.healthUI.name}");
        }
        
        // Check health bars array
        if (playerHealthSystem.healthBars == null || playerHealthSystem.healthBars.Length == 0)
        {
            Debug.LogError("❌ Health Bars array is empty or null!");
        }
        else
        {
            Debug.Log($"✅ Health Bars array has {playerHealthSystem.healthBars.Length} elements");
            
            for (int i = 0; i < playerHealthSystem.healthBars.Length; i++)
            {
                if (playerHealthSystem.healthBars[i] == null)
                {
                    Debug.LogError($"❌ Health Bar {i} is NULL!");
                }
                else
                {
                    Image healthBar = playerHealthSystem.healthBars[i];
                    Debug.Log($"✅ Health Bar {i}: {healthBar.name} - Fill: {healthBar.fillAmount}");
                }
            }
        }
        
        // Check health text
        if (playerHealthSystem.healthText == null)
        {
            Debug.LogWarning("⚠️ Health Text not assigned");
        }
        else
        {
            Debug.Log($"✅ Health Text: {playerHealthSystem.healthText.text}");
        }
    }
    
    private void CheckAttackSystem()
    {
        Debug.Log("⚔️ === CHECKING ATTACK SYSTEM ===");
        
        if (dullahanAttack == null)
        {
            Debug.LogWarning("⚠️ DullahanMeleeAttack not found");
            return;
        }
        
        Debug.Log($"✅ DullahanMeleeAttack found on: {dullahanAttack.gameObject.name}");
        Debug.Log($"🎯 Can Attack: {dullahanAttack.canAttack}");
        Debug.Log($"📏 Attack Range: {dullahanAttack.attackRange}");
        Debug.Log($"💥 Attack Damage: {dullahanAttack.attackDamage}");
        
        // Check reference to PlayerHealthSystem
        if (dullahanAttack.playerHealthSystem == null)
        {
            Debug.LogError("❌ DullahanMeleeAttack.playerHealthSystem is NULL!");
        }
        else
        {
            Debug.Log("✅ DullahanMeleeAttack has PlayerHealthSystem reference");
        }
        
        // Check attack patterns
        if (dullahanAttack.attackPatterns == null || dullahanAttack.attackPatterns.Length == 0)
        {
            Debug.LogError("❌ No attack patterns assigned!");
        }
        else
        {
            Debug.Log($"✅ {dullahanAttack.attackPatterns.Length} attack patterns found");
        }
    }
    
    private void CheckPlayerGameObject()
    {
        Debug.Log("👤 === CHECKING PLAYER GAMEOBJECT ===");
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("❌ No GameObject with 'Player' tag found!");
        }
        else
        {
            Debug.Log($"✅ Player GameObject found: {player.name}");
            
            // Check if PlayerHealthSystem is on the player
            PlayerHealthSystem healthComp = player.GetComponent<PlayerHealthSystem>();
            if (healthComp == null)
            {
                Debug.LogWarning("⚠️ PlayerHealthSystem not on Player GameObject");
            }
            else
            {
                Debug.Log("✅ PlayerHealthSystem found on Player GameObject");
            }
        }
    }
    
    private void HandleDebugInput()
    {
        // Damage player
        if (Input.GetKeyDown(damageKey))
        {
            Debug.Log("🔥 === MANUAL DAMAGE TEST ===");
            if (playerHealthSystem != null)
            {
                int healthBefore = playerHealthSystem.GetCurrentHealth();
                playerHealthSystem.TakeDamage(1);
                int healthAfter = playerHealthSystem.GetCurrentHealth();
                Debug.Log($"Health Before: {healthBefore}, After: {healthAfter}");
            }
        }
        
        // Heal player
        if (Input.GetKeyDown(healKey))
        {
            Debug.Log("💚 === MANUAL HEAL TEST ===");
            if (playerHealthSystem != null)
            {
                int healthBefore = playerHealthSystem.GetCurrentHealth();
                playerHealthSystem.Heal(1);
                int healthAfter = playerHealthSystem.GetCurrentHealth();
                Debug.Log($"Health Before: {healthBefore}, After: {healthAfter}");
            }
        }
        
        // Full heal
        if (Input.GetKeyDown(fullHealKey))
        {
            Debug.Log("✨ === FULL HEAL TEST ===");
            if (playerHealthSystem != null)
            {
                playerHealthSystem.RestoreFullHealth();
                Debug.Log($"Health restored to: {playerHealthSystem.GetCurrentHealth()}");
            }
        }
        
        // Force attack
        if (Input.GetKeyDown(forceAttackKey))
        {
            Debug.Log("⚔️ === FORCE ATTACK TEST ===");
            if (dullahanAttack != null)
            {
                dullahanAttack.StartAttack();
            }
        }
        
        // Toggle invulnerability
        if (Input.GetKeyDown(toggleInvulnerabilityKey))
        {
            manualInvulnerability = !manualInvulnerability;
            Debug.Log($"🛡️ Manual Invulnerability: {manualInvulnerability}");
        }
    }
    
    private void MonitorHealthChanges()
    {
        // This runs every frame to catch issues
        if (playerHealthSystem == null) return;
        
        // Check for invalid health values
        int currentHealth = playerHealthSystem.GetCurrentHealth();
        int maxHealth = playerHealthSystem.GetMaxHealth();
        
        if (currentHealth < 0)
        {
            Debug.LogError($"❌ INVALID HEALTH: {currentHealth} (below 0!)");
        }
        
        if (currentHealth > maxHealth)
        {
            Debug.LogError($"❌ INVALID HEALTH: {currentHealth} (above max {maxHealth}!)");
        }
    }
    
    private void LogDebugControls()
    {
        Debug.Log("🎮 === DEBUG CONTROLS ===");
        Debug.Log($"🔥 {damageKey} - Damage Player");
        Debug.Log($"💚 {healKey} - Heal Player");
        Debug.Log($"✨ {fullHealKey} - Full Heal");
        Debug.Log($"⚔️ {forceAttackKey} - Force Dullahan Attack");
        Debug.Log($"🛡️ {toggleInvulnerabilityKey} - Toggle Invulnerability");
    }
    
    private void OnGUI()
    {
        if (!showDebugGUI) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 400, 300));
        GUILayout.Label("=== HEALTH SYSTEM DEBUG ===", GUI.skin.box);
        
        if (showHealthBars && playerHealthSystem != null)
        {
            GUILayout.Label($"💖 Health: {playerHealthSystem.GetCurrentHealth()}/{playerHealthSystem.GetMaxHealth()}");
            GUILayout.Label($"🛡️ Invulnerable: {playerHealthSystem.IsInvulnerable()}");
            GUILayout.Label($"💀 Dead: {playerHealthSystem.IsDead()}");
            GUILayout.Label($"⚠️ Critical: {playerHealthSystem.IsCriticalHealth()}");
        }
        
        if (showReferences)
        {
            GUILayout.Label($"🎯 PlayerHealthSystem: {(playerHealthSystem != null ? "✅" : "❌")}");
            GUILayout.Label($"⚔️ DullahanAttack: {(dullahanAttack != null ? "✅" : "❌")}");
        }
        
        if (showTimers && dullahanAttack != null)
        {
            GUILayout.Label($"🔄 Can Attack: {dullahanAttack.canAttack}");
            GUILayout.Label($"👤 Player In Range: {dullahanAttack.IsPlayerInRange()}");
            GUILayout.Label($"⏰ Next Attack: {dullahanAttack.GetTimeUntilNextAttack():F1}s");
        }
        
        GUILayout.Space(10);
        GUILayout.Label("=== CONTROLS ===");
        GUILayout.Label($"{damageKey} - Damage | {healKey} - Heal | {fullHealKey} - Full Heal");
        GUILayout.Label($"{forceAttackKey} - Force Attack | {toggleInvulnerabilityKey} - Toggle Invulnerable");
        
        GUILayout.EndArea();
    }
}
