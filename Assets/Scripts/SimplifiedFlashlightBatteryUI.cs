using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simplified flashlight battery UI that displays battery level with color-coded fill.
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

    /// <summary>
    /// Manually assign references if not set in inspector
    /// </summary>
    public void SetReferences(FlashlightController controller, Slider slider, Image fill, PlayerHealthSystem health = null)
    {
        flashlight = controller;
        batterySlider = slider;
        fillImage = fill;
        playerHealth = health;
    }
}
