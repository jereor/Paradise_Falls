using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxChainController : MonoBehaviour
{
    public GameObject parentOfParts;
    public List<GameObject> chainParts = new List<GameObject>();
    public GameObject chainMount;
    public GameObject clawPart;

    private bool cut;

    private StaticMovingBox staticMovingBox;
    private BackAndForthMovingBox backAndForthBox;
    // Start is called before the first frame update
    void Start()
    {
        // Because this script is used in both BackAndForthMovingBox and StaticMovingBox scripts, assign the correct script to be used.
        if (GetComponentInParent<StaticMovingBox>() != null)
            staticMovingBox = GetComponentInParent<StaticMovingBox>();
        else if (GetComponentInParent<BackAndForthMovingBox>() != null)
            backAndForthBox = GetComponentInParent<BackAndForthMovingBox>();

        if (staticMovingBox != null) {
            foreach (GameObject chain in chainParts)
            {
                chain.GetComponent<Chain>().ChainIsCuttable(staticMovingBox.getIsCuttableChain());
            }
        }
        else if (backAndForthBox != null)
        {
            foreach (GameObject chain in chainParts)
            {
                chain.GetComponent<Chain>().ChainIsCuttable(backAndForthBox.getIsCuttableChain());
            }
        }
    }

    public void ChainIsCut(GameObject chain)
    {
        cut = true;
        bool cutRest = false;
        Destroy(clawPart);
        for (int i = 0; i < chainParts.Count; i++)
        {
            if (cutRest || chainParts[i].Equals(chain))
            {
                Destroy(chainParts[i]);
                if (!cutRest)
                    cutRest = true;              
            }
        }

        parentOfParts.transform.SetParent(null);
    }

    public List<GameObject> getChains()
    {
        return chainParts;
    }

    public bool getIfCut()
    {
        return cut;
    }

    public void setIfCut(bool b)
    {
        cut = b;
    }
}
