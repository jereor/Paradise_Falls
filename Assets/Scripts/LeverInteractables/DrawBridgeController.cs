using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DrawBridgeController : MonoBehaviour
{
    public float moveTime = 3f;
    [Header("Direction the drawbridge is opening")]
    public leftOrRight direction = new leftOrRight();

    [Header("Amount of rotation")]
    public float rotateAmount;

    private bool isDrawBridgeLowered = false;
    private bool moving = false;
    private Quaternion startRotation;

    void Start()
    {
        startRotation = transform.rotation;
    }
    public void Work()
    {
        if(!isDrawBridgeLowered && !moving)
        {
            StartCoroutine(DrawBridgeMoving());
            switch (direction)
            {
                case leftOrRight.Left:
                    transform.DORotate(new Vector3(0, 0, startRotation.z - rotateAmount), moveTime);
                    break;

                case leftOrRight.Right:
                    transform.DORotate(new Vector3(0, 0, startRotation.z + rotateAmount), moveTime);
                    break;
            }
            isDrawBridgeLowered = true;
        }
        else if(isDrawBridgeLowered && !moving)
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
            isDrawBridgeLowered = false;
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
        moving = false;
    }

    public bool getMoving()
    {
        return moving;
    }
}
