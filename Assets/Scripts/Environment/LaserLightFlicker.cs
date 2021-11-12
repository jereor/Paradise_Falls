using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

[RequireComponent(typeof(Light2D))]
public class LaserLightFlicker : MonoBehaviour
{
    [Header("Flicker Parameters")]
    [SerializeField] [Range(.01f, .1f)] private float flickerTime; // .01f recommended
    [SerializeField] private float minIntensity; // 0.5 recommended
    [SerializeField] private float maxIntensity; // 0.8 recommended

    private Light2D thisLight;
    private float intensity;
    private float flickerTimer;

    private void Start()
    {
        thisLight = GetComponent<Light2D>();
        intensity = thisLight.intensity;
    }

    private void Update()
    {
        if (!thisLight.enabled) return;

        if (flickerTimer >= flickerTime)
            StartCoroutine(Flicker());
        flickerTimer += Time.deltaTime;
    }

    private IEnumerator Flicker()
    {
        // Increase light intensity periodically to simulate laser light flicker
        thisLight.intensity = Random.Range(minIntensity, maxIntensity);
        yield return new WaitForEndOfFrame();
        flickerTimer = 0;
    }
}
