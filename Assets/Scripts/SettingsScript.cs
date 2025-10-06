using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles runtime configuration of core video/audio options exposed by the Settings menu UI.
/// Hook the serialized fields up to the hierarchy shown in the provided screenshot:
/// Settings Menu → SettingsMenu → Panel Settings → {BrightnessGroup, VolumeGroup, DropdownGroup, ToggleGroup}.
/// </summary>
public class SettingsScript : MonoBehaviour
{
    [Header("Brightness")]
    [SerializeField] private Light globalLight;
    [SerializeField, Range(0.1f, 3f)] private float minBrightness = 0.3f;
    [SerializeField, Range(0.1f, 3f)] private float maxBrightness = 2.0f;
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
    [SerializeField] private string settingsSceneName;

    private Resolution[] availableResolutions;
    private const string PrefBrightness = "Settings_Brightness";
    private const string PrefVolume = "Settings_MasterVolume";
    private const string PrefResolution = "Settings_ResolutionIndex";
    private const string PrefFullscreen = "Settings_Fullscreen";
    private const string PrefVSync = "Settings_VSync";

    private void Awake()
    {
        CacheResolutions();
        LoadPreferences();
        ApplyAllSettings();
    }

    public void OpenSettings()
    {
        if (string.IsNullOrWhiteSpace(settingsSceneName))
        {
            Debug.LogWarning("Settings scene name is not configured.", this);
            return;
        }

        if (SceneManager.GetSceneByName(settingsSceneName).isLoaded)
        {
            Debug.LogWarning("Settings scene is already loaded.", this);
            return;
        }

        SceneManager.LoadScene(settingsSceneName, LoadSceneMode.Additive);
    }

    public void OnBrightnessChanged(float value)
    {
        if (brightnessSlider == null)
        {
            return;
        }

        float normalized = Mathf.Clamp01(value);
        float intensity = Mathf.Lerp(minBrightness, maxBrightness, normalized);

        if (globalLight != null)
        {
            globalLight.intensity = intensity;
        }

        PlayerPrefs.SetFloat(PrefBrightness, normalized);
    }

    public void OnVolumeChanged(float value)
    {
        if (volumeSlider == null || masterMixer == null)
        {
            return;
        }

        float normalized = Mathf.Clamp01(value);
        float db = Mathf.Lerp(minVolumeDb, maxVolumeDb, normalized);
        masterMixer.SetFloat(masterVolumeParameter, db);

        PlayerPrefs.SetFloat(PrefVolume, normalized);
    }

    public void OnResolutionChanged(int dropdownIndex)
    {
        if (availableResolutions == null || availableResolutions.Length == 0)
        {
            return;
        }

        dropdownIndex = Mathf.Clamp(dropdownIndex, 0, availableResolutions.Length - 1);
        Resolution chosen = availableResolutions[dropdownIndex];

        Screen.SetResolution(chosen.width, chosen.height, Screen.fullScreenMode, chosen.refreshRateRatio);
        PlayerPrefs.SetInt(PrefResolution, dropdownIndex);
    }

    public void OnFullscreenChanged(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt(PrefFullscreen, isFullscreen ? 1 : 0);
    }

    public void OnVSyncChanged(bool isEnabled)
    {
        QualitySettings.vSyncCount = isEnabled ? 1 : 0;
        PlayerPrefs.SetInt(PrefVSync, isEnabled ? 1 : 0);
    }

    public void OnApplyButtonPressed()
    {
        PlayerPrefs.Save();
        Debug.Log("Settings applied and saved.", this);

        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    public void OnBackButtonPressed()
    {
        PlayerPrefs.Save();
        if (!string.IsNullOrWhiteSpace(settingsSceneName))
        {
            Scene scene = SceneManager.GetSceneByName(settingsSceneName);
            if (scene.isLoaded)
            {
                SceneManager.UnloadSceneAsync(scene);
            }
        }
    }

    private void CacheResolutions()
    {
        availableResolutions = Screen.resolutions;

        if (resolutionDropdown == null && resolutionDropdownTMP == null)
        {
            return;
        }

        var options = new System.Collections.Generic.List<string>();
        foreach (Resolution res in availableResolutions)
        {
            options.Add($"{res.width} x {res.height} @ {res.refreshRateRatio.value:F0}Hz");
        }

        if (resolutionDropdown != null)
        {
            resolutionDropdown.ClearOptions();
            resolutionDropdown.AddOptions(options);
        }

        if (resolutionDropdownTMP != null)
        {
            resolutionDropdownTMP.ClearOptions();
            var optionDatas = new System.Collections.Generic.List<TMP_Dropdown.OptionData>();
            foreach (string option in options)
            {
                optionDatas.Add(new TMP_Dropdown.OptionData(option));
            }

            resolutionDropdownTMP.AddOptions(optionDatas);
        }
    }

    private void LoadPreferences()
    {
        float brightness = PlayerPrefs.GetFloat(PrefBrightness, 0.75f);
        float volume = PlayerPrefs.GetFloat(PrefVolume, 1f);
        int resolutionIndex = PlayerPrefs.GetInt(PrefResolution, GetCurrentResolutionIndex());
        bool fullscreen = PlayerPrefs.GetInt(PrefFullscreen, Screen.fullScreen ? 1 : 0) == 1;
        bool vSyncEnabled = PlayerPrefs.GetInt(PrefVSync, QualitySettings.vSyncCount > 0 ? 1 : 0) == 1;

        if (brightnessSlider != null)
        {
            brightnessSlider.SetValueWithoutNotify(brightness);
        }

        if (volumeSlider != null)
        {
            volumeSlider.SetValueWithoutNotify(volume);
        }

        if (resolutionDropdown != null)
        {
            resolutionIndex = Mathf.Clamp(resolutionIndex, 0, Mathf.Max(availableResolutions.Length - 1, 0));
            resolutionDropdown.SetValueWithoutNotify(resolutionIndex);
        }

        if (resolutionDropdownTMP != null)
        {
            resolutionIndex = Mathf.Clamp(resolutionIndex, 0, Mathf.Max(availableResolutions.Length - 1, 0));
            resolutionDropdownTMP.SetValueWithoutNotify(resolutionIndex);
        }

        if (fullscreenToggle != null)
        {
            fullscreenToggle.SetIsOnWithoutNotify(fullscreen);
        }

        if (vSyncToggle != null)
        {
            vSyncToggle.SetIsOnWithoutNotify(vSyncEnabled);
        }
    }

    private void ApplyAllSettings()
    {
        if (brightnessSlider != null)
        {
            OnBrightnessChanged(brightnessSlider.value);
        }

        if (volumeSlider != null)
        {
            OnVolumeChanged(volumeSlider.value);
        }

        int resolutionValue = 0;
        bool hasResolutionDropdown = false;

        if (resolutionDropdown != null)
        {
            resolutionValue = resolutionDropdown.value;
            hasResolutionDropdown = true;
        }

        if (resolutionDropdownTMP != null)
        {
            resolutionValue = resolutionDropdownTMP.value;
            hasResolutionDropdown = true;
        }

        if (hasResolutionDropdown)
        {
            OnResolutionChanged(resolutionValue);
        }

        if (fullscreenToggle != null)
        {
            OnFullscreenChanged(fullscreenToggle.isOn);
        }

        if (vSyncToggle != null)
        {
            OnVSyncChanged(vSyncToggle.isOn);
        }
    }

    private int GetCurrentResolutionIndex()
    {
        if (availableResolutions == null || availableResolutions.Length == 0)
        {
            return 0;
        }

        Resolution current = Screen.currentResolution;
        for (int i = 0; i < availableResolutions.Length; i++)
        {
            Resolution candidate = availableResolutions[i];
            if (candidate.width == current.width && candidate.height == current.height &&
                Mathf.Approximately((float)candidate.refreshRateRatio.value, (float)current.refreshRateRatio.value))
            {
                return i;
            }
        }

        return availableResolutions.Length - 1;
    }
}
