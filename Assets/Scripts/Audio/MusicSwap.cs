using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicSwap : MonoBehaviour
{
    public AudioClip newTrack;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !MusicManager.instance.currentlyPlaying.Equals(newTrack.name))
        {
            MusicManager.instance.SwapTrack(newTrack);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !MusicManager.instance.currentlyPlaying.Equals(newTrack.name))
        {
            MusicManager.instance.SwapTrack(newTrack);
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
