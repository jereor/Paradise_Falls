using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DoorController : MonoBehaviour
{
    public Vector2 endPosition;
    public float moveTime;

    // Draws simple gizmos from object locations to their designated spots.
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, new Vector2(endPosition.x, endPosition.y));
    }

    // Moves the game object to the designated location.
    public void Work()
    {
        transform.DOMove(new Vector2(transform.position.x + endPosition.x, transform.position.y + endPosition.y), moveTime);
    }
}
