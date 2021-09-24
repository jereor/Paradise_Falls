using UnityEngine;

public class SavePoint : Interactable
{
    [SerializeField] private bool playerIsClose;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.name.Contains("Player")) 
        {
            collision.GetComponent<PlayerInteractions>().AllowInteract(true);
            collision.GetComponent<PlayerInteractions>().GiveGameObject(gameObject);
            playerIsClose = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.name.Contains("Player"))
        {
            collision.GetComponent<PlayerInteractions>().AllowInteract(false);
            collision.GetComponent<PlayerInteractions>().GiveGameObject(null);
            playerIsClose = false;
        }
    }

    public override void Interact()
    {
        Debug.Log("Saving game with interaction...");
        Level01Loader.levelLoaderInstance.Save();    
        
        // aseta pelaajan hp
    }

    public bool getPlayerIsClose()
    {
        return playerIsClose;
    }
}
