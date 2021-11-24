using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleDroneController : MonoBehaviour
{
    public void AnimationFinished()
    {
        Destroy(gameObject);
    }
}
