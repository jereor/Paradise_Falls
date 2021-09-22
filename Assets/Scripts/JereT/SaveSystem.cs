using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem 
{
    /*
     * Function to return always same path with same file name
     * Easier to change path and file name if needed
     */
    private static string getSavePath()
    {
        //Debug.Log(Application.persistentDataPath);    //path should be: C:/Users/user/AppData/LocalLow/DefaultCompany/Paradise_Falls
        return Application.persistentDataPath + "/savedata.haha"; ;
    }

    /*
     * Check if there is file in path and Overwrite or create new file depending if file is found
     */
    public static void SaveData(TMPPlayer player)
    {
        Debug.Log("Saving data for: " + player.gameObject.name);

        string path = getSavePath();
        if (File.Exists(path))
        {
            Debug.Log("Found File from given path. Overwriting save file...");

            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Create);

            SaveData data = new SaveData(player);
            formatter.Serialize(stream, data);
            stream.Close();
        }
        else
        {
            Debug.Log("Creating new save file...");

            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Create);

            SaveData data = new SaveData(player);
            formatter.Serialize(stream, data);
            stream.Close();
        }
    }

    /*
     * Checks file path if there is file return SaveData object else LogError
     */
    public static SaveData LoadData()
    {
        Debug.Log("Loading data...");
        string path = getSavePath();
        if (File.Exists(path))
        {
            Debug.Log("Found a file");

            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            SaveData data = formatter.Deserialize(stream) as SaveData;

            stream.Close();
            return data;
        }
        else
        {
            Debug.LogError("Save file not found in: " + path);
            return null;
        }
    }

    /*
     * Checks path with getSavePath() and if File is found delete file else LogError
     */
    public static void DeleteSave()
    {
        Debug.Log("Delete saved data");
        string path = getSavePath();
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        else
        {
            Debug.LogError("Save file not found in: " + path);
        }
    }

    /*
     * Checks path with getSavePath() and if File is found Log info else LogError
     * FOR DEBUGGING
     */
    public static void CheckIfFileExists()
    {
        string path = getSavePath();
        if (File.Exists(path))
        {
            Debug.Log("Found a file");
        }
        else
        {
            Debug.LogError("Save file not found in: " + path);
        }
    }
}