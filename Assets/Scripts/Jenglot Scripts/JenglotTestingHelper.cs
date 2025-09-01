using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Jenglot Testing Helper
/// Provides tools for testing the Jenglot behavior system
/// Use this for debugging and validating the Jenglot implementation
/// </summary>
public class JenglotTestingHelper : MonoBehaviour
{
    [Header("Testing Configuration")]
    [SerializeField] private JenglotBehavior jenglotToTest;
    [SerializeField] private bool autoFindJenglot = true;
    [SerializeField] private bool runTestsOnStart = false;
    
    [Header("Test Controls")]
    [SerializeField] private KeyCode forceActivateKey = KeyCode.F1;
    [SerializeField] private KeyCode forceDeactivateKey = KeyCode.F2;
    [SerializeField] private KeyCode toggleDebugKey = KeyCode.F3;
    [SerializeField] private KeyCode runDiagnosticsKey = KeyCode.F4;
    
    [Header("Scene Validation")]
    [SerializeField] private bool checkNavMesh = true;
    [SerializeField] private bool checkPlayerController = true;
    [SerializeField] private bool checkAudioSources = true;
    
    [Header("Debug Info")]
    [SerializeField] private bool showRealTimeInfo = true;
    [SerializeField] private float infoUpdateRate = 0.5f;
    
    private float lastInfoUpdate = 0f;
    private Transform player;
    private Camera playerCamera;
    
    private void Start()
    {
        InitializeComponents();
        
        if (runTestsOnStart)
        {
            RunDiagnostics();
        }
    }
    
    private void InitializeComponents()
    {
        // Auto-find Jenglot if not assigned
        if (jenglotToTest == null && autoFindJenglot)
        {
            jenglotToTest = FindObjectOfType<JenglotBehavior>();
            if (jenglotToTest != null)
            {
                Debug.Log($"Auto-found Jenglot: {jenglotToTest.name}");
            }
        }
        
        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerCamera = player.GetComponentInChildren<Camera>();
        }
    }
    
    private void Update()
    {
        HandleTestInputs();
        UpdateRealTimeInfo();
    }
    
    private void HandleTestInputs()
    {
        if (Input.GetKeyDown(forceActivateKey))
        {
            ForceActivateJenglot();
        }
        
        if (Input.GetKeyDown(forceDeactivateKey))
        {
            ForceDeactivateJenglot();
        }
        
        if (Input.GetKeyDown(toggleDebugKey))
        {
            ToggleDebugVisualization();
        }
        
        if (Input.GetKeyDown(runDiagnosticsKey))
        {
            RunDiagnostics();
        }
    }
    
    private void UpdateRealTimeInfo()
    {
        if (!showRealTimeInfo || Time.time - lastInfoUpdate < infoUpdateRate)
            return;

        lastInfoUpdate = Time.time;

        if (jenglotToTest != null && player != null)
        {
            float distance = Vector3.Distance(jenglotToTest.transform.position, player.position);

            // Get persistent following status using reflection
            bool persistentFollowing = GetPersistentFollowingStatus();

            string info = $"Jenglot Status - Active: {jenglotToTest.IsCurrentlyActive}, " +
                         $"Frozen: {jenglotToTest.IsCurrentlyFrozen}, " +
                         $"Persistent Following: {persistentFollowing}, " +
                         $"Distance: {distance:F1}m";

            Debug.Log(info);
        }
    }

    private bool GetPersistentFollowingStatus()
    {
        if (jenglotToTest == null) return false;

        var field = jenglotToTest.GetType().GetField("persistentFollowing",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            return (bool)field.GetValue(jenglotToTest);
        }

        return false;
    }
    
    [ContextMenu("Force Activate Jenglot")]
    public void ForceActivateJenglot()
    {
        if (jenglotToTest != null)
        {
            jenglotToTest.ForceActivate();
            Debug.Log("Jenglot force activated");
        }
        else
        {
            Debug.LogWarning("No Jenglot assigned to test!");
        }
    }
    
    [ContextMenu("Force Deactivate Jenglot")]
    public void ForceDeactivateJenglot()
    {
        if (jenglotToTest != null)
        {
            jenglotToTest.ForceDeactivate();
            Debug.Log("Jenglot force deactivated");
        }
        else
        {
            Debug.LogWarning("No Jenglot assigned to test!");
        }
    }
    
    [ContextMenu("Toggle Debug Visualization")]
    public void ToggleDebugVisualization()
    {
        // Note: This would require making showDebugGizmos public in JenglotBehavior
        // or adding a public method to toggle it
        Debug.Log("Debug visualization toggle requested");
    }
    
    [ContextMenu("Run Diagnostics")]
    public void RunDiagnostics()
    {
        Debug.Log("=== JENGLOT SYSTEM DIAGNOSTICS ===");
        
        CheckJenglotComponent();
        CheckPlayerSetup();
        CheckNavMeshSetup();
        CheckAudioSetup();
        CheckSceneRequirements();
        
        Debug.Log("=== DIAGNOSTICS COMPLETE ===");
    }
    
    private void CheckJenglotComponent()
    {
        Debug.Log("--- Jenglot Component Check ---");
        
        if (jenglotToTest == null)
        {
            Debug.LogError("❌ No JenglotBehavior found in scene!");
            return;
        }
        
        Debug.Log("✅ JenglotBehavior found");
        
        // Check required components
        NavMeshAgent agent = jenglotToTest.GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("❌ NavMeshAgent missing on Jenglot");
        }
        else
        {
            Debug.Log("✅ NavMeshAgent present");
            
            if (!agent.isOnNavMesh)
            {
                Debug.LogWarning("⚠️ Jenglot is not on NavMesh surface");
            }
            else
            {
                Debug.Log("✅ Jenglot is on NavMesh");
            }
        }
        
        AudioSource audioSource = jenglotToTest.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogWarning("⚠️ AudioSource missing on Jenglot");
        }
        else
        {
            Debug.Log("✅ AudioSource present");
        }
        
        Collider collider = jenglotToTest.GetComponent<Collider>();
        if (collider == null)
        {
            Debug.LogWarning("⚠️ Collider missing on Jenglot");
        }
        else
        {
            Debug.Log("✅ Collider present");
        }
    }
    
    private void CheckPlayerSetup()
    {
        Debug.Log("--- Player Setup Check ---");
        
        if (player == null)
        {
            Debug.LogError("❌ No GameObject with 'Player' tag found!");
            return;
        }
        
        Debug.Log("✅ Player GameObject found");
        
        if (playerCamera == null)
        {
            Debug.LogError("❌ No Camera found on player or children!");
        }
        else
        {
            Debug.Log("✅ Player Camera found");
        }
        
        // Check for player movement controllers
        var simpleMovement = player.GetComponent<SimplePlayerMovement>();
        var fpController = player.GetComponent<FirstPersonController>();
        
        if (simpleMovement == null && fpController == null)
        {
            Debug.LogWarning("⚠️ No recognized player movement controller found");
        }
        else
        {
            Debug.Log("✅ Player movement controller found");
        }
    }
    
    private void CheckNavMeshSetup()
    {
        if (!checkNavMesh) return;
        
        Debug.Log("--- NavMesh Setup Check ---");
        
        // Check if NavMesh exists in scene
        NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();
        
        if (triangulation.vertices.Length == 0)
        {
            Debug.LogError("❌ No NavMesh found in scene! Please bake NavMesh.");
        }
        else
        {
            Debug.Log($"✅ NavMesh found with {triangulation.vertices.Length} vertices");
        }
        
        // Check if player position is reachable
        if (player != null)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(player.position, out hit, 5f, NavMesh.AllAreas))
            {
                Debug.Log("✅ Player position is reachable by NavMesh");
            }
            else
            {
                Debug.LogWarning("⚠️ Player position may not be reachable by NavMesh");
            }
        }
    }
    
    private void CheckAudioSetup()
    {
        if (!checkAudioSources) return;
        
        Debug.Log("--- Audio Setup Check ---");
        
        // Check for AudioListener
        AudioListener listener = FindObjectOfType<AudioListener>();
        if (listener == null)
        {
            Debug.LogError("❌ No AudioListener found in scene!");
        }
        else
        {
            Debug.Log("✅ AudioListener found");
        }
        
        // Check Jenglot audio
        if (jenglotToTest != null)
        {
            AudioSource jenglotAudio = jenglotToTest.GetComponent<AudioSource>();
            if (jenglotAudio != null)
            {
                if (jenglotAudio.spatialBlend < 0.8f)
                {
                    Debug.LogWarning("⚠️ Jenglot AudioSource spatialBlend should be closer to 1.0 for 3D sound");
                }
                else
                {
                    Debug.Log("✅ Jenglot AudioSource configured for 3D sound");
                }
            }
        }
    }
    
    private void CheckSceneRequirements()
    {
        Debug.Log("--- Scene Requirements Check ---");
        
        // Check for lighting
        Light[] lights = FindObjectsOfType<Light>();
        if (lights.Length == 0)
        {
            Debug.LogWarning("⚠️ No lights found in scene");
        }
        else
        {
            Debug.Log($"✅ {lights.Length} light(s) found in scene");
        }
        
        // Check for ground/floor
        Collider[] colliders = FindObjectsOfType<Collider>();
        int groundColliders = 0;
        foreach (var col in colliders)
        {
            if (col.bounds.min.y < 0.1f && col.bounds.size.x > 5f && col.bounds.size.z > 5f)
            {
                groundColliders++;
            }
        }
        
        if (groundColliders == 0)
        {
            Debug.LogWarning("⚠️ No apparent ground/floor colliders found");
        }
        else
        {
            Debug.Log($"✅ {groundColliders} potential ground collider(s) found");
        }
    }
    
    // Performance testing
    [ContextMenu("Run Performance Test")]
    public void RunPerformanceTest()
    {
        if (jenglotToTest == null)
        {
            Debug.LogWarning("No Jenglot to test!");
            return;
        }
        
        Debug.Log("Starting Jenglot performance test...");
        
        // Test activation/deactivation cycles
        StartCoroutine(PerformanceTestCoroutine());
    }
    
    private System.Collections.IEnumerator PerformanceTestCoroutine()
    {
        int cycles = 10;
        float totalTime = 0f;
        
        for (int i = 0; i < cycles; i++)
        {
            float startTime = Time.realtimeSinceStartup;
            
            jenglotToTest.ForceActivate();
            yield return new WaitForSeconds(0.1f);
            
            jenglotToTest.ForceDeactivate();
            yield return new WaitForSeconds(0.1f);
            
            float cycleTime = Time.realtimeSinceStartup - startTime;
            totalTime += cycleTime;
        }
        
        float averageTime = totalTime / cycles;
        Debug.Log($"Performance test complete. Average cycle time: {averageTime:F4}s");
    }
    
    private void OnGUI()
    {
        if (!showRealTimeInfo) return;

        GUILayout.BeginArea(new Rect(10, 10, 350, 180));
        GUILayout.Box("Jenglot Testing Helper");

        if (jenglotToTest != null)
        {
            GUILayout.Label($"Active: {jenglotToTest.IsCurrentlyActive}");
            GUILayout.Label($"Frozen: {jenglotToTest.IsCurrentlyFrozen}");
            GUILayout.Label($"Persistent Following: {GetPersistentFollowingStatus()}");
            if (player != null)
            {
                float dist = Vector3.Distance(jenglotToTest.transform.position, player.position);
                GUILayout.Label($"Distance: {dist:F1}m");
            }
        }
        else
        {
            GUILayout.Label("No Jenglot assigned");
        }

        GUILayout.Space(10);
        GUILayout.Label("Controls:");
        GUILayout.Label($"{forceActivateKey}: Force Activate");
        GUILayout.Label($"{forceDeactivateKey}: Force Deactivate");
        GUILayout.Label($"{toggleDebugKey}: Toggle Debug");
        GUILayout.Label($"{runDiagnosticsKey}: Run Diagnostics");

        GUILayout.EndArea();
    }
}
