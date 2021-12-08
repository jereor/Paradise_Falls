using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

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

    // Fade to Black volume
    private Volume fadeToBlackVolume;
    private ColorAdjustments colorAdjustments;
    private Vignette vignette;

    private void Awake()
    {
        Instance = this;
        mainCam = GameObject.Find("Main Camera");

        fadeToBlackVolume = GameObject.Find("Fade to Black Volume").GetComponent<Volume>();
        fadeToBlackVolume.profile.TryGet(out colorAdjustments);
        fadeToBlackVolume.profile.TryGet(out vignette);

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

    public void CameraFadeIn(float timer, bool giveControls)
    {
        if (Time.timeScale != 1f)
            Time.timeScale = 1f;
        StartCoroutine(FadeIn(timer, giveControls));
    }

    private IEnumerator FadeIn(float timer, bool giveControls)
    {
        // Start at max values
        colorAdjustments.postExposure.value = -10;
        vignette.intensity.value = 1;
        yield return new WaitForSeconds(.5f);

        // Fade in / Remove black screen
        float counter = 0;

        while (counter < timer)
        {
            counter += Time.deltaTime;
            var newColorAdjustmentValue = Mathf.Lerp(-10, 0, counter / timer); // Interpolate to the desired value
            var newVignetteIntensity = Mathf.Lerp(1, 0, counter / timer);
            colorAdjustments.postExposure.value = newColorAdjustmentValue;
            vignette.intensity.value = newVignetteIntensity;
            yield return new WaitForEndOfFrame();
        }
        if (giveControls)
            Player.Instance.HandleAllPlayerControlInputs(true);
    }

    public void CameraFadeOut(float timer)
    {
        StartCoroutine(FadeOut(timer));
    }

    private IEnumerator FadeOut(float timer)
    {
        float counter = 0;
        while (counter < timer)
        {
            counter += Time.deltaTime;
            var newColorAdjustmentValue = Mathf.Lerp(0, -10, counter / timer); // Interpolate to the desired value
            var newVignetteIntensity = Mathf.Lerp(0, 1, counter / timer);
            colorAdjustments.postExposure.value = newColorAdjustmentValue;
            vignette.intensity.value = newVignetteIntensity;
            yield return new WaitForEndOfFrame();
        }
    }
}
