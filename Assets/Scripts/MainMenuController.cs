using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class MainMenuController : MonoBehaviour
{
    public RectTransform credits;
    public CanvasGroup fader, buttons;
    public GameObject settingsMenu;
    // Start is called before the first frame update
    void Start()
    {
        gameObject.GetComponent<Canvas>().worldCamera = Camera.main;
        //fader.alpha = 1;
        //fader.DOFade(0, 1).SetUpdate(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }

    public void ContinueGame()
    {

    }


    public void OpenCredits()
    {
        buttons
            .DOFade(0, .3f)
            .SetUpdate(true);
        credits
            .DOAnchorPos(Vector2.zero, .3f)
            .SetUpdate(true);
    }
    public void CloseCredits()
    {
        buttons
            .DOFade(1, .3f)
            .SetUpdate(true);
        credits
            .DOAnchorPos(new Vector2(0, -500), .3f)
            .SetUpdate(true);
    }

    public void QuitGame()
    {
        Debug.Log("Application closed.");
        Application.Quit();
    }
}
