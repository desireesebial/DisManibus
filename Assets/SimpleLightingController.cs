using UnityEngine;

public class SimpleLightingController : MonoBehaviour
{
    [Header("Lighting Settings")]
    public Light mainLight;
    
    [Header("Atmosphere Presets")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private float normalIntensity = 1f;
    
    [SerializeField] private Color eerieColor = new Color(0.8f, 0.6f, 0.4f);
    [SerializeField] private float eerieIntensity = 0.6f;
    
    [SerializeField] private Color scaryColor = new Color(0.6f, 0.2f, 0.2f);
    [SerializeField] private float scaryIntensity = 0.3f;
    
    [SerializeField] private Color creepyColor = new Color(0.4f, 0.2f, 0.6f);
    [SerializeField] private float creepyIntensity = 0.1f;
    
    [Header("Developer Controls")]
    [SerializeField] private bool enableKeyboardControls = true;
    [SerializeField] private KeyCode normalKey = KeyCode.Alpha1;
    [SerializeField] private KeyCode eerieKey = KeyCode.Alpha2;
    [SerializeField] private KeyCode scaryKey = KeyCode.Alpha3;
    [SerializeField] private KeyCode creepyKey = KeyCode.Alpha4;
    
    [Header("Transition Settings")]
    public float transitionSpeed = 2f;
    
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
        // Handle keyboard input (developer only)
        if (enableKeyboardControls)
        {
            if (Input.GetKeyDown(normalKey))
            {
                SetNormalLighting();
            }
            else if (Input.GetKeyDown(eerieKey))
            {
                SetEerieLighting();
            }
            else if (Input.GetKeyDown(scaryKey))
            {
                SetScaryLighting();
            }
            else if (Input.GetKeyDown(creepyKey))
            {
                SetCreepyLighting();
            }
        }
        
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
        RenderSettings.ambientLight = normalColor;
    }
    
    public void SetEerieLighting()
    {
        if (mainLight != null)
        {
            mainLight.color = eerieColor;
            mainLight.intensity = eerieIntensity;
        }
        RenderSettings.ambientLight = eerieColor * 0.5f;
    }
    
    public void SetScaryLighting()
    {
        if (mainLight != null)
        {
            mainLight.color = scaryColor;
            mainLight.intensity = scaryIntensity;
        }
        RenderSettings.ambientLight = scaryColor * 0.3f;
    }
    
    public void SetCreepyLighting()
    {
        if (mainLight != null)
        {
            mainLight.color = creepyColor;
            mainLight.intensity = creepyIntensity;
        }
        RenderSettings.ambientLight = creepyColor * 0.2f;
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
        RenderSettings.ambientLight = Color.Lerp(RenderSettings.ambientLight, targetColor * 0.5f, Time.deltaTime * transitionSpeed);
        
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