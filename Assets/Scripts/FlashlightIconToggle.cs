using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple script to toggle flashlight icon opacity based on flashlight on/off state
/// Attach this to the Battery GameObject that contains the FlashlightIcon
/// </summary>
public class FlashlightIconToggle : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to the FlashlightController")]
    public FlashlightController flashlight;

    [Tooltip("The flashlight icon Image (will be auto-found if not assigned)")]
    public Image flashlightIcon;

    [Header("Settings")]
    [Tooltip("Opacity when flashlight is OFF (0-1)")]
    [Range(0f, 1f)]
    public float iconOffOpacity = 0.5f;

    void Start()
    {
        // Auto-find flashlight controller if not assigned
        if (flashlight == null)
        {
            flashlight = FindObjectOfType<FlashlightController>();
        }

        // Auto-find icon if not assigned
        if (flashlightIcon == null)
        {
            Transform iconTransform = transform.Find("FlashlightIcon");
            if (iconTransform != null)
            {
                flashlightIcon = iconTransform.GetComponent<Image>();
            }
        }
    }

    void Update()
    {
        if (flashlight == null || flashlightIcon == null)
            return;

        // Check if flashlight is currently on
        bool isOn = flashlight.IsFlashlightOn();

        // Get current color
        Color iconColor = flashlightIcon.color;

        // Set opacity based on flashlight state
        iconColor.a = isOn ? 1f : iconOffOpacity;

        // Apply color
        flashlightIcon.color = iconColor;
    }
}
