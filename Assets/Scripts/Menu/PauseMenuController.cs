using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.InputSystem;

public class PauseMenuController : MonoBehaviour
{
    // Info if game is paused default false
    public static bool GameIsPaused = false;

    public CanvasGroup fader, buttons, popUp;
    public GameObject pauseMenuUI;
    public GameObject settingsMenu;
    public GameObject warningPopUp;
    [SerializeField] private bool warningAnswered;
    
    // Start is called before the first frame update
    void Start()
    {
        // Set this here so we are 100% sure we are following the right camera
        gameObject.GetComponent<Canvas>().worldCamera = Camera.main;
    }

    // Input event Pause is triggered
    public void Pause(InputAction.CallbackContext context)
    {     
        if (GameIsPaused)
        {
            Debug.Log("Closing Pause Menu");
            Resume();
        }
        else
        {
            Debug.Log("Opening Pause Menu");
            Pause();
        }
    }

    // Resume and close pauseMenuUI, called from Pause input and button press event from button ResumeButton
    public void Resume()
    {
        pauseMenuUI.SetActive(false);

        // Time scale back to default
        Time.timeScale = 1f;
        GameIsPaused = false;
    }

    // Pause and open pauseMenuUI, called from Pause input
    private void Pause()
    {
        pauseMenuUI.SetActive(true);

        // Freeze time
        Time.timeScale = 0f;
        GameIsPaused = true;
    }

    //Button action for leaving to Main Menu
    public void WarningPopUpYes()
    {
        warningPopUp.SetActive(false);
        popUp
            .DOFade(0, .2f)
            .SetUpdate(true);

        warningAnswered = true;
        QuitToMenu();
    }

    //Button action for continuing game
    public void WarningPopUpNo()
    {
        warningPopUp.SetActive(false);
        buttons
            .DOFade(1, .3f)
            .SetUpdate(true);

        warningAnswered = true;
        QuitToMenu();
    }

    //Confirms players intention and loads Main Menu Scene
    public void QuitToMenu()
    {
        if (!warningAnswered)
        {
            buttons
                .DOFade(0, .3f)
                .SetUpdate(true);

            warningPopUp.SetActive(true);
        }
        else
        {
            Debug.Log("Leaving to Main Menu");

            // Set timescale back to normal before loading Main Menu timescale is global in Unity
            // Close pauseMenuUI just to be sure it will be closed when we arrive back to game
            Resume();   // If Resume() act funky do timescale default and Paused bool false here instead
            
            warningAnswered = false;
            SceneManager.LoadScene(0); // MainMenu or intro scene should be at buildindex 0
        }
    }
}
