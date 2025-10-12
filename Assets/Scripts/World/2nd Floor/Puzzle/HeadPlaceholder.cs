using UnityEngine;

/// <summary>
/// Controls the visual appearance and behavior of the head placeholder.
/// This placeholder shows where the player should place the head.
/// It can pulse, glow, and change appearance based on player interaction.
/// </summary>
public class HeadPlaceholder : MonoBehaviour
{
    [Header("Visibility Settings")]
    [Tooltip("Is the placeholder visible by default?")]
    public bool isVisibleByDefault = false;
    
    [Tooltip("Fade in/out instead of instant show/hide")]
    public bool useFadeTransition = true;
    
    [Tooltip("Duration of fade transition in seconds")]
    public float fadeDuration = 0.5f;
    
    [Header("Visual Effects")]
    [Tooltip("Should the placeholder pulse?")]
    public bool enablePulsing = true;
    
    [Tooltip("Speed of the pulsing effect")]
    public float pulseSpeed = 2f;
    
    [Tooltip("Minimum scale during pulse")]
    public float pulseMinScale = 0.95f;
    
    [Tooltip("Maximum scale during pulse")]
    public float pulseMaxScale = 1.05f;
    
    [Tooltip("Enable glow effect")]
    public bool enableGlow = true;
    
    [Tooltip("Color of the glow")]
    public Color glowColor = new Color(0.5f, 0.8f, 1f, 0.5f);
    
    [Header("Materials")]
    [Tooltip("Material when placeholder is empty")]
    public Material emptyMaterial;
    
    [Tooltip("Material when hovering with valid head")]
    public Material validMaterial;
    
    [Tooltip("Material when hovering with invalid head")]
    public Material invalidMaterial;
    
    [Header("Components")]
    [Tooltip("Renderer component for the placeholder")]
    public Renderer placeholderRenderer;
    
    [Tooltip("Light component for glow effect")]
    public Light glowLight;
    
    [Tooltip("Particle system for ambient particles")]
    public ParticleSystem ambientParticles;
    
    // Private variables
    private Vector3 originalScale;
    private Material currentMaterial;
    private float currentAlpha = 0f;
    private float targetAlpha = 0f;
    private bool isInitialized = false;
    
    public enum PlaceholderState
    {
        Empty,
        ValidHover,
        InvalidHover
    }
    
    private PlaceholderState currentState = PlaceholderState.Empty;
    
    void Start()
    {
        Initialize();
    }
    
    void Update()
    {
        if (!isInitialized) return;
        
        if (enablePulsing && gameObject.activeSelf)
        {
            ApplyPulsingEffect();
        }
        
        if (useFadeTransition)
        {
            UpdateFadeEffect();
        }
    }
    
    private void Initialize()
    {
        // Store original scale
        originalScale = transform.localScale;
        
        // Setup renderer
        if (placeholderRenderer == null)
            placeholderRenderer = GetComponent<Renderer>();
        
        if (placeholderRenderer != null)
        {
            // Apply initial material
            if (emptyMaterial != null)
            {
                placeholderRenderer.material = emptyMaterial;
                currentMaterial = emptyMaterial;
            }
            else
            {
                currentMaterial = placeholderRenderer.material;
            }
        }
        
        // Setup glow light
        if (glowLight == null)
            glowLight = GetComponentInChildren<Light>();
        
        if (glowLight != null && enableGlow)
        {
            glowLight.color = glowColor;
            glowLight.enabled = true;
        }
        else if (glowLight != null)
        {
            glowLight.enabled = false;
        }
        
        // Setup particles
        if (ambientParticles != null)
        {
            if (isVisibleByDefault)
                ambientParticles.Play();
            else
                ambientParticles.Stop();
        }
        
        // Set initial visibility
        SetVisibility(isVisibleByDefault, instant: true);
        
        isInitialized = true;
        
        Debug.Log($"[HeadPlaceholder] Initialized. Visible: {isVisibleByDefault}");
    }
    
    private void ApplyPulsingEffect()
    {
        float pulse = Mathf.Lerp(pulseMinScale, pulseMaxScale, 
            (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f);
        transform.localScale = originalScale * pulse;
    }
    
    private void UpdateFadeEffect()
    {
        if (Mathf.Abs(currentAlpha - targetAlpha) > 0.01f)
        {
            currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, Time.deltaTime / fadeDuration);
            ApplyAlpha(currentAlpha);
        }
    }
    
    private void ApplyAlpha(float alpha)
    {
        if (placeholderRenderer == null || currentMaterial == null) return;
        
        // Apply alpha to material
        Color color = currentMaterial.color;
        color.a = alpha;
        placeholderRenderer.material.color = color;
        
        // Apply alpha to glow light
        if (glowLight != null && enableGlow)
        {
            glowLight.intensity = alpha * 2f;
        }
    }
    
    /// <summary>
    /// Set the visibility of the placeholder
    /// </summary>
    public void SetVisibility(bool visible, bool instant = false)
    {
        targetAlpha = visible ? 1f : 0f;
        
        if (instant)
        {
            currentAlpha = targetAlpha;
            ApplyAlpha(currentAlpha);
        }
        
        // Control particles
        if (ambientParticles != null)
        {
            if (visible)
                ambientParticles.Play();
            else
                ambientParticles.Stop();
        }
        
        // Show/hide the entire object if not using fade
        if (!useFadeTransition)
        {
            gameObject.SetActive(visible);
        }
        
        Debug.Log($"[HeadPlaceholder] Visibility set to: {visible}");
    }
    
    /// <summary>
    /// Show the placeholder
    /// </summary>
    public void Show(bool instant = false)
    {
        SetVisibility(true, instant);
    }
    
    /// <summary>
    /// Hide the placeholder
    /// </summary>
    public void Hide(bool instant = false)
    {
        SetVisibility(false, instant);
    }
    
    /// <summary>
    /// Set the state of the placeholder
    /// </summary>
    public void SetState(PlaceholderState state)
    {
        if (currentState == state) return;
        
        currentState = state;
        
        Material newMaterial = null;
        
        switch (state)
        {
            case PlaceholderState.Empty:
                newMaterial = emptyMaterial;
                if (glowLight != null) glowLight.color = glowColor;
                break;
                
            case PlaceholderState.ValidHover:
                newMaterial = validMaterial;
                if (glowLight != null) glowLight.color = Color.green;
                break;
                
            case PlaceholderState.InvalidHover:
                newMaterial = invalidMaterial;
                if (glowLight != null) glowLight.color = Color.red;
                break;
        }
        
        if (newMaterial != null && placeholderRenderer != null)
        {
            placeholderRenderer.material = newMaterial;
            currentMaterial = newMaterial;
            ApplyAlpha(currentAlpha);
        }
        
        Debug.Log($"[HeadPlaceholder] State changed to: {state}");
    }
    
    /// <summary>
    /// Reset placeholder to default state
    /// </summary>
    public void ResetToDefault()
    {
        SetState(PlaceholderState.Empty);
        transform.localScale = originalScale;
    }
    
    /// <summary>
    /// Flash the placeholder (useful for feedback)
    /// </summary>
    public void Flash(Color flashColor, float duration = 0.3f)
    {
        StartCoroutine(FlashCoroutine(flashColor, duration));
    }
    
    private System.Collections.IEnumerator FlashCoroutine(Color flashColor, float duration)
    {
        if (placeholderRenderer == null) yield break;
        
        Material originalMat = currentMaterial;
        Color originalColor = originalMat.color;
        
        // Flash to new color
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            Color lerpedColor = Color.Lerp(flashColor, originalColor, t);
            placeholderRenderer.material.color = lerpedColor;
            
            yield return null;
        }
        
        // Restore original color
        placeholderRenderer.material.color = originalColor;
    }
    
    /// <summary>
    /// Play a pulse animation
    /// </summary>
    public void PlayPulseAnimation()
    {
        StartCoroutine(PulseAnimationCoroutine());
    }
    
    private System.Collections.IEnumerator PulseAnimationCoroutine()
    {
        float duration = 0.5f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            float scale = Mathf.Lerp(1f, 1.3f, Mathf.Sin(t * Mathf.PI));
            transform.localScale = originalScale * scale;
            
            yield return null;
        }
        
        transform.localScale = originalScale;
    }
    
    void OnDrawGizmos()
    {
        // Draw a sphere to show placeholder location in editor
        Gizmos.color = new Color(0.5f, 0.8f, 1f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, 0.2f);
    }
}

