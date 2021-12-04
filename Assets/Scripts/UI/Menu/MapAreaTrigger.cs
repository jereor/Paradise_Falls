using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapAreaTrigger : MonoBehaviour
{
    private bool found = false;

    [Header("Activate these objects on trigger enter")]
    public GameObject[] areaObjectsToActivate;
    [Header("Deactivate these objects on trigger enter")]
    public GameObject[] areaObjectsToDeactivate;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !found)
        {
            HandleActivations();

            found = true;
        }
    }

    private void HandleActivations()
    {
        // Activate all objects in areaObjectsToActivate array
        if (areaObjectsToActivate.Length > 0)
        {
            foreach (GameObject obj in areaObjectsToActivate)
            {
                obj.SetActive(true);
            }
        }
        // Deactivate all objects in areaObjectsToActivate array
        if (areaObjectsToActivate.Length > 0)
        {
            foreach (GameObject obj in areaObjectsToDeactivate)
            {
                obj.SetActive(false);
            }
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
        // If found unlock map parts if not leave it hidden default
        if(b)
            HandleActivations();
        found = b;
    }
}
