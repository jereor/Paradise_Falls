using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        transform.parent.GetComponent<TransitionTrigger>().ClearTrigger(); // Resets the parent trigger so it can trigger again later

        GameObject currentCam = CameraTransitions.Instance.GetCurrentCamera(); // Current camera that's in use
        GameObject zoneCam = transform.parent.GetComponent<TransitionTrigger>().GetZoneCamera(); // Current zone's camera

        // If current camera is different than the current zone's camera, switch it
        if (currentCam != zoneCam)
            CameraTransitions.Instance.SwitchCameras(zoneCam);
    }
}
