using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxSFX : MonoBehaviour
{
    private AudioSource audioSource;

    public AudioClip chainCut;
    public AudioClip[] hitGround;
    public AudioClip breaking;

    private float oneShotVolume = 0.4f;

    private void Start()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
    }

    public void PlayChainCutSound()
    {
        audioSource.PlayOneShot(chainCut, oneShotVolume);
    }

    public void PlayHitGroundSound()
    {
        audioSource.PlayOneShot(hitGround[(int)Random.Range(0, hitGround.Length - 1)], oneShotVolume);
    }

    public void PlayBreakingSound()
    {
        audioSource.PlayOneShot(breaking, oneShotVolume);
    }
}
