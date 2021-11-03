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
    // Remember to add UpdateParameter() -function to GameStatus

    public float[] position;    // 0 = X and 1 = Y, easier to save

    public bool[] bossesDefeated;             // true = defeated , false = not defeated

    public bool[] enemiesDefeated;

    public bool shield;
    public bool multitool;
    public bool wallJump;
    public bool grappling;
    public bool shockwave;

    public bool levers;

    public string camera;

    /*
     *  Constructor for creating SaveData from given object.
     *  Personalize data when needed. 
     */
    public SaveData()
    {
        // If problems arise with arrays hardcode initialize sizes here
        position = new float[2];

        bossesDefeated = new bool[1];

        enemiesDefeated = new bool[4];

        camera = "";
    }

    /*
     * DeepCopy function to copy attributes
     */
    public SaveData Clone()
    {
        SaveData temp = new SaveData();
        
        for(int i = 0; i < position.Length; i++)
        {
            temp.position[i] = this.position[i];
        }

        for (int i = 0; i < bossesDefeated.Length; i++)
        {
            temp.bossesDefeated[i] = this.bossesDefeated[i];
        }

        return temp;
    }

}
