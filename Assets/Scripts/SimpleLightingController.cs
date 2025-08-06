using UnityEngine;

public class SimpleLightingController : MonoBehaviour
{
    [Header("Lighting Settings")]
    public Light mainLight;
    [ColorUsage(true, true)]
    public Color customColor = Color.white;
    [Range(0, 8)]
    public float customIntensity = 1.0f;

    private void Start()
    {
        ApplyLighting();
    }

    private void OnValidate()
    {
        ApplyLighting();
    }

    private void ApplyLighting()
    {
        if (mainLight == null)
        {
            mainLight = FindAnyObjectByType<Light>();
        }
        if (mainLight != null)
        {
            mainLight.color = customColor;
            mainLight.intensity = customIntensity;
        }
        RenderSettings.ambientLight = customColor * 0.3f;
    }
} 