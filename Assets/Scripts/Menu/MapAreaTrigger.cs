using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapAreaTrigger : MonoBehaviour
{
    private bool found = false;

    public Image mapVisibilityBlocker;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !found)
        {
            Debug.Log("New area");
            mapVisibilityBlocker.enabled = false;
            found = true;
        }
    }

    // For saving
    public bool GetFound()
    {
        return found;
    }
    // For loading
    public void SetFound(bool b)
    {
        if (b)
            mapVisibilityBlocker.enabled = false;
        found = b;
    }
}
