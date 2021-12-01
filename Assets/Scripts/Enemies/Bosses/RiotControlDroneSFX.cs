using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiotControlDroneSFX : MonoBehaviour
{
    AudioSource audioSource;
    private Health myHealthScript;
    private RiotControlDrone myAIScript;

    [Header("MeleeSwing")]
    public AudioClip meleeSwingSound;

    [Header("Stunned")]
    public AudioClip stunnedSound;

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
        if (myAIScript.getPlaySoundCharge())
            PlayeChargeSound();


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

    public void PlayeChargeSound()
    {
        audioSource.PlayOneShot(chargeSound);
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
