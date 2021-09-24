using UnityEngine;
using UnityEngine.SceneManagement;

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

        // Main Game Scene
        if (SceneManager.GetActiveScene().name.Contains("01"))
        {
            Level01Loader.levelLoaderInstance.Save();
        }
        if (SceneManager.GetActiveScene().name.Contains("Demo"))
        {
            DemoLoader.levelLoaderInstance.Save();
        }


    }

    public bool getPlayerIsClose()
    {
        return playerIsClose;
    }
}
