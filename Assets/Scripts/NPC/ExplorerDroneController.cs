using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.InputSystem;

public class ExplorerDroneController : MonoBehaviour
{
    public MapController mapController;

    [Header("Text Boxes")]
    [SerializeField] private GameObject textBox;
    [SerializeField] private TextMeshProUGUI textDisplay;


    [Header("Things To Say")]
    [SerializeField] private string[] sentences;
    [SerializeField] private string[] sentencesWhenTalkedBefore;
    [SerializeField] private float typingSpeed;

    [Header("What NPC Unlocks For Player")]
    [SerializeField] private bool unlocksWallJump;
    [SerializeField] private bool unlocksDash;
    [SerializeField] private bool unlocksGrappling;
    [SerializeField] private bool unlocksShockWaveJumpAndDive;
    [SerializeField] private bool unlocksShockWaveAttack;
    [SerializeField] private bool unlocksShieldGrind;

    private GameObject player;
    private Player playerControl;
    private GameObject hudUI;

    private Vector2 scale;

    private int index;

    private bool isFacingRight;
    private bool hasBeenTalkedToBefore = false;
    private bool isInteracting = false;
    private bool isStillTalking = false;

    [Tooltip("Child text object of this item")]
    public TMP_Text floatingText;       // This can later be changed to be static text field for all info popups in Main Canvas if need be
    [SerializeField] private bool showFloatingText = true;
    [SerializeField] private bool updateTextOnInteract = true;
    [SerializeField] private InputActionAsset inputActions;

    // These should be edited in inspector of desired item
    [Tooltip("# is placeholder char for actual interact input binding key")]
    [SerializeField] private string textToShow = "Press # to interact";     // Default string to be shown if showFloatingText is true  
    [SerializeField] private string interactedText = "Interacted";      // Default string to be replaced and shown if updateTextOnInteract is true
    private bool floatingTextShown = false;

    private void Start()
    {
        player = GameObject.Find("Player");
        playerControl = player.GetComponent<Player>();
        hudUI = GameObject.Find("[HUD]");
        scale = new Vector2(-1, 1);

        if (transform.localScale.x == 1)
            isFacingRight = true;
        else
            isFacingRight = false;

        UpdateTextBinding();

        // Set text at start to ""
        floatingText.text = "";
    }

    private void OnBecameInvisible()
    {
        enabled = false;
    }

    private void OnBecameVisible()
    {
        enabled = true;
    }

    // Updates # to correct key
    private void UpdateTextBinding()
    {
        // Get the input key for interact action
        textToShow = textToShow.Replace("#", inputActions.FindAction("Interact").controls.ToArray()[0].name);
    }

    // Makes text object visible, called from item script when player comes close
    private void ShowFloatingText()
    {
        if (showFloatingText)
        {
            // If text was changed to interactedText and HideFloatingText() was called change text back to textToShow
            if (!floatingText.text.Equals(textToShow))
            {
                floatingText.text = textToShow;     // Press F to interact
                floatingTextShown = true;
            }
        }
    }
    // Makes text object hidden, called from item script when player leaves triggerarea
    private void HideFloatingText()
    {
        if (showFloatingText)
        {
            floatingText.text = "";
            floatingTextShown = false;
        }
    }

    // Should be called when Interact functions is Invoked if you wish to show some text after interacting not required
    private void InteractedTextUpdate()
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

    // When interacted with an NPC, disable player inputs, hide floating text, hide HUD, bring the textbox to the screen.
    public void Interact()
    {
        // Deactivates the map if active during the interaction.
        if (MapController.MapOpen)
        {
            mapController.HandleMapState();
        }

        transform.localScale = new Vector2(player.transform.position.x - transform.position.x > 0 ? 1 : -1, 0.75f);
        if (player.transform.position.x - transform.position.x > 0)
            isFacingRight = true;
        else
            isFacingRight = false;

        if(!isInteracting)
        {
            HideFloatingText(); // Hide the floating interact text when talked with NPC.

            playerControl.HandleAllPlayerControlInputs(false); // Disable player inputs for the duration of interaction.
            player.GetComponent<PlayerInteractions>().AllowTextAdvance(true); // Allows player to go through the text NPC has to say.

            if (!hasBeenTalkedToBefore)
            {
                textBox.GetComponent<RectTransform>().DOAnchorPos(new Vector2(0, 200), .3f);
                StartCoroutine(Type());
            }
            else
            {
                textBox.GetComponent<RectTransform>().DOAnchorPos(new Vector2(0, 200), .3f);
                StartCoroutine(TypeWhenTalkedBefore());
            }
            isInteracting = true;

            // Disable UI
            hudUI.SetActive(false);
        }

    }

    // This function is executed when player is in interaction with NPC and player clicks the right mouse button.
    // Last click will close the textbox and allow player inputs again.
    public void AdvanceText()
    {

        if (!hasBeenTalkedToBefore)
        {
            if (index == sentences.Length - 1 && !isStillTalking) // if the NPC isn't talking anymore, enable player inputs, hide the textbox and set text advancing to false in Player Interactions script.
            {
                hasBeenTalkedToBefore = true;
                if(unlocksWallJump)
                {
                    playerControl.UnlockWalljump();
                }
                if(unlocksShockWaveJumpAndDive)
                {
                    playerControl.UnlockJumpAndDive();
                }
                if(unlocksDash)
                {
                    playerControl.UnlockDash();
                }
                if(unlocksGrappling)
                {
                    playerControl.UnlockGrappling();
                }
                if(unlocksShockWaveAttack)
                {
                    playerControl.UnlockShockwaveAttack();
                }
                if(unlocksShieldGrind)
                {
                    playerControl.UnlockShieldGrind();
                }

                playerControl.HandleAllPlayerControlInputs(true);
                textBox.GetComponent<RectTransform>().DOAnchorPos(new Vector2(0, -200), .3f);
                player.GetComponent<PlayerInteractions>().AllowTextAdvance(false);
                textDisplay.text = "";
                hudUI.SetActive(true);
                index = 0;
                isInteracting = false;
                GameObject.Find("Player").GetComponent<PlayerInteractions>().SetIsInteractingWithNPC(false);
            }
            else
            {
                NextSentence();
            }
        }
        else // NPC has been talked to before. Goes through different text.
        {
            if (index == sentencesWhenTalkedBefore.Length - 1 && !isStillTalking)
            {
                playerControl.HandleAllPlayerControlInputs(true);
                textBox.GetComponent<RectTransform>().DOAnchorPos(new Vector2(0, -200), .3f);
                player.GetComponent<PlayerInteractions>().AllowTextAdvance(false);
                textDisplay.text = "";
                hudUI.SetActive(true);
                index = 0;
                isInteracting = false;
                GameObject.Find("Player").GetComponent<PlayerInteractions>().SetIsInteractingWithNPC(false);
            }
            else
            {
                NextSentenceWhenTalkedBefore();
            }
        }
    }
    // Coroutines for text display. Gradually reveals the letters in the sentence.
    private IEnumerator Type()
    {
        isStillTalking = true;
        foreach (char letter in sentences[index].ToCharArray())
        {
            textDisplay.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        isStillTalking = false;
    }

    // Coroutine for displaying the other string array letters. Used when player has talked to the NPC before.
    private IEnumerator TypeWhenTalkedBefore()
    {
        isStillTalking = true;
        foreach (char letter in sentencesWhenTalkedBefore[index].ToCharArray())
        {
            textDisplay.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        isStillTalking = false;
    }

    // Player clicks while text is still being revealed, fast forwards to the end of the sentence.
    // Otherwise begins to gradually display the next sentence.
    private void NextSentence()
    {
        if(isStillTalking) // NPC is still talking it's current sentence. Display the whole sentence at once.
        {
            StopAllCoroutines();
            textDisplay.text = sentences[index];
            isStillTalking = false;
        }
        else if (index < sentences.Length - 1) // Normal case for the next sentence displaying.
        {
            index++;
            textDisplay.text = "";
            StartCoroutine(Type());
        }
        else // If it's the last sentence in the dialog when talking first time.
        {
            StopAllCoroutines();
            textDisplay.text = sentences[index];
            isStillTalking = false;
        }
    }

    // Same function as above, but the array is different.
    private void NextSentenceWhenTalkedBefore()
    {
        if (isStillTalking)
        {
            StopAllCoroutines();
            textDisplay.text = sentencesWhenTalkedBefore[index];
            isStillTalking = false;
        }
        else if (index < sentencesWhenTalkedBefore.Length - 1)
        {
            index++;
            textDisplay.text = "";
            StartCoroutine(TypeWhenTalkedBefore());
        }
        else
        {
            StopAllCoroutines();
            textDisplay.text = sentencesWhenTalkedBefore[index];
            isStillTalking = false;
        }
    }

    // Player is in the range of NPC.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.name.Contains("Player"))
        {
            collision.GetComponent<PlayerInteractions>().GiveGameObject(gameObject);
            collision.GetComponent<PlayerInteractions>().IsNPC(true);
            if (isFacingRight)
                floatingText.transform.localScale = new Vector2(1, 1);
            else
                floatingText.transform.localScale = scale;

            ShowFloatingText();
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.name.Contains("Player"))
        {
            if (isFacingRight)
                floatingText.transform.localScale = new Vector2(1, 1);
            else
                floatingText.transform.localScale = scale;
        }
    }

    // Player is out of range of NPC.
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.name.Contains("Player"))
        {
            collision.GetComponent<PlayerInteractions>().IsNPC(false);
            collision.GetComponent<PlayerInteractions>().GiveGameObject(null);
            HideFloatingText();
        }
    }

    public bool GetIsInteracting()
    {
        return isInteracting;
    }

    public bool GetHasBeenTalkedBefore()
    {
        return hasBeenTalkedToBefore;
    }

    public int GetWhatNPCUnlocks()
    {
        if (unlocksWallJump)
            return 1;
        if (unlocksDash)
            return 2;
        if (unlocksGrappling)
            return 3;
        if (unlocksShockWaveJumpAndDive)
            return 4;
        if (unlocksShieldGrind)
            return 5;
        if (unlocksShockWaveAttack)
            return 6;

        return 0;
    }

    public void SetHasBeenTalkedBefore(bool b)
    {
        hasBeenTalkedToBefore = b;
    }
}
