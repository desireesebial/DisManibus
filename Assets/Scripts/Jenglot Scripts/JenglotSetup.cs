using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Jenglot Setup Helper
/// Automatically configures a Jenglot enemy with all necessary components
/// Use this to quickly set up a Jenglot in your scene
/// </summary>
public class JenglotSetup : MonoBehaviour
{
    [Header("Setup Configuration")]
    [SerializeField] private bool autoSetupOnStart = true;
    [SerializeField] private bool useExistingComponents = true;
    
    [Header("Jenglot Model")]
    [SerializeField] private GameObject jenglotModelPrefab;
    [SerializeField] private string jenglotModelPath = "Assets/Assets/3rd Floor/Jenglot/Character_output.fbx";
    
    [Header("Behavior Settings")]
    [SerializeField] private float activationRange = 10f;
    [SerializeField] private float followSpeed = 2f;
    [SerializeField] private float stareDetectionAngle = 45f;
    [SerializeField] private float stareMaxDistance = 15f;
    [SerializeField] private bool persistentFollowing = true;  // Once activated, follow player everywhere
    [SerializeField] private float deactivationRange = 100f;   // Only deactivate if player goes extremely far
    
    [Header("Room Trigger Settings")]
    [SerializeField] private bool createRoomTrigger = true;
    [SerializeField] private Vector3 roomTriggerSize = new Vector3(20f, 5f, 20f);
    [SerializeField] private Vector3 roomTriggerOffset = Vector3.zero;
    
    [Header("Audio")]
    [SerializeField] private AudioClip activationSound;
    [SerializeField] private AudioClip movementSound;
    [SerializeField] private AudioClip freezeSound;
    
    [Header("Materials")]
    [SerializeField] private Material normalMaterial;
    [SerializeField] private Material frozenMaterial;
    
    private void Start()
    {
        if (autoSetupOnStart)
        {
            SetupJenglot();
        }
    }
    
    [ContextMenu("Setup Jenglot")]
    public void SetupJenglot()
    {
        Debug.Log("Setting up Jenglot...");
        
        // Setup basic components
        SetupBasicComponents();
        
        // Setup Jenglot behavior
        SetupJenglotBehavior();
        
        // Setup model
        SetupJenglotModel();
        
        // Setup room trigger if requested
        if (createRoomTrigger)
        {
            SetupRoomTrigger();
        }
        
        // Final configurations
        FinalizeSetup();
        
        Debug.Log("Jenglot setup completed!");
    }
    
    private void SetupBasicComponents()
    {
        // Set tag
        gameObject.tag = "Jenglot";
        
        // Add NavMeshAgent if not present
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            agent = gameObject.AddComponent<NavMeshAgent>();
            Debug.Log("Added NavMeshAgent component");
        }
        
        // Configure NavMeshAgent
        agent.speed = followSpeed;
        agent.angularSpeed = 120f;
        agent.acceleration = 8f;
        agent.stoppingDistance = 2f;
        agent.radius = 0.5f;
        agent.height = 1.5f;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        
        // Add AudioSource if not present
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f; // 3D sound
            audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
            audioSource.maxDistance = 20f;
            Debug.Log("Added AudioSource component");
        }
        
        // Add Collider if not present
        Collider collider = GetComponent<Collider>();
        if (collider == null)
        {
            CapsuleCollider capsule = gameObject.AddComponent<CapsuleCollider>();
            capsule.radius = 0.5f;
            capsule.height = 1.5f;
            capsule.center = new Vector3(0, 0.75f, 0);
            Debug.Log("Added CapsuleCollider component");
        }
    }
    
    private void SetupJenglotBehavior()
    {
        JenglotBehavior behavior = GetComponent<JenglotBehavior>();
        if (behavior == null)
        {
            behavior = gameObject.AddComponent<JenglotBehavior>();
            Debug.Log("Added JenglotBehavior component");
        }

        // Configure behavior settings using reflection
        if (behavior != null)
        {
            // Set activation range
            SetPrivateField(behavior, "activationRange", activationRange);

            // Set follow speed
            SetPrivateField(behavior, "followSpeed", followSpeed);

            // Set stare detection settings
            SetPrivateField(behavior, "stareDetectionAngle", stareDetectionAngle);
            SetPrivateField(behavior, "stareMaxDistance", stareMaxDistance);

            // Set persistent following settings
            SetPrivateField(behavior, "persistentFollowing", persistentFollowing);
            SetPrivateField(behavior, "deactivationRange", deactivationRange);

            // Set audio clips
            SetPrivateField(behavior, "activationSound", activationSound);
            SetPrivateField(behavior, "movementSound", movementSound);
            SetPrivateField(behavior, "freezeSound", freezeSound);

            // Set materials
            SetPrivateField(behavior, "normalMaterial", normalMaterial);
            SetPrivateField(behavior, "frozenMaterial", frozenMaterial);

            Debug.Log($"JenglotBehavior configured - Persistent Following: {persistentFollowing}");
        }
    }

    private void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(target, value);
        }
    }
    
    private void SetupJenglotModel()
    {
        // Check if model already exists as child
        Transform existingModel = transform.Find("JenglotModel");
        if (existingModel != null && useExistingComponents)
        {
            Debug.Log("Using existing Jenglot model");
            return;
        }
        
        GameObject modelObject = null;
        
        // Try to use assigned prefab first
        if (jenglotModelPrefab != null)
        {
            modelObject = Instantiate(jenglotModelPrefab, transform);
            modelObject.name = "JenglotModel";
            Debug.Log("Instantiated Jenglot model from prefab");
        }
        else
        {
            // Try to load from path
            GameObject loadedPrefab = Resources.Load<GameObject>(jenglotModelPath);
            if (loadedPrefab != null)
            {
                modelObject = Instantiate(loadedPrefab, transform);
                modelObject.name = "JenglotModel";
                Debug.Log("Loaded and instantiated Jenglot model from path");
            }
            else
            {
                // Create a simple placeholder
                modelObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                modelObject.transform.SetParent(transform);
                modelObject.transform.localPosition = Vector3.zero;
                modelObject.name = "JenglotModel_Placeholder";
                
                // Make it look more creature-like
                modelObject.transform.localScale = new Vector3(0.8f, 1.2f, 0.8f);
                
                Debug.LogWarning("Created placeholder Jenglot model. Assign a proper model prefab for better visuals.");
            }
        }
        
        // Configure model
        if (modelObject != null)
        {
            // Ensure it has proper materials
            Renderer renderer = modelObject.GetComponent<Renderer>();
            if (renderer != null && normalMaterial != null)
            {
                renderer.material = normalMaterial;
            }
            
            // Add animator if model has one (optional for animations)
            Animator animator = modelObject.GetComponent<Animator>();
            if (animator == null)
            {
                // Don't automatically add animator - let user decide if they want animations
                Debug.Log("Jenglot: No Animator component found. System will work with basic visual feedback.");
                Debug.Log("Jenglot: Add Animator component manually if you want walking animations.");
            }
            else
            {
                // Try to find and assign the controller
                RuntimeAnimatorController controller = Resources.Load<RuntimeAnimatorController>("Assets/Assets/3rd Floor/Jenglot/Controller");
                if (controller != null)
                {
                    animator.runtimeAnimatorController = controller;
                    Debug.Log("Assigned Jenglot animator controller");
                }
                else
                {
                    Debug.LogWarning("Jenglot: Animator found but no controller assigned. Please assign animation controller manually.");
                }
            }
        }
    }
    
    private void SetupRoomTrigger()
    {
        // Create room trigger as a child object
        GameObject triggerObject = new GameObject("JenglotRoomTrigger");
        triggerObject.transform.SetParent(transform);
        triggerObject.transform.localPosition = roomTriggerOffset;
        
        // Add box collider for room detection
        BoxCollider triggerCollider = triggerObject.AddComponent<BoxCollider>();
        triggerCollider.isTrigger = true;
        triggerCollider.size = roomTriggerSize;
        
        // Add room trigger component
        JenglotRoomTrigger roomTrigger = triggerObject.AddComponent<JenglotRoomTrigger>();
        roomTrigger.SetJenglotBehavior(GetComponent<JenglotBehavior>());
        
        Debug.Log("Created Jenglot room trigger");
    }
    
    private void FinalizeSetup()
    {
        // Ensure the GameObject is properly positioned
        if (transform.position.y < 0.1f)
        {
            transform.position = new Vector3(transform.position.x, 0.1f, transform.position.z);
        }
        
        // Validate NavMesh
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null && !agent.isOnNavMesh)
        {
            Debug.LogWarning("Jenglot is not on NavMesh! Make sure the area has NavMesh baked or reposition the Jenglot.");
        }
        
        Debug.Log($"Jenglot setup completed at position: {transform.position}");
    }
    
    [ContextMenu("Remove Jenglot Components")]
    public void RemoveJenglotComponents()
    {
        // Remove components
        JenglotBehavior behavior = GetComponent<JenglotBehavior>();
        if (behavior != null)
        {
            DestroyImmediate(behavior);
        }
        
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            DestroyImmediate(agent);
        }
        
        // Remove room trigger
        Transform roomTrigger = transform.Find("JenglotRoomTrigger");
        if (roomTrigger != null)
        {
            DestroyImmediate(roomTrigger.gameObject);
        }
        
        Debug.Log("Jenglot components removed");
    }
    
    [ContextMenu("Test Jenglot Activation")]
    public void TestJenglotActivation()
    {
        JenglotBehavior behavior = GetComponent<JenglotBehavior>();
        if (behavior != null)
        {
            behavior.ForceActivate();
            Debug.Log("Jenglot force activated for testing");
        }
    }
    
    // Validation
    private void OnValidate()
    {
        // Ensure reasonable values
        activationRange = Mathf.Max(1f, activationRange);
        followSpeed = Mathf.Max(0.1f, followSpeed);
        stareDetectionAngle = Mathf.Clamp(stareDetectionAngle, 1f, 180f);
        stareMaxDistance = Mathf.Max(1f, stareMaxDistance);
        
        roomTriggerSize.x = Mathf.Max(1f, roomTriggerSize.x);
        roomTriggerSize.y = Mathf.Max(1f, roomTriggerSize.y);
        roomTriggerSize.z = Mathf.Max(1f, roomTriggerSize.z);
    }
}
