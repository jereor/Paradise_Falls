using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraTransitions : MonoBehaviour
{
    // This object is attached to the CinemachineBrain gameobject
    // Access this script from other scripts through the Instance

    // EXAMPLE: CameraTransitions.Instance.SwitchCameras(newCamera);

    public static CameraTransitions Instance { get; private set; }

    [SerializeField] private GameObject currentCamera;
    [SerializeField] private List<TransitionTrigger> triggerList;

    private void Awake()
    {
        Instance = this;
        currentCamera.SetActive(true);
    }

    // Switches current camera to the new one
    public void SwitchCameras(GameObject newCamera)
    {
        currentCamera.SetActive(false);
        newCamera.SetActive(true);
        currentCamera = newCamera;
    }

    public List<TransitionTrigger> GetTriggers()
    {
        return triggerList;
    }

    public GameObject GetCurrentCamera()
    {
        return currentCamera;
    }
}
