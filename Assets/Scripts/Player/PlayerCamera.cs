using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerCamera : MonoBehaviour
{
    // ChangeCameraOffset toimii asettamalla x akselin arvon
    // esim. PlayerCamera.Instance.ChangeCameraOffset(0.4f, true, -1); jossa 0.4f on prosessin aika, true on falling ja -1 offset määrä
    public static PlayerCamera Instance { get; private set; }

    [SerializeField] private CinemachineVirtualCamera cmCamera;
    [SerializeField] private float yOffset;

    // Cinemachine Components
    private CinemachineFramingTransposer transposer;

    private void Awake()
    {
        Instance = this;
        transposer = cmCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
    }

    public void ChangeCameraOffset(float timer, bool falling, float x)
    {
        if (falling)
            StartCoroutine(Offset(timer, transposer.m_TrackedObjectOffset.x, x, falling));
        else
            StartCoroutine(Offset(timer, transposer.m_TrackedObjectOffset.x, x, falling));
    }

    private IEnumerator Offset(float timer, float start, float end, bool falling)
    {
        // yOffset is still work in progress

        if (falling)
            yOffset = -2; // Lower the camera a bit

        float counter = 0;
        while (counter < timer)
        {
            counter += Time.deltaTime;
            var newX = Mathf.Lerp(start, end, counter / timer);
            var newY = Mathf.Lerp(0, yOffset, counter / timer);
            transposer.m_TrackedObjectOffset = new Vector3(newX, newY, 0);
            yield return new WaitForEndOfFrame();
        }
        yOffset = 0; // Reset yOffset
    }
}
