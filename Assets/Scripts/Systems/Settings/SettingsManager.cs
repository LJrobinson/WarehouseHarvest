using UnityEngine;
using UnityEngine.Audio;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    [Header("Audio")]
    public AudioMixer audioMixer;

    private const string FULLSCREEN_KEY = "Fullscreen";
    private const string RESOLUTION_KEY = "ResolutionIndex";
    private const string VSYNC_KEY = "VSync";
    private const string FPSLIMIT_KEY = "FPSLimit";

    private bool fullscreen = true;
    private int resolutionIndex = 0;
    private bool vSync = true;
    private int fpsLimit = 60;

    private const string MASTER_KEY = "MasterVolume";
    private const string MUSIC_KEY = "MusicVolume";
    private const string SFX_KEY = "SFXVolume";

    private float masterVolume = 1f;
    private float musicVolume = 1f;
    private float sfxVolume = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadSettings();
        ApplyAudio();
        ApplyDisplaySettings();
    }

    public void SetMasterVolume(float value)
    {
        masterVolume = value;
        SetMixerVolume("MasterVolume", value);
        PlayerPrefs.SetFloat(MASTER_KEY, value);
    }

    public void SetMusicVolume(float value)
    {
        musicVolume = value;
        SetMixerVolume("MusicVolume", value);
        PlayerPrefs.SetFloat(MUSIC_KEY, value);
    }

    public void SetSFXVolume(float value)
    {
        sfxVolume = value;
        SetMixerVolume("SFXVolume", value);
        PlayerPrefs.SetFloat(SFX_KEY, value);
    }

    public float GetMasterVolume() => masterVolume;
    public float GetMusicVolume() => musicVolume;
    public float GetSFXVolume() => sfxVolume;

    private void LoadSettings()
    {
        fullscreen = PlayerPrefs.GetInt(FULLSCREEN_KEY, 1) == 1;
        resolutionIndex = PlayerPrefs.GetInt(RESOLUTION_KEY, 0);
        vSync = PlayerPrefs.GetInt(VSYNC_KEY, 1) == 1;
        fpsLimit = PlayerPrefs.GetInt(FPSLIMIT_KEY, 60);
        masterVolume = PlayerPrefs.GetFloat(MASTER_KEY, 1f);
        musicVolume = PlayerPrefs.GetFloat(MUSIC_KEY, 1f);
        sfxVolume = PlayerPrefs.GetFloat(SFX_KEY, 1f);
    }

    private void ApplyAudio()
    {
        SetMixerVolume("MasterVolume", masterVolume);
        SetMixerVolume("MusicVolume", musicVolume);
        SetMixerVolume("SFXVolume", sfxVolume);
    }

    private void SetMixerVolume(string param, float value)
    {
        // Slider is 0.0001 to 1, convert to decibels
        float dB = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20f;
        audioMixer.SetFloat(param, dB);
    }

    public void ApplyDisplaySettings()
    {
        // Fullscreen
        Screen.fullScreen = fullscreen;

        // Resolution
        Resolution[] resolutions = Screen.resolutions;

        if (resolutions.Length > 0)
        {
            resolutionIndex = Mathf.Clamp(resolutionIndex, 0, resolutions.Length - 1);
            Resolution res = resolutions[resolutionIndex];
            Screen.SetResolution(res.width, res.height, fullscreen);
        }

        // VSync
        QualitySettings.vSyncCount = vSync ? 1 : 0;

        // FPS Limit
        Application.targetFrameRate = fpsLimit;
    }

    public void SetFullscreen(bool value)
    {
        fullscreen = value;
        PlayerPrefs.SetInt(FULLSCREEN_KEY, value ? 1 : 0);
        ApplyDisplaySettings();
    }

    public void SetResolutionIndex(int index)
    {
        resolutionIndex = index;
        PlayerPrefs.SetInt(RESOLUTION_KEY, index);
        ApplyDisplaySettings();
    }

    public void SetVSync(bool value)
    {
        vSync = value;
        PlayerPrefs.SetInt(VSYNC_KEY, value ? 1 : 0);
        ApplyDisplaySettings();
    }

    public void SetFPSLimit(int value)
    {
        fpsLimit = value;
        PlayerPrefs.SetInt(FPSLIMIT_KEY, value);
        ApplyDisplaySettings();
    }

    public bool GetFullscreen() => fullscreen;
    public int GetResolutionIndex() => resolutionIndex;
    public bool GetVSync() => vSync;
    public int GetFPSLimit() => fpsLimit;
}