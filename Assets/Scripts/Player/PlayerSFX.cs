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
    public PlayerHealth playerHealthScript;

    public Rigidbody2D myRB;

    public Coroutine blockCoroutine;
    public Coroutine grindCoroutine;
    public Coroutine fadeCoroutine;

    [Header("Scales for sounds")]
    public float stepVolScale = 1f;
    public float softLandingVolScale = 1f;
    public float hardLandingVolScale = 1f;
    public float meleeSwingVolScale = 1f;
    public float meleeHitVolScale = 1f;
    public float throwVolScale = 1f;
    public float jumpVolScale = 1f;
    public float doubleJumpVolScale = 1f;
    public float dashVolScale = 1f;
    public float shieldGrindVolScale = 1f;
    public float blockHoldVolScale = 1f;
    public float wallSlideVolScale = 1f;


    private bool playBlockSound = false;
    private bool playLandingSound = false;
    private bool playGPPullSound = false;
    private bool playShieldGrindSound = false;
    private bool playWallSlideSound = false;

    // Audio clips
    [Header("Footsteps")]
    public AudioClip[] playerSteps;

    [Header("Wallslide")]
    public AudioClip wallSlide;

    [Header("Landing")]
    public AudioClip landSoftly;
    public AudioClip landHard;
    public AudioClip groundPound;

    [Header("Jump")]
    public AudioClip jump;
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

    private void FixedUpdate()
    {
        if (PauseMenuController.GameIsPaused && !playerAudioSource.mute)
            playerAudioSource.mute = true;
        else if (!PauseMenuController.GameIsPaused && playerAudioSource.mute)
            playerAudioSource.mute = false;
        else if (playerAudioSource.mute)
            return;
        // Sound effects that dont have own animation or are part of bigger event

        // Jump from the ground
        if (PlayerMovement.Instance.getPlaySoundJump())
            playerAudioSource.PlayOneShot(jump, jumpVolScale);

        // Double jump
        if (shScript.getPlaySoundJump())
            playerAudioSource.PlayOneShot(doubleJump, doubleJumpVolScale);

        // Dash
        if (shScript.getPlaySoundDash())
        {
            playerAudioSource.PlayOneShot(dash, dashVolScale);
        }

        // Block activation
        if (shieldScript.getPlaySoundBlockActivate() && !playBlockSound)
        {
            playerAudioSource.PlayOneShot(blockActivation);
            if (blockCoroutine == null)
                blockCoroutine = StartCoroutine(PlayClipDelayed(block, blockActivation.length/2, true));
            playerAudioSource.volume = blockHoldVolScale;
            playBlockSound = true;
        }
        // Block end
        if (playBlockSound && !shieldScript.Blocking)
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
            playerAudioSource.volume = 1f;
            playBlockSound = false;
        }
        // Parry + (block end)
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
            playerAudioSource.volume = 1f;
            playBlockSound = false;
            // Plays parry sound
            playerAudioSource.PlayOneShot(parry);
        }

        // Landing sound
        if (!PlayerMovement.Instance.IsGrounded() && !playLandingSound && !PlayerMovement.Instance.getClimbing())
            playLandingSound = true;
        else if (PlayerMovement.Instance.getClimbing() && playLandingSound)
            playLandingSound = false;
        else if (PlayerMovement.Instance.IsGrounded() && playLandingSound && !PlayerMovement.Instance.getIfClimbingMovingPlatform())
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
            playerAudioSource.PlayOneShot(meleeHit, meleeHitVolScale);
        if (PlayerCombat.Instance.getPlaySoundWPHit())
            playerAudioSource.PlayOneShot(meleeWPHit, meleeHitVolScale);

        // Shield Grind start
        if (grindScript.PipeCheck() && PlayerMovement.Instance.IsGrounded() && !playShieldGrindSound)
        {
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeVolume(0f, shieldGrindVolScale, 0.1f, false));
            playerAudioSource.clip = shieldGrind;
            playerAudioSource.loop = true;
            playerAudioSource.Play();
            playShieldGrindSound = true;
            grindCoroutine = StartCoroutine(LerpGrindPitch());
        }
        // Shield Grind end
        else if((!grindScript.PipeCheck() || !PlayerMovement.Instance.IsGrounded()) && playShieldGrindSound)
        {
            playerAudioSource.loop = false;
            playerAudioSource.Stop();
            playerAudioSource.clip = null;
            playerAudioSource.volume = 1f;
            playShieldGrindSound = false;
            StopCoroutine(grindCoroutine);
            grindCoroutine = null;
            playerAudioSource.pitch = 1f;
        }

        // Taking damage
        if (playerHealthScript.getPlaySoundHurt())
            playerAudioSource.PlayOneShot(takingDamage[(int)Random.Range(0, takingDamage.Length - 1)]);
        if (playerHealthScript.getPlaySoundHurtShielded())
        {
            playerAudioSource.PlayOneShot(blockDamaged);
        }

        // Wallslide
        if (PlayerMovement.Instance.getWallSliding() && !playWallSlideSound && myRB.velocity.y < -1f)
        {
            fadeCoroutine = StartCoroutine(FadeVolume(0f, wallSlideVolScale, 0.1f, false));
            playerAudioSource.clip = wallSlide;
            playerAudioSource.loop = true;
            playerAudioSource.Play();
            playWallSlideSound = true;
        }
        else if(!PlayerMovement.Instance.getWallSliding() && playWallSlideSound && myRB.velocity.y > -1f)
        {
            playerAudioSource.loop = false;
            playerAudioSource.Stop();
            playerAudioSource.clip = null;
            playerAudioSource.volume = 1f;
            playWallSlideSound = false;
        }
    }

    private IEnumerator FadeVolume(float from, float to, float duration, bool setItToFrom)
    {
        float currentTime = 0;
        playerAudioSource.volume = from;
        float start = playerAudioSource.volume;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            playerAudioSource.volume = Mathf.Lerp(start, to, currentTime / duration);
            yield return null;
        }
        if (setItToFrom)
            playerAudioSource.volume = from;

        fadeCoroutine = null;
        yield break;
    }

    private IEnumerator LerpGrindPitch()
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
        playerAudioSource.PlayOneShot(playerSteps[random], stepVolScale);
    }

    public void PlayPlayerBlockSound()
    {
        playerAudioSource.PlayOneShot(blockDamaged);
    } 

    public void PlayPlayerLandingSound()
    {
        if (shScript.getPlaySoundDive())
            playerAudioSource.PlayOneShot(groundPound, hardLandingVolScale);
        else if (Player.Instance.GetWillLand())
            playerAudioSource.PlayOneShot(landHard, hardLandingVolScale);
        else if (playLandingSound)
            playerAudioSource.PlayOneShot(landSoftly, softLandingVolScale);

        playLandingSound = false;
    }

    public void PlayPlayerThrowSound()
    {
        playerAudioSource.PlayOneShot(throwWeapon, throwVolScale);
    }

    public void PlayPlayerSwingSound()
    {
        playerAudioSource.PlayOneShot(meleeSwing, meleeSwingVolScale);
    }
}
