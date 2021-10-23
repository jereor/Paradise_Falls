using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject newCamera; // new camera attached in this object's inspector

    [Header("Cooldown")]
    [SerializeField] private float transitionCooldown;

    // References
    private GameObject currentCamera;

    // State variables
    private bool inTransition;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If collision with player, switch camera to the new camera
        if (collision.CompareTag("Player"))
        {
            currentCamera = CameraTransitions.Instance.GetCurrentCamera();
            if (currentCamera != newCamera) // New camera is different than the current camera
            {
                CameraTransitions.Instance.SwitchCameras(newCamera);
                StartCoroutine(ActivateTransitionCD());
            }
            //if (currentCamera != newCamera && !inTransition) // New camera is different and camera is currently not in transition
            //{
            //    CameraTransitions.Instance.SwitchCameras(newCamera);
            //    StartCoroutine(ActivateTransitionCD());
            //}
        }
    }

    private IEnumerator ActivateTransitionCD()
    {
        inTransition = true;

        float transitionTimer = 0;
        while (transitionTimer <= transitionCooldown)
        {
            transitionTimer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        inTransition = false;
    }
}
