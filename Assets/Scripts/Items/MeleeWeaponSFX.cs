using UnityEngine;

public class MeleeWeaponSFX : MonoBehaviour
{
    public AudioSource meleeWeaponAudioSource;

    [Header("Impacts")]
    public AudioClip throwHitEnemy;
    public AudioClip throwHitWeakpoint;
    public AudioClip throwHitEnvironment;

    public void PlayHitEnemySound()
    {
        meleeWeaponAudioSource.PlayOneShot(throwHitEnemy);
    }

    public void PlayWPHitEnemySound()
    {
        meleeWeaponAudioSource.PlayOneShot(throwHitWeakpoint);
    }

    public void PlayHitEnvironmentSound()
    {
        meleeWeaponAudioSource.PlayOneShot(throwHitEnvironment);
    }
}
