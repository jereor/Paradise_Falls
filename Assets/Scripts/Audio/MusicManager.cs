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
    // MusicSwap will compare to this if it should Swap track
    public string currentlyPlaying = "";

    // These are used to keep track of volume levels if some track will be needed to be played with lower volume
    private float track01Volume = 1f;
    private float track02Volume = 1f;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    private void Start()
    {
        // Instantiate AudioSources
        track01 = gameObject.AddComponent<AudioSource>();
        track01.outputAudioMixerGroup = musicGroup;
        track01.loop = true;
        track02 = gameObject.AddComponent<AudioSource>();
        track02.outputAudioMixerGroup = musicGroup;
        track02.loop = true;

        // Game will not have "default" song
        //SwapTrack(defaultTrack);
    }


    public void SwapTrack(AudioClip newClip, float maxVolume)
    {
        StopAllCoroutines();

        StartCoroutine(FadeTrack(newClip, maxVolume));

        isPlayingTrack01 = !isPlayingTrack01;
    }

    public void ReturnToDefault()
    {
        SwapTrack(defaultTrack, 1f);
    }

    // Fades out currently playing track and fades in newClip
    private IEnumerator FadeTrack(AudioClip newClip, float maxVolume)
    {
        float timeToFade = 1.25f;
        float timeElapsed = 0f;

        // Audio source 1 aka track01 is playing we need to fade out track01 and fade in track02
        if (isPlayingTrack01)
        {
            track02.clip = newClip;
            currentlyPlaying = newClip.name;
            track02.Play();

            while (timeElapsed < timeToFade)
            {
                // Fade in track02 to maxVolume
                track02.volume = Mathf.Lerp(0, maxVolume, timeElapsed / timeToFade);
                track01.volume = Mathf.Lerp(track01Volume, 0, timeElapsed / timeToFade);
                timeElapsed += Time.deltaTime;
                yield return null;
            }
            // Save track02Volume
            track02Volume = maxVolume;
            track01.Pause();
        }
        // Audio source 2 aka track02 is playing we need to fade out track02 and fade in track01
        else
        {
            track01.clip = newClip;
            currentlyPlaying = newClip.name;
            track01.Play();

            while (timeElapsed < timeToFade)
            {
                track01.volume = Mathf.Lerp(0, maxVolume, timeElapsed / timeToFade);
                track02.volume = Mathf.Lerp(track02Volume, 0, timeElapsed / timeToFade);
                timeElapsed += Time.deltaTime;
                yield return null;
            }

            track01Volume = maxVolume;
            track02.Pause();
        }
    }
}
