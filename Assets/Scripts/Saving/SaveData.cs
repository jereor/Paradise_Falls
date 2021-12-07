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

    public bool[] firstBossDoors;


    public bool shield;
    public bool multitool;
    public bool wallJump;
    public bool dash;
    public bool grappling;
    public bool shJump;
    public bool shieldGrind;
    public bool shAttack;

    public bool[] levers;

    public bool[] doors;

    public bool[] meleePickups;
    public bool[] throwPickups;
    public bool[] energyPickups;
    public bool[] healthPickups;

    public string camera;

    public float masterVolume;
    public float effectsVolume;
    public float musicVolume;

    public bool fullscreen;
    public int resolutionIndex = 0;

    public bool[] mapTriggers;

    /*
     *  Constructor for creating SaveData from given object.
     *  Personalize data when needed. 
     */
    public SaveData()
    {
        // If problems arise with arrays hardcode initialize sizes here
        position = new float[2];

        bossesDefeated = new bool[1];

        firstBossDoors = new bool[4];

        camera = "";

        levers = new bool[11];

        doors = new bool[15];

        meleePickups = new bool[2];
        throwPickups = new bool[2];
        healthPickups = new bool[4];
        energyPickups = new bool[2];

        mapTriggers = new bool[19];
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
