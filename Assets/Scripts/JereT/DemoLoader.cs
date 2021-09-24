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

    [Header("Boss Objects")]
    public GameObject firstBossObject;
    public bool firstBossKilled;

    [Header("Savepoints")]
    public GameObject savePointsParent;
    [SerializeField] private List<GameObject> savePoints = new List<GameObject>();
    [SerializeField] private Vector2 respawnPoint;

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
            if (GameObject.Find("Player").activeInHierarchy)
            {
                playerObject = GameObject.Find("Player");
                playerObject.transform.position = new Vector2(GameStatus.status.getLoadedData().position[0], GameStatus.status.getLoadedData().position[1]);
            }
            else
            {
                playerObject = Respawn(playerPrefab, new Vector2(GameStatus.status.getLoadedData().position[0], GameStatus.status.getLoadedData().position[1]));
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


        // FOR DEBUGGING -- Kill BOSS
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("Instakill bouss");
            Destroy(firstBossObject);
            firstBossKilled = true;
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

    public GameObject Respawn(GameObject obj, Vector2 pos)
    {
        return Instantiate(obj, pos, Quaternion.identity);
    }
}
