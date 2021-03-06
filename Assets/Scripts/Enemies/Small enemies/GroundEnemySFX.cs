using System.Collections;
using UnityEngine;

public class GroundEnemySFX : MonoBehaviour
{
    AudioListener audioListener;
    AudioSource audioSource;
    private Health myHealthScript;
    private float distanceFromPlayer;
    private GroundEnemyAI myAIScript;


    [Header("Step sounds")]
    public AudioClip[] stepSounds;

    [Header("MeleeSwing")]
    public AudioClip meleeSwingSound;

    [Header("Taking dmg sounds")]
    public AudioClip[] takeDMGSounds;

    [Header("Destroyed sounds")]
    public AudioClip[] destroySounds;

    // Start is called before the first frame update
    void Start()
    {
        audioListener = GameObject.FindGameObjectWithTag("Player").GetComponent<AudioListener>();
        audioSource = gameObject.GetComponent<AudioSource>();
        myHealthScript = gameObject.GetComponent<Health>();
        myAIScript = gameObject.GetComponent<GroundEnemyAI>();

        distanceFromPlayer = Vector3.Distance(transform.position, audioListener.transform.position);
        if (distanceFromPlayer > audioSource.maxDistance)
        {
            audioSource.mute = true;
        }
    }

    void Update()
    {
        distanceFromPlayer = Vector3.Distance(transform.position, audioListener.transform.position);
        // Player is close enough to hear my sound :)
        if (distanceFromPlayer <= audioSource.maxDistance && audioSource.mute)
            ToggleAudioSource(true);
        // Player is too far away to hear my sound >:.(
        else if (distanceFromPlayer > audioSource.maxDistance && !audioSource.mute)
            ToggleAudioSource(false);

        if (myHealthScript.getPlaySoundHurt())
        {
            PlayTakeDMGSound();
        }
        if (myAIScript.getPlaySoundMelee())
            PlayMeleeSound();
    }

    public void ToggleAudioSource(bool isAudible)
    {
        if (!isAudible && audioSource.isPlaying)
        {
            audioSource.mute = true;
        }
        else if (isAudible && !audioSource.isPlaying)
        {
            audioSource.mute = false;
        }
    }


    public void PlayStepSound()
    {
        audioSource.PlayOneShot(stepSounds[(int)Random.Range(0, stepSounds.Length - 1)], 0.5f);
    }

    public void PlayTakeDMGSound()
    {
        audioSource.PlayOneShot(takeDMGSounds[(int)Random.Range(0, takeDMGSounds.Length - 1)]);
    }

    public void PlayMeleeSound()
    {
        audioSource.PlayOneShot(meleeSwingSound);
    }

    public void PlayDestroySound()
    {
        // First disable spriterenderer to "hide" enemy before destroying this gameobject (so we can play destroy sound)
        GetComponent<SpriteRenderer>().enabled = false;
        // Disables script so enemy will not deal dmg while sprite is invisible
        GetComponent<GroundEnemyAI>().enabled = false;
        GetComponent<Collider2D>().enabled = false;
        // Play sound
        audioSource.Stop();
        AudioClip soundToPlay = destroySounds[(int)Random.Range(0, destroySounds.Length - 1)];
        audioSource.PlayOneShot(soundToPlay);
        StartCoroutine(DestroyAfterSound(soundToPlay.length));
    }

    // Destroys object after playing sound (fences)
    private IEnumerator DestroyAfterSound(float duration)
    {
        yield return new WaitForSeconds(duration);
        Destroy(gameObject);
    }
}
