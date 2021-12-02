using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiotControlDroneSFX : MonoBehaviour
{
    AudioSource audioSource;
    private Health myHealthScript;
    private RiotControlDrone myAIScript;

    public float stepTimeCharge = 0.5f;
    private Coroutine chargeCoroutine;
    private Coroutine runCoroutine;


    [Header("MeleeSwing")]
    public AudioClip meleeSwingSound;

    [Header("Stunned")]
    public AudioClip stunnedSound;

    [Header("Step Sounds")]
    public AudioClip[] stepSounds;
    public AudioClip[] runSounds;

    [Header("Charge sound")]
    public AudioClip chargeSound;

    [Header("Tazer shoot")]
    public AudioClip tazerShootSound;

    [Header("Seed shoot")]
    public AudioClip seedShootSound;

    [Header("Taking dmg sounds")]
    public AudioClip[] takeDMGSounds;

    [Header("Destroyed sounds")]
    public AudioClip[] destroySounds;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
        myHealthScript = gameObject.GetComponent<Health>();
        myAIScript = gameObject.GetComponent<RiotControlDrone>();
    }

    void Update()
    {
        if (myHealthScript.getPlaySoundHurt())
        {
            PlayTakeDMGSound();
        }
        if (myAIScript.getPlaySoundMelee())
            PlayMeleeSound();
        if (myAIScript.getPlaySoundTazer())
            PlayTazerShootSound();
        if (myAIScript.getPlaySoundSeed())
            PlaySeedShootSound();
        if (myAIScript.getPlaySoundStunned())
            PlayStunnedSound();
        if (myAIScript.getPlaySoundCharge() && chargeCoroutine == null)
            chargeCoroutine = StartCoroutine(PlayChargeSound());
        if (myAIScript.getPlaySoundStep())
            PlayStepSound();
        if (myAIScript.getPlaySoundRun() && runCoroutine == null)
            runCoroutine = StartCoroutine(PlayRunSound());
            
    }

    public void PlayTakeDMGSound()
    {
        audioSource.PlayOneShot(takeDMGSounds[(int)Random.Range(0, takeDMGSounds.Length - 1)]);
    }

    public void PlayMeleeSound()
    {
        audioSource.PlayOneShot(meleeSwingSound);
    }

    public void PlayTazerShootSound()
    {
        audioSource.PlayOneShot(tazerShootSound);
    }

    public void PlaySeedShootSound()
    {
        audioSource.PlayOneShot(seedShootSound);
    }

    public void PlayStunnedSound()
    {
        audioSource.PlayOneShot(stunnedSound);
    }

    public void PlayStepSound()
    {
        audioSource.PlayOneShot(stepSounds[(int)Random.Range(0, stepSounds.Length - 1)]);
    }

    public IEnumerator PlayRunSound()
    {
        audioSource.PlayOneShot(runSounds[(int)Random.Range(0, runSounds.Length - 1)]);
        yield return new WaitForSeconds(myAIScript.getRunStepInterval());
        runCoroutine = null;
    }

    public IEnumerator PlayChargeSound()
    {
        audioSource.PlayOneShot(chargeSound);
        yield return new WaitForSeconds(myAIScript.getRunStepInterval());
        chargeCoroutine = null;
    }

    public void PlayDestroySound()
    {
        // First disable spriterenderer to "hide" enemy before destroying this gameobject (so we can play destroy sound)
        SpriteRenderer[] childRenderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer renderer in childRenderers)
        {
            renderer.enabled = false;
        }
        // Disables script so enemy will not deal dmg while sprite is invisible
        myAIScript.enabled = false;
        //GetComponent<Collider2D>().enabled = false;
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
