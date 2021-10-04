using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DemoLoader : MonoBehaviour
{
    public static DemoLoader levelLoaderInstance;

    [Header("Player Objects")]
    public GameObject playerPrefab;
    public GameObject playerObject;

    [Header("Enemies")]
    public GameObject parentOfGroundEnemies;
    public GameObject parentOfFlyingEnemies;
    [SerializeField] private List<GameObject> enemies = new List<GameObject>();
    public bool[] enemiesKilled;

    [Header("Boss Objects")]
    public GameObject firstBossObject;
    public bool firstBossKilled;

    [Header("Pick ups")]
    public GameObject wallJumpPickUp;
    public GameObject weaponPickUp;

    [Header("Savepoints")]
    public GameObject savePointsParent;
    [SerializeField] private List<GameObject> savePoints = new List<GameObject>();
    [SerializeField] public Vector2 respawnPoint;

    [Header("Respawn point")]
    [SerializeField] private Transform currentRespawnPoint;
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
            Debug.Log("Player spawning to: " + GameStatus.status.getLoadedData().position[0] + ", " + GameStatus.status.getLoadedData().position[1]);
            // Set respawn point as loaded position
            respawnPoint = new Vector2(GameStatus.status.getLoadedData().position[0], GameStatus.status.getLoadedData().position[1]);
            if (GameObject.Find("Player").activeInHierarchy)
            {
                // Player respawnPosition
                playerObject = GameObject.Find("Player");
                if(respawnPoint == Vector2.zero)
                {
                    respawnPoint = currentRespawnPoint.transform.position;
                }
                currentRespawnPoint.transform.position = respawnPoint;
                playerObject.transform.position = respawnPoint;

                // Weapon
                if (GameStatus.status.getLoadedData().weaponAcquired)
                {
                    playerObject.GetComponent<PlayerCombat>().PickUpWeapon();
                    Destroy(weaponPickUp);
                }

                // WallJump
                if (GameStatus.status.getLoadedData().wallJumpAcquired)
                {
                    playerObject.GetComponent<PlayerMovement>().AllowWallJump();
                    Destroy(wallJumpPickUp);
                }

            }
            // No player in scene we instantiate it then
            else
            {
                playerObject = Respawn(playerPrefab, respawnPoint);
            }

            enemiesKilled = new bool[parentOfGroundEnemies.transform.childCount + parentOfFlyingEnemies.transform.childCount];

            // Enemies to List
            for (int i = 0; i < parentOfGroundEnemies.transform.childCount; i++)
            {
                Debug.Log("G");
                enemies.Add(parentOfGroundEnemies.transform.GetChild(i).gameObject);
            }
            for (int i = 0; i < parentOfFlyingEnemies.transform.childCount; i++)
            {
                Debug.Log("F");
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
            Debug.Log("Respawning, atm loading scene with loaded save");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public void Save()
    {
        if (CheckIfPlayerAtSavePoint())
        {
            // Here update dataToSave 
            GameStatus.status.UpdatePlayerPosition(respawnPoint.x, respawnPoint.y);

            GameStatus.status.UpdateBossKilled(0, firstBossKilled);

            GameStatus.status.UpdateWeapon(playerObject.GetComponent<PlayerCombat>().getWeaponWielded());

            GameStatus.status.UpdateWallJump(playerObject.GetComponent<PlayerMovement>().getAllowWallJump());

            for(int i = 0; i < enemiesKilled.Length; i++)
            {
                 GameStatus.status.UpdateEnemyKilled(i, enemiesKilled[i]);
            }

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
        foreach  (GameObject enemy in enemies)
        {
            if(enemy == enemyThatIsKilled)
            {
                enemiesKilled[enemies.IndexOf(enemy)] = true;
            }
        }
    }

    private GameObject Respawn(GameObject obj, Vector2 pos)
    {
        return Instantiate(obj, pos, Quaternion.identity);
    }

    public void PlayerDeathRespawn()
    {
        Debug.Log("Respawning, atm loading scene with loaded save");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

}
