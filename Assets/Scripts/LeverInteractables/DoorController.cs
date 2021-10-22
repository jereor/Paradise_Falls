using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DoorController : MonoBehaviour
{
    private Vector2 startPosition;
    public Vector2 endPosition;
    public float moveTime;
    private bool moving = false;
    private bool isDoorOpen = false;

    void Start()
    {
        startPosition = transform.position;
    }

    // Draws simple gizmos from object locations to their designated spots.
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, new Vector2(endPosition.x, endPosition.y));
    }

    // Moves the game object to the designated location.
    public void Work()
    {
        if(!isDoorOpen)
        {
            StartCoroutine(DoorMoving());
            transform.DOMove(new Vector2(transform.position.x + endPosition.x, transform.position.y + endPosition.y), moveTime);
        }
        else if(isDoorOpen)
        {
            StartCoroutine(DoorMoving());
            transform.DOMove(new Vector2(startPosition.x, startPosition.y), moveTime);
        }
    }
    private IEnumerator DoorMoving()
    {
        moving = true;
        yield return new WaitForSeconds(moveTime);
        isDoorOpen = !isDoorOpen;
        moving = false;
    }

    public bool getMoving()
    {
        return moving;
    }
}
