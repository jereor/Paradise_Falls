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


    public void Work()
    {
        switch(direction)
        {
            case leftOrRight.Left:
                transform.DORotate(new Vector3(0, 0, gameObject.transform.rotation.z - rotateAmount), duration);
                break;

            case leftOrRight.Right:
                transform.DORotate(new Vector3(0, 0, transform.rotation.z + rotateAmount), duration);
                break;
        }
    }

    public enum leftOrRight
    {
        Left,
        Right
    };

}
