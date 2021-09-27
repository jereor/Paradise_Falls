using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraEffects : MonoBehaviour
{
    // Shake camera by calling the instance like this:
    // CameraEffects.Instance.ShakeCamera(float intensity, float time);

    // Change offset by calling the instance like this:
    // CameraEffects.Instance.ChangeOffset(float timer, float x);

    [SerializeField] public static CameraEffects Instance { get; private set; }
    private CinemachineVirtualCamera virtualCamera;
    private CinemachineFramingTransposer transposer;
    private const float YOffset = -.5f;

    private void Awake()
    {
        Instance = this;
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
        transposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
    }

    private void Start()
    {
        transposer.m_TrackedObjectOffset = new Vector3(0, YOffset, 0);
    }

    public void ShakeCamera(float intensity, float time)
    {
        StartCoroutine(SmoothShake(intensity, time));
    }

    private IEnumerator SmoothShake(float intensity, float time)
    {
        var perlin = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        perlin.m_AmplitudeGain = intensity;
        yield return new WaitForSeconds(time);
        perlin.m_AmplitudeGain = 0f;
    }

    public void ChangeOffset(float timer, float x)
    {
        StartCoroutine(Offset(timer, transposer.m_TrackedObjectOffset.x, x));
    }

    private IEnumerator Offset(float timer, float start, float stop)
    {
        float counter = 0;
        while (counter < timer)
        {
            counter += Time.deltaTime;
            var newX = Mathf.Lerp(start, stop, counter / timer);
            transposer.m_TrackedObjectOffset = new Vector3(newX, YOffset, 0);
            yield return new WaitForEndOfFrame();
        }
    }
}
