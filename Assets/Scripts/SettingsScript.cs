using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering;

/// <summary>
/// Comprehensive settings controller for brightness, audio, and display options.
///
/// FEATURES:
/// - Brightness control via URP post-processing exposure
/// - Volume control via AudioMixer (with AudioManager fallback)
/// - Resolution, fullscreen, and VSync settings
/// - Auto-save on change (enabled by default)
/// - Reset to defaults with confirmation dialog
/// - Robust validation and error handling
///
/// SETUP REQUIREMENTS:
/// - Assign VolumeProfile with ColorAdjustments component for brightness
/// - Assign AudioMixer with "MasterVolume" parameter for audio control
/// - Assign UI elements (sliders, dropdowns, toggles) in Inspector
/// - Optional: Assign reset confirmation dialog GameObject
///
/// INTEGRATION:
/// - Works with PauseMenu for in-game settings
/// - Works with MainMenuController for main menu settings
/// - AudioSources should be routed through AudioMixer groups for proper volume control
/// </summary>
public class SettingsScript : MonoBehaviour
{
    [Header("Post-Processing (Brightness via Exposure)")]
    [SerializeField] private VolumeProfile globalVolumeProfile;
    [SerializeField] private float minExposure = -2f;
    [SerializeField] private float maxExposure = 2f;
    [SerializeField] private Slider brightnessSlider;

    [Header("Audio")]
    [SerializeField] private AudioMixer masterMixer;
    [SerializeField] private string masterVolumeParameter = "MasterVolume";
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private float minVolumeDb = -40f;
    [SerializeField] private float maxVolumeDb = 0f;

    [Header("Display")]
    [SerializeField] private Dropdown resolutionDropdown;
    [SerializeField] private TMP_Dropdown resolutionDropdownTMP;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Toggle vSyncToggle;

    [Header("Optional")]
    [SerializeField] private string settingsSceneName;
    [SerializeField] private GameObject pauseMenuRoot;
    [SerializeField] private GameObject settingsMenuRoot;
    [SerializeField] private GameObject resetConfirmationDialog;

    private Resolution[] availableResolutions;
    private bool isApplyingChanges;
    private bool autoSaveEnabled = true; // Auto-save settings on change

    private void Awake()
    {
        // Validate critical components
        ValidateComponents();

        if (brightnessSlider != null)
        {
            brightnessSlider.minValue = 0f;
            brightnessSlider.maxValue = 1f;
            brightnessSlider.onValueChanged.AddListener(OnBrightnessSliderChanged);
        }

        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
            volumeSlider.onValueChanged.AddListener(OnVolumeSliderChanged);
        }

        if (fullscreenToggle != null)
        {
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggleChanged);
        }

        if (vSyncToggle != null)
        {
            vSyncToggle.onValueChanged.AddListener(OnVSyncToggleChanged);
        }

        PopulateResolutionDropdown();
        LoadSavedSettings();
    }

    /// <summary>
    /// Validates that all critical components are assigned and logs warnings if missing
    /// </summary>
    private void ValidateComponents()
    {
        if (globalVolumeProfile == null)
        {
            Debug.LogWarning("[SettingsScript] Global Volume Profile is not assigned! Brightness control will not work. " +
                           "Please assign the VolumeProfile in the Inspector.", this);
        }
        else
        {
            // Check if ColorAdjustments component exists in the profile
            if (!globalVolumeProfile.TryGet(out UnityEngine.Rendering.Universal.ColorAdjustments colorAdjustments))
            {
                Debug.LogWarning("[SettingsScript] ColorAdjustments component not found in Volume Profile! " +
                               "Add ColorAdjustments to enable brightness control.", this);
            }
        }

        if (masterMixer == null)
        {
            Debug.LogWarning("[SettingsScript] Master Audio Mixer is not assigned! Volume control will not work. " +
                           "Please assign the AudioMixer in the Inspector.", this);
        }
        else
        {
            // Verify the master volume parameter exists
            float testValue;
            if (!masterMixer.GetFloat(masterVolumeParameter, out testValue))
            {
                Debug.LogError($"[SettingsScript] Audio Mixer parameter '{masterVolumeParameter}' not found! " +
                             $"Please ensure the parameter exists in the mixer or update the parameter name.", this);
            }
        }

        if (brightnessSlider == null)
        {
            Debug.LogWarning("[SettingsScript] Brightness Slider is not assigned. Brightness control UI will not work.", this);
        }

        if (volumeSlider == null)
        {
            Debug.LogWarning("[SettingsScript] Volume Slider is not assigned. Volume control UI will not work.", this);
        }
    }

    private void OnDestroy()
    {
        if (brightnessSlider != null)
        {
            brightnessSlider.onValueChanged.RemoveListener(OnBrightnessSliderChanged);
        }

        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.RemoveListener(OnVolumeSliderChanged);
        }

        if (fullscreenToggle != null)
        {
            fullscreenToggle.onValueChanged.RemoveListener(OnFullscreenToggleChanged);
        }

        if (vSyncToggle != null)
        {
            vSyncToggle.onValueChanged.RemoveListener(OnVSyncToggleChanged);
        }
    }

    private void PopulateResolutionDropdown()
    {
        if (resolutionDropdown == null && resolutionDropdownTMP == null)
        {
            return;
        }

        availableResolutions = Screen.resolutions;

        var options = new List<string>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < availableResolutions.Length; i++)
        {
            string option = $"{availableResolutions[i].width} x {availableResolutions[i].height} @ {availableResolutions[i].refreshRate}Hz";
            options.Add(option);

            if (availableResolutions[i].width == Screen.currentResolution.width &&
                availableResolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        if (resolutionDropdown != null)
        {
            resolutionDropdown.ClearOptions();
            resolutionDropdown.AddOptions(options);

            // Prevent onChange callbacks during initialization
            isApplyingChanges = true;
            resolutionDropdown.value = currentResolutionIndex;
            resolutionDropdown.RefreshShownValue();
            isApplyingChanges = false;

            resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        }

        if (resolutionDropdownTMP != null)
        {
            resolutionDropdownTMP.ClearOptions();
            resolutionDropdownTMP.AddOptions(options);

            // Prevent onChange callbacks during initialization
            isApplyingChanges = true;
            resolutionDropdownTMP.value = currentResolutionIndex;
            resolutionDropdownTMP.RefreshShownValue();
            isApplyingChanges = false;

            resolutionDropdownTMP.onValueChanged.AddListener(OnResolutionChangedTMP);
        }
    }

    /// <summary>
    /// Loads saved settings from PlayerPrefs with validation and applies them
    /// </summary>
    private void LoadSavedSettings()
    {
        isApplyingChanges = true;

        // Load brightness with clamping to valid range
        if (brightnessSlider != null)
        {
            float savedExposure = PlayerPrefs.GetFloat("Settings_Brightness", 0f);
            // Clamp to valid exposure range
            savedExposure = Mathf.Clamp(savedExposure, minExposure, maxExposure);
            brightnessSlider.value = Mathf.InverseLerp(minExposure, maxExposure, savedExposure);
            ApplyBrightness(savedExposure);
        }

        // Load volume with clamping to 0-1 range
        if (volumeSlider != null)
        {
            float savedVolume = PlayerPrefs.GetFloat("Settings_Volume", 1f);
            // Clamp to normalized range
            savedVolume = Mathf.Clamp01(savedVolume);
            volumeSlider.value = savedVolume;
            ApplyVolume(savedVolume);
        }

        // Load fullscreen setting
        if (fullscreenToggle != null)
        {
            bool isFullscreen = PlayerPrefs.GetInt("Settings_Fullscreen", Screen.fullScreen ? 1 : 0) == 1;
            fullscreenToggle.isOn = isFullscreen;
            Screen.fullScreen = isFullscreen;
        }

        // Load VSync setting
        if (vSyncToggle != null)
        {
            bool isVSync = PlayerPrefs.GetInt("Settings_VSync", QualitySettings.vSyncCount > 0 ? 1 : 0) == 1;
            vSyncToggle.isOn = isVSync;
            QualitySettings.vSyncCount = isVSync ? 1 : 0;
        }

        // Load resolution with robust validation
        if (availableResolutions != null && availableResolutions.Length > 0)
        {
            int savedResolutionIndex = PlayerPrefs.GetInt("Settings_ResolutionIndex", -1);

            // Validate the saved index
            if (savedResolutionIndex >= 0 && savedResolutionIndex < availableResolutions.Length)
            {
                ApplyResolution(savedResolutionIndex);

                if (resolutionDropdown != null)
                {
                    resolutionDropdown.value = savedResolutionIndex;
                    resolutionDropdown.RefreshShownValue();
                }

                if (resolutionDropdownTMP != null)
                {
                    resolutionDropdownTMP.value = savedResolutionIndex;
                    resolutionDropdownTMP.RefreshShownValue();
                }
            }
            else if (savedResolutionIndex != -1)
            {
                // Invalid saved index - clear it and use current resolution
                Debug.LogWarning($"[SettingsScript] Saved resolution index {savedResolutionIndex} is invalid (valid range: 0-{availableResolutions.Length - 1}). Resetting to current resolution.");
                PlayerPrefs.DeleteKey("Settings_ResolutionIndex");
            }
            else
            {
                // First launch - no saved resolution, apply native/current resolution
                Debug.Log("[SettingsScript] First launch detected - applying native resolution for best presentation.");

                // Find the index of the current/native resolution
                int nativeResolutionIndex = -1;
                for (int i = 0; i < availableResolutions.Length; i++)
                {
                    if (availableResolutions[i].width == Screen.currentResolution.width &&
                        availableResolutions[i].height == Screen.currentResolution.height)
                    {
                        nativeResolutionIndex = i;
                        break;
                    }
                }

                // If we found the native resolution, use it; otherwise use highest available
                if (nativeResolutionIndex >= 0)
                {
                    ApplyResolution(nativeResolutionIndex);

                    if (resolutionDropdown != null)
                    {
                        resolutionDropdown.value = nativeResolutionIndex;
                        resolutionDropdown.RefreshShownValue();
                    }

                    if (resolutionDropdownTMP != null)
                    {
                        resolutionDropdownTMP.value = nativeResolutionIndex;
                        resolutionDropdownTMP.RefreshShownValue();
                    }

                    // Save this as the default for future launches
                    PlayerPrefs.SetInt("Settings_ResolutionIndex", nativeResolutionIndex);
                    PlayerPrefs.Save();

                    Debug.Log($"[SettingsScript] Applied native resolution: {availableResolutions[nativeResolutionIndex].width}x{availableResolutions[nativeResolutionIndex].height}");
                }
                else
                {
                    // Fallback: use highest available resolution
                    int highestIndex = availableResolutions.Length - 1;
                    ApplyResolution(highestIndex);

                    if (resolutionDropdown != null)
                    {
                        resolutionDropdown.value = highestIndex;
                        resolutionDropdown.RefreshShownValue();
                    }

                    if (resolutionDropdownTMP != null)
                    {
                        resolutionDropdownTMP.value = highestIndex;
                        resolutionDropdownTMP.RefreshShownValue();
                    }

                    PlayerPrefs.SetInt("Settings_ResolutionIndex", highestIndex);
                    PlayerPrefs.Save();

                    Debug.Log($"[SettingsScript] Applied highest resolution: {availableResolutions[highestIndex].width}x{availableResolutions[highestIndex].height}");
                }
            }
        }

        isApplyingChanges = false;
    }

    public void OpenSettings()
    {
        if (!string.IsNullOrEmpty(settingsSceneName))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(settingsSceneName);
        }
    }

    /// <summary>
    /// Called when Apply button is pressed. Saves all settings.
    /// Note: With auto-save enabled, settings are already saved on change.
    /// This provides an additional save guarantee.
    /// </summary>
    public void ApplyButtonPressed()
    {
        SaveSettings();
    }

    /// <summary>
    /// Shows a confirmation dialog before resetting settings to defaults
    /// </summary>
    public void ResetToDefaults()
    {
        if (resetConfirmationDialog != null)
        {
            resetConfirmationDialog.SetActive(true);
        }
        else
        {
            // No confirmation dialog assigned - proceed directly with reset
            Debug.LogWarning("[SettingsScript] Reset confirmation dialog is not assigned. Resetting settings without confirmation.");
            ConfirmResetToDefaults();
        }
    }

    /// <summary>
    /// Confirms and executes the reset to default settings
    /// Call this from the "Yes" button on the confirmation dialog
    /// </summary>
    public void ConfirmResetToDefaults()
    {
        PlayerPrefs.DeleteKey("Settings_Brightness");
        PlayerPrefs.DeleteKey("Settings_Volume");
        PlayerPrefs.DeleteKey("Settings_Fullscreen");
        PlayerPrefs.DeleteKey("Settings_VSync");
        PlayerPrefs.DeleteKey("Settings_ResolutionIndex");
        PlayerPrefs.Save();

        // Hide confirmation dialog
        if (resetConfirmationDialog != null)
        {
            resetConfirmationDialog.SetActive(false);
        }

        // Reload settings with defaults
        LoadSavedSettings();

        Debug.Log("[SettingsScript] Settings have been reset to defaults.");
    }

    /// <summary>
    /// Cancels the reset operation
    /// Call this from the "No/Cancel" button on the confirmation dialog
    /// </summary>
    public void CancelResetToDefaults()
    {
        if (resetConfirmationDialog != null)
        {
            resetConfirmationDialog.SetActive(false);
        }
    }

    private void SaveSettings()
    {
        if (brightnessSlider != null)
        {
            float targetExposure = Mathf.Lerp(minExposure, maxExposure, brightnessSlider.value);
            PlayerPrefs.SetFloat("Settings_Brightness", targetExposure);
        }

        if (volumeSlider != null)
        {
            PlayerPrefs.SetFloat("Settings_Volume", volumeSlider.value);
        }

        if (fullscreenToggle != null)
        {
            PlayerPrefs.SetInt("Settings_Fullscreen", fullscreenToggle.isOn ? 1 : 0);
        }

        if (vSyncToggle != null)
        {
            PlayerPrefs.SetInt("Settings_VSync", vSyncToggle.isOn ? 1 : 0);
        }

        int selectedResolutionIndex = -1;

        if (resolutionDropdown != null)
        {
            selectedResolutionIndex = resolutionDropdown.value;
        }
        else if (resolutionDropdownTMP != null)
        {
            selectedResolutionIndex = resolutionDropdownTMP.value;
        }

        if (selectedResolutionIndex >= 0)
        {
            PlayerPrefs.SetInt("Settings_ResolutionIndex", selectedResolutionIndex);
        }

        PlayerPrefs.Save();
    }

    private void OnBrightnessSliderChanged(float normalizedValue)
    {
        if (isApplyingChanges)
        {
            return;
        }

        float targetExposure = Mathf.Lerp(minExposure, maxExposure, normalizedValue);
        ApplyBrightness(targetExposure);

        // Auto-save brightness setting
        if (autoSaveEnabled)
        {
            PlayerPrefs.SetFloat("Settings_Brightness", targetExposure);
            PlayerPrefs.Save();
        }
    }

    private void ApplyBrightness(float exposureValue)
    {
        if (globalVolumeProfile == null)
        {
            return;
        }

        if (globalVolumeProfile.TryGet(out UnityEngine.Rendering.Universal.ColorAdjustments colorAdjustments))
        {
            colorAdjustments.postExposure.Override(exposureValue);
        }
    }

    private void OnVolumeSliderChanged(float normalizedValue)
    {
        if (isApplyingChanges)
        {
            return;
        }

        ApplyVolume(normalizedValue);

        // Auto-save volume setting
        if (autoSaveEnabled)
        {
            PlayerPrefs.SetFloat("Settings_Volume", normalizedValue);
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// Applies volume setting to the master mixer
    /// NOTE: For proper audio control, ensure all AudioSources in your scenes are routed through
    /// the Master Mixer's audio groups. This allows centralized volume control.
    /// </summary>
    private void ApplyVolume(float normalizedValue)
    {
        if (masterMixer == null)
        {
            return;
        }

        float volumeDb = Mathf.Lerp(minVolumeDb, maxVolumeDb, normalizedValue);
        masterMixer.SetFloat(masterVolumeParameter, volumeDb);

        // Additional: Update AudioManager if it exists and uses direct AudioSource volumes
        // This is a fallback for audio sources not routed through the mixer
        UpdateAudioManagerVolumes(normalizedValue);
    }

    /// <summary>
    /// Updates AudioManager volumes to match the master volume setting
    /// This is a helper method for audio sources not routed through the mixer
    /// </summary>
    private void UpdateAudioManagerVolumes(float masterVolume)
    {
        AudioManager audioManager = FindObjectOfType<AudioManager>();
        if (audioManager != null)
        {
            // Note: This assumes AudioManager's volume parameters are multipliers
            // If AudioManager is properly integrated with the mixer, this step isn't needed

            // Store original volume ratios and apply master volume as a multiplier
            if (audioManager.mapAmbience != null)
            {
                audioManager.mapAmbience.volume = audioManager.ambienceVolume * masterVolume;
            }
            if (audioManager.enemyAmbience != null)
            {
                audioManager.enemyAmbience.volume = audioManager.ambienceVolume * masterVolume;
            }
            if (audioManager.enemyFootsteps != null)
            {
                audioManager.enemyFootsteps.volume = audioManager.footstepsVolume * masterVolume;
            }
            if (audioManager.chaseMusic != null)
            {
                audioManager.chaseMusic.volume = audioManager.chaseMusicVolume * masterVolume;
            }
            if (audioManager.randomAmbienceSource != null)
            {
                audioManager.randomAmbienceSource.volume = 0.5f * masterVolume; // Default 0.5 from RandomAmbienceCoroutine
            }
        }
    }

    private void OnFullscreenToggleChanged(bool isFullscreen)
    {
        if (isApplyingChanges)
        {
            return;
        }

        Screen.fullScreen = isFullscreen;

        // Auto-save fullscreen setting
        if (autoSaveEnabled)
        {
            PlayerPrefs.SetInt("Settings_Fullscreen", isFullscreen ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    private void OnVSyncToggleChanged(bool isOn)
    {
        if (isApplyingChanges)
        {
            return;
        }

        QualitySettings.vSyncCount = isOn ? 1 : 0;

        // Auto-save VSync setting
        if (autoSaveEnabled)
        {
            PlayerPrefs.SetInt("Settings_VSync", isOn ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    private void OnResolutionChanged(int index)
    {
        if (isApplyingChanges)
        {
            return;
        }

        ApplyResolution(index);

        // Auto-save resolution setting
        if (autoSaveEnabled)
        {
            PlayerPrefs.SetInt("Settings_ResolutionIndex", index);
            PlayerPrefs.Save();
        }
    }

    private void OnResolutionChangedTMP(int index)
    {
        if (isApplyingChanges)
        {
            return;
        }

        ApplyResolution(index);

        // Auto-save resolution setting
        if (autoSaveEnabled)
        {
            PlayerPrefs.SetInt("Settings_ResolutionIndex", index);
            PlayerPrefs.Save();
        }
    }

    private void ApplyResolution(int index)
    {
        if (availableResolutions == null || index < 0 || index >= availableResolutions.Length)
        {
            return;
        }

        Resolution target = availableResolutions[index];
        Screen.SetResolution(target.width, target.height, Screen.fullScreen, target.refreshRate);
    }

    /// <summary>
    /// Called when Apply button is pressed in settings panel.
    /// Saves settings and navigates back to the appropriate menu.
    /// </summary>
    public void OnApplyButtonPressed()
    {
        SaveSettings();

        // Navigate back to previous menu (pause menu or main menu)
        if (pauseMenuRoot != null)
        {
            // In-game: return to pause menu
            ShowPauseMenu();
        }
        else if (settingsMenuRoot != null)
        {
            // Main menu: just close settings panel
            settingsMenuRoot.SetActive(false);
        }
    }

    /// <summary>
    /// Called when Back button is pressed in settings panel.
    /// Saves settings and returns to pause menu.
    /// </summary>
    public void OnBackButtonPressed()
    {
        SaveSettings();
        ShowPauseMenu();
    }

    /// <summary>
    /// Alternative callback for brightness slider (for UI events that use OnValueChanged with drag detection)
    /// </summary>
    public void OnBrightnessSliderDragged(float normalizedValue)
    {
        OnBrightnessSliderChanged(normalizedValue);
    }

    /// <summary>
    /// Alternative callback for volume slider (for UI events that use OnValueChanged with drag detection)
    /// </summary>
    public void OnVolumeSliderDragged(float normalizedValue)
    {
        OnVolumeSliderChanged(normalizedValue);
    }

    public void ShowSettingsMenu()
    {
        if (pauseMenuRoot != null)
        {
            pauseMenuRoot.SetActive(false);
        }

        if (settingsMenuRoot != null)
        {
            settingsMenuRoot.SetActive(true);
        }
    }

    public void ShowPauseMenu()
    {
        if (settingsMenuRoot != null)
        {
            settingsMenuRoot.SetActive(false);
        }

        if (pauseMenuRoot != null)
        {
            pauseMenuRoot.SetActive(true);
        }
    }
}
