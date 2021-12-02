using System.Collections;
using UnityEngine;

public class GroundEnemySFX : MonoBehaviour
{
    AudioSource audioSource;
    private Health myHealthScript;
    private GroundEnemyAI myAIScript;

    private Coroutine stepCoroutine;

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
        audioSource = gameObject.GetComponent<AudioSource>();
        myHealthScript = gameObject.GetComponent<Health>();
        myAIScript = gameObject.GetComponent<GroundEnemyAI>();
    }

    void Update()
    {
        if (myHealthScript.getPlaySoundHurt())
        {
            PlayTakeDMGSound();
        }
        if (myAIScript.getPlaySoundMelee())
            PlayMeleeSound();
        if (myAIScript.getPlaySoundStep() && stepCoroutine == null)
            stepCoroutine = StartCoroutine(PlayStepSound());


    }

    public IEnumerator PlayStepSound()
    {
        audioSource.PlayOneShot(stepSounds[(int)Random.Range(0, stepSounds.Length - 1)], 0.5f);
        yield return new WaitForSeconds(myAIScript.getWalkStepInterval());
        stepCoroutine = null;
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
