using UnityEngine;

public class Chain : MonoBehaviour
{
    [SerializeField] private bool amICuttable = false;
    public bool cut = false;

    private BoxChainController chainController;

    // Start is called before the first frame update
    void Awake()
    {
        // Because this script is used in both BackAndForthMovingBox and StaticMovingBox scripts, assign the correct script to be used.
        if (GetComponentInParent<BoxChainController>() != null)
            chainController = GetComponentInParent<BoxChainController>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (cut && chainController != null)
            chainController.ChainIsCut(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "MeleeWeapon" && amICuttable)
        {
            cut = true;
        }
    }

    public void ChainIsCuttable(bool b)
    {
        amICuttable = b;
    }

    public void CutChain(bool b)
    {
        cut = b;
    }
}
