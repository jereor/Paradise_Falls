using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretSFX : MonoBehaviour
{
    AudioListener audioListener;
    AudioSource audioSource;
    private Health myHealthScript;
    private CannonEnemyAI myAIScript;
    private float distanceFromPlayer;
    public float minVolume = 0.3f;
    public float fadeDuration = 0.2f;

    private Coroutine aimCoroutine;
    private Coroutine fadeCoroutine;

    public SpriteRenderer cannon1;
    public SpriteRenderer cannon2;
    public SpriteRenderer cannonBase;

    [Header("Shooting sounds")]
    public AudioClip shootSound;

    [Header("Aiming sounds")]
    public AudioClip[] aimSounds;

    [Header("Taking dmg sounds")]
    public AudioClip[] takeDMGSounds;

    [Header("Destroyed sounds")]
    public AudioClip[] destroySounds;

    void Start()
    {
        audioListener = GameObject.FindGameObjectWithTag("Player").GetComponent<AudioListener>();
        audioSource = gameObject.GetComponent<AudioSource>();
        myHealthScript = gameObject.GetComponent<Health>();
        myAIScript = gameObject.GetComponent<CannonEnemyAI>();

        distanceFromPlayer = Vector3.Distance(transform.position, audioListener.transform.position);
        if (distanceFromPlayer > audioSource.maxDistance)
        {
            audioSource.Pause();
        }
    }

    void Update()
    {
        if (myHealthScript.getPlaySoundHurt())
        {
            PlayTakeDMGSound();
        }

        if (myAIScript.getPlaySoundShoot())
            PlayShootSound();
        if (myAIScript.getPlaySoundAim() && aimCoroutine == null)
            PlayAimSound();

        distanceFromPlayer = Vector3.Distance(transform.position, audioListener.transform.position);
        // Player is close enough to hear my sound :)
        if (distanceFromPlayer <= audioSource.maxDistance && !audioSource.isPlaying)
            ToggleAudioSource(true);
        // Player is too far away to hear my sound >:.(
        else if (distanceFromPlayer > audioSource.maxDistance && audioSource.isPlaying)
            ToggleAudioSource(false);

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

    public void PlayTakeDMGSound()
    {
        audioSource.PlayOneShot(takeDMGSounds[(int)Random.Range(0, takeDMGSounds.Length - 1)]);
    }

    public void PlayShootSound()
    {
        audioSource.PlayOneShot(shootSound);
    }

    public void PlayDestroySound()
    {
        // First disable spriterenderer to "hide" enemy before destroying this gameobject (so we can play destroy sound)
        cannon1.enabled = false;
        cannon2.enabled = false;
        cannonBase.enabled = false;
        // Disables script so enemy will not deal dmg while sprite is invisible
        GetComponent<CannonEnemyAI>().enabled = false;
        GetComponent<Collider2D>().enabled = false;
        // Play sound
        audioSource.Stop();
        AudioClip soundToPlay = destroySounds[(int)Random.Range(0, destroySounds.Length - 1)];
        audioSource.PlayOneShot(soundToPlay);
        StartCoroutine(DestroyAfterSound(soundToPlay.length));
    }

    public void PlayAimSound()
    {
        AudioClip aimClip = aimSounds[(int)Random.Range(0, aimSounds.Length - 1)];
        audioSource.PlayOneShot(aimClip);
        aimCoroutine = StartCoroutine(AimTimer(aimClip.length));
    }

    // Destroys object after playing sound (fences)
    private IEnumerator DestroyAfterSound(float duration)
    {
        yield return new WaitForSeconds(duration);
        Destroy(gameObject);
    }

    private IEnumerator AimTimer(float duration)
    {
        yield return new WaitForSeconds(duration);
        aimCoroutine = null;
    }
}
