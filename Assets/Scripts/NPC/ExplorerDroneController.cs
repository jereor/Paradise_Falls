using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.InputSystem;

public class ExplorerDroneController : MonoBehaviour
{
    [Header("Text Boxes")]
    [SerializeField] private GameObject textBox;
    [SerializeField] private TextMeshProUGUI textDisplay;


    [Header("Things To Say")]
    [SerializeField] private string[] sentences;
    [SerializeField] private string[] sentencesWhenTalkedBefore;
    [SerializeField] private float typingSpeed;

    [Header("What NPC Unlocks For Player")]
    [SerializeField] private bool unlocksWallJump;
    [SerializeField] private bool unlocksShockWaveJumpAndDive;
    [SerializeField] private bool unlocksShockWaveAttack;
    [SerializeField] private bool unlocksShieldGrind;

    //[SerializeField] private bool npcWalkAround;
    //[SerializeField] private bool isOnWalkCoolDown;
    //[SerializeField] private float walkCoolDown;
    //[SerializeField] private float speed;
    //[SerializeField] private float walkDistance;
    //[SerializeField] private Vector2 offset;
    //[SerializeField] private Vector2 range;

    //[SerializeField] LayerMask nPCLayer;

    private GameObject player;
    private Player playerControl;
    private GameObject hud;
    //private Rigidbody2D rb;

    private Vector2 scale;
    //private Vector2 spawnPosition;

    private int index;
    //private float counter = 0;

    private bool isFacingRight;
    private bool hasBeenTalkedToBefore = false;
    private bool isInteracting = false;
    private bool isStillTalking = false;
    //private bool gizmoPositionChange = true;
    //private bool flippedRecently = false;

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
        hud = GameObject.Find("[HUD]");
        scale = new Vector2(-1, 1);
        //spawnPosition = transform.position;
        //rb = GetComponent<Rigidbody2D>();

        if (transform.localScale.x == 1)
            isFacingRight = true;
        else
            isFacingRight = false;

        UpdateTextBinding();

        // Set text at start to ""
        floatingText.text = "";

        //gizmoPositionChange = false;
    }

    //private void Update()
    //{
    //    if (!isOnWalkCoolDown && npcWalkAround)
    //    {
    //        StartCoroutine(WalkAround());
    //    }
    //    if (flippedRecently)
    //        counter += Time.deltaTime;

    //    if (counter > 5)
    //        flippedRecently = false;

    //}

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
        //isOnWalkCoolDown = true;
        //StopAllCoroutines();
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
                if(unlocksWallJump)
                {
                    playerControl.UnlockWalljump();
                }
                if(unlocksShockWaveJumpAndDive)
                {
                    playerControl.UnlockJumpAndDive();
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
                //hud.SetActive(true);
                index = 0;
                isInteracting = false;
                //isOnWalkCoolDown = false;
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
                //isOnWalkCoolDown = false;
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

    //private IEnumerator WalkAround()
    //{
    //    isOnWalkCoolDown = true;
    //    //for(int i = 0; i < walkDistance; i++)
    //        transform.position = Vector2.Lerp(Vector2.right * speed * transform.localScale.x * Time.deltaTime);

    //    yield return new WaitForSeconds(walkCoolDown);
    //    if(!ExplorerDroneWalkRange() && !flippedRecently)
    //    {
    //        Flip();
    //        flippedRecently = true;
    //        counter = 0;
    //    }
    //    isOnWalkCoolDown = false;
    //}

    //public void Flip()
    //{
    //    // Character flip
    //    isFacingRight = !isFacingRight;
    //    Vector3 localScale = transform.localScale;
    //    localScale.x *= -1f;
    //    transform.localScale = localScale;
    //}

    //private bool ExplorerDroneWalkRange()
    //{
    //    return Physics2D.OverlapBox(new Vector2(spawnPosition.x + offset.x, spawnPosition.y + offset.y), range, 0, nPCLayer);
    //}

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

    //private void OnDrawGizmosSelected()
    //{
    //    if(gizmoPositionChange)
    //        Gizmos.DrawWireCube(new Vector2(transform.position.x +  offset.x, transform.position.y + offset.y), range);
    //    else
    //        Gizmos.DrawWireCube(new Vector2(spawnPosition.x + offset.x, spawnPosition.y + offset.y), range);
    //}
}
