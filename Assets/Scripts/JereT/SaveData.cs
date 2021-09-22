
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

    /*
     *  Constructor for creating SaveData from given object.
     *  Personalize data when needed. 
     */
    public SaveData(TMPPlayer player)
    {
        position = new float[2];
        position[0] = player.transform.position.x;
        position[1] = player.transform.position.y;
    }
}
