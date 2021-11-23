using System.Collections;
using UnityEngine;

// This script only plays sounds
public class PlayerSFX : MonoBehaviour
{
    // References to components that are needed
    [Header("References")]
    public AudioSource playerAudioSource;
    public ShockwaveTool shScript;
    public Shield shieldScript;

    public Coroutine blockCoroutine;

    // Audio clips
    [Header("Footsteps")]
    public AudioClip[] playerSteps;

    [Header("Landing")]
    public AudioClip land;
    public AudioClip groundPound;

    [Header("Jump")]
    public AudioClip doubleJump;

    [Header("Dash")]
    public AudioClip dash;

    [Header("Throwing")]
    public AudioClip throwWeapon;
    public AudioClip throwHitEnemy;
    public AudioClip throwHitWeakpoint;
    public AudioClip throwHitEnvironment;

    [Header("Melee")]
    public AudioClip meleeSwing;
    public AudioClip meleeHit;

    [Header("Shield")]
    public AudioClip blockActivation;
    public AudioClip block;
    public AudioClip parry;
    public AudioClip blockDamaged;

    private void Update()
    {
        // Sound effects that dont have own animation or are part of bigger animation
        // Double jump
        if (shScript.getPlaySoundJump())
            playerAudioSource.PlayOneShot(doubleJump);
        // Dash
        if (shScript.getPlaySoundDash())
            playerAudioSource.PlayOneShot(dash);
        // Block activation
        if (shieldScript.getPlaySoundBlockActivate())
        {
            playerAudioSource.PlayOneShot(blockActivation);
            if (blockCoroutine == null)
                blockCoroutine = StartCoroutine(PlayeClipDelayed(block, blockActivation.length));
        }
        // Parry
        if (shieldScript.getPlaySoundParry())
        {
            // Disables loop and stops block sound playing
            playerAudioSource.loop = false;   
            playerAudioSource.Stop();
            playerAudioSource.clip = null;
            if (blockCoroutine != null)
            {
                StopCoroutine(blockCoroutine);
                blockCoroutine = null;
            }
            playerAudioSource.PlayOneShot(parry);
        }

    }

    private IEnumerator PlayeClipDelayed(AudioClip clip, float delay)
    {
        yield return new WaitForSeconds(delay);
        playerAudioSource.loop = true;
        playerAudioSource.clip = clip;
        playerAudioSource.Play();

        blockCoroutine = null;
    }

    public void PlayRandomPlayerStepSound()
    {
        int random = Random.Range(0, 1);
        playerAudioSource.PlayOneShot(playerSteps[random]);
    }

    public void PlayPlayerLandingSound()
    {
        if (shScript.getPlaySoundDive())
            playerAudioSource.PlayOneShot(groundPound);
        else
            playerAudioSource.PlayOneShot(land);
    }

    public void PlayPlayerThrowSound()
    {
        playerAudioSource.PlayOneShot(throwWeapon);
    }

    public void PlayPlayerHitSound()
    {
        playerAudioSource.PlayOneShot(meleeHit);
    }

    public void PlayPlayerSwingSound()
    {
        playerAudioSource.PlayOneShot(meleeSwing);
    }
}
