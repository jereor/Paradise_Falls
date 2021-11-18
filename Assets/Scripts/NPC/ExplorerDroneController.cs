using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class ExplorerDroneController : Interactable
{
    [Header("Text Boxes")]
    [SerializeField] private GameObject textBox;
    [SerializeField] private TextMeshProUGUI textDisplay;
    [SerializeField] private int index;
    [SerializeField] private string[] sentences;
    [SerializeField] private string[] sentencesWhenTalkedBefore;
    [SerializeField] private float typingSpeed;

    private GameObject player;
    private Player playerControl;
    private GameObject hud;
    private Vector2 scale;
    private bool isFacingRight;

    private bool hasBeenTalkedToBefore = false;
    private bool isInteracting = false;
    private bool isStillTalking = false;

    private void Awake()
    {
        player = GameObject.Find("Player");
        playerControl = player.GetComponent<Player>();
        hud = GameObject.Find("[HUD]");
        scale = new Vector2(-1, 1);
    }

    // Player is in the range of NPC.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.name.Contains("Player"))
        {
            collision.GetComponent<PlayerInteractions>().AllowInteract(true);
            collision.GetComponent<PlayerInteractions>().GiveGameObject(gameObject);
            if (isFacingRight)
                floatingText.transform.localScale = scale;
            else
                floatingText.transform.localScale = new Vector2(1, 1);
            ShowFloatingText();
        }
    }

    // Player is out of range of NPC.
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.name.Contains("Player"))
        {
            collision.GetComponent<PlayerInteractions>().AllowInteract(false);
            collision.GetComponent<PlayerInteractions>().GiveGameObject(null);
            HideFloatingText();
        }
    }

    // When interacted with an NPC, disable player inputs, hide floating text, hide HUD, bring the textbox to the screen.
    public override void Interact()
    {
        transform.localScale = new Vector2(player.transform.position.x - transform.position.x > 0 ? -1 : 1, 0.75f);
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
                Debug.Log("talk");
                textBox.GetComponent<RectTransform>().DOAnchorPos(new Vector2(0, 200), .3f);
                StartCoroutine(TypeWhenTalkedBefore());
            }
            isInteracting = true;

            // Disable UI
            //hud.SetActive(false);
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
                playerControl.UnlockWalljump();
                playerControl.HandleAllPlayerControlInputs(true);
                textBox.GetComponent<RectTransform>().DOAnchorPos(new Vector2(0, -200), .3f);
                player.GetComponent<PlayerInteractions>().AllowTextAdvance(false);
                textDisplay.text = "";
                //hud.SetActive(true);
                index = 0;
                isInteracting = false;
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
                //hud.SetActive(true);
                index = 0;
                isInteracting = false;
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
        else // If it's the last sentence in the dialog.
        {
            StopAllCoroutines();
            textDisplay.text = sentences[index];
            isStillTalking = false;
        }
    }

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
}
