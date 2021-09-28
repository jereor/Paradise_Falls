using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerCamera : MonoBehaviour
{
    // ChangeCameraOffset toimii asettamalla x akselin arvon
    // esim. PlayerCamera.Instance.ChangeCameraOffset(0.4f, -1); jossa 0.4f on prosessin aika ja -1 offset määrä
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

    public void ChangeCameraOffset(float timer, float x)
    {
        StartCoroutine(Offset(timer, transposer.m_TrackedObjectOffset.x, x));
    }

    private IEnumerator Offset(float timer, float start, float end)
    {
        float counter = 0;
        while (counter < timer)
        {
            counter += Time.deltaTime;
            var newX = Mathf.Lerp(start, end, counter / timer);
            transposer.m_TrackedObjectOffset = new Vector3(newX, yOffset, 0);
            yield return new WaitForEndOfFrame();
        }
    }
}
