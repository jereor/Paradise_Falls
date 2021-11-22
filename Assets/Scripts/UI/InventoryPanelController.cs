using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class InventoryPanelController : MonoBehaviour
{
    public static bool InventoryIsActive = false;

    [SerializeField] GameObject hudUI;
    [SerializeField] GameObject inventoryPanel;
    public PlayerInteractions playerInteractions;
    public PauseMenuController pauseController;
    public MapController mapController;
    public Player player;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpenInventory(InputAction.CallbackContext context)
    {
        if(context.started && !playerInteractions.GetisInteractingWithNPC() && !PauseMenuController.GameIsPaused)
        {
            if (PauseMenuController.GameIsPaused)
                pauseController.HandlePauseState();

            if (MapController.MapOpen)
                mapController.HandleMapState();

            HandleInventoryState();
        }
    }

    public void HandleInventoryState()
    {
        if(!InventoryIsActive)
        {
            inventoryPanel.SetActive(true);
            player.HandleAllPlayerControlInputs(false);
            InventoryIsActive = true;
        }
        else if(InventoryIsActive)
        {
            inventoryPanel.SetActive(false);
            player.HandleAllPlayerControlInputs(true);
            InventoryIsActive = false;
        }
    }

}
