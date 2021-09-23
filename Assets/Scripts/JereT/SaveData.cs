using UnityEngine;

[System.Serializable]
public class SaveData
{
    /*
     * Add variables here 
     * ie. 
     * int defeatedBosses[]
     * int skillsOpened[]
     * float/int health
     */

    public float[] position;    // 0 = X and 1 = Y, easier to save

    public bool[] bossesDefeated;             // 
    /*
     *  Constructor for creating SaveData from given object.
     *  Personalize data when needed. 
     */
    public SaveData()
    {
        position = new float[2];
        bossesDefeated = new bool[1];
    }
}
