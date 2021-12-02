using UnityEngine;
using UnityEngine.Audio;

public class GameStatus : MonoBehaviour
{
    public static GameStatus status;

    // This SaveData will be initialized on loading game scene as loadedData(no data loss if player saves game).
    // Its parameters will be changed via funtions in this class that contain Update...().
    // Updates for parameters should happen when game is saved and other script (TMPLevel) keeps track of parameter values when eq. boss is killed or item received
    [SerializeField] private SaveData dataToSave;

    // This is updated on Load() DO NOT MODIFY ITS PARAMETERS VIA CODE 
    // Parameters will be updated when Save has happened and Load is done
    [SerializeField] private SaveData loadedData;
    public SaveData getLoadedData() { return loadedData;}

    void Start()
    {
        if (status == null)
        {
            DontDestroyOnLoad(gameObject);
            status = this;
        }
        // Should not happen ever :)
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

        Load();
    }

    public void SaveFromSceneLoader(SceneLoader loader)
    {
        SaveSystem.SaveData(dataToSave);

        loadedData = SaveSystem.LoadData();

        if (loadedData != null)
        {
            // If you save without updating dataToSave you will use loadedData to overwrite old save no data loss
            ResetDataToSave();

            Debug.Log("Save loaded");
            loader.ReloadSceneOnSave();
        }
        else
        {
            Debug.LogError("No file found or data to insert");
        }
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
        // Replace old SaveData with new aka Delete loaded save data from unity
        loadedData = new SaveData();

        // Delete file
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
        //dataToSave = loadedData.Clone();
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
    public void UpdateShield(bool b)
    {
        dataToSave.shield = b;
    }
    public void UpdateMultitool(bool b)
    {
        dataToSave.multitool = b;
    }
    public void UpdateWallJump(bool b)
    {
        dataToSave.wallJump = b;
    }
    public void UpdateDash(bool b)
    {
        dataToSave.dash = b;
    }
    public void UpdateGrappling(bool b)
    {
        dataToSave.grappling = b;
    }
    public void UpdateSHJump(bool b)
    {
        dataToSave.shJump = b;
    }
    public void UpdateShieldGrind(bool b)
    {
        dataToSave.shieldGrind = b;
    }
    public void UpdateSHAttack(bool b)
    {
        dataToSave.shAttack = b;
    }
    public void UpdateCamera(string currentCamera)
    {
        dataToSave.camera = currentCamera;
    }
    public void UpdateMapTriggers(int i, bool b)
    {
        dataToSave.mapTriggers[i] = b;
    }
    public void UpdateDoors(int i, bool b)
    {
        dataToSave.doors[i] = b;
    }
    public void UpdateFirstBossDoors(int i, bool b)
    {
        dataToSave.firstBossDoors[i] = b;
    }
    public void UpdateMeleePickups(int i, bool b)
    {
        dataToSave.meleePickups[i] = b;
    }
    public void UpdateThrowPickups(int i, bool b)
    {
        dataToSave.throwPickups[i] = b;
    }
    public void UpdateHealthPickups(int i, bool b)
    {
        dataToSave.healthPickups[i] = b;
    }
    public void UpdateEnergyPickups(int i, bool b)
    {
        dataToSave.energyPickups[i] = b;
    }
    public void UpdateMasterVolume(float volume)
    {
        dataToSave.masterVolume = volume;
    }
    public void UpdateEffectVolume(float volume)
    {
        dataToSave.effectsVolume = volume;
    }
    public void UpdateMusicVolume(float volume)
    {
        dataToSave.musicVolume = volume;
    }
    public void UpdateFullScreen(bool b)
    {
        dataToSave.fullscreen = b;
    }
    public void UpdateResolution(int index)
    {
        dataToSave.resolutionIndex = index;
    }
}
