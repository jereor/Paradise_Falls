using UnityEngine;

/*
 * GameStatus uses 2 objects of this class to hande saving and loading: loadedData and dataToSave
 * 
 * Ns olio jota k‰ytet‰‰n SaveSystemissa, jotta datan kirjoittaminen ja lukeminen onnistuu kivuttomammin kuin esim int ja bool arvojen tallentaminen erikseen
 */

[System.Serializable]
public class SaveData
{
    /*
     * Add variables here 
     * ie. 
     * int defeatedBosses[]
     * int skillsOpened[]
     * float/int health
     * bool itemsCollected[]
     * bool wallBroken[]
     */

    public float[] position;    // 0 = X and 1 = Y, easier to save

    public bool[] bossesDefeated;             // true = defeated , false = not defeated

    /*
     *  Constructor for creating SaveData from given object.
     *  Personalize data when needed. 
     */
    public SaveData()
    {
        // If problems arise with arrays hardcode initialize sizes here
        position = new float[2];
        bossesDefeated = new bool[1];
    }
}
