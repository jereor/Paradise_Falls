using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ExplorerDroneController : Interactable
{
    [SerializeField] private RectTransform textBox;

    private GameObject player;
    private Player playerControl;
    private GameObject hud;
    [SerializeField] private List<GameObject> textToSay;
    [SerializeField] private GameObject textObject;
    [SerializeField] private int textCounter = 0;

    private void Awake()
    {
        textObject = gameObject.transform.GetChild(1).GetChild(0).gameObject;
        player = GameObject.Find("Player");
        playerControl = player.GetComponent<Player>();
        hud = GameObject.Find("[HUD]");

        for(int i = 0; i < textObject.transform.childCount; i++)
        {
            textToSay.Add(textObject.transform.GetChild(i).gameObject);
        }
            
    }

    // Player is in the range of lever
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.name.Contains("Player"))
        {
            collision.GetComponent<PlayerInteractions>().AllowInteract(true);
            collision.GetComponent<PlayerInteractions>().GiveGameObject(gameObject);
            ShowFloatingText();
        }
    }
    //private void OnTriggerStay2D(Collider2D collision)
    //{
    //    if (collision.name.Contains("Player"))
    //    {
    //        collision.GetComponent<PlayerInteractions>().AllowInteract(true);
    //        collision.GetComponent<PlayerInteractions>().GiveGameObject(gameObject);
    //        ShowFloatingText();
    //    }

    //}

    // Player is out of range of lever.
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.name.Contains("Player"))
        {
            collision.GetComponent<PlayerInteractions>().AllowInteract(false);
            collision.GetComponent<PlayerInteractions>().GiveGameObject(null);
            HideFloatingText();
        }
    }

    //public override void ShowFloatingText()
    //{
    //        base.ShowFloatingText();


    //}

    // Basic interaction function for levers. Action happens in the object lever is pointing at.
    public override void Interact()
    {
        textBox.DOAnchorPos(new Vector2(0, -300), .3f);
        HideFloatingText();
        // Disable player inputs
        playerControl.HandleAllPlayerControlInputs(false);
        player.GetComponent<PlayerInteractions>().AllowTextAdvance(true);
        Debug.Log(textToSay.Count);
        textToSay[textCounter].SetActive(true);
        textCounter++;

        // Disable UI
        //hud.SetActive(false);
        // Enable created canvas

        // Only specific keys are usable
    }

    public void AdvanceText()
    {
        if(textCounter == textToSay.Count)
        {
            textToSay[textCounter - 1].SetActive(false);
            playerControl.HandleAllPlayerControlInputs(true);
            textBox.DOAnchorPos(new Vector2(0, -900), .3f);
            //hud.SetActive(true);
            textCounter = 0;
        }
        else
        {
            textToSay[textCounter].SetActive(true);
            textToSay[textCounter - 1].SetActive(false);
            textCounter++;
        }

    }
}
