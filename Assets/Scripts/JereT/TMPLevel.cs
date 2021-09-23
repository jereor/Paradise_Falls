using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TMPLevel : MonoBehaviour
{
    public GameObject playerPrefab;

    public GameObject playerObject;

    public GameObject bossObject;

    public bool boss1;

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
            // Here update dataToSave 
            GameStatus.status.UpdatePlayerPosition(playerObject.transform.position.x ,playerObject.transform.position.y);

            GameStatus.status.UpdateBossKilled(0, boss1);

            GameStatus.status.Save();
        }
        // FOR DEBUGGING
        if (Input.GetKeyDown(KeyCode.R))
        {
            Destroy(playerObject);
            playerObject = Respawn(playerPrefab, new Vector2(GameStatus.status.loadedData.position[0], GameStatus.status.loadedData.position[1]));
        }
    }

    public GameObject Respawn(GameObject obj, Vector2 pos)
    {
        return Instantiate(obj, pos, Quaternion.identity);
    }

}
