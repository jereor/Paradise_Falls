using UnityEngine;

public class ShieldGrindEndPointTrigger : MonoBehaviour
{
    public bool horizontalTrigger;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // IF player enters this trigger check if he is grinding to stop grinding
        if (collision.gameObject.CompareTag("Player") && collision.TryGetComponent(out ShieldGrind script))
        {
            if (script.getGrinding())
            { 
                script.PlayerLeavePipe(horizontalTrigger);
            }
        }
    }
}
