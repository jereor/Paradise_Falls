using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.InputSystem;

// This class is inherited by items that are interactable in game examples health pick up, savepoint, levers, doors, etc.
// Virtual functions are inherited by all items that inherit this class and can be overridden if need be
// Interact() should always be overridden to do something different than print to console
// Text -functions can be overridden, now only handles if pop up that is child of the item is showing or not
public class Interactable : MonoBehaviour
{
    [Header("Variables from Interactable script")]
    public UnityEvent itemEvent;        // Should work like Event (example: OnClick()) in inspector aka drag script select function to call runs Interact and this given function)
    [Tooltip("Child text object of this item")]
    public TMP_Text floatingText;       // This can later be changed to be static text field for all info popups in Main Canvas if need be
    [SerializeField] private bool showFloatingText = true;
    [SerializeField] private bool updateTextOnInteract = true;
    [SerializeField] private InputActionAsset inputActions;

    // These should be edited in inspector of desired item
    [Tooltip("# is placeholder char for actual interact input binding key")]
    [SerializeField] private string textToShow = "Press # to interact";
    [SerializeField] private string interactedText = "Interacted";

    private void Start()
    {
        // Get the input key for interact action
        textToShow = textToShow.Replace("#", inputActions.FindAction("Interact").controls.ToArray()[0].name);
     
        // These two sets should/can be done regardless of showFloatingText being false
        // Set text at start
        floatingText.text = textToShow;
        // Hide text as default
        floatingText.gameObject.SetActive(false);

        // If item event is not set we set new UnityEvent and AddListener to Interact.
        // Interact() is called in PlayerInteractions when pressed F as a Event with Invoke()
        if (itemEvent == null)
        {
            itemEvent = new UnityEvent();
            //itemEvent.AddListener(Interact);
        }
        itemEvent.AddListener(Interact);
    }

    // Makes text object visible, called from item script when player comes close
    public virtual void ShowFloatingText()
    {
        if (showFloatingText)
        {
            // If text was changed to interactedText and HideFloatingText() was called change text back to textToShow
            if (!floatingText.text.Equals(textToShow))
            {
                floatingText.text = textToShow;     // Press F to interact
            }
            floatingText.gameObject.SetActive(true);
        }
    }
    // Makes text object hidden, called from item script when player leaves triggerarea
    public virtual void HideFloatingText()
    {
        if (showFloatingText)
        {
            floatingText.gameObject.SetActive(false);
        }
    }

    // Should be called when Interact functions is Invoked if you wish to show some text after interacting not required
    public virtual void InteractedTextUpdate()
    {
        if (showFloatingText)
        {
            // If we want to update text or no
            if (updateTextOnInteract)
            {
                floatingText.text = interactedText;     // Interacted
            }
            // Best default case is to hide text as item can be pick up (override if need be in item script)
            else
            {
                HideFloatingText();
            }
        }
    }

    // All items will have this functions as default override this in items own script to do something. Example save or heal when picked up
    public virtual void Interact()
    {
        if (showFloatingText)
        {
            InteractedTextUpdate();
        }
        Debug.Log("Interacted with: " + gameObject.transform.name);
    }
}
