using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTransitions : MonoBehaviour
{
    public static CameraTransitions Instance { get; private set; }

    [SerializeField] private GameObject currentCamera;

    private void Awake()
    {
        Instance = this;
    }

    public GameObject GetCurrentCamera()
    {
        return currentCamera;
    }

    public void SwitchCameras(GameObject newCamera)
    {
        PlayerCamera.Instance.StopAllCoroutines();
        currentCamera.SetActive(false);

        newCamera.SetActive(true);
        currentCamera = newCamera;
    }
}
