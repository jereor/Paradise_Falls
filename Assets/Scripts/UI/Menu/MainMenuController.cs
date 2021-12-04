using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.EventSystems;

public class MainMenuController : MonoBehaviour
{
    public RectTransform credits;
    public CanvasGroup fader, buttons, popUp;
    public Button newGameButton;
    public Button continueButton;
    public Button creditsButton;
    public Button creditsBackButton;
    public Button warningNoButton;

    public GameObject settingsMenu;
    
    public GameObject warningPopUp;

    public GameObject flyingTitleEnemyOne;
    public GameObject flyingTitleEnemyTwo;
    public GameObject flyingTitleEnemyThree;
    public GameObject flyingTitleEnemyFour;
    private GameObject flyingTitleEnemyOneInstance;
    private GameObject flyingTitleEnemyTwoInstance;
    private GameObject flyingTitleEnemyThreeInstance;
    private GameObject flyingTitleEnemyFourInstance;

    private float counter = 0;

    private bool firstCanBeInstantiated = false;
    private bool secondCanBeInstantiated = false;

    private bool creditsOpen; // If these are true and ControllerInput is called with current selected EventSystem == null select creditsBackButton
    private bool warningOpen;


    private void Awake()
    {
        //// If game finds save file show continue button
        //if (GameStatus.status.CheckData())
        //{
        //    continueButton.gameObject.SetActive(true);
        //    continueButton.Select();
        //    GameStatus.status.Load();
        //    settingsMenu.GetComponent<SettingsController>().mainMixer.SetFloat("MasterVolume", GameStatus.status.getLoadedData().masterVolume);
        //    settingsMenu.GetComponent<SettingsController>().mainMixer.SetFloat("EffectVolume", GameStatus.status.getLoadedData().effectsVolume);
        //    settingsMenu.GetComponent<SettingsController>().mainMixer.SetFloat("MusicVolume", GameStatus.status.getLoadedData().musicVolume);
        //}
        //else
        //{
        //    continueButton.gameObject.SetActive(false);
        //    newGameButton.Select();
        //}
    }

    void Start()
    {
        gameObject.GetComponent<Canvas>().worldCamera = Camera.main;

        // If game finds save file show continue button
        if (GameStatus.status.CheckData())
        {
            continueButton.gameObject.SetActive(true);
            continueButton.Select();
            GameStatus.status.Load();
            settingsMenu.GetComponent<SettingsController>().mainMixer.SetFloat("MasterVolume", GameStatus.status.getLoadedData().masterVolume);
            settingsMenu.GetComponent<SettingsController>().mainMixer.SetFloat("EffectVolume", GameStatus.status.getLoadedData().effectsVolume);
            settingsMenu.GetComponent<SettingsController>().mainMixer.SetFloat("MusicVolume", GameStatus.status.getLoadedData().musicVolume);
        }
        else
        {
            continueButton.gameObject.SetActive(false);
            newGameButton.Select();
        }
        //fader.alpha = 1;
        //fader.DOFade(0, 1).SetUpdate(true);
        StartCoroutine(InstantiateFirstSetFlyingDrones());
        StartCoroutine(InstantiateSecondSetFlyingDrones());
    }

    // Update is called once per frame
    void Update()
    {
        counter += Time.deltaTime;
        if(counter > 30)
        {
            StartCoroutine(InstantiateFirstSetFlyingDrones());
            StartCoroutine(InstantiateSecondSetFlyingDrones());
            counter = 0;
        }

        if (firstCanBeInstantiated)
        {
            flyingTitleEnemyOneInstance = Instantiate(flyingTitleEnemyOne);
            flyingTitleEnemyTwoInstance = Instantiate(flyingTitleEnemyTwo);
            flyingTitleEnemyOneInstance.transform.SetParent(gameObject.transform.GetChild(0));
            flyingTitleEnemyTwoInstance.transform.SetParent(gameObject.transform.GetChild(0));
            firstCanBeInstantiated = false;
        }
        if (secondCanBeInstantiated)
        {
            flyingTitleEnemyThreeInstance = Instantiate(flyingTitleEnemyThree);
            flyingTitleEnemyFourInstance = Instantiate(flyingTitleEnemyFour);
            flyingTitleEnemyThreeInstance.transform.SetParent(gameObject.transform.GetChild(0));
            flyingTitleEnemyFourInstance.transform.SetParent(gameObject.transform.GetChild(0));
            secondCanBeInstantiated = false;
        }
    }

    private IEnumerator InstantiateFirstSetFlyingDrones()
    {
        firstCanBeInstantiated = false;
        yield return new WaitForSeconds(8);
        firstCanBeInstantiated = true;
    }

    private IEnumerator InstantiateSecondSetFlyingDrones()
    {
        secondCanBeInstantiated = false;
        yield return new WaitForSeconds(15);
        secondCanBeInstantiated = true;
    }

    //Starts new game file while asking to delete the old one if it exists.
    public void StartGame()
    {
        //if(nosavedata) {Start game} else {WarningPopUpWindow = active}
        //if(saveData.CheckData())
        if (GameStatus.status.CheckData())
        {
            buttons
                .DOFade(0, .3f)
                .SetUpdate(true);

            warningPopUp.SetActive(true);

            warningOpen = true;
            warningNoButton.Select();
        }
        else
        {
            //Aloitetaan peli alusta.
            Debug.Log("New game started.");

            // Opening Main Game Scene
            //StartCoroutine(LoadAsynchronously(1));
            // Opening demo scene
            StartCoroutine(LoadAsynchronously(SceneManager.sceneCountInBuildSettings - 1));
        }
        
    }

    //Continues the game from the last save.
    public void ContinueGame()
    {
        //saveData.LoadPlayer();
        // Load completed no errors
        if (GameStatus.status.Load() == true)
        {
            // Opening Main Game Scene
            //StartCoroutine(LoadAsynchronously(1));
            // Opening demo scene
            StartCoroutine(LoadAsynchronously(SceneManager.sceneCountInBuildSettings - 1));
        }
        // Loading had errors don't open scene
        else
        {
            Debug.Log("Loading had errors");
        }
    }

    //Button action for deleting all save data.
    public void WarningPopUpYes()
    {
        //Delete all save data here.
        GameStatus.status.DeleteSave();
        Debug.Log("Save data deleted through main menu.");
        popUp
            .DOFade(0, .2f)
            .SetUpdate(true);

        warningPopUp.SetActive(false);
        warningOpen = false;
        StartGame();
    }

    //Button action for closing the Warning popup for deleting save data.
    public void WarningPopUpNo()
    {
        warningPopUp.SetActive(false);
        buttons
            .DOFade(1, .3f)
            .SetUpdate(true);

        warningOpen = false;
    }

    //Opens the credits panel
    public void OpenCredits()
    {
        buttons
            .DOFade(0, .3f)
            .SetUpdate(true);
        credits
            .DOAnchorPos(new Vector2(0, -50), .3f)
            .SetUpdate(true);

        creditsOpen = true;
        creditsBackButton.Select();
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

        creditsOpen = false;
        creditsButton.Select();
    }

    //Closes the application.
    public void QuitGame()
    {
        Debug.Log("Application closed.");
        Application.Quit();
    }

    // Brackeys tutorial:  https://www.youtube.com/watch?v=YMj2qPq9CP8
    IEnumerator LoadAsynchronously(int sceneIndex)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);

        // Wait for loading scene
        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            Debug.Log("Loading: " + progress);

            yield return null;
        }
    }


    public void ControllerInput(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {      
        // We have presed screen with mouse or something so we have null in selected
        if (context.performed && context.ReadValue<Vector2>().magnitude > 0 && EventSystem.current.currentSelectedGameObject == null) 
        {
            if (creditsOpen) // Credits panel is open select creditsBackButton
                creditsBackButton.Select();
            else if (warningOpen)   // Warning panel is open select warningNoButton
                warningNoButton.Select();
            else if (settingsMenu.GetComponent<SettingsController>().settingsOpen) // Settings panel is open select settingsBackButton
                settingsMenu.GetComponent<SettingsController>().settingsBackButton.Select();
            else // MainMenu panel is only one open so select creditsButton since if ControllerInput is activated Sumbit bind it Presses the button instantly
                creditsButton.Select();
        }
    }
}
