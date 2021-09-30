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

    public void SwitchCameras(GameObject newCamera)
    {
        currentCamera.SetActive(false);
        newCamera.SetActive(true);
        currentCamera = newCamera;
    }
}
