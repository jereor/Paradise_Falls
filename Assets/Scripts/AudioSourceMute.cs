using System.Collections;
using UnityEngine;

// Copied from https://gamedevbeginner.com/unity-audio-optimisation-tips/ Coder John French
// Fade added
public class AudioSourceMute : MonoBehaviour
{
    AudioListener audioListener;
    AudioSource audioSource;
    private float distanceFromPlayer;
    public float minVolume = 0.3f;
    public float fadeDuration = 0.2f;
    private Coroutine fadeCoroutine = null;

    void Start()
    {
        // Finds the Audio Listener and the Audio Source on the object
        audioListener = GameObject.FindGameObjectWithTag("Player").GetComponent<AudioListener>();
        audioSource = gameObject.GetComponent<AudioSource>();
    }

    void Update()
    {
        distanceFromPlayer = Vector3.Distance(transform.position, audioListener.transform.position);

        if (distanceFromPlayer <= audioSource.maxDistance)
        {
            ToggleAudioSource(true);
        }
        else
        {
            ToggleAudioSource(false);
        }
        // If audio source is emitting lerp volume with distance
        if (audioSource.isPlaying && fadeCoroutine == null)
            audioSource.volume = Mathf.Lerp(1f, minVolume, distanceFromPlayer / audioSource.maxDistance);
    }

    // Fades volume to targetVolume if pause is true set audiosource to pause mode when fade is done
    private IEnumerator FadeSound(float duration, float targetVolume, bool pause)
    {
        float currentTime = 0;
        float start = audioSource.volume;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(start, targetVolume, currentTime / duration);
            yield return null;
        }
        if (pause)
            audioSource.Pause();

        fadeCoroutine = null;
        yield break;
    }

    void ToggleAudioSource(bool isAudible)
    {
        if (!isAudible && audioSource.isPlaying)
        {
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeSound(fadeDuration, 0f, true));
        }
        else if (isAudible && !audioSource.isPlaying)
        {
            audioSource.Play();
            audioSource.volume = 0f;
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeSound(fadeDuration, minVolume, false));
        }
    }
}
