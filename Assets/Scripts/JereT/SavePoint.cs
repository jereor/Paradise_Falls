using UnityEngine;

public class SavePoint : MonoBehaviour
{
    public bool playerIsClose;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.name.Contains("Player")) {
            playerIsClose = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.name.Contains("Player"))
        {
            playerIsClose = false;
        }
    }
}
