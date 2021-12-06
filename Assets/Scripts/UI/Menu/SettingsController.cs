using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Audio;
using TMPro;
public class SettingsController : MonoBehaviour
{
    public CanvasGroup buttons;
    public RectTransform _options;
    public bool settingsOpen;
    public Button settingsBackButton;
    public Button settingsButton;
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;

    // Sliders
    public Slider masterVolumeSlider;
    public Slider effectVolumeSlider;
    public Slider musicVolumeSlider;

    public AudioMixer mainMixer;

    Resolution[] resolutions;
    private int currentResolutionIndex;

    private bool resoLoadBuffer = true;
    private bool fullscreenLoadBuffer = true;

    // Start is called before the first frame update
    void Start()
    {
        // Gets computers usable resolutions and adds them to dropdown
        // Checks our current resolution and chooses it from dropdown
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();
        List<string> resolutionScrings = new List<string>();
        currentResolutionIndex = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            resolutionScrings.Add(option);
            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
                currentResolutionIndex = i;
        }
        resolutionDropdown.AddOptions(resolutionScrings);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();

        _options = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (mainMixer.GetFloat("MasterVolume", out float masterValue) && Mathf.Pow(10f, masterValue / 20f) != masterVolumeSlider.value)
        {
            masterVolumeSlider.value = Mathf.Pow(10f, masterValue / 20f);
            if (mainMixer.GetFloat("EffectVolume", out float effectValue))
                effectVolumeSlider.value = Mathf.Pow(10f, effectValue / 20f);
            if (mainMixer.GetFloat("MusicVolume", out float musicValue))
                musicVolumeSlider.value = Mathf.Pow(10f, musicValue / 20f);
        }
        // No need to save resolution since coded that it takes default resolution from the system
        //if (GameStatus.status != null && GameStatus.status.getLoadedData().resolutionIndex != currentResolutionIndex && resoLoadBuffer)
        //{
        //    Resolution resolution = resolutions[GameStatus.status.getLoadedData().resolutionIndex];
        //    Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        //    currentResolutionIndex = GameStatus.status.getLoadedData().resolutionIndex;
        //    resolutionDropdown.value = currentResolutionIndex;
        //    resolutionDropdown.RefreshShownValue();
        //    resoLoadBuffer = false;
        //}
        if (fullscreenToggle.gameObject.activeInHierarchy && GameStatus.status != null && GameStatus.status.getLoadedData().fullscreen != Screen.fullScreen && fullscreenLoadBuffer)
        {
            Screen.fullScreen = GameStatus.status.getLoadedData().fullscreen;
            if (fullscreenToggle.isOn != GameStatus.status.getLoadedData().fullscreen)
                fullscreenToggle.isOn = GameStatus.status.getLoadedData().fullscreen;
            fullscreenLoadBuffer = false;
        }
    }

    //Opens the settings menu as a popup window while disabling the main menu buttons.
    public void OpenSettings()
    {
        //_resSpot = PlayerPrefs.GetInt("Res");
        //_volume = PlayerPrefs.GetFloat("Vol");
        //toggle.isOn = PlayerPrefs.GetInt("FullS") == 1;
        //ChangeResText();
        //VolumeSlider(_volume);
        //AdjustSlider(_volume.ToString());

        buttons
            .DOFade(0, .3f)
            .SetUpdate(true);
        _options
            .DOAnchorPos(new Vector2(0, -50), .3f)
            .SetUpdate(true);

        settingsOpen = true;
        settingsBackButton.Select();
    }

    //Closes the settings while enabling the main menu buttons again.
    public void CloseSettings()
    {
        buttons
            .DOFade(1, .3f)
            .SetUpdate(true);
        _options
            .DOAnchorPos(new Vector2(0, -500), .3f)
            .SetUpdate(true);
        if(GameStatus.status != null)
            GameStatus.status.Save();

        settingsOpen = false;
        settingsButton.Select();
    }

    public void SubmitMasterSliderValue(float sliderValue)
    {
        mainMixer.SetFloat("MasterVolume", Mathf.Log10(sliderValue) * 20);
        if(GameStatus.status != null)
            GameStatus.status.UpdateMasterVolume(Mathf.Log10(sliderValue) * 20);
    }
    public void SubmitEffectliderValue(float sliderValue)
    {
        mainMixer.SetFloat("EffectVolume", Mathf.Log10(sliderValue) * 20);
        if (GameStatus.status != null)
            GameStatus.status.UpdateEffectVolume(Mathf.Log10(sliderValue) * 20);
    }
    public void SubmitMusicSliderValue(float sliderValue)
    {
        mainMixer.SetFloat("MusicVolume", Mathf.Log10(sliderValue) * 20);
        if (GameStatus.status != null)
            GameStatus.status.UpdateMusicVolume(Mathf.Log10(sliderValue) * 20);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        if (GameStatus.status != null)
            GameStatus.status.UpdateFullScreen(isFullscreen);
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        currentResolutionIndex = resolutionIndex;
        if (GameStatus.status != null)
            GameStatus.status.UpdateResolution(currentResolutionIndex);
    }
}
