using UnityEngine;
using UnityEngine.UI;

public class ShieldPickUp : Interactable
{
    [Header("Variables from This script")]
    [SerializeField] private bool playerIsClose;
    [SerializeField] private GameObject playerObject;
    [SerializeField] private InventoryPanelController inventoryPanelController;
    [SerializeField] private Button shieldSkillButton;

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
        if (playerObject != null)
        {
            // Unlock Shield for the Player
            playerObject.GetComponent<Player>().UnlockShield();
            // Destroy this item from scene
            Destroy(gameObject);
            // Open inventory and show the shield
            inventoryPanelController.CallOpenInventory();
            shieldSkillButton.onClick.Invoke();
        }
    }

    // LevelLoader calls this to check where Player is saving the game to set spawnpoint
    public bool getPlayerIsClose()
    {
        return playerIsClose;
    }
}
