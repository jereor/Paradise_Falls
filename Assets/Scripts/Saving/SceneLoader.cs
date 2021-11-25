using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance;

    [Header("Player Objects")]
    public GameObject playerPrefab; // Prefab if we need to instantiate player
    public GameObject playerObject; // Assigned on Start()

    [Header("Boss Objects")]
    public GameObject firstBossObject;
    public bool firstBossKilled;

    public GameObject secondBossObject;
    public bool secondBossKilled;

    [Header("Upgrades")]
    public GameObject shieldUpgrade;
    public GameObject multitoolUpgrade;

    [Header("Npc")]
    public GameObject npc;
    // Npc gives commented upgrades
    //public GameObject wallJumpUpgrade; // Used in start to check if have acquired wallJump ability before last save
    //public GameObject dashUpgrade;
    public GameObject grapplingUpgrade;
    //public GameObject shJumpUpgrade;
    //public GameObject shieldGrindUpgrade;
    //public GameObject shAttackUpgrade;

    [Header("Pick ups")]
    public GameObject[] meleePickups;
    public GameObject[] throwPickups;
    public GameObject[] energyPickups;
    public GameObject[] healthPickups;

    [Header("Levers / buttons")]
    public GameObject[] levers;

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
            Debug.Log("Binds for save tests(alpha keys): 0 save, 9 checksave, 8 delete, 7 load, O respawn, P kill boss");
            //Debug.Log("Player spawning to: " + GameStatus.status.getLoadedData().position[0] + ", " + GameStatus.status.getLoadedData().position[1]);

            // Set respawn point as loaded position
            respawnPoint = new Vector2(GameStatus.status.getLoadedData().position[0], GameStatus.status.getLoadedData().position[1]);
            if (GameObject.Find("Player").activeInHierarchy)
            {
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

                // Compares loaded data and gives Player pickup upgrade and destroys that object what was saved
                CheckPickups();

                // Map triggers
                for (int i = 0; i < GameStatus.status.getLoadedData().mapTriggers.Length; i++)
                {
                    mapTriggers[i].GetComponent<MapAreaTrigger>().SetFound(GameStatus.status.getLoadedData().mapTriggers[i]);
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
                            //Debug.Log("Found");
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
            if (firstBossObject != null && GameStatus.status.getLoadedData().bossesDefeated[0] == true)
            {
                // Do something to not show boss

                Destroy(firstBossObject);
                firstBossKilled = GameStatus.status.getLoadedData().bossesDefeated[0];
            }
            if (secondBossObject != null && GameStatus.status.getLoadedData().bossesDefeated[0] == true)
            {
                // Do something to not show boss

                Destroy(secondBossObject);
                secondBossKilled = GameStatus.status.getLoadedData().bossesDefeated[0];
            }

            // Make list of savePoints
            for (int i = 0; i < savePointsParent.transform.childCount; i++)
            {
                savePoints.Add(savePointsParent.transform.GetChild(i).gameObject);
            }
        }
        else
        {
            Debug.Log("No GameStatus object in scene if testing: either start from MainMenu or drag player prefab to scene");
        }
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
        // WallJump
        if (GameStatus.status.getLoadedData().wallJump)
        {
            Player.Instance.UnlockWalljump();
            // NPC juttuja !!------
        }
        // Dash
        if (GameStatus.status.getLoadedData().dash)
        {
            Player.Instance.UnlockDash();
            // NPC juttuja !!------
        }
        // Grappling
        if (GameStatus.status.getLoadedData().grappling)
        {
            Player.Instance.UnlockGrappling();
            Destroy(grapplingUpgrade);
        }
        // Shockwave Jump and Dive
        if (GameStatus.status.getLoadedData().shJump)
        {
            Player.Instance.UnlockJumpAndDive();
            // NPC juttuja !!------
        }
        // Shield Grind
        if (GameStatus.status.getLoadedData().shieldGrind)
        {
            Player.Instance.UnlockShieldGrind();
            // NPC juttuja !!------
        }
        // Shockwave attack
        if (GameStatus.status.getLoadedData().shAttack)
        {
            Player.Instance.UnlockShockwaveAttack();
            // NPC juttuja !!------
        }
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
            GameStatus.status.UpdateBossKilled(0, firstBossKilled);

            GameStatus.status.UpdateBossKilled(1, secondBossKilled);

            // Shield
            GameStatus.status.UpdateWallJump(Player.Instance.ShieldUnlocked());

            // Multitool
            GameStatus.status.UpdateMultitool(Player.Instance.MultitoolUnlocked());

            // Wall jump ability
            GameStatus.status.UpdateWallJump(Player.Instance.WalljumpUnlocked());

            // Dash ability
            GameStatus.status.UpdateDash(Player.Instance.DashUnlocked());

            // Grappling
            GameStatus.status.UpdateWallJump(Player.Instance.GrapplingUnlocked());

            // SH jump
            GameStatus.status.UpdateSHJump(Player.Instance.ShockwaveJumpAndDiveUnlocked());

            // Shield grind
            GameStatus.status.UpdateShieldGrind(Player.Instance.ShieldGrindUnlocked());

            // SH attack
            GameStatus.status.UpdateSHAttack(Player.Instance.ShockwaveAttackUnlocked());

            // Map triggers
            for (int i = 0; i < mapTriggers.Length; i++)
            {
                if (mapTriggers[i].TryGetComponent(out MapAreaTrigger script))
                    GameStatus.status.UpdateMapTriggers(i, script.GetFound());
            }

            // Levers
            for (int i = 0; i < levers.Length; i++)
            {
                //if (levers[i].TryGetComponent(out BasicButton buttonScript))
                //    GameStatus.status.UpdateLevers(i, buttonScript);
                //else if (levers[i].TryGetComponent(out Lever leverScript))
                //    GameStatus.status.UpdateLevers(i, leverScript);
            }

            GameStatus.status.UpdateCamera(CameraTransitions.Instance.GetCurrentCamera().name);

            GameStatus.status.Save();
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

    //public void EnemyKilled(GameObject enemyThatIsKilled)
    //{
    //    // As enemyThatIsKilled will be Destroyed from scene when health is less that zero we have bool array 
    //    foreach (GameObject enemy in enemies)
    //    {
    //        // GameObject given is found in array
    //        if (enemy == enemyThatIsKilled)
    //        {
    //            // enemies and enemiesKilled indexes are the same made in Start()
    //            enemiesKilled[enemies.IndexOf(enemy)] = true;
    //            break; // No need to the end
    //        }
    //    }
    //}

    private GameObject Respawn(GameObject obj, Vector2 pos)
    {
        return Instantiate(obj, pos, Quaternion.identity);
    }

    // Lazy version for respawn load scene again with loaded data works as Save function updates saveData only
    public void PlayerDeathRespawn()
    {
        Debug.Log("Respawning, atm loading scene with loaded save");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

}
