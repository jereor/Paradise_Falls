using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject newCamera;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            CameraTransitions.Instance.SwitchCameras(newCamera);
        }
    }
}
