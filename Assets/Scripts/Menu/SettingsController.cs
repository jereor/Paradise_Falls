using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Audio;
public class SettingsController : MonoBehaviour
{
    public CanvasGroup buttons;
    public RectTransform _options;
    public bool settingsOpen;
    public Button settingsBackButton;
    public Button settingsButton;

    // Sliders
    public Slider masterVolumeSlider;
    public Slider effectVolumeSlider;
    public Slider musicVolumeSlider;

    public AudioMixer mainMixer;

    // Start is called before the first frame update
    void Start()
    {
        _options = GetComponent<RectTransform>();

        // Set sliders to correct volumes
        if (mainMixer.GetFloat("MasterVolume", out float masterValue))
            masterVolumeSlider.value = masterValue;
        if (mainMixer.GetFloat("EffectVolume", out float effectValue))
            effectVolumeSlider.value = effectValue;
        if (mainMixer.GetFloat("MusicVolume", out float musicValue))
            musicVolumeSlider.value = musicValue;

    }

    // Update is called once per frame
    void Update()
    {

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
            .DOAnchorPos(Vector2.zero, .3f)
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

        settingsOpen = false;
        settingsButton.Select();
    }

    public void SubmitMasterSliderValue(Slider slider)
    {
        mainMixer.SetFloat("MasterVolume", slider.value);
        if(GameStatus.status != null)
            GameStatus.status.UpdateMasterVolume(slider.value);
    }
    public void SubmitEffectliderValue(Slider slider)
    {
        mainMixer.SetFloat("EffectVolume", slider.value);
        if (GameStatus.status != null)
            GameStatus.status.UpdateEffectVolume(slider.value);
    }
    public void SubmitMusicSliderValue(Slider slider)
    {
        mainMixer.SetFloat("MusicVolume", slider.value);
        if (GameStatus.status != null)
            GameStatus.status.UpdateMusicVolume(slider.value);
    }
}
