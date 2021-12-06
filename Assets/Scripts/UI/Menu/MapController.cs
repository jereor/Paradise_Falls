using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class MapController : MonoBehaviour
{
    public static bool MapOpen = false;
    [Header("Objects")]
    public GameObject mapPanel;
    public Camera mapCamera; // Camera that captures map
    public RawImage mapImage; // RawImage where camera view is projected
    public PauseMenuController pauseController;
    public InventoryPanelController inventoryPanelController;
    public PlayerInteractions playerInteractions;
    public GameObject[] areaTriggers; // game objects of these triggers

    [Header("Variables scale/zoom")]
    [SerializeField] private float currentScale; 
    [SerializeField] private float maxScale;
    [SerializeField] private float minScale;
    [Tooltip("How much scale is multiplied per scroll")]
    [SerializeField] private float increment;

    [Header("Controller usage variables")]
    [Tooltip("Time how often zoom happends if controller zoom button is held down")]
    [SerializeField] private float controllerIncrementTimer; 
    private float controllerIncrementCounter = 0f;
    [SerializeField] private float navigationSpeed;

    // Controller needs these since behaviour differs from mouse button and scroll
    private Vector2 controllerNavigationInput;
    private float controllerZoomInput;
    // Start is called before the first frame update
    void Start()
    {
        currentScale = mapImage.rectTransform.localScale.x;
    }

    private void Update()
    {
        // Negative since anchor is right upper corner
        mapImage.rectTransform.anchoredPosition -= controllerNavigationInput;

        // Controller zoom done in Update to enable button holding down zooming
        if (controllerIncrementCounter >= controllerIncrementTimer)
            ControllerZoom();
        else
            controllerIncrementCounter += Time.deltaTime;

    }

    // Bases on https://forum.unity.com/threads/zoom-in-out-on-scrollrect-content-image.284655/ -rainbowegg
    // Modified to work on new input system
    public void Zoom(InputAction.CallbackContext context)
    {
        // Input comes from controller
        if (context.control.device.displayName.Contains("Controller"))
        {
            // Update controllerZoomInput to be used in ControllerZoom()
            if (context.ReadValue<Vector2>().normalized.y != 0)
                controllerZoomInput = Mathf.Sign(context.ReadValue<Vector2>().normalized.y);
            else
                controllerZoomInput = 0f;
        }
        // Input comes from mouse
        else
        {
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
    }

    public void Navigate(InputAction.CallbackContext context)
    {
        // Navigate in Update()
        controllerNavigationInput = new Vector2(Mathf.Round(context.ReadValue<Vector2>().x),Mathf.Round(context.ReadValue<Vector2>().y)) * navigationSpeed;
    }

    // Run every time controllerIncrementCounter is >= controllerIncrementTimer so zoom is not too fast(zoomed every Update)
    // Function enables holding button down to zoom
    private void ControllerZoom()
    {
        // If value is something else than zero since scrolling calls this function with scroll (- or +)value and scroll stop 0
        if (controllerZoomInput == 0)
            return;

        if (controllerZoomInput == -1)
            currentScale -= increment;
        else if (controllerZoomInput == 1)
            currentScale += increment;

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

        // Reset counter
        controllerIncrementCounter = 0f;
    }

    // Called from input map button (or pause map button)
    public void OpenMap(InputAction.CallbackContext context)
    {
        if (context.started && !playerInteractions.GetisInteractingWithNPC())
        {
            // Game is currently paused map button is pressed Resume() and open map
            if (PauseMenuController.GameIsPaused)
                pauseController.HandlePauseState();

            if (InventoryPanelController.InventoryIsActive)
                inventoryPanelController.HandleInventoryState();

            
            HandleMapState();
        }
    }

    // Called when map key is pressed
    public void HandleMapState()
    {
        // Map not open -> open it
        if (!MapOpen)
        {
            mapPanel.SetActive(true);
            MapOpen = true;
        }
        // Map open ...
        else if (MapOpen)
        {
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
    public GameObject[] GetAreaTriggers()
    {
        return areaTriggers;
    }

    public void SetAreaBlocks(int i, bool b)
    {
        // Get correct trigger with index saved and bool b
        areaTriggers[i].TryGetComponent(out MapAreaTrigger script);
        script.SetFound(b);
    }
}
