using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerCamera : MonoBehaviour
{
    // This object is attached to the CinemachineBrain gameobject
    // Access this script from other scripts through the Instance

    // EXAMPLE: PlayerCamera.Instance.ChangeCameraOffset(0.2f, falling, 0.8f);

    public static PlayerCamera Instance { get; private set; }

    [SerializeField] private float yOffsetFalling;
    private float yOffset;

    // Main camera
    private GameObject mainCam;

    // Cinemachine Components
    private CinemachineFramingTransposer transposer;

    private void Awake()
    {
        Instance = this;
        mainCam = GameObject.Find("Main Camera");
        transposer = GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachineFramingTransposer>();
    }

    public void ChangeCameraOffset(float timer, bool falling, float x)
    {
        // Check if the camera is currently active so we don't try to access a deactivated camera
        if (gameObject.activeInHierarchy)
            StartCoroutine(Offset(timer, transposer.m_TrackedObjectOffset.x, x, falling));
    }

    public void ChangeBlendSpeed(float value)
    {
        var brain = mainCam.GetComponent<CinemachineBrain>();
        brain.m_DefaultBlend.m_Time = value;
    }

    private IEnumerator Offset(float timer, float start, float end, bool falling)
    {
        var yStart = transposer.m_TrackedObjectOffset.y; // Get camera's starting y-position
        if (falling)
            yOffset = yOffsetFalling; // Lower the camera a bit
        else
            yOffset = 0; // Reset y-offset when not falling

        float counter = 0;
        while (counter < timer)
        {
            counter += Time.deltaTime;
            var newX = Mathf.Lerp(start, end, counter / timer); // Interpolate to the desired x-offset
            var newY = Mathf.Lerp(yStart, yOffset, counter / timer); // Interpolate to the desired y-offset
            transposer.m_TrackedObjectOffset = new Vector3(newX, newY, 0); // Update offset on each tick
            yield return new WaitForEndOfFrame();
        }
    }
}
