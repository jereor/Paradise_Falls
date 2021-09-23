using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TMPLevel : MonoBehaviour
{
    [Header("Player Objects")]
    public GameObject playerPrefab;
    public GameObject playerObject;

    [Header("Boss Objects")]
    public GameObject firstBossObject;
    public bool firstBossKilled;

    [Header("Savepoints")]
    public GameObject savePointsParent;
    [SerializeField] private List<GameObject> savePoints = new List<GameObject>();

    /*
     * GameScene loads initialize player and bosses
     */
    private void Start()
    {
        if (GameStatus.status != null)
        {
            // SCENE INITIALIZATION --- could be done in Awake too test which is better
            playerObject = Respawn(playerPrefab, new Vector2(GameStatus.status.getLoadedData().position[0], GameStatus.status.getLoadedData().position[1]));

            if (GameStatus.status.getLoadedData().bossesDefeated[0] == true)
            {
                Destroy(firstBossObject);
                firstBossKilled = GameStatus.status.getLoadedData().bossesDefeated[0];
            }

            // Make list of savePoints
            for(int i = 0; i < savePointsParent.transform.childCount; i++)
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
            if (CheckIfPlayerAtSavePoint())
            {
                // Here update dataToSave 
                GameStatus.status.UpdatePlayerPosition(playerObject.transform.position.x, playerObject.transform.position.y);

                GameStatus.status.UpdateBossKilled(0, firstBossKilled);

                GameStatus.status.Save();
            }
            else
            {
                Debug.Log("Not close to any savePoint");
            }
        }


        // FOR DEBUGGING -- Kill BOSS
        if (Input.GetKeyDown(KeyCode.B))
        {
            Destroy(firstBossObject);
            firstBossKilled = true;
        }
        // FOR DEBUGGING -- Respawn 
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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
