using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class MapController : MonoBehaviour
{

    public bool mapOpen = false;

    public GameObject mapPanel;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpenMap()
    {
        if (!mapOpen)
        {
            mapPanel.SetActive(true);
            mapOpen = true;
        }
        else if (mapOpen)
        {
            mapPanel.SetActive(false);
            mapOpen = false;
        }
    }
}
