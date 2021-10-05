using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DrawBridgeController : MonoBehaviour
{
    public float duration = 3f;
    public leftOrRight direction = new leftOrRight();
    public float rotateAmount;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

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
