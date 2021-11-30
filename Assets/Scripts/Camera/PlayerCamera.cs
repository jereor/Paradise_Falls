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

    [SerializeField] [Range(5, 10)] private float zoomOutLevel = 6.5f;
    [SerializeField] private float yOffsetFalling;
    private float yOffset;

    // Main camera
    private GameObject mainCam;
    private CinemachineVirtualCamera virtualCam;

    // Cinemachine Components
    private CinemachineFramingTransposer transposer;

    private void Awake()
    {
        Instance = this;
        mainCam = GameObject.Find("Main Camera");
        virtualCam = GetComponent<CinemachineVirtualCamera>();
        transposer = virtualCam.GetCinemachineComponent<CinemachineFramingTransposer>();
    }

    private void OnEnable()
    {
        virtualCam.m_Lens.OrthographicSize = zoomOutLevel;
    }

    public void ChangeCameraOffset(float timer, bool falling, float x)
    {
        // Check if the camera is currently active so we don't try to access a deactivated camera
        if (gameObject.activeInHierarchy)
            StartCoroutine(Offset(timer, transposer.m_TrackedObjectOffset.x, x, falling));
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

    public void ChangeBlendSpeed(float value)
    {
        if (gameObject.activeInHierarchy)
        {
            var brain = mainCam.GetComponent<CinemachineBrain>();
            brain.m_DefaultBlend.m_Time = value;
        }
    }

    // Smooth camera follow: Activated when climbing to dampen camera follow speed
    public void SmoothFollow(float smoothTime)
    {
        // Check if the camera is currently active so we don't try to access a deactivated camera
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(Smooth(smoothTime)); // Set smoothing for the required time
        }
    }

    private IEnumerator Smooth(float smoothTime)
    {
        transposer.m_UnlimitedSoftZone = true;
        yield return new WaitForSeconds(smoothTime);
        transposer.m_UnlimitedSoftZone = false;
    }

    public void CameraShake(float intensity, float time)
    {
        if (gameObject.activeInHierarchy)
            StartCoroutine(Shake(intensity, time));
    }

    private IEnumerator Shake(float intensity, float time)
    {
        var perlin = virtualCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        perlin.m_AmplitudeGain = intensity;
        yield return new WaitForSeconds(time);
        perlin.m_AmplitudeGain = 0f;
    }
}
