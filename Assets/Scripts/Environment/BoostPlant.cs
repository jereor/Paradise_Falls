using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostPlant : MonoBehaviour
{
    [SerializeField] private PlayerMovement movementScript;
    [SerializeField] private float launchDistance;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var launchDirection = transform.up;
        if (collision.gameObject.CompareTag("Player"))
            movementScript.ActivateLaunch(launchDistance, launchDirection);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        var launchDirection = transform.up;
        if (collision.gameObject.CompareTag("Player"))
            movementScript.ActivateLaunch(launchDistance, launchDirection);
    }
}
