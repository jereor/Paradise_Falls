using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TMPLevel : MonoBehaviour
{
    public GameObject playerPrefab;

    public GameObject playerObject;

    public GameObject bossObject;

    public bool boss1;


    public GameObject savePointsContainer;
    private List<GameObject> savePoints = new List<GameObject>();

    /*
     * GameScene loads initialize player and bosses
     */
    private void Start()
    {
        if (GameStatus.status != null)
        {
            // SCENE INITIALIZATION
            playerObject = Respawn(playerPrefab, new Vector2(GameStatus.status.loadedData.position[0], GameStatus.status.loadedData.position[1]));

            if (GameStatus.status.loadedData.bossesDefeated[0] == true)
            {
                Destroy(bossObject);
            }

            for(int i = 0; i < savePointsContainer.transform.childCount; i++)
            {
                savePoints.Add(savePointsContainer.transform.GetChild(i).gameObject);
            }
        }
        else
        {
            Debug.LogError("No GameStatus object in scene");
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

                GameStatus.status.UpdateBossKilled(0, boss1);

                GameStatus.status.Save();
            }
            else
            {
                Debug.Log("Not close to save point");
            }
        }
        // FOR DEBUGGING
        if (Input.GetKeyDown(KeyCode.B))
        {
            Destroy(bossObject);
            boss1 = true;
        }
        // FOR DEBUGGING
        if (Input.GetKeyDown(KeyCode.R))
        {
            Destroy(playerObject);
            playerObject = Respawn(playerPrefab, new Vector2(GameStatus.status.loadedData.position[0], GameStatus.status.loadedData.position[1]));
        }
    }

    private bool CheckIfPlayerAtSavePoint()
    {
        foreach (GameObject savePoint in savePoints)
        {
            if (savePoint.GetComponent<SavePoint>().playerIsClose == true)
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
