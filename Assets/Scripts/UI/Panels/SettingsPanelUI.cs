using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPanelUI : UIPanel
{
    [Header("Audio Sliders")]
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;

    [Header("Display")]
    public Toggle fullscreenToggle;
    public Toggle vSyncToggle;
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown fpsDropdown;

    private Resolution[] resolutions;

    private void OnEnable()
    {
        if (SettingsManager.Instance == null)
            return;

        // Audio
        masterSlider.SetValueWithoutNotify(SettingsManager.Instance.GetMasterVolume());
        musicSlider.SetValueWithoutNotify(SettingsManager.Instance.GetMusicVolume());
        sfxSlider.SetValueWithoutNotify(SettingsManager.Instance.GetSFXVolume());

        // Display Toggles
        fullscreenToggle.SetIsOnWithoutNotify(SettingsManager.Instance.GetFullscreen());
        vSyncToggle.SetIsOnWithoutNotify(SettingsManager.Instance.GetVSync());

        // Resolutions
        resolutions = Screen.resolutions;
        PopulateResolutions();

        // FPS dropdown
        PopulateFPSLimits();
    }

    private void PopulateResolutions()
    {
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();

        for (int i = 0; i < resolutions.Length; i++)
        {
            Resolution res = resolutions[i];
            options.Add($"{res.width} x {res.height} @ {res.refreshRateRatio.value:0}hz");
        }

        resolutionDropdown.AddOptions(options);

        int savedIndex = SettingsManager.Instance.GetResolutionIndex();
        savedIndex = Mathf.Clamp(savedIndex, 0, resolutions.Length - 1);

        resolutionDropdown.SetValueWithoutNotify(savedIndex);
        resolutionDropdown.RefreshShownValue();
    }

    private void PopulateFPSLimits()
    {
        fpsDropdown.ClearOptions();

        List<string> options = new List<string>()
        {
            "30",
            "60",
            "120",
            "144",
            "240",
            "Unlimited"
        };

        fpsDropdown.AddOptions(options);

        int currentLimit = SettingsManager.Instance.GetFPSLimit();

        int dropdownIndex = currentLimit switch
        {
            30 => 0,
            60 => 1,
            120 => 2,
            144 => 3,
            240 => 4,
            _ => 5
        };

        fpsDropdown.SetValueWithoutNotify(dropdownIndex);
        fpsDropdown.RefreshShownValue();
    }

    // ===== Audio =====
    public void OnMasterChanged(float value) => SettingsManager.Instance.SetMasterVolume(value);
    public void OnMusicChanged(float value) => SettingsManager.Instance.SetMusicVolume(value);
    public void OnSFXChanged(float value) => SettingsManager.Instance.SetSFXVolume(value);

    // ===== Display =====
    public void OnFullscreenChanged(bool value) => SettingsManager.Instance.SetFullscreen(value);
    public void OnVSyncChanged(bool value) => SettingsManager.Instance.SetVSync(value);

    public void OnResolutionChanged(int index)
    {
        SettingsManager.Instance.SetResolutionIndex(index);
    }

    public void OnFPSLimitChanged(int index)
    {
        int fps = index switch
        {
            0 => 30,
            1 => 60,
            2 => 120,
            3 => 144,
            4 => 240,
            _ => -1 // unlimited
        };

        SettingsManager.Instance.SetFPSLimit(fps);
    }
}