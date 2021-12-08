using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance;

    [Header("Parents of shadow objects")]
    public GameObject exteriorShadowsParent;
    public GameObject interiorShadowsParent;
    public List<GameObject> freeformLights;

    [Header("Player Objects")]
    public GameObject playerPrefab; // Prefab if we need to instantiate player
    public GameObject playerObject; // Assigned on Start()

    [Header("Boss Objects")]
    public GameObject firstBossObject;
    public DoorController[] bossDoors;
    public GameObject[] bossTriggers;

    [Header("Upgrades")]
    public GameObject shieldUpgrade;
    public GameObject multitoolUpgrade;

    [Header("Npc")]
    public GameObject[] npcObjects;
    // Npc gives commented upgrades
    //public GameObject wallJumpUpgrade; // Used in start to check if have acquired wallJump ability before last save
    //public GameObject dashUpgrade;
    //public GameObject grapplingUpgrade;
    //public GameObject shJumpUpgrade;
    //public GameObject shieldGrindUpgrade;
    //public GameObject shAttackUpgrade;

    [Header("Pick ups")]
    public GameObject[] meleePickups;
    public GameObject[] throwPickups;
    public GameObject[] energyPickups;
    public GameObject[] healthPickups;
    [Header("Levers")]
    public GameObject[] levers;

    [Header("Doors (that will be opened via buttons)")]
    public GameObject[] doors;

    [Header("Savepoints")]
    public GameObject savePointsParent;
    [SerializeField] private List<GameObject> savePoints = new List<GameObject>();
    [SerializeField] public Vector2 respawnPoint;

    [Header("Scene utilities")]
    [SerializeField] private Transform currentRespawnPoint; // Default respawn point, players transform in scene
    [SerializeField] private List<GameObject> cameras = new List<GameObject>();

    [Header("Map triggers")]
    [SerializeField] private GameObject[] mapTriggers;
    /*
     * GameScene loads initialize player and bosses
     */
    private void Start()
    {
        Instance = this;

        if (GameStatus.status != null)
        {
            // SCENE INITIALIZATION --- could be done in Awake too test which is better
            //Debug.Log("Binds for save tests(alpha keys): 0 save, 9 checksave, 8 delete, 7 load, O respawn, P kill boss");
            //Debug.Log("Player spawning to: " + GameStatus.status.getLoadedData().position[0] + ", " + GameStatus.status.getLoadedData().position[1]);

            // Set respawn point as loaded position
            respawnPoint = new Vector2(GameStatus.status.getLoadedData().position[0], GameStatus.status.getLoadedData().position[1]);
            if (GameObject.Find("Player").activeInHierarchy)
            {
                // Shadows
                // Interiors
                if (!GameStatus.status.getLoadedData().enableInteriors)
                {
                    foreach (Transform iChilds in interiorShadowsParent.transform)
                    {
                        iChilds.gameObject.SetActive(false);
                    }
                }
                // Exteriors
                if (!GameStatus.status.getLoadedData().enableExteriors)
                {
                    foreach (Transform eChilds in exteriorShadowsParent.transform)
                    {
                        eChilds.gameObject.SetActive(false);
                    }
                    foreach (GameObject light in freeformLights)
                    {
                        light.SetActive(false);
                    }
                }

                // Player respawnPosition
                playerObject = GameObject.Find("Player");
                // If there was zero vector loaded, set new respawn point as currentRespawnPoint (default) 
                if (respawnPoint == Vector2.zero)
                {
                    respawnPoint = currentRespawnPoint.transform.position;
                }
                // Copy respawn point if it was or wasn't zero vector
                currentRespawnPoint.transform.position = respawnPoint;
                // Move player to respawn
                playerObject.transform.position = respawnPoint;

                // Compares loaded data and gives Player pickup upgrade and destroys that object what was saved or calls NPC functions
                CheckUpgrades();

                // Map triggers
                for (int i = 0; i < GameStatus.status.getLoadedData().mapTriggers.Length; i++)
                {
                    mapTriggers[i].GetComponent<MapAreaTrigger>().SetFound(GameStatus.status.getLoadedData().mapTriggers[i]);
                }

                // Compares loaded data and gives Player pickup upgrade and destroys that object what was saved
                // Pick ups
                for (int i = 0; i < GameStatus.status.getLoadedData().meleePickups.Length; i++)
                {
                    if (GameStatus.status.getLoadedData().meleePickups[i] && meleePickups[i].TryGetComponent(out SmallUpgradePickUp script))
                        script.SetSaveBuffer(true, playerObject);
                }
                for (int i = 0; i < GameStatus.status.getLoadedData().throwPickups.Length; i++)
                {
                    if (GameStatus.status.getLoadedData().throwPickups[i])
                    {
                        if (GameStatus.status.getLoadedData().throwPickups[i] && throwPickups[i].TryGetComponent(out SmallUpgradePickUp script))
                            script.SetSaveBuffer(true, playerObject);
                    }
                }
                for (int i = 0; i < GameStatus.status.getLoadedData().healthPickups.Length; i++)
                {
                    if (GameStatus.status.getLoadedData().healthPickups[i])
                    {
                        if (GameStatus.status.getLoadedData().healthPickups[i] && healthPickups[i].TryGetComponent(out SmallUpgradePickUp script))
                            script.SetSaveBuffer(true, playerObject);
                    }
                }
                for (int i = 0; i < GameStatus.status.getLoadedData().energyPickups.Length; i++)
                {
                    if (GameStatus.status.getLoadedData().energyPickups[i])
                    {
                        if (GameStatus.status.getLoadedData().energyPickups[i] && energyPickups[i].TryGetComponent(out SmallUpgradePickUp script))
                            script.SetSaveBuffer(true, playerObject);
                    }
                }

                // Levers and buttons
                for (int i = 0; i < GameStatus.status.getLoadedData().levers.Length; i++)
                {
                    if (GameStatus.status.getLoadedData().levers[i] && levers[i].TryGetComponent(out BasicButton buttonScript))
                        buttonScript.Interact();
                    else if (GameStatus.status.getLoadedData().levers[i] && levers[i].TryGetComponent(out Lever leverScript))
                        leverScript.Interact();
                }

                // Doors
                for (int i = 0; i < GameStatus.status.getLoadedData().doors.Length; i++)
                {
                    if(GameStatus.status.getLoadedData().doors[i] == true)
                        Destroy(doors[i]);
                }

                // Camera
                // Check if there is saved camera string
                if (GameStatus.status.getLoadedData().camera.Length > 1)
                {
                    //Debug.Log("Searching for camera");
                    foreach (GameObject camera in cameras)
                    {
                        // Compare strings if found SwitchCameras to it
                        if (camera.name.Equals(GameStatus.status.getLoadedData().camera))
                        {
                            CameraTransitions.Instance.SwitchCameras(camera);
                            break; // No need to check List anymore
                        }
                    }
                }

            }
            // No player in scene we instantiate it then
            else
            {
                playerObject = Respawn(playerPrefab, respawnPoint);
            }

            // Boss
            if (GameStatus.status.getLoadedData().bossesDefeated[0] == true)
            {
                foreach (GameObject trigger in bossTriggers)
                {
                    Destroy(trigger);
                }
                Destroy(firstBossObject);   
            }
            // Boss doors
            for (int i = 0; i < bossDoors.Length; i++)
            {
                bossDoors[i].GetComponent<DoorController>().SetIsDoorOpen(GameStatus.status.getLoadedData().firstBossDoors[i]);
            }

            if (savePointsParent != null)
            {
                // Make list of savePoints
                for (int i = 0; i < savePointsParent.transform.childCount; i++)
                {
                    savePoints.Add(savePointsParent.transform.GetChild(i).gameObject);
                }
            }
        }
        else
        {
            Debug.Log("No GameStatus object in scene if testing: either start from MainMenu or drag player prefab to scene");
        }

        PlayerCamera.Instance.CameraFadeIn(1);
    }

    // Update is called once per frame
    void Update()
    {
        // Later from interaction
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            Save();
        }

        // FOR DEBUGGING -- Respawn 
        if (Input.GetKeyDown(KeyCode.O))
        {
            PlayerDeathRespawn();
        }
    }

    private void CheckUpgrades()
    {
        // Shield
        if (GameStatus.status.getLoadedData().shield)
        {
            Player.Instance.UnlockShield();
            Destroy(shieldUpgrade);
        }
        // Weapon if multitool == true destroy weaponUpgrade from scnene if not leave it untouched
        if (GameStatus.status.getLoadedData().multitool)
        {
            Player.Instance.UnlockMultitool();
            Destroy(multitoolUpgrade);
        }
        // Rest are from NPC
        // WallJump
        if (GameStatus.status.getLoadedData().wallJump)
        {
            Player.Instance.UnlockWalljump();
            foreach (GameObject npc in npcObjects)
            {
                // If returns true we found correct npc no need to check rest npc
                if (CheckNPCUnlock(npc))
                    break;
            }
        }
        // Dash
        if (GameStatus.status.getLoadedData().dash)
        {
            Player.Instance.UnlockDash();
            foreach (GameObject npc in npcObjects)
            {
                // If returns true we found correct npc no need to check rest npc
                if (CheckNPCUnlock(npc))
                    break;
            }
        }
        // Grappling
        if (GameStatus.status.getLoadedData().grappling)
        {
            Player.Instance.UnlockGrappling();
            foreach (GameObject npc in npcObjects)
            {
                // If returns true we found correct npc no need to check rest npc
                if (CheckNPCUnlock(npc))
                    break;
            }
        }
        //SH Jump
        if (GameStatus.status.getLoadedData().shJump)
        {
            Player.Instance.UnlockJumpAndDive();
            foreach (GameObject npc in npcObjects)
            {
                // If returns true we found correct npc no need to check rest npc
                if (CheckNPCUnlock(npc))
                    break;
            }
        }
        // Shield grind
        if (GameStatus.status.getLoadedData().shieldGrind)
        {
            Player.Instance.UnlockShieldGrind();
            foreach (GameObject npc in npcObjects)
            {
                // If returns true we found correct npc no need to check rest npc
                if (CheckNPCUnlock(npc))
                    break;
            }
        }
        // SH attack
        if (GameStatus.status.getLoadedData().shAttack)
        {
            Player.Instance.UnlockShockwaveAttack();
            foreach (GameObject npc in npcObjects)
            {
                // If returns true we found correct npc no need to check rest npc
                if (CheckNPCUnlock(npc))
                    break;
            }
        }
    }

    private bool CheckNPCUnlock(GameObject npc)
    {
        if (npc.TryGetComponent(out ExplorerDroneController script))
        {
            // Walljump
            if (script.GetWhatNPCUnlocks() == 1)
            {
                Debug.Log("Found");
                script.SetHasBeenTalkedBefore(true);
                return true;
            }
            // Dash
            else if (script.GetWhatNPCUnlocks() == 2)
            {
                script.SetHasBeenTalkedBefore(true);
                return true;
            }
            // Grappling
            else if (script.GetWhatNPCUnlocks() == 3)
            {
                script.SetHasBeenTalkedBefore(true);
                return true;
            }
            // SH jump 
            else if (script.GetWhatNPCUnlocks() == 4)
            {
                script.SetHasBeenTalkedBefore(true);
                return true;
            }
            // Shield grind
            else if (script.GetWhatNPCUnlocks() == 5)
            {
                script.SetHasBeenTalkedBefore(true);
                return true;
            }
            // SH attack
            else if (script.GetWhatNPCUnlocks() == 6)
            {
                script.SetHasBeenTalkedBefore(true);
                return true;
            }
        }
        // Did not find correct unlock
        return false;
    }

    private void CheckPickups()
    {
        // health pickups
        for (int i = 0; i < healthPickups.Length; i++)
        {
            if (GameStatus.status.getLoadedData().healthPickups[i])
            {
                //Player.Instance.getPickup();
                Destroy(healthPickups[i]);
            }
        }
        // Throw pickups
        for (int i = 0; i < throwPickups.Length; i++)
        {
            if (GameStatus.status.getLoadedData().throwPickups[i])
            {
                //Player.Instance.getPickup();
                Destroy(throwPickups[i]);
            }
        }
        // Energy pickups
        for (int i = 0; i < energyPickups.Length; i++)
        {
            if (GameStatus.status.getLoadedData().energyPickups[i])
            {
                //Player.Instance.getPickup();
                Destroy(energyPickups[i]);
            }
        }
        // Health pickups
        for (int i = 0; i < healthPickups.Length; i++)
        {
            if (GameStatus.status.getLoadedData().healthPickups[i])
            {
                //Player.Instance.getPickup();
                Destroy(healthPickups[i]);
            }
        }
    }

    public void Save()
    {
        if (CheckIfPlayerAtSavePoint())
        {
            // Here update dataToSave 
            GameStatus.status.UpdatePlayerPosition(respawnPoint.x, respawnPoint.y);

            // Boss
            if (firstBossObject != null)
                GameStatus.status.UpdateBossKilled(0, false);
            else
                GameStatus.status.UpdateBossKilled(0, true);
            // First boss doors
            for (int i = 0; i < bossDoors.Length; i++)
            {
                GameStatus.status.UpdateFirstBossDoors(i, bossDoors[i].GetComponent<DoorController>().GetIsDoorOpen());
            }

            //GameStatus.status.UpdateBossKilled(1, secondBossKilled);

            // Shield
            GameStatus.status.UpdateShield(Player.Instance.ShieldUnlocked());

            // Multitool
            GameStatus.status.UpdateMultitool(Player.Instance.MultitoolUnlocked());

            // Wall jump ability
            GameStatus.status.UpdateWallJump(Player.Instance.WalljumpUnlocked());

            // Dash ability
            GameStatus.status.UpdateDash(Player.Instance.DashUnlocked());

            // Grappling
            GameStatus.status.UpdateGrappling(Player.Instance.GrapplingUnlocked());

            // SH jump
            GameStatus.status.UpdateSHJump(Player.Instance.ShockwaveJumpAndDiveUnlocked());

            // Shield grind
            GameStatus.status.UpdateShieldGrind(Player.Instance.ShieldGrindUnlocked());

            // SH attack
            GameStatus.status.UpdateSHAttack(Player.Instance.ShockwaveAttackUnlocked());

            for (int i = 0; i < meleePickups.Length; i++)
            {
                if (meleePickups[i] != null)
                    GameStatus.status.UpdateMeleePickups(i, false);
                else
                    GameStatus.status.UpdateMeleePickups(i, true);
            }

            for (int i = 0; i < throwPickups.Length; i++)
            {
                if (throwPickups[i] != null)
                    GameStatus.status.UpdateThrowPickups(i, false);
                else
                    GameStatus.status.UpdateThrowPickups(i, true);
            }

            for (int i = 0; i < healthPickups.Length; i++)
            {
                if (healthPickups[i] != null)
                    GameStatus.status.UpdateHealthPickups(i, false);
                else
                    GameStatus.status.UpdateHealthPickups(i, true);
            }

            for (int i = 0; i < energyPickups.Length; i++)
            {
                if (energyPickups[i] != null)
                    GameStatus.status.UpdateEnergyPickups(i, false);
                else
                    GameStatus.status.UpdateEnergyPickups(i, true);
            }

            // Map triggers
            for (int i = 0; i < mapTriggers.Length; i++)
            {
                if (mapTriggers[i].TryGetComponent(out MapAreaTrigger script))
                    GameStatus.status.UpdateMapTriggers(i, script.GetFound());
            }

            // Levers and buttons
            for (int i = 0; i < levers.Length; i++)
            {
                if (levers[i].TryGetComponent(out BasicButton buttonScript))
                    GameStatus.status.UpdateLevers(i, buttonScript.GetIsUsed());
                else if (levers[i].TryGetComponent(out Lever leverScript))
                    GameStatus.status.UpdateLevers(i, leverScript.GetIsUsed());
            }

            // Doors
            for (int i = 0; i < doors.Length; i++)
            {
                if (doors[i] != null)
                    GameStatus.status.UpdateDoors(i, false);
                else
                    GameStatus.status.UpdateDoors(i, true);
            }


            GameStatus.status.UpdateCamera(CameraTransitions.Instance.GetCurrentCamera().name);

            GameStatus.status.SaveFromSceneLoader(this);
        }
        else
        {
            Debug.Log("Not close to any savePoint");
        }
    }

    /*
     * Checks if player has activated TriggerEnter on any savepoint to allow saving
     */
    private bool CheckIfPlayerAtSavePoint()
    {
        foreach (GameObject savePoint in savePoints)
        {
            if (savePoint.GetComponent<SavePoint>().getPlayerIsClose() == true)
            {
                respawnPoint = new Vector2(savePoint.transform.position.x, savePoint.transform.position.y);
                return true;
            }
        }
        return false;
    }

    private GameObject Respawn(GameObject obj, Vector2 pos)
    {
        return Instantiate(obj, pos, Quaternion.identity);
    }

    // Lazy version for respawn load scene again with loaded data works as Save function updates saveData only
    public void PlayerDeathRespawn()
    {
        //Debug.Log("Respawning, atm loading scene with loaded save");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ReloadSceneOnSave()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

}
