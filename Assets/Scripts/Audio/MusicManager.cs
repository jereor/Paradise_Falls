using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
public class MusicManager : MonoBehaviour
{
    public AudioClip defaultTrack;

    private AudioSource track01, track02;

    public AudioMixerGroup musicGroup;
    private bool isPlayingTrack01;

    public static MusicManager instance;

    public string currentlyPlaying = "";

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    private void Start()
    {
        track01 = gameObject.AddComponent<AudioSource>();
        track01.outputAudioMixerGroup = musicGroup;
        track01.loop = true;
        track02 = gameObject.AddComponent<AudioSource>();
        track02.outputAudioMixerGroup = musicGroup;
        track02.loop = true;


        //SwapTrack(defaultTrack);
    }


    public void SwapTrack(AudioClip newClip)
    {
        StopAllCoroutines();

        StartCoroutine(FadeTrack(newClip));

        isPlayingTrack01 = !isPlayingTrack01;
    }

    public void ReturnToDefault()
    {
        SwapTrack(defaultTrack);
    }

    private IEnumerator FadeTrack(AudioClip newClip)
    {
        float timeToFade = 1.25f;
        float timeElapsed = 0f;

        // Audio source 1 aka track01 ins playing we need to fade out track01 and fade in track02
        if (isPlayingTrack01)
        {
            track02.clip = newClip;
            currentlyPlaying = newClip.name;
            track02.Play();

            while (timeElapsed < timeToFade)
            {
                track02.volume = Mathf.Lerp(0, 1, timeElapsed / timeToFade);
                track01.volume = Mathf.Lerp(1, 0, timeElapsed / timeToFade);
                timeElapsed += Time.deltaTime;
                yield return null;
            }

            track01.Pause();
        }
        else
        {
            track01.clip = newClip;
            currentlyPlaying = newClip.name;
            track01.Play();

            while (timeElapsed < timeToFade)
            {
                track01.volume = Mathf.Lerp(0, 1, timeElapsed / timeToFade);
                track02.volume = Mathf.Lerp(1, 0, timeElapsed / timeToFade);
                timeElapsed += Time.deltaTime;
                yield return null;
            }

            track02.Pause();
        }
    }
}
