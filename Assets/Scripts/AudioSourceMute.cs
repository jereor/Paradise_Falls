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
    private Collider2D myCollider;

    [Header("Plays random sound from these (variation)")]
    public bool playIdleSound = true;
    public AudioClip[] clipsToChoose;

    [Header("Sound to be played if do event")]
    public AudioClip shutdownSound;
    public bool destroyAfterShutdown = true;

    void Start()
    {
        // Finds the Audio Listener and the Audio Source on the object
        audioListener = GameObject.FindGameObjectWithTag("Player").GetComponent<AudioListener>();
        audioSource = gameObject.GetComponent<AudioSource>();
        if (TryGetComponent(out Collider2D col))
            myCollider = col;
        if (clipsToChoose.Length > 0)
        {
            audioSource.clip = clipsToChoose[(int)Random.Range(0, clipsToChoose.Length - 1)];
        }
        distanceFromPlayer = Vector3.Distance(transform.position, audioListener.transform.position);
        if (distanceFromPlayer > audioSource.maxDistance)
        {
            audioSource.Pause();
            audioSource.volume = 0f;
        }
    }

    void Update()
    {
        if (playIdleSound && audioSource.loop)
        {
            distanceFromPlayer = Vector3.Distance(transform.position, audioListener.transform.position);

            // Game is paused and we are still playing!? No >:(
            if (PauseMenuController.GameIsPaused && audioSource.isPlaying)
                audioSource.Pause();
            // Game is not paused and we aren't playing!? No >:(
            else if (!PauseMenuController.GameIsPaused && distanceFromPlayer <= audioSource.maxDistance && !audioSource.isPlaying)
                audioSource.Play();
            // Player is close enough to hear my sound :)
            else if (distanceFromPlayer <= audioSource.maxDistance && !audioSource.isPlaying)
                ToggleAudioSource(true);
            // Player is too far away to hear my sound >:.(
            else if (distanceFromPlayer > audioSource.maxDistance && audioSource.isPlaying)
                ToggleAudioSource(false);

            // If audio source is emitting lerp volume with distance
            if (audioSource.isPlaying && fadeCoroutine == null)
                audioSource.volume = Mathf.Lerp(1f, minVolume, distanceFromPlayer / audioSource.maxDistance);
        }
        if (!myCollider.enabled && audioSource.loop)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }
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

    public void ToggleAudioSource(bool isAudible)
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

    public void ToggleLoop(bool b)
    {
        audioSource.loop = b;
    }

    //Toggles audio playing (conveyor belt)
    public void TogglePause()
    {
        if (audioSource.loop)
        {
            ToggleLoop(false);
            ToggleAudioSource(false);
        }
        else
        {
            ToggleLoop(true);
            ToggleAudioSource(true);
        }
    }

    public void PlayShutdownSound()
    {
        // Play sound
        audioSource.Stop();
        audioSource.PlayOneShot(shutdownSound);
        if (destroyAfterShutdown)
            StartCoroutine(DestroyAfterSound(shutdownSound.length));
    }
    // Destroys object after playing sound (fences)
    private IEnumerator DestroyAfterSound(float duration)
    {
        yield return new WaitForSeconds(duration);
        Destroy(gameObject);
    }
}
