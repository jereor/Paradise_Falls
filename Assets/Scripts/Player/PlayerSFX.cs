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
    public ShieldGrind grindScript;

    public Coroutine blockCoroutine;
    public Coroutine grindCoroutine;

    private bool playLandingSound = false;
    private bool playGPPullSound = false;
    private bool playShieldGrindSound = false;

    // Audio clips
    [Header("Footsteps")]
    public AudioClip[] playerSteps;

    [Header("Landing")]
    public AudioClip landSoftly;
    public AudioClip landHard;
    public AudioClip groundPound;

    [Header("Jump")]
    public AudioClip doubleJump;

    [Header("Dash")]
    public AudioClip dash;

    [Header("Throwing")]
    public AudioClip throwWeapon;

    [Header("Melee")]
    public AudioClip meleeSwing;
    public AudioClip meleeHit;
    public AudioClip meleeWPHit;

    [Header("Shield")]
    public AudioClip blockActivation;
    public AudioClip block;
    public AudioClip parry;
    public AudioClip blockDamaged; // Not implemented yet

    [Header("Grappling point")]
    public AudioClip playerPulled;

    [Header("Player take damage")]
    public AudioClip[] takingDamage;

    [Header("Shield grind")]
    public AudioClip shieldGrind;
    [SerializeField] private float defaultPitch = 1f;
    [SerializeField] private float maxPitch = 2f;

    private void Update()
    {
        // Sound effects that dont have own animation or are part of bigger event

        // Double jump
        if (shScript.getPlaySoundJump())
            playerAudioSource.PlayOneShot(doubleJump);
        // Dash
        if (shScript.getPlaySoundDash())
        {
            playerAudioSource.PlayOneShot(dash);
        }
        // Block activation
        if (shieldScript.getPlaySoundBlockActivate())
        {
            playerAudioSource.PlayOneShot(blockActivation);
            if (blockCoroutine == null)
                blockCoroutine = StartCoroutine(PlayClipDelayed(block, blockActivation.length/2, true));
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
        // Landing sound
        if (!PlayerMovement.Instance.IsGrounded() && !playLandingSound && !PlayerMovement.Instance.getClimbing())
            playLandingSound = true;
        else if (PlayerMovement.Instance.getClimbing() && playLandingSound)
            playLandingSound = false;
        else if (PlayerMovement.Instance.IsGrounded() && playLandingSound)
            PlayPlayerLandingSound();
        // Grappling pull
        if (PlayerCombat.Instance.getIsPlayerBeingPulled() && !playGPPullSound) 
        {
            playerAudioSource.loop = true;
            playerAudioSource.clip = playerPulled;
            playerAudioSource.Play();
            playGPPullSound = true;
        }
        else if(!PlayerCombat.Instance.getIsPlayerBeingPulled() && playGPPullSound)
        {
            playerAudioSource.loop = false;
            playerAudioSource.Stop();
            playerAudioSource.clip = null;
            playGPPullSound = false;
        }
        // Melee
        if (PlayerCombat.Instance.getPlaySoundHit())
            playerAudioSource.PlayOneShot(meleeHit);
        if (PlayerCombat.Instance.getPlaySoundWPHit())
            playerAudioSource.PlayOneShot(meleeWPHit);
        // Shield Grind
        if (grindScript.PipeCheck() && PlayerMovement.Instance.IsGrounded() && !playShieldGrindSound)
        {
            playerAudioSource.clip = shieldGrind;
            playerAudioSource.loop = true;
            playerAudioSource.Play();
            playShieldGrindSound = true;
            grindCoroutine = StartCoroutine(LerpPitch());
        }
        else if((!grindScript.PipeCheck() || !PlayerMovement.Instance.IsGrounded()) && playShieldGrindSound)
        {
            playerAudioSource.loop = false;
            playerAudioSource.Stop();
            playerAudioSource.clip = null;
            playShieldGrindSound = false;
            StopCoroutine(grindCoroutine);
            grindCoroutine = null;
            playerAudioSource.pitch = 1f;
        }
        // 
        //if (playShieldGrindSound)
        //{
        //    playerAudioSource.pitch = Mathf.Lerp(2f, playerAudioSource.pitch, grindScript.getSpeed() / grindScript.getMaxSpeed() * Time.deltaTime);
        //}

    }

    private IEnumerator LerpPitch()
    {
        float start = playerAudioSource.pitch;
        while (playerAudioSource.pitch < maxPitch)
        {
            if (shScript.getPlaySoundDash())
            {
                playerAudioSource.pitch = 2f;
            }
            else
                playerAudioSource.pitch = Mathf.Lerp(start, maxPitch, grindScript.getSpeed() / grindScript.getMaxSpeed());
            yield return null;
        }

        yield break;
    }

    // Plays given AudioClip with delay and looped if we desire
    private IEnumerator PlayClipDelayed(AudioClip clip, float delay, bool loop)
    {
        yield return new WaitForSeconds(delay);
        playerAudioSource.loop = loop;
        playerAudioSource.clip = clip;
        playerAudioSource.Play();

        blockCoroutine = null;
    }

    public void PlayRandomPlayerStepSound()
    {
        int random = Random.Range(0, playerSteps.Length - 1);
        playerAudioSource.PlayOneShot(playerSteps[random]);
    }

    public void PlayRandomPlayerDamagepSound()
    {
        int random = Random.Range(0, takingDamage.Length - 1);
        playerAudioSource.PlayOneShot(takingDamage[random]);
    }

    public void PlayPlayerBlockSound()
    {
        playerAudioSource.PlayOneShot(blockDamaged);
    } 

    public void PlayPlayerLandingSound()
    {
        if (shScript.getPlaySoundDive())
            playerAudioSource.PlayOneShot(groundPound);
        else if (Player.Instance.GetWillLand())
            playerAudioSource.PlayOneShot(landHard);
        else if (playLandingSound)
            playerAudioSource.PlayOneShot(landSoftly);

        playLandingSound = false;
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
