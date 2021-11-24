using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventoryPanelController : MonoBehaviour
{
    public static bool InventoryIsActive = false;

    [SerializeField] GameObject hudUI;
    [SerializeField] GameObject inventoryPanel;
    [SerializeField] GameObject collectiblePanel;
    [SerializeField] GameObject informationPanel;
    public PlayerInteractions playerInteractions;
    public PauseMenuController pauseController;
    public MapController mapController;
    public Player player;
    public List<GameObject> skillList = new List<GameObject>();

    [Header("Collectible Counts")]
    [SerializeField] private int healthUpgradeCount;
    [SerializeField] private int energyUpgradeCount;
    [SerializeField] private int meleeDmgUpgradeCount;
    [SerializeField] private int throwWeaponUpgradeCount;

    [Header("Maximum Collectible Counts")]
    [SerializeField] private int maxHealthUpgradeCount;
    [SerializeField] private int maxEnergyUpgradeCount;
    [SerializeField] private int maxMeleeDmgUpgradeCount;
    [SerializeField] private int maxThrowWeaponUpgradeCount;

    public TMP_Text healthUpgradeObject;
    public TMP_Text energyUpgradeObject;
    public TMP_Text meleeUpgradeObject;
    public TMP_Text throwUpgradeObject;

    [SerializeField] private bool isUsingController = false;

    // Start is called before the first frame update
    void Start()
    {
        UpdateSkillView();

        UpdateCollectibleCounts();

        UpdateControlScheme();
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

            UpdateCollectibleCounts();
            HandleInventoryState();
        }
    }

    public void HandleInventoryState()
    {
        if(!InventoryIsActive)
        {
            hudUI.SetActive(false);
            inventoryPanel.SetActive(true);
            player.HandleAllPlayerControlInputs(false);
            InventoryIsActive = true;
        }
        else if(InventoryIsActive)
        {
            hudUI.SetActive(true);
            informationPanel.SetActive(false);
            inventoryPanel.SetActive(false);
            player.HandleAllPlayerControlInputs(true);
            InventoryIsActive = false;
        }
    }

    public void UpdateSkillView()
    {
        if(player.ShieldUnlocked())
        {
            skillList[0].SetActive(true);
        }
        if (player.MultitoolUnlocked())
        {
            skillList[1].SetActive(true);
        }
        if (player.WalljumpUnlocked())
        {
            skillList[2].SetActive(true);
        }
        if (player.DashUnlocked())
        {
            skillList[3].SetActive(true);
        }
        if (player.GrapplingUnlocked())
        {
            skillList[4].SetActive(true);
        }
        if (player.ShockwaveJumpAndDiveUnlocked())
        {
            skillList[5].SetActive(true);
            skillList[6].SetActive(true);

        }
        if (player.ShockwaveAttackUnlocked())
        {
            skillList[7].SetActive(true);
        }
        if (player.ShieldGrindUnlocked())
        {
            skillList[8].SetActive(true);
        }
    }

    public void UpdateCollectibleCounts()
    {
        healthUpgradeObject.text = "Health: " + healthUpgradeCount + "/" + maxHealthUpgradeCount + "    " + (int)(healthUpgradeCount / (float)maxHealthUpgradeCount * 100) + "%";
        energyUpgradeObject.text = "Energy: " + energyUpgradeCount + "/" + maxEnergyUpgradeCount + "    " + (int)(energyUpgradeCount / (float)maxEnergyUpgradeCount * 100) + "%";
        meleeUpgradeObject.text = "Melee Damage: " + meleeDmgUpgradeCount + "/" + maxMeleeDmgUpgradeCount + "    " + (int)(meleeDmgUpgradeCount / (float)maxMeleeDmgUpgradeCount * 100) + "%";
        throwUpgradeObject.text = "Throw Damage: " + throwWeaponUpgradeCount + "/" + maxThrowWeaponUpgradeCount + "    " + (int)(throwWeaponUpgradeCount / (float)maxThrowWeaponUpgradeCount * 100) + "%";
    }

    public void UpdateControlScheme()
    {
        if(isUsingController)
        {
            foreach (GameObject skill in skillList)
            {
                skill.GetComponent<SkillInformationDisplayController>().SetIsUsingController(true);
            }
        }
        else if(!isUsingController)
        {
            foreach (GameObject skill in skillList)
            {
                skill.GetComponent<SkillInformationDisplayController>().SetIsUsingController(false);
            }
        }
    }

}
