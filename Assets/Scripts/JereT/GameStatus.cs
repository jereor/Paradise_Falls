using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStatus : MonoBehaviour
{
    public static GameStatus status;

    // This SaveData will be initialized on loading game scene as loadedData(no data loss if player saves game).
    // Its parameters will be changed via funtions in this class that contain Update...().
    // Updates for parameters should happen when game is saved and other script (TMPLevel) keeps track of parameter values when eq. boss is killed or item received
    public SaveData dataToSave;

    // This is updated on Load() DO NOT MODIFY ITS PARAMETERS VIA CODE 
    // Parameters will be updated when Save has happened and Load is done
    public SaveData loadedData;

    void Start()
    {
        if (status == null)
        {
            DontDestroyOnLoad(gameObject);
            status = this;
        }
        // Should't happen ever :)
        else
        {
            Debug.Log("Destroyed gamestatus");
            Destroy(gameObject);
        }

        // These initializations help with occasional errors
        dataToSave = new SaveData();
        loadedData = new SaveData();
    }

    private void Update()
    {
        // FOR DEBUGGING
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

    // --- FUNCTIONS FOR Save / Load / Delete / Check ---

    public void Save()
    {
        SaveSystem.SaveData(dataToSave);
    }

    public bool Load()
    {
        loadedData = SaveSystem.LoadData();

        if (loadedData != null)
        {
            // If you save without updating dataToSave you will use loadedData to overwrite old save no data loss
            ResetDataToSave();

            Debug.Log("Save loaded");
            return true;
        }
        else
        {
            Debug.LogError("No file found or data to insert");
            return false;
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


    // FUNCTIONS THAT UPDATE dataToSave 
    // Best call place: just before SAVING
    // loadedData doesn't need updates we get them from LOADING

    /*
     * Function to be used for example in death event
     */
    public void ResetDataToSave()
    {
        dataToSave = loadedData;
    }

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
