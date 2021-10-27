using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class MapController : MonoBehaviour
{
    public static bool MapOpen = false;
    [Header("Objects")]
    public GameObject mapPanel;
    public Camera mapCamera; // Camera that captures map
    public RawImage mapImage; // RawImage where camera view is projected
    public PauseMenuController pauseController;
    public GameObject[] areaBlocks; // game objects of these triggers

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
    public void OpenMap(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            HandleMapState();
        }
    }

    // Called when map key is pressed
    public void HandleMapState()
    {
        // Map not open -> open it
        if (!MapOpen)
        {
            // Game not paused -> pause it
            if (!PauseMenuController.GameIsPaused)
            {
                pauseController.HandlePauseState();
            }
            mapPanel.SetActive(true);
            MapOpen = true;
        }
        // Map open ...
        else if (MapOpen)
        {
            if (PauseMenuController.GameIsPaused)
            {
                pauseController.HandlePauseState();
            }
            mapPanel.SetActive(false);
            MapOpen = false;
        }
    }

    // Called when pause (or back) button is pressed while map is open
    public void GoToPauseMenu()
    {
        mapPanel.SetActive(false);
        MapOpen = false;
    }


    // For saving and loading
    public GameObject[] GetAreaBlocks()
    {
        return areaBlocks;
    }

    public void SetAreaBlocks(int i, bool b)
    {
        // Get correct trigger with index saved and bool b
        areaBlocks[i].TryGetComponent(out MapAreaTrigger script);
        script.SetFound(b);
    }
}
