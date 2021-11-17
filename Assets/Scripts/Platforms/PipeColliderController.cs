using UnityEngine;

public class PipeColliderController : MonoBehaviour
{
    public Collider2D pipeCollider;
    public Collider2D groundCollider;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player") && ( pipeCollider.enabled || groundCollider.enabled ))
        {
            pipeCollider.enabled = false;
            groundCollider.enabled = false;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && (!pipeCollider.enabled || !groundCollider.enabled))
        {
            pipeCollider.enabled = true;
            groundCollider.enabled = true;
        }
    }
}
