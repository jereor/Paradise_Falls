using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSFX : MonoBehaviour
{
    [SerializeField] private AudioSource[] playerSteps;
    public void PlayRandomPlayerStepSound()
    {
        int random = Random.Range(0, 1);
        playerSteps[random].Play();
    }
}
