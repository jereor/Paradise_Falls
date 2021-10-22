using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DrawBridgeController : MonoBehaviour
{
    [SerializeField] private float moveTime = 3f;
    [Header("Direction the drawbridge is opening")]
    [SerializeField] private leftOrRight direction = new leftOrRight();

    [Header("Amount of rotation")]
    [SerializeField] private float rotateAmount;

    private bool isDrawBridgeLowered = false;
    private bool moving = false; // Is the object already moving?
    private Quaternion startRotation; // Quaternion for returning back to starting rotation.

    void Start()
    {
        startRotation = transform.rotation;
    }

    // Functionality for drawbridge rising and lowering.
    public void Work()
    {
        if(!isDrawBridgeLowered)
        {
            StartCoroutine(DrawBridgeMoving()); // Drawbridge is moving.
            switch (direction)
            {
                case leftOrRight.Left:
                    transform.DORotate(new Vector3(0, 0, startRotation.z - rotateAmount), moveTime); // Rotate the GameObject.
                    break;

                case leftOrRight.Right:
                    transform.DORotate(new Vector3(0, 0, startRotation.z + rotateAmount), moveTime);
                    break;
            }
        }
        else if(isDrawBridgeLowered)
        {
            StartCoroutine(DrawBridgeMoving());
            switch (direction)
            {
                case leftOrRight.Left:
                    transform.DORotate(new Vector3(0, 0, startRotation.z), moveTime);
                    break;

                case leftOrRight.Right:
                    transform.DORotate(new Vector3(0, 0, startRotation.z), moveTime);
                    break;
            }
        }

    }

    public enum leftOrRight
    {
        Left,
        Right
    };

    private IEnumerator DrawBridgeMoving()
    {
        moving = true;
        yield return new WaitForSeconds(moveTime);
        isDrawBridgeLowered = !isDrawBridgeLowered;
        moving = false;
    }

    public bool getMoving()
    {
        return moving;
    }
}
