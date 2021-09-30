using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject newCamera; // new camera attached in this object's inspector

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If collision with player, switch camera to the new camera
        if (collision.CompareTag("Player"))
        {
            CameraTransitions.Instance.SwitchCameras(newCamera);
        }
    }
}
