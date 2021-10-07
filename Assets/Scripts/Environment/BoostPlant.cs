using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostPlant : MonoBehaviour
{
    [SerializeField] private PlayerMovement movementScript;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            ActivateBoostJump();
    }

    // Activates boost jump from PlayerMovement-script
    private void ActivateBoostJump()
    {
        // To-do: Jump direction based on plant rotation
        var direction = Vector2.up;
        movementScript.BoostJump(direction);
    }
}
