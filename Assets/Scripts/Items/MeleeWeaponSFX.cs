using UnityEngine;

public class MeleeWeaponSFX : MonoBehaviour
{
    public AudioSource meleeWeaponAudioSource;

    [Header("Impacts")]
    public AudioClip throwHitEnemy;
    public AudioClip throwHitWeakpoint;
    public AudioClip throwHitHard;
    public AudioClip throwHitSoft;

    public void PlayHitEnemySound()
    {
        meleeWeaponAudioSource.PlayOneShot(throwHitEnemy);
    }

    public void PlayWPHitEnemySound()
    {
        meleeWeaponAudioSource.PlayOneShot(throwHitWeakpoint);
    }

    public void PlayHitEnvironmentSound(bool hitSoft)
    {
        if(hitSoft)
            meleeWeaponAudioSource.PlayOneShot(throwHitSoft);
        else
            meleeWeaponAudioSource.PlayOneShot(throwHitHard);
    }
}
