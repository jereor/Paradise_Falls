using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class CreditsPanelController : MonoBehaviour
{
    public GameObject thanksPanel;
    public GameObject creditsPanel;

    public Button thanksContinueButton;
    public Button creditsContinueButton;

    private int buttonPresses = 0;
    public void RollCredits()
    {
        Player.Instance.HandleAllPlayerControlInputs(false);
        creditsPanel.SetActive(true);
        StartCoroutine(ShowButton(3f, thanksContinueButton));
    }

    private IEnumerator ShowButton(float duration, Button button)
    {
        yield return new WaitForSeconds(duration);
        button.gameObject.SetActive(true);
        button.Select();
    }

    public void ContinueButton()
    {
            thanksPanel.SetActive(false);
            creditsPanel.SetActive(true);          

            EventSystem.current.SetSelectedGameObject(null);
            thanksContinueButton.gameObject.SetActive(false);
            StartCoroutine(ShowButton(3f, creditsContinueButton));
    }

    public void ToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
