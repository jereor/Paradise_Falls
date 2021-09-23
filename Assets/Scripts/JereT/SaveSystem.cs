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
        return Application.persistentDataPath + "/savedata.haha";
    }

    /*
     * Check if there is file in path and Overwrite or create new file depending if file is found
     * 
     * Route to save:
     * - this function called from other Script with SaveSystem.SaveData(objectContaining info to save)
     * - check path
     * - open stream to path
     * - create new SaveData object with costructor with given object, !!! Check SaveData script for what variables are being saved
     * - Formate created SaveData from JSON?(doesn't really matter what type of data it is here) to binary file
     * - close stream
     */
    public static void SaveData(SaveData dataToSave)
    {
        //Debug.Log("Saving data for: " + player.gameObject.name);

        string path = getSavePath();
        if (File.Exists(path))
        {
            Debug.Log("Found File from given path. Overwriting save file...");

            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Create);

            //SaveData data = new SaveData(player);
            formatter.Serialize(stream, dataToSave);
            stream.Close();
        }
        else
        {
            Debug.Log("Creating new save file...");

            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Create);

            //SaveData data = new SaveData(player);
            formatter.Serialize(stream, dataToSave);
            stream.Close();
        }
    }

    /*
     * Checks file path if there is file return SaveData object else LogError
     * Route to load:
     * - this function called from other Script with SaveSystem.LoadData()
     * - check path
     * - open stream to path
     * - create new SaveData object with costructor with given object, !!! Check SaveData script for what variables are being saved
     * - Formate data found to SaveData object
     * - close stream
     * - return SaveData object 
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
    public static bool CheckIfFileExists()
    {
        string path = getSavePath();
        if (File.Exists(path))
        {
            Debug.Log("Found a file");
            return true;
        }
        else
        {
            Debug.Log("Save file not found in: " + path);
            return false;
        }
    }
}