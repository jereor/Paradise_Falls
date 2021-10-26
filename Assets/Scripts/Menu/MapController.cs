using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class MapController : MonoBehaviour
{
    public bool mapOpen = false;
    [Header("Objects")]
    public GameObject mapPanel;
    public Camera mapCamera;
    public RawImage mapImage;
    public PauseMenuController pauseController;
    public GameObject[] areaBlocks;


    [Header("Variables scale/zoom")]
    [SerializeField] private float currentScale; 
    [SerializeField] private float maxScale;
    [SerializeField] private float minScale;
    [Tooltip("How much scale is multiplied per scroll")]
    [SerializeField] private float increment;


    // Start is called before the first frame update
    void Start()
    {
        currentScale = mapImage.rectTransform.localScale.x;
    }

    // Update is called once per frame
    void Update()
    {

    }

    // Bases on https://forum.unity.com/threads/zoom-in-out-on-scrollrect-content-image.284655/ -rainbowegg
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
        // Max scaling (cannot go over maxScale value)
        if (currentScale >= maxScale)
        {
            currentScale = maxScale;
        }
        // Min scaling (cannot go over minScale value)
        else if (currentScale <= minScale)
        {
            currentScale = minScale;
        }
        // We can use x value since both x and y values scale with same increment value to preserve image resolution
        float oldLocalScaleX = mapImage.rectTransform.localScale.x;

        // Scale image
        mapImage.rectTransform.localScale = new Vector2(currentScale, currentScale);

        // Move image slightly to zoom in the middle of view 
        // If not done image "moves" towards origin point we lose the image we are currently viewing
        mapImage.rectTransform.anchoredPosition *= mapImage.rectTransform.localScale.x / oldLocalScaleX;

    }

    // Called from input map button (or pause map button)
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
