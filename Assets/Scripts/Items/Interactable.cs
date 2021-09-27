using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class Interactable : MonoBehaviour
{
    [Header("Variables from Interactable script")]
    public UnityEvent itemEvent;
    public TMP_Text floatingText;
    [SerializeField] private string textToShow = "Press F to interact";
    [SerializeField] private string interactedText = "Interacted";

    private void Start()
    {
        // Set text at start
        floatingText.text = textToShow;
        // Hide text as default
        HideFloatingText();
        // If item event is not set we set new UnityEvent and AddListener to Interact.
        // Interact() is called in PlayerInteractions when pressed F as a Event with Invoke()
        if (itemEvent == null)
        {
            itemEvent = new UnityEvent();
        }
        
        itemEvent.AddListener(Interact);
    }

    // Makes text object visible 
    public virtual void ShowFloatingText()
    {
        // If text was changed to interactedText and HideFloatingText() was called change text back to textToShow
        if (!floatingText.text.Equals(textToShow))
        {
            floatingText.text = textToShow;     // Press F to interact
        }
        floatingText.gameObject.SetActive(true);
    }

    public virtual void HideFloatingText()
    {
        floatingText.gameObject.SetActive(false);
    }

    // Should be called when Interact functions is Invoked if you wish to show some text after interacting not required
    public virtual void InteractedTextUpdate()
    {
        floatingText.text = interactedText;     // Interacted
    }

    // All items will have this functions as default override this in items own script to do something. Example save or heal when picked up
    public virtual void Interact()
    {
        InteractedTextUpdate();
        Debug.Log("Interacted with: " + gameObject.transform.name);
    }
}
