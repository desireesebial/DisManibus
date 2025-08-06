using UnityEngine;

public class SimpleLightingController : MonoBehaviour
{
    [Header("Lighting Settings")]
    public Light mainLight;
    
    [Header("Horror Atmosphere Presets")]
    [SerializeField] private Color normalColor = new Color(0.9f, 0.9f, 0.8f); // Slightly warm white
    [SerializeField] private float normalIntensity = 0.8f;
    
    [SerializeField] private Color eerieColor = new Color(0.6f, 0.4f, 0.2f); // Dark orange/amber
    [SerializeField] private float eerieIntensity = 0.4f;
    
    [SerializeField] private Color scaryColor = new Color(0.4f, 0.1f, 0.1f); // Dark red
    [SerializeField] private float scaryIntensity = 0.2f;
    
    [SerializeField] private Color creepyColor = new Color(0.2f, 0.1f, 0.4f); // Dark purple
    [SerializeField] private float creepyIntensity = 0.1f;
    
    [Header("Transition Settings")]
    public float transitionSpeed = 3f;
    
    private Color targetColor;
    private float targetIntensity;
    private bool isTransitioning = false;
    
    void Start()
    {
        // Find the main light if not assigned
        if (mainLight == null)
        {
            mainLight = FindAnyObjectByType<Light>();
        }
        
        // Set initial lighting to normal
        SetNormalLighting();
    }
    
    void Update()
    {
        // Update transition
        if (isTransitioning)
        {
            UpdateLightingTransition();
        }
    }
    
    // Simple methods to set different atmospheres
    public void SetNormalLighting()
    {
        if (mainLight != null)
        {
            mainLight.color = normalColor;
            mainLight.intensity = normalIntensity;
        }
        RenderSettings.ambientLight = normalColor * 0.3f;
    }
    
    public void SetEerieLighting()
    {
        if (mainLight != null)
        {
            mainLight.color = eerieColor;
            mainLight.intensity = eerieIntensity;
        }
        RenderSettings.ambientLight = eerieColor * 0.2f;
    }
    
    public void SetScaryLighting()
    {
        if (mainLight != null)
        {
            mainLight.color = scaryColor;
            mainLight.intensity = scaryIntensity;
        }
        RenderSettings.ambientLight = scaryColor * 0.1f;
    }
    
    public void SetCreepyLighting()
    {
        if (mainLight != null)
        {
            mainLight.color = creepyColor;
            mainLight.intensity = creepyIntensity;
        }
        RenderSettings.ambientLight = creepyColor * 0.05f;
    }
    
    // Smooth transition methods
    public void TransitionToNormal()
    {
        StartTransition(normalColor, normalIntensity);
    }
    
    public void TransitionToEerie()
    {
        StartTransition(eerieColor, eerieIntensity);
    }
    
    public void TransitionToScary()
    {
        StartTransition(scaryColor, scaryIntensity);
    }
    
    public void TransitionToCreepy()
    {
        StartTransition(creepyColor, creepyIntensity);
    }
    
    private void StartTransition(Color newColor, float newIntensity)
    {
        if (mainLight == null) return;
        
        targetColor = newColor;
        targetIntensity = newIntensity;
        isTransitioning = true;
    }
    
    private void UpdateLightingTransition()
    {
        if (mainLight == null) return;
        
        // Smoothly change light color and intensity
        mainLight.color = Color.Lerp(mainLight.color, targetColor, Time.deltaTime * transitionSpeed);
        mainLight.intensity = Mathf.Lerp(mainLight.intensity, targetIntensity, Time.deltaTime * transitionSpeed);
        
        // Smoothly change ambient light
        RenderSettings.ambientLight = Color.Lerp(RenderSettings.ambientLight, targetColor * 0.2f, Time.deltaTime * transitionSpeed);
        
        // Check if transition is complete (compare color components)
        if (Mathf.Abs(mainLight.color.r - targetColor.r) < 0.01f &&
            Mathf.Abs(mainLight.color.g - targetColor.g) < 0.01f &&
            Mathf.Abs(mainLight.color.b - targetColor.b) < 0.01f)
        {
            isTransitioning = false;
        }
    }
    
    // Public methods for other scripts to call
    public void MakeItEerie() => TransitionToEerie();
    public void MakeItScary() => TransitionToScary();
    public void MakeItCreepy() => TransitionToCreepy();
    public void MakeItNormal() => TransitionToNormal();
    
    // Developer helper methods
    [ContextMenu("Set Normal Lighting")]
    private void SetNormalLightingContext()
    {
        SetNormalLighting();
    }
    
    [ContextMenu("Set Eerie Lighting")]
    private void SetEerieLightingContext()
    {
        SetEerieLighting();
    }
    
    [ContextMenu("Set Scary Lighting")]
    private void SetScaryLightingContext()
    {
        SetScaryLighting();
    }
    
    [ContextMenu("Set Creepy Lighting")]
    private void SetCreepyLightingContext()
    {
        SetCreepyLighting();
    }
} 