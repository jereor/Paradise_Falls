using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TMPPlayer : MonoBehaviour
{


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            SavePlayer();
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            LoadPlayer();
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            DeleteSave();
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            CheckData();
        }
    }


    public void SavePlayer()
    {
        SaveSystem.SaveData(this);
    }

    public void LoadPlayer()
    {
        SaveData data = SaveSystem.LoadData();
        if (data != null)
        {
            Vector2 position = new Vector2(data.position[0], data.position[1]);
            transform.position = position;
        }
        else
        {
            Debug.LogError("No data to insert");
        }
    }

    public void DeleteSave()
    {
        SaveSystem.DeleteSave();
    }

    public void CheckData()
    {
        SaveSystem.CheckIfFileExists();
    }
}
