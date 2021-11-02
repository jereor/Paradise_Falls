using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DemoLoader : MonoBehaviour
{
    public static DemoLoader levelLoaderInstance;

    [Header("Player Objects")]
    public GameObject playerPrefab; // Prefab if we need to instantiate player
    public GameObject playerObject; // Assigned on Start()

    [Header("Enemies")]
    public GameObject parentOfGroundEnemies;    // Used to create enemies List and lenght of enemiesKilled
    public GameObject parentOfFlyingEnemies;    // 
    [SerializeField] private List<GameObject> enemies = new List<GameObject>(); // List of enemy objects in this scene 
    public bool[] enemiesKilled; // Array of booleans if true this enemy on current index is defeated aka destroy enemy on load

    [Header("Boss Objects")]
    public GameObject firstBossObject;
    public bool firstBossKilled;

    [Header("Pick ups")]
    public GameObject wallJumpPickUp; // Used in start to check if have acquired wallJump ability before last save
    public GameObject weaponPickUp;

    [Header("Savepoints")]
    public GameObject savePointsParent;
    [SerializeField] private List<GameObject> savePoints = new List<GameObject>();
    [SerializeField] public Vector2 respawnPoint;

    [Header("Scene utilities")]
    [SerializeField] private Transform currentRespawnPoint; // Default respawn point
    [SerializeField] private List<GameObject> cameras = new List<GameObject>();
    /*
     * GameScene loads initialize player and bosses
     */
    private void Start()
    {
        levelLoaderInstance = this;

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
                if(respawnPoint == Vector2.zero)
                {
                    respawnPoint = currentRespawnPoint.transform.position;
                }
                // Copy respawn point if it was or wasn't zero vector
                currentRespawnPoint.transform.position = respawnPoint;
                // Move player to respawn
                playerObject.transform.position = respawnPoint;

                // Weapon if weaponAcquired == true destroy weaponPickUp from scnene if not leave it untouched
                if (GameStatus.status.getLoadedData().multitool)
                {
                    playerObject.GetComponent<PlayerCombat>().PickUpWeapon();
                    Destroy(weaponPickUp);
                }

                // WallJump
                if (GameStatus.status.getLoadedData().wallJump)
                {
                    Player.Instance.UnlockWalljump();
                    Destroy(wallJumpPickUp);
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

            // This is sent to GameStatus on save default values are false
            enemiesKilled = new bool[parentOfGroundEnemies.transform.childCount + parentOfFlyingEnemies.transform.childCount];

            // Enemies to List
            for (int i = 0; i < parentOfGroundEnemies.transform.childCount; i++)
            {
                //Debug.Log("G");
                enemies.Add(parentOfGroundEnemies.transform.GetChild(i).gameObject);
            }
            for (int i = 0; i < parentOfFlyingEnemies.transform.childCount; i++)
            {
                //Debug.Log("F");
                enemies.Add(parentOfFlyingEnemies.transform.GetChild(i).gameObject);
            }

            // Check if enemy is defeated
            for (int i = 0; i < GameStatus.status.getLoadedData().enemiesDefeated.Length; i++)
            {
                if (GameStatus.status.getLoadedData().enemiesDefeated[i])
                {
                    Destroy(enemies[i]);                  
                }
            }

            if (firstBossObject != null && GameStatus.status.getLoadedData().bossesDefeated[0] == true)
            {
                Destroy(firstBossObject);
                firstBossKilled = GameStatus.status.getLoadedData().bossesDefeated[0];
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


        // FOR DEBUGGING -- Kill ENEMY 1
        if (Input.GetKeyDown(KeyCode.P))
        {
            EnemyKilled(enemies[0]);
        }
        // FOR DEBUGGING -- Respawn 
        if (Input.GetKeyDown(KeyCode.O))
        {
            PlayerDeathRespawn();
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

            // Weapon
            GameStatus.status.UpdateWeapon(playerObject.GetComponent<PlayerCombat>().getWeaponWielded());

            // Wall jump ability
            GameStatus.status.UpdateWallJump(Player.Instance.WalljumpUnlocked());

            // Enemies
            for (int i = 0; i < enemies.Count; i++)
            {
                GameStatus.status.UpdateEnemyKilled(i, enemiesKilled[i]);
                if (enemies[i] == null)
                {
                    //Debug.Log("dead enemy");
                    enemiesKilled[i] = true;
                }
                GameStatus.status.UpdateEnemyKilled(i, enemiesKilled[i]);
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

    public void EnemyKilled(GameObject enemyThatIsKilled)
    {
        // As enemyThatIsKilled will be Destroyed from scene when health is less that zero we have bool array 
        foreach  (GameObject enemy in enemies)
        {
            // GameObject given is found in array
            if(enemy == enemyThatIsKilled)
            {
                // enemies and enemiesKilled indexes are the same made in Start()
                enemiesKilled[enemies.IndexOf(enemy)] = true;
                break; // No need to the end
            }
        }
    }

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
