using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SettingsController : MonoBehaviour
{
    public CanvasGroup buttons;
    public RectTransform _options;
    // Start is called before the first frame update
    void Start()
    {
        _options = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {

    }

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
    }
    public void CloseSettings()
    {
        buttons
            .DOFade(1, .3f)
            .SetUpdate(true);
        _options
            .DOAnchorPos(new Vector2(0, -500), .3f)
            .SetUpdate(true);
    }
}
