using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.InputSystem;

public class PauseMenuController : MonoBehaviour
{
    public static bool GameIsPaused;

    public CanvasGroup fader, buttons, popUp;
    public GameObject pauseMenuUI;
    public GameObject settingsMenu;
    public GameObject warningPopUp;
    [SerializeField] private bool warningAnswered;
    
    // Start is called before the first frame update
    void Start()
    {
        gameObject.GetComponent<Canvas>().worldCamera = Camera.main;
    }

    public void Pause(InputAction.CallbackContext context)
    {     
        if (GameIsPaused)
        {
            Debug.Log("Opening Pause Menu");
            Resume();
        }
        else
        {
            Debug.Log("Opening Pause Menu");
            Pause();
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);

        // Time scale back to default
        Time.timeScale = 1f;
        GameIsPaused = false;
    }

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

    //Button action for continueing game
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
            Time.timeScale = 1f;
            pauseMenuUI.SetActive(false);
            warningAnswered = false;
            SceneManager.LoadScene(0);
        }
    }
}
