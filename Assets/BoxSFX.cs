using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxSFX : MonoBehaviour
{
    private AudioSource audioSource;

    public AudioClip chainCut;
    public AudioClip[] hitGround;
    public AudioClip breaking;

    private void Start()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
    }

    public void PlayChainCutSound()
    {
        audioSource.PlayOneShot(chainCut);
    }

    public void PlayHitGroundSound()
    {
        audioSource.PlayOneShot(hitGround[(int)Random.Range(0, hitGround.Length - 1)]);
    }

    public void PlayBreakingSound()
    {
        audioSource.PlayOneShot(breaking);
    }
}
