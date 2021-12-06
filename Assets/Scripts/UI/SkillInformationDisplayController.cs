using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SkillInformationDisplayController : MonoBehaviour
{
    public InventoryPanelController inventoryController;

    public string skillNameInfo;
    public TMP_Text skillNameDisplayAsset;

    [Header("Image to Show")]
    public Sprite animationToDisplay;
    public Image animationToDisplayAsset;

    [Header("Information")]
    public string informationToDisplay;
    public TMP_Text informationDisplayAsset;

    [Header("Button To Use Information")]
    public string buttonToUseDisplay = "Press # to interact";     // Default string to be shown if showFloatingText is true  
    public TMP_Text buttonToUseDisplayAsset;

    [Header("Input Actions Asset and Skill Name")]
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private string[] skillName;

    private bool isUsingController;

    private void Start()
    {
        UpdateTextBinding();
    }

    // Updates #, & and % to correct key respectively.
    private void UpdateTextBinding()
    {
        if(!isUsingController)
        {
            if (buttonToUseDisplay.Contains("#"))
                buttonToUseDisplay = buttonToUseDisplay.Replace("#", InputActionRebindingExtensions.GetBindingDisplayString(inputActions.FindAction(skillName[0]), 0));

            if (buttonToUseDisplay.Contains("&"))
                buttonToUseDisplay = buttonToUseDisplay.Replace("&", InputActionRebindingExtensions.GetBindingDisplayString(inputActions.FindAction(skillName[1]), 0));

            if (buttonToUseDisplay.Contains("%"))
                buttonToUseDisplay = buttonToUseDisplay.Replace("%", InputActionRebindingExtensions.GetBindingDisplayString(inputActions.FindAction(skillName[2]), 0));
        }
        else if(isUsingController)
        {
            if (buttonToUseDisplay.Contains("#"))
                buttonToUseDisplay = buttonToUseDisplay.Replace("#", InputActionRebindingExtensions.GetBindingDisplayString(inputActions.FindAction(skillName[0]), 1));

            if (buttonToUseDisplay.Contains("&"))
                buttonToUseDisplay = buttonToUseDisplay.Replace("&", InputActionRebindingExtensions.GetBindingDisplayString(inputActions.FindAction(skillName[1]), 1));

            if (buttonToUseDisplay.Contains("%"))
                buttonToUseDisplay = buttonToUseDisplay.Replace("%", InputActionRebindingExtensions.GetBindingDisplayString(inputActions.FindAction(skillName[2]), 1));
        }
    }

    public void UpdateInformation()
    {
        informationDisplayAsset.text = informationToDisplay;
    }

    public void UpdateButtonToUse()
    {
        buttonToUseDisplayAsset.text = buttonToUseDisplay;
    }

    public void UpdateAnimationDisplay()
    {
        animationToDisplayAsset.sprite = animationToDisplay;
    }

    public void UpdateSkillNameDisplay()
    {
        skillNameDisplayAsset.text = skillNameInfo;
    }

    public void SetIsUsingController(bool b)
    {
        isUsingController = b;
    }

    public bool GetisUsingController()
    {
        return isUsingController;
    }


}
