using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CutsceneTrigger : MonoBehaviour
{
    public string collideWith = "";
    public UnityEvent @event;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Cinematic Trigger!");
        if (other.gameObject.CompareTag(collideWith))
            @event.Invoke();
    }
}