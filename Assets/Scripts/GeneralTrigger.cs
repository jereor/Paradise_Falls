using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider2D))]
public class GeneralTrigger : MonoBehaviour
{
    public string collideWithTag = "";
    public UnityEvent @event;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(collideWithTag))
            @event.Invoke();
    }
}