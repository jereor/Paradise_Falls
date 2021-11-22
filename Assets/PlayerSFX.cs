using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSFX : MonoBehaviour
{
    // References to components that are needed
    [Header("References")]
    public AudioSource playerAudioSource;
    public ShockwaveTool shScript;

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

    private void Update()
    {
        // Sound effects that dont have own animation or are part of bigger animation
        if (shScript.getPlaySoundJump())
            playerAudioSource.PlayOneShot(doubleJump);
        if (shScript.getPlaySoundDash())
            playerAudioSource.PlayOneShot(dash);

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
