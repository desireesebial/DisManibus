using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simplified flashlight battery UI that displays battery level with color-coded fill.
/// Shows flashlight on/off state by swapping icon sprites and changing circle background color.
/// Hides during pause, cutscenes, and when player is dead.
/// </summary>
public class SimplifiedFlashlightBatteryUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to your FlashlightController")]
    public FlashlightController flashlight;

    [Tooltip("The battery slider UI")]
    public Slider batterySlider;

    [Tooltip("The Fill image of the slider")]
    public Image fillImage;

    [Header("Flashlight Icon")]
    [Tooltip("Flashlight icon image component")]
    public Image flashlightIcon;

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

    [Tooltip("Reference to PlayerHealthSystem (auto-assigned)")]
    public PlayerHealthSystem playerHealth;

    private GameObject panelObject;

    void Start()
    {
        // Cache the panel GameObject (this script's GameObject)
        panelObject = gameObject;

        // Try to auto-find PlayerHealthSystem if not assigned
        if (playerHealth == null)
        {
            playerHealth = Object.FindObjectOfType<PlayerHealthSystem>();
        }
    }

    void Update()
    {
        if (flashlight == null || batterySlider == null)
        {
            Debug.LogWarning("[SimplifiedFlashlightBatteryUI] Missing references!");
            return;
        }

        // Check if UI should be hidden
        HandleVisibility();

        // Always update battery values (even when hidden)
        UpdateBatteryUI();

        // Update flashlight icon opacity
        UpdateFlashlightIcon();
    }

    private void HandleVisibility()
    {
        // Determine if UI should be hidden
        bool isPaused = Time.timeScale == 0f;
        bool isCutscenePlaying = CutsceneControl.IsPlayingCutscene;
        bool isPlayerDead = playerHealth != null && playerHealth.IsDead();

        bool shouldHide = isPaused || isCutscenePlaying || isPlayerDead;

        // Update visibility
        if (panelObject != null && panelObject.activeSelf == shouldHide)
        {
            panelObject.SetActive(!shouldHide);
        }
    }

    private void UpdateBatteryUI()
    {
        if (flashlight == null || batterySlider == null)
            return;

        // Calculate percentage based on time remaining vs total capacity
        float percent = flashlight.batterySecondsRemaining / flashlight.batteryCapacitySeconds;
        percent = Mathf.Clamp01(percent);

        // Update slider value
        batterySlider.value = percent;

        // Color shift from green → yellow → red
        if (fillImage != null)
        {
            if (percent > 0.5f)
            {
                // Green to Yellow (50% - 100%)
                fillImage.color = Color.Lerp(Color.yellow, Color.green, (percent - 0.5f) * 2f);
            }
            else
            {
                // Red to Yellow (0% - 50%)
                fillImage.color = Color.Lerp(Color.red, Color.yellow, percent * 2f);
            }
        }
    }

    private void UpdateFlashlightIcon()
    {
        if (flashlightIcon == null || flashlight == null)
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

    /// <summary>
    /// Manually assign references if not set in inspector
    /// </summary>
    public void SetReferences(FlashlightController controller, Slider slider, Image fill, PlayerHealthSystem health = null, Image icon = null, Sprite onSprite = null, Sprite offSprite = null, Image circle = null)
    {
        flashlight = controller;
        batterySlider = slider;
        fillImage = fill;
        playerHealth = health;
        flashlightIcon = icon;
        flashlightOnSprite = onSprite;
        flashlightOffSprite = offSprite;
        circleBackground = circle;
    }
}
