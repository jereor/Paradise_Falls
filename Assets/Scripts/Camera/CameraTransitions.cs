using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraTransitions : MonoBehaviour
{
    public static CameraTransitions Instance { get; private set; }

    [SerializeField] private GameObject currentCamera;

    private void Awake()
    {
        Instance = this;
        currentCamera.SetActive(true);
    }

    public GameObject GetCurrentCamera()
    {
        return currentCamera;
    }

    public CinemachineFramingTransposer GetCurrentTransposer()
    {
        return currentCamera.GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachineFramingTransposer>();
    }

    public void SwitchCameras(GameObject newCamera)
    {
        PlayerCamera.Instance.StopAllCoroutines();


        currentCamera.SetActive(false);
        newCamera.SetActive(true);
        currentCamera = newCamera;

        var newTransposer = currentCamera.GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachineFramingTransposer>();
        PlayerCamera.Instance.ChangeTransposer(newTransposer);
    }
}
