using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionTrigger : MonoBehaviour
{
    [Header("State")]
    [SerializeField] private bool triggered;

    [Header("References")]
    [SerializeField] private GameObject zoneCamera; // new camera attached in this object's inspector

    // References
    private GameObject currentCamera;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If collision with player, switch camera to the new camera
        if (collision.CompareTag("Player"))
        {
            triggered = true;

            CameraTransitions ctInstance = CameraTransitions.Instance;
            currentCamera = ctInstance.GetCurrentCamera();

            // Make sure not in transition and that new camera is different than the current camera
            if (!InTransition() && currentCamera != zoneCamera)
                ctInstance.SwitchCameras(zoneCamera);
        }
    }

    private bool InTransition()
    {
        List<TransitionTrigger> triggerList = CameraTransitions.Instance.GetTriggers();
        foreach (TransitionTrigger trigger in triggerList)
        {
            if (trigger.triggered && trigger != this)
                return true;
        }
        return false; // Return false if none of the triggers are triggered
    }

    public void ClearTrigger()
    {
        triggered = false;   
    }

    public GameObject GetZoneCamera()
    {
        return zoneCamera;
    }
}
