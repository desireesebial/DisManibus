using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Toggles flashlight icon sprite and circle background color based on flashlight on/off state
/// Attach this to the Battery GameObject that contains the FlashlightIcon
/// </summary>
public class FlashlightIconToggle : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to the FlashlightController")]
    public FlashlightController flashlight;

    [Tooltip("The flashlight icon Image (will be auto-found if not assigned)")]
    public Image flashlightIcon;

    [Header("Icon Sprites")]
    [Tooltip("Sprite to use when flashlight is ON")]
    public Sprite flashlightOnSprite;

    [Tooltip("Sprite to use when flashlight is OFF")]
    public Sprite flashlightOffSprite;

    [Header("Circle Background")]
    [Tooltip("Circle background image behind the flashlight icon")]
    public Image circleBackground;

    [Tooltip("Color of circle when flashlight is ON")]
    public Color circleOnColor = new Color(1f, 0.8f, 0f, 0.8f); // Bright yellow-orange with transparency

    [Tooltip("Color of circle when flashlight is OFF")]
    public Color circleOffColor = new Color(0.2f, 0.2f, 0.2f, 0.6f); // Dark gray with transparency

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

        // Swap icon sprite based on flashlight state
        if (flashlightOnSprite != null && flashlightOffSprite != null)
        {
            flashlightIcon.sprite = isOn ? flashlightOnSprite : flashlightOffSprite;
        }

        // Ensure icon is fully opaque
        Color iconColor = flashlightIcon.color;
        iconColor.a = 1f;
        flashlightIcon.color = iconColor;

        // Update circle background color
        if (circleBackground != null)
        {
            circleBackground.color = isOn ? circleOnColor : circleOffColor;
        }
    }
}
