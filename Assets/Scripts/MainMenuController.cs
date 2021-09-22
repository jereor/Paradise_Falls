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

    //Starts new game file while asking to delete the old one if it exists.
    public void StartGame()
    {
        //if(nosavedata) {Start game immediately} else {WarningPopUpWindow = active}
        SceneManager.LoadScene(1);
    }

    //Continues the game from the last save.
    public void ContinueGame()
    {

    }

    //Button action for deleting all save data.
    public void WarningPopUpYes()
    {
        //Delete all save data here.
    }

    //Button action for closing the Warning popup for deleting save data.
    public void WarningPopUpNo()
    {

    }

    //Opens the credits panel
    public void OpenCredits()
    {
        buttons
            .DOFade(0, .3f)
            .SetUpdate(true);
        credits
            .DOAnchorPos(Vector2.zero, .3f)
            .SetUpdate(true);
    }

    //Closes the credits panel
    public void CloseCredits()
    {
        buttons
            .DOFade(1, .3f)
            .SetUpdate(true);
        credits
            .DOAnchorPos(new Vector2(0, -500), .3f)
            .SetUpdate(true);
    }

    //Closes the application.
    public void QuitGame()
    {
        Debug.Log("Application closed.");
        Application.Quit();
    }
}
