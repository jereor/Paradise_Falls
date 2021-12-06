using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicSwap : MonoBehaviour
{
    public AudioClip newTrack;
    [Tooltip("This tracks maximum volume will be save to MusicManager when Swapping track")]
    public float maxVolume = 1f; 
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If player enters my trigger and it is playing other track that I would play
        if (collision.CompareTag("Player") && !MusicManager.instance.currentlyPlaying.Equals(newTrack.name))
        {
            MusicManager.instance.SwapTrack(newTrack, maxVolume);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !MusicManager.instance.currentlyPlaying.Equals(newTrack.name))
        {
            MusicManager.instance.SwapTrack(newTrack, maxVolume);
        }
    }

    //private void OnTriggerExit2D(Collider2D collision)
    //{
    //    if (collision.CompareTag("Player"))
    //    {
    //        MusicManager.instance.ReturnToDefault();
    //    }
    //}
}
