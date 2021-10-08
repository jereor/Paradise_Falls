using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostPlant : MonoBehaviour
{
    [SerializeField] private PlayerMovement movementScript;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            ActivateBoostJump();

            //RaycastHit hit;
            //Ray ray = new Ray(collision.gameObject.transform.position, this.transform.position);

            //Vector2 plantFaceDirection;
            //if (Physics.Raycast(ray, out hit))
            //{
            //    plantFaceDirection = hit.normal;
            //    ActivateBoostJump(plantFaceDirection);
            //}
        }
    }

    // Activates boost jump from PlayerMovement-script
    private void ActivateBoostJump()
    {
        Debug.Log(transform.up);
        movementScript.BoostJump(transform.up);
    }
}
