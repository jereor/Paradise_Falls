using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class SkillInformationDisplayController : MonoBehaviour
{
    [Header("Image to Show")]
    public Sprite animationToDisplay;

    [Header("Information")]
    public string informationToDisplay;
    public TMP_Text informationDisplayAsset;

    [Header("Button To Use Information")]
    public string buttonToUseDisplay = "Press # to interact";     // Default string to be shown if showFloatingText is true  
    public TMP_Text buttonToUseDisplayAsset;

    [Header("Input Actions Asset and Skill Name")]
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private string[] skillName;

    private void Start()
    {
        UpdateTextBinding();
    }

    // Updates # to correct key
    private void UpdateTextBinding()
    {
        if(buttonToUseDisplay.Contains("#"))
            buttonToUseDisplay = buttonToUseDisplay.Replace("#", inputActions.FindAction(skillName[0]).controls.ToArray()[0].name);
        // Get the input key for interact action
        if(buttonToUseDisplay.Contains("¤"))
            buttonToUseDisplay = buttonToUseDisplay.Replace("¤", inputActions.FindAction(skillName[1]).controls.ToArray()[0].name);
    }

    public void UpdateInformation()
    {
        informationDisplayAsset.text = informationToDisplay;
    }

    public void UpdateButtonToUse()
    {
        buttonToUseDisplayAsset.text = buttonToUseDisplay;
    }

}
