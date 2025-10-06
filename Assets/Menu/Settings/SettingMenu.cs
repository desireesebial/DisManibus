using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsMenu : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Slider brightnessSlider;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Toggle vSyncToggle;

    [Header("Brightness Overlay")]
    [Tooltip("A full-screen black Image above gameplay (Raycast Target OFF).")]
    [SerializeField] private Image dimmerImage;
    [Range(0f, 1f)][SerializeField] private float dimmerMaxAlpha = 0.6f;

    // PlayerPrefs keys
    const string PP_BRIGHTNESS = "pp_brightness";
    const string PP_MASTER_VOL = "pp_master_vol";
    const string PP_FULLSCREEN = "pp_fullscreen";
    const string PP_VSYNC = "pp_vsync";
    const string PP_RES_INDEX = "pp_resolution_index";

    // Fixed options to match your dropdown entries
    readonly Vector2Int[] fixedResolutions = new Vector2Int[]
    {
        new Vector2Int(1920, 1080),
        new Vector2Int(1600, 900),
        new Vector2Int(1366, 768),
        new Vector2Int(1280, 720),
        new Vector2Int(1024, 768),
        new Vector2Int(800, 600),
    };

    void Awake()
    {
        // Ensure dropdown entries exist
        if (resolutionDropdown && resolutionDropdown.options.Count != fixedResolutions.Length)
        {
            var opts = new List<string>();
            foreach (var r in fixedResolutions) opts.Add($"{r.x}×{r.y}");
            resolutionDropdown.ClearOptions();
            resolutionDropdown.AddOptions(opts);
        }

        // Load prefs or defaults
        float br = PlayerPrefs.GetFloat(PP_BRIGHTNESS, 0.6f);
        float vol = PlayerPrefs.GetFloat(PP_MASTER_VOL, 0.8f);
        bool fs = PlayerPrefs.GetInt(PP_FULLSCREEN, Screen.fullScreen ? 1 : 0) == 1;
        bool vs = PlayerPrefs.GetInt(PP_VSYNC, QualitySettings.vSyncCount > 0 ? 1 : 0) == 1;
        int idx = Mathf.Clamp(PlayerPrefs.GetInt(PP_RES_INDEX, FindClosestResIndexToCurrent()), 0, fixedResolutions.Length - 1);

        // Push to UI
        if (brightnessSlider) brightnessSlider.value = br;
        if (masterVolumeSlider) masterVolumeSlider.value = vol;
        if (fullscreenToggle) fullscreenToggle.isOn = fs;
        if (vSyncToggle) vSyncToggle.isOn = vs;
        if (resolutionDropdown) resolutionDropdown.value = idx;

        // Apply to game
        ApplyBrightness(br);
        ApplyMasterVolume(vol);
        ApplyFullscreen(fs);
        ApplyVSync(vs);
        ApplyResolution(idx);
    }

    // === Apply handlers (hook via Inspector) ===
    public void ApplyBrightness(float t01)
    {
        t01 = Mathf.Clamp01(t01);
        if (dimmerImage)
        {
            var c = dimmerImage.color;
            c.a = Mathf.Lerp(dimmerMaxAlpha, 0f, t01);
            dimmerImage.color = c;
        }
        PlayerPrefs.SetFloat(PP_BRIGHTNESS, t01);
    }

    public void ApplyMasterVolume(float t01)
    {
        t01 = Mathf.Clamp01(t01);
        AudioListener.volume = t01; // global
        PlayerPrefs.SetFloat(PP_MASTER_VOL, t01);
    }

    public void ApplyResolution(int index)
    {
        index = Mathf.Clamp(index, 0, fixedResolutions.Length - 1);
        var r = fixedResolutions[index];
        Screen.SetResolution(r.x, r.y, Screen.fullScreen);
        PlayerPrefs.SetInt(PP_RES_INDEX, index);
        if (resolutionDropdown) resolutionDropdown.RefreshShownValue();
    }

    public void ApplyFullscreen(bool on)
    {
        Screen.fullScreen = on;
        PlayerPrefs.SetInt(PP_FULLSCREEN, on ? 1 : 0);
    }

    public void ApplyVSync(bool on)
    {
        QualitySettings.vSyncCount = on ? 1 : 0;
        PlayerPrefs.SetInt(PP_VSYNC, on ? 1 : 0);
    }

    public void OnApplyButton()
    {
        PlayerPrefs.Save();
    }

    public void OnBackButton()
    {
        PlayerPrefs.Save();
        gameObject.SetActive(false);   // PauseMenu.BackFromSubPanel() will show the pause panel again
    }

    int FindClosestResIndexToCurrent()
    {
        var cur = new Vector2Int(Screen.currentResolution.width, Screen.currentResolution.height);
        int best = 0; int bestDist = int.MaxValue;
        for (int i = 0; i < fixedResolutions.Length; i++)
        {
            int dx = fixedResolutions[i].x - cur.x;
            int dy = fixedResolutions[i].y - cur.y;
            int d2 = dx * dx + dy * dy;
            if (d2 < bestDist) { bestDist = d2; best = i; }
        }
        return best;
    }
}
