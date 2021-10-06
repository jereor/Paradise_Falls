using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SettingsController : MonoBehaviour
{
    public CanvasGroup buttons;
    public RectTransform _options;

    public Button settingsBackButton;
    public Button settingsButton;
    // Start is called before the first frame update
    void Start()
    {
        _options = GetComponent<RectTransform>();
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

        settingsButton.Select();
    }
}
