using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStatus : MonoBehaviour
{
    public static GameStatus status;

    public SaveData dataToSave;

    public SaveData loadedData;

   // public GameObject test;

    // Start is called before the first frame update
    void Start()
    {
        // We go to main menu we need reset for loadedData !!! NOT THE BEST PLACE TO DO THIS
        //if (SceneManager.GetActiveScene().name.Equals("MainMenu"))
        //{
        //    loadedData = new SaveData();
        //}
        if (status == null)
        {
            DontDestroyOnLoad(gameObject);
            status = this;
        }
        else
        {
            Debug.Log("Destroyed gamestatus");
            Destroy(gameObject);
        }

        dataToSave = new SaveData();
        loadedData = new SaveData();

        //Load();

        //Instantiate(test, test.transform.position, Quaternion.identity);
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.S) && playerObject != null)
        //{
        //    Save();
        //}
        //else
        //{
        //    Debug.LogError("Cannot save due to playerObject is null");
        //}

        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            Load();
        }
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            DeleteSave();
        }
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            CheckData();
        }
    }

    // Funtions to Save / Load / Delete / Check 
    public void Save()
    {
        SaveSystem.SaveData(dataToSave);
    }

    public void Load()
    {
        loadedData = SaveSystem.LoadData();

        if (loadedData != null)
        {
            // If you save without updating dataToSave you will use loadedData to overwrite old save no data loss
            ResetDataToSave();

            Debug.Log("Save loaded");
        }
        else
        {
            Debug.LogError("No file found or data to insert");
        }
    }

    public void DeleteSave()
    {
        SaveSystem.DeleteSave();
    }

    public bool CheckData()
    {
        return SaveSystem.CheckIfFileExists();
    }


    // Functions to update SaveData dataToSave
    // loadedData doesn't need updates we get them from loading

    /*
     * Function to be used for example in death event
     */
    public void ResetDataToSave()
    {
        dataToSave = loadedData;
    }

    // Unity 2D transform.position returns Vector 3 and we use only x and y components
    public void UpdatePlayerPosition(float x, float y)
    {
        dataToSave.position[0] = x;
        dataToSave.position[1] = y;
    }
    
    public void UpdateBossKilled(int index, bool defeated)
    {
        dataToSave.bossesDefeated[index] = defeated;
    }
}
