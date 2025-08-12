using UnityEngine;

/// <summary>
/// Helper script to set up basic character animations
/// This script provides guidance and basic animation setup for character movement
/// </summary>
public class SimpleAnimationController : MonoBehaviour
{
    [Header("Animation Setup Guide")]
    [TextArea(5, 10)]
    public string setupInstructions = 
        "ANIMATION SETUP GUIDE:\n\n" +
        "1. Select your character model in the Hierarchy\n" +
        "2. In the Inspector, click 'Add Component' and add 'Animator'\n" +
        "3. Create an Animator Controller: Right-click in Project > Create > Animator Controller\n" +
        "4. Drag the Animator Controller to the 'Controller' field in the Animator component\n" +
        "5. Double-click the Animator Controller to open the Animator window\n" +
        "6. Add animation clips to your controller\n" +
        "7. Set up animation parameters and transitions\n\n" +
        "REQUIRED ANIMATION PARAMETERS:\n" +
        "- IsWalking (Bool)\n" +
        "- IsSprinting (Bool)\n" +
        "- IsCrouching (Bool)\n" +
        "- Speed (Float)\n" +
        "- VerticalInput (Float)\n" +
        "- HorizontalInput (Float)";

    [Header("Animation Clips")]
    public AnimationClip idleAnimation;
    public AnimationClip walkAnimation;
    public AnimationClip runAnimation;
    public AnimationClip crouchAnimation;
    public AnimationClip crouchWalkAnimation;

    [Header("Animation Settings")]
    public float animationBlendTime = 0.25f;
    public bool enableRootMotion = false;

    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        
        if (animator == null)
        {
            Debug.LogError("No Animator component found! Please add an Animator component to your character.");
            return;
        }

        // Set root motion based on preference
        animator.applyRootMotion = enableRootMotion;

        // Log available parameters for debugging
        LogAnimatorParameters();
    }

    void LogAnimatorParameters()
    {
        if (animator == null || animator.runtimeAnimatorController == null) return;

        Debug.Log("=== ANIMATOR PARAMETERS ===");
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            Debug.Log($"Parameter: {param.name} (Type: {param.type})");
        }
        Debug.Log("==========================");
    }

    /// <summary>
    /// Creates a basic animation controller with common parameters
    /// Call this from the Inspector or another script
    /// </summary>
    [ContextMenu("Create Basic Animation Controller")]
    public void CreateBasicAnimationController()
    {
        Debug.Log("To create a basic animation controller:\n" +
                  "1. Right-click in Project window\n" +
                  "2. Create > Animator Controller\n" +
                  "3. Name it 'PlayerAnimator'\n" +
                  "4. Double-click to open Animator window\n" +
                  "5. Add your animation clips\n" +
                  "6. Set up parameters and transitions");
    }

    /// <summary>
    /// Validates that all required animation parameters exist
    /// </summary>
    [ContextMenu("Validate Animation Parameters")]
    public void ValidateAnimationParameters()
    {
        if (animator == null || animator.runtimeAnimatorController == null)
        {
            Debug.LogError("No Animator Controller assigned!");
            return;
        }

        string[] requiredParams = { "IsWalking", "IsSprinting", "IsCrouching", "Speed", "VerticalInput", "HorizontalInput" };
        bool allParamsExist = true;

        foreach (string paramName in requiredParams)
        {
            bool exists = false;
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.name == paramName)
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                Debug.LogError($"Missing required animation parameter: {paramName}");
                allParamsExist = false;
            }
        }

        if (allParamsExist)
        {
            Debug.Log("All required animation parameters are present! ✓");
        }
    }

    /// <summary>
    /// Sets up basic animation transitions
    /// </summary>
    [ContextMenu("Setup Basic Transitions")]
    public void SetupBasicTransitions()
    {
        Debug.Log("To set up basic transitions:\n" +
                  "1. Open your Animator Controller\n" +
                  "2. Create states for: Idle, Walk, Run, Crouch\n" +
                  "3. Add transitions between states\n" +
                  "4. Set transition conditions using the parameters\n" +
                  "5. Adjust transition duration for smooth blending");
    }

    void OnValidate()
    {
        // Validate animation clips
        if (idleAnimation != null && !idleAnimation.isLooping)
        {
            Debug.LogWarning("Idle animation should be set to loop for better performance!");
        }
    }
} 