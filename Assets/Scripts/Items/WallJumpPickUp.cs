
using UnityEngine;

public class WallJumpPickUp : Interactable
{
    [Header("Variables from This script")]
    [SerializeField] private bool playerIsClose;
    [SerializeField] private GameObject playerObject;


    private void Start()
    {
        UpdateTextBinding();

        HideFloatingText();

        itemEvent.AddListener(Interact);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.name.Contains("Player"))
        {
            // Gives persmission to save and gives GameObject that this script is attached so PlayerInteraction know of whos Interact() to Invoke
            collision.GetComponent<PlayerInteractions>().AllowInteract(true);
            collision.GetComponent<PlayerInteractions>().GiveGameObject(gameObject);
            playerObject = collision.gameObject;
            // Mark for this item that player is close (easier to track interactions when debugging)
            playerIsClose = true;


            ShowFloatingText();
        }
    }

    // Reverse of OnTriggerEnter2D()
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.name.Contains("Player"))
        {
            collision.GetComponent<PlayerInteractions>().AllowInteract(false);
            collision.GetComponent<PlayerInteractions>().GiveGameObject(null);
            playerObject = null;
            playerIsClose = false;

            HideFloatingText();
        }
    }

    public override void Interact()
    {
        if(playerObject != null)
        {
            // Set allowWallJump true
            playerObject.GetComponent<PlayerMovement>().AllowWallJump();
            // Destroy this item from skene
            Destroy(gameObject);
        }
    }

    // LevelLoader calls this to check where Player is saving the game to set spawnpoint
    public bool getPlayerIsClose()
    {
        return playerIsClose;
    }
}
