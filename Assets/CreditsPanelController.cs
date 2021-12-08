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
    public GameObject backgroundImage;
    public Button thanksContinueButton;
    public Button creditsContinueButton;

    public void RollCredits()
    {
        Player.Instance.HandleAllPlayerControlInputs(false);

        PlayerCamera.Instance.CameraFadeOut(1);
        StartCoroutine(WaitForFadeOut(1));
    }

    private IEnumerator WaitForFadeOut(float duration)
    {
        yield return new WaitForSeconds(duration);

        PlayerCamera.Instance.CameraFadeIn(1, false);

        backgroundImage.SetActive(true);
        thanksPanel.SetActive(true);
        thanksContinueButton.gameObject.SetActive(false);
        StartCoroutine(ShowButton(5f, thanksContinueButton));
    }

    private IEnumerator ShowButton(float duration, Button button)
    {
        yield return new WaitForSeconds(duration);
        button.gameObject.SetActive(true);
        button.Select();
    }

    public void AdvanceThanks()
    {
        thanksPanel.SetActive(false);
        creditsPanel.SetActive(true);
        creditsContinueButton.gameObject.SetActive(false);

        EventSystem.current.SetSelectedGameObject(null);
        thanksContinueButton.gameObject.SetActive(false);
        StartCoroutine(ShowButton(5f, creditsContinueButton));
    }

    public void ToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
