using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DrawBridgeController : MonoBehaviour
{
    [Header("")]
    public float duration = 3f;
    [Header("Direction the drawbridge is opening")]
    public leftOrRight direction = new leftOrRight();

    [Header("Amount of rotation")]
    public float rotateAmount;

    private bool isDrawBridgeLowered = false;
    private Quaternion startRotation;

    void Start()
    {
        startRotation = transform.rotation;
    }
    public void Work()
    {
        if(!isDrawBridgeLowered)
        {
            switch (direction)
            {
                case leftOrRight.Left:
                    transform.DORotate(new Vector3(0, 0, startRotation.z - rotateAmount), duration);
                    break;

                case leftOrRight.Right:
                    transform.DORotate(new Vector3(0, 0, startRotation.z + rotateAmount), duration);
                    break;
            }
            isDrawBridgeLowered = true;
        }
        else if(isDrawBridgeLowered)
        {
            switch (direction)
            {
                case leftOrRight.Left:
                    transform.DORotate(new Vector3(0, 0, startRotation.z), duration);
                    break;

                case leftOrRight.Right:
                    transform.DORotate(new Vector3(0, 0, startRotation.z), duration);
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

}
