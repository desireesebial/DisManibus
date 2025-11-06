using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Manages health-based vision effects (vignette/tunnel vision and blur)
/// Effects intensify as player health decreases
/// </summary>
public class HealthVisionEffects : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to PlayerHealthSystem to monitor health changes")]
    public PlayerHealthSystem playerHealthSystem;

    [Tooltip("UI Image for vignette overlay (fullscreen, with radial gradient)")]
    public Image vignetteOverlay;

    [Tooltip("Optional: Post-process volume for blur effect")]
    public MonoBehaviour postProcessVolume;

    [Header("Vignette Settings")]
    [Tooltip("Color of the vignette (usually black or dark red)")]
    public Color vignetteColor = new Color(0f, 0f, 0f, 1f);

    [Tooltip("Maximum vignette intensity at lowest health (0-1)")]
    [Range(0f, 1f)]
    public float maxVignetteIntensity = 0.8f;

    [Tooltip("Minimum vignette intensity at full health (0-1)")]
    [Range(0f, 1f)]
    public float minVignetteIntensity = 0f;

    [Header("Health Thresholds")]
    [Tooltip("Health percentage values for different effect levels")]
    public float fullHealthPercent = 1f;        // 100% health
    public float highHealthPercent = 0.66f;     // 66% health (2/3 HP)
    public float midHealthPercent = 0.33f;      // 33% health (1/3 HP)
    public float lowHealthPercent = 0.01f;      // Near death

    [Header("Intensity at Each Threshold")]
    [Tooltip("Vignette intensity at each health threshold (0-1)")]
    [Range(0f, 1f)] public float vignetteAtFull = 0f;
    [Range(0f, 1f)] public float vignetteAtHigh = 0.2f;
    [Range(0f, 1f)] public float vignetteAtMid = 0.5f;
    [Range(0f, 1f)] public float vignetteAtLow = 0.8f;

    [Header("Blur Settings")]
    [Tooltip("Enable blur effect based on health")]
    public bool enableBlur = true;

    [Tooltip("Maximum blur intensity at lowest health")]
    [Range(0f, 10f)]
    public float maxBlurIntensity = 5f;

    [Header("Blur Intensity at Each Threshold")]
    [Tooltip("Blur intensity at each health level")]
    [Range(0f, 10f)] public float blurAtFull = 0f;
    [Range(0f, 10f)] public float blurAtHigh = 2.5f;
    [Range(0f, 10f)] public float blurAtMid = 3.0f;
    [Range(0f, 10f)] public float blurAtLow = 3.5f;

    [Header("Transition Settings")]
    [Tooltip("How fast effects transition when health changes")]
    public float transitionSpeed = 3f;

    [Tooltip("Use smooth easing for transitions")]
    public bool useSmoothEasing = true;

    [Header("Pulse Effect")]
    [Tooltip("Enable pulsing effect at critical health")]
    public bool enablePulseAtCritical = true;

    [Tooltip("Pulse speed at critical health")]
    public float pulseSpeed = 2f;

    [Tooltip("Pulse intensity variation")]
    [Range(0f, 0.3f)]
    public float pulseAmount = 0.15f;

    [Header("Debug")]
    [Tooltip("Show debug information")]
    public bool showDebug = true;

    // Private variables
    private float currentVignetteIntensity = 0f;
    private float targetVignetteIntensity = 0f;
    private float currentBlurIntensity = 0f;
    private float targetBlurIntensity = 0f;
    private bool isCriticalHealth = false;
    private float pulseTimer = 0f;

    void Start()
    {
        Initialize();
    }

    void Update()
    {
        UpdateEffects();
    }

    private void Initialize()
    {
        // Find references if not assigned
        if (playerHealthSystem == null)
        {
            playerHealthSystem = FindObjectOfType<PlayerHealthSystem>();
            if (playerHealthSystem == null)
            {
                Debug.LogError("[HealthVisionEffects] PlayerHealthSystem not found!");
                enabled = false;
                return;
            }
        }

        if (vignetteOverlay == null)
        {
            Debug.LogWarning("[HealthVisionEffects] Vignette overlay not assigned! Vision effects will not work.");
        }

        // Subscribe to health changes
        if (playerHealthSystem != null)
        {
            playerHealthSystem.OnHealthChanged.AddListener(OnHealthChanged);
        }

        // Initial setup
        if (vignetteOverlay != null)
        {
            vignetteOverlay.color = vignetteColor;
            Color c = vignetteOverlay.color;
            c.a = 0f;
            vignetteOverlay.color = c;
        }

        // Set initial effect based on current health
        UpdateTargetIntensity();
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        if (playerHealthSystem != null)
        {
            playerHealthSystem.OnHealthChanged.RemoveListener(OnHealthChanged);
        }
    }

    private void OnHealthChanged(int newHealth)
    {
        if (showDebug)
        {
            Debug.Log($"[HealthVisionEffects] Health changed to {newHealth}");
        }

        UpdateTargetIntensity();
    }

    private void UpdateTargetIntensity()
    {
        if (playerHealthSystem == null) return;

        int currentHealth = playerHealthSystem.CurrentHealth;
        int maxHealth = playerHealthSystem.MaxHealth;

        // Determine target intensities based on exact health value
        targetVignetteIntensity = CalculateVignetteIntensity(currentHealth);
        targetBlurIntensity = enableBlur ? CalculateBlurIntensity(currentHealth) : 0f;

        // Check if at critical health for pulse effect (1 HP remaining)
        isCriticalHealth = currentHealth == 1;

        if (showDebug)
        {
            Debug.Log($"[HealthVisionEffects] Health: {currentHealth}/{maxHealth} | Target Vignette: {targetVignetteIntensity:F2} | Target Blur: {targetBlurIntensity:F2} | Critical: {isCriticalHealth}");
        }
    }

    private float CalculateVignetteIntensity(int currentHealth)
    {
        // Simple discrete health checks for 3-health system
        // Health = 3 → No vignette
        // Health = 2 → Mild vignette
        // Health = 1 → Strong vignette + pulse

        if (currentHealth >= 3)
        {
            return vignetteAtFull; // No vignette at full health
        }
        else if (currentHealth == 2)
        {
            return vignetteAtHigh; // Mild vignette at 2 HP
        }
        else if (currentHealth == 1)
        {
            return vignetteAtLow; // Strong vignette at 1 HP (critical)
        }
        else
        {
            // Dead or 0 health - maximum vignette
            return maxVignetteIntensity;
        }
    }

    private float CalculateBlurIntensity(int currentHealth)
    {
        // Simple discrete health checks for 3-health system
        // Health = 3 → No blur
        // Health = 2 → Moderate blur
        // Health = 1 → Strong blur

        if (currentHealth >= 3)
        {
            return blurAtFull; // No blur at full health
        }
        else if (currentHealth == 2)
        {
            return blurAtHigh; // Moderate blur at 2 HP
        }
        else if (currentHealth == 1)
        {
            return blurAtLow; // Strong blur at 1 HP (critical)
        }
        else
        {
            // Dead or 0 health - maximum blur
            return maxBlurIntensity;
        }
    }

    private void UpdateEffects()
    {
        // Smoothly lerp current values to targets
        if (useSmoothEasing)
        {
            currentVignetteIntensity = Mathf.Lerp(currentVignetteIntensity, targetVignetteIntensity, Time.deltaTime * transitionSpeed);
            currentBlurIntensity = Mathf.Lerp(currentBlurIntensity, targetBlurIntensity, Time.deltaTime * transitionSpeed);
        }
        else
        {
            currentVignetteIntensity = Mathf.MoveTowards(currentVignetteIntensity, targetVignetteIntensity, Time.deltaTime * transitionSpeed);
            currentBlurIntensity = Mathf.MoveTowards(currentBlurIntensity, targetBlurIntensity, Time.deltaTime * transitionSpeed);
        }

        // Apply pulse effect at critical health
        float finalVignetteIntensity = currentVignetteIntensity;
        if (enablePulseAtCritical && isCriticalHealth)
        {
            pulseTimer += Time.deltaTime * pulseSpeed;
            float pulse = Mathf.Sin(pulseTimer) * pulseAmount;
            finalVignetteIntensity = Mathf.Clamp01(currentVignetteIntensity + pulse);
        }

        // Apply vignette
        if (vignetteOverlay != null)
        {
            Color c = vignetteColor;
            c.a = finalVignetteIntensity;
            vignetteOverlay.color = c;
        }

        // Apply blur (if post-process volume exists)
        if (enableBlur && postProcessVolume != null)
        {
            postProcessVolume.enabled = currentBlurIntensity > 0.01f;
            // Note: Actual blur intensity adjustment depends on your post-processing setup
            // You may need to access specific blur parameters here
        }
    }

    /// <summary>
    /// Manually set effect intensity (useful for testing or scripted events)
    /// </summary>
    public void SetEffectIntensity(float vignetteIntensity, float blurIntensity)
    {
        targetVignetteIntensity = Mathf.Clamp01(vignetteIntensity);
        targetBlurIntensity = Mathf.Clamp(blurIntensity, 0f, maxBlurIntensity);
    }

    /// <summary>
    /// Reset effects to normal (useful for healing or scene transitions)
    /// </summary>
    public void ResetEffects()
    {
        targetVignetteIntensity = 0f;
        targetBlurIntensity = 0f;
        currentVignetteIntensity = 0f;
        currentBlurIntensity = 0f;

        if (vignetteOverlay != null)
        {
            Color c = vignetteOverlay.color;
            c.a = 0f;
            vignetteOverlay.color = c;
        }
    }

    // Context menu for testing
    [ContextMenu("Test - Set to Critical Health")]
    void TestCriticalHealth()
    {
        SetEffectIntensity(vignetteAtLow, maxBlurIntensity);
    }

    [ContextMenu("Test - Set to Mid Health")]
    void TestMidHealth()
    {
        SetEffectIntensity(vignetteAtMid, maxBlurIntensity * 0.5f);
    }

    [ContextMenu("Test - Reset Effects")]
    void TestResetEffects()
    {
        ResetEffects();
    }
}
