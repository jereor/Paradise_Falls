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

    public RawImage mapImage;

    public float currentScale;
    public float maxScale;
    public float minScale;
    public float increment;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    // Bases on https://forum.unity.com/threads/zoom-in-out-on-scrollrect-content-image.284655/ rainbowegg
    // Modified to work on new input system
    public void Zoom(InputAction.CallbackContext context)
    {
        //Debug.Log("Zoom " + context.ReadValue<Vector2>().normalized.y);
        // If value is something else than zero since scrolling calls this function with scroll (- or +)value and scroll stop 0
        if (context.ReadValue<Vector2>().normalized.y != 0)
        {
            if (Mathf.Sign(context.ReadValue<Vector2>().normalized.y) == -1)
                currentScale -= increment;
            else if (Mathf.Sign(context.ReadValue<Vector2>().normalized.y) == 1)
                currentScale += increment;
        }
        // Max scaling
        if (currentScale >= maxScale)
        {
            currentScale = maxScale;
        }
        // Min scaling
        else if (currentScale <= minScale)
        {
            currentScale = minScale;
        }
        // Scale image
        mapImage.rectTransform.localScale = new Vector2(currentScale, currentScale);
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
