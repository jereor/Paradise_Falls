using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class PauseMenuController : MonoBehaviour
{
    // Info if game is paused default false
    public static bool GameIsPaused = false;

    public CanvasGroup fader, buttons, popUp;
    public GameObject pauseMenuUI;
    public MapController mapControllerScript;

    public Button continueButton;
    public Button warningNoButton;

    public GameObject settingsMenu;
    public GameObject warningPopUp;
    [SerializeField] private bool warningAnswered;

    private bool warningOpen;

    // Start is called before the first frame update
    void Start()
    {
        // Set this here so we are 100% sure we are following the right camera
        gameObject.GetComponent<Canvas>().worldCamera = Camera.main;
    }

    // Input event Pause is triggered 
    public void PauseInput(InputAction.CallbackContext context)
    {
        // started since we only want to pause when button is pressed down
        if (context.started)
        {
            // Map is currently open -> close map and pause panel comes visible
            if (MapController.MapOpen)
                mapControllerScript.GoToPauseMenu();
            // Normal pause
            else
                HandlePauseState();
        }
    }

    // In own function so MapController can call pause without context
    public void HandlePauseState()
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

        EventSystem.current.SetSelectedGameObject(null);
        
        // Time scale back to default
        Time.timeScale = 1f;
        GameIsPaused = false;
        // Activate inputs
        Player.Instance.HandleAllPlayerControlInputs(true);
    }

    // Pause and open pauseMenuUI, called from Pause input
    private void Pause()
    {
        pauseMenuUI.SetActive(true);

        continueButton.Select();

        // Freeze time
        Time.timeScale = 0f;
        GameIsPaused = true;
        // Disable inputs
        Player.Instance.HandleAllPlayerControlInputs(false);
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

        warningOpen = false;
        warningPopUp.SetActive(false);
        continueButton.Select();
    }

    //Confirms players intention and loads Main Menu Scene
    public void QuitToMenu()
    {
        if (!warningAnswered)
        {
            buttons
                .DOFade(0, .3f)
                .SetUpdate(true);

            warningOpen = true;
            warningPopUp.SetActive(true);
            warningNoButton.Select();
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

    // Checks input from controller if we have selected null aka pressed screen but not UI elements
    public void ControllerInput(InputAction.CallbackContext context)
    {
        // We have presed screen with mouse or something so we have null in selected
        if (context.performed && context.ReadValue<Vector2>().magnitude > 0 && EventSystem.current.currentSelectedGameObject == null)
        {
            if (settingsMenu.GetComponent<SettingsController>().settingsOpen) // Settings panel is open select settingsBackButton
                settingsMenu.GetComponent<SettingsController>().settingsBackButton.Select();
            else if (warningOpen) // Warning panel is open select warningNoButton
                warningNoButton.Select();
            else // PauseMenu is only one opened select continueButton
                continueButton.Select();
        }
    }
}
