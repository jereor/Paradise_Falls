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

    private void Update()
    {
        // Sound effects that dont have own animation or are part of bigger animation
        if (shScript.getSFXJump())
            playerAudioSource.PlayOneShot(doubleJump);

    }

    public void PlayRandomPlayerStepSound()
    {
        int random = Random.Range(0, 1);
        playerAudioSource.PlayOneShot(playerSteps[random]);
    }

    public void PlayPlayerLanding()
    {
        if (shScript.getSFXDive())
            playerAudioSource.PlayOneShot(groundPound);
        else
            playerAudioSource.PlayOneShot(land);
    }
}
