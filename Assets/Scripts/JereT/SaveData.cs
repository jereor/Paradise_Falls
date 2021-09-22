using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    //public int level;
  
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
