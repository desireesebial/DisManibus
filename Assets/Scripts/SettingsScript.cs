using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering;

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

    private Resolution[] availableResolutions;
    private bool isApplyingChanges;

    private void Awake()
    {
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
            resolutionDropdown.value = currentResolutionIndex;
            resolutionDropdown.RefreshShownValue();
            resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        }

        if (resolutionDropdownTMP != null)
        {
            resolutionDropdownTMP.ClearOptions();
            resolutionDropdownTMP.AddOptions(options);
            resolutionDropdownTMP.value = currentResolutionIndex;
            resolutionDropdownTMP.RefreshShownValue();
            resolutionDropdownTMP.onValueChanged.AddListener(OnResolutionChangedTMP);
        }
    }

    private void LoadSavedSettings()
    {
        isApplyingChanges = true;

        if (brightnessSlider != null)
        {
            float savedExposure = PlayerPrefs.GetFloat("Settings_Brightness", 0f);
            brightnessSlider.value = Mathf.InverseLerp(minExposure, maxExposure, savedExposure);
            ApplyBrightness(savedExposure);
        }

        if (volumeSlider != null)
        {
            float savedVolume = PlayerPrefs.GetFloat("Settings_Volume", 1f);
            volumeSlider.value = savedVolume;
            ApplyVolume(savedVolume);
        }

        if (fullscreenToggle != null)
        {
            bool isFullscreen = PlayerPrefs.GetInt("Settings_Fullscreen", Screen.fullScreen ? 1 : 0) == 1;
            fullscreenToggle.isOn = isFullscreen;
            Screen.fullScreen = isFullscreen;
        }

        if (vSyncToggle != null)
        {
            bool isVSync = PlayerPrefs.GetInt("Settings_VSync", QualitySettings.vSyncCount > 0 ? 1 : 0) == 1;
            vSyncToggle.isOn = isVSync;
            QualitySettings.vSyncCount = isVSync ? 1 : 0;
        }

        int savedResolutionIndex = PlayerPrefs.GetInt("Settings_ResolutionIndex", -1);
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

        isApplyingChanges = false;
    }

    public void OpenSettings()
    {
        if (!string.IsNullOrEmpty(settingsSceneName))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(settingsSceneName);
        }
    }

    public void ApplyButtonPressed()
    {
        SaveSettings();
    }

    public void ResetToDefaults()
    {
        PlayerPrefs.DeleteKey("Settings_Brightness");
        PlayerPrefs.DeleteKey("Settings_Volume");
        PlayerPrefs.DeleteKey("Settings_Fullscreen");
        PlayerPrefs.DeleteKey("Settings_VSync");
        PlayerPrefs.DeleteKey("Settings_ResolutionIndex");

        LoadSavedSettings();
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
    }

    private void ApplyVolume(float normalizedValue)
    {
        if (masterMixer == null)
        {
            return;
        }

        float volumeDb = Mathf.Lerp(minVolumeDb, maxVolumeDb, normalizedValue);
        masterMixer.SetFloat(masterVolumeParameter, volumeDb);
    }

    private void OnFullscreenToggleChanged(bool isFullscreen)
    {
        if (isApplyingChanges)
        {
            return;
        }

        Screen.fullScreen = isFullscreen;
    }

    private void OnVSyncToggleChanged(bool isOn)
    {
        if (isApplyingChanges)
        {
            return;
        }

        QualitySettings.vSyncCount = isOn ? 1 : 0;
    }

    private void OnResolutionChanged(int index)
    {
        if (isApplyingChanges)
        {
            return;
        }

        ApplyResolution(index);
    }

    private void OnResolutionChangedTMP(int index)
    {
        if (isApplyingChanges)
        {
            return;
        }

        ApplyResolution(index);
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

    public void OnBackButtonPressed()
    {
        SaveSettings();
        ShowPauseMenu();
    }

    public void OnBrightnessSliderDragged(float normalizedValue)
    {
        OnBrightnessSliderChanged(normalizedValue);
    }

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
