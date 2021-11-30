using UnityEngine;
using UnityEngine.SceneManagement;

public class SavePoint : Interactable
{
    [Header("Variables from This script")]
    [SerializeField] private bool playerIsClose;
    [SerializeField] private bool saveBuffer;    // Checks if player has allready saved game and wont allow save again until leaves proximity

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.name.Contains("Player")) 
        {
            // Gives persmission to save and gives GameObject that this script is attached so PlayerInteraction know of whos Interact() to Invoke
            collision.GetComponent<PlayerInteractions>().AllowInteract(true);
            collision.GetComponent<PlayerInteractions>().GiveGameObject(gameObject);
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
            playerIsClose = false;
            // Player leaves items proximity so we can allow saving again when Player comes back to this item
            saveBuffer = false;


            HideFloatingText();
        }
    }

    public override void Interact()
    {
        // Check if player has saved with this item if not allow save if yes disable saving until Player leaves proximity
        if (!saveBuffer)
        {
            Debug.Log("Saving game with interaction...");

            InteractedTextUpdate();

            // We need to find levelLoaderInstance of this scene all scenes should have personalized LevelLoaders (different map aka positions of items and enemies)
            // Main Game Scene
            if (SceneManager.GetActiveScene().name.Contains("01"))
            {
                Level01Loader.levelLoaderInstance.Save();
            }
            else if (SceneManager.GetActiveScene().name.Contains("Demo"))
            {
                //DemoLoader.levelLoaderInstance.Save();
            }
            else if (SceneManager.GetActiveScene().name.Contains("Paradise"))
                SceneLoader.Instance.Save();
            saveBuffer = true;
        }
        else
        {
            Debug.Log("Save spamming is not allowed");
        }      
    }

    // LevelLoader calls this to check where Player is saving the game to set spawnpoint
    public bool getPlayerIsClose()
    {
        return playerIsClose;
    }
}
