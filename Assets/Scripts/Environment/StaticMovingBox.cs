using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class StaticMovingBox : MonoBehaviour
{
    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    [SerializeField] private float moveTime;
    [SerializeField] private Vector2[] moves;
    [SerializeField] private Vector2[] movesBack;
    private Vector2 startPosition;
    [SerializeField] private int stepCounter = 0;


    private bool changeState = false;
    [SerializeField] private bool returningObject = false;

    private bool isWaiting = false;
    private bool isChainCut = false;
    [SerializeField] private bool colliderDisabledAtStart = false;
    private bool gizmoPositionChange = true;

    [SerializeField] private bool cuttableChain = false;
    [SerializeField] private bool loop = true;
    [SerializeField] private bool destroyAfterPathComplete = true;

    // Start is called before the first frame update

    private void Awake()
    {

    }
    void Start()
    {

        movesBack = new Vector2[moves.Length + 1]; // Assign the array with the length of moves-array. Needed to script work properly!!
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        gizmoPositionChange = false;
        startPosition = transform.position;

        // Is the chain cuttable by melee weapon
        if (!cuttableChain)
            transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.black;
        else
            transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.red;

        // Sets the collider inactive if bool is true.

        if (colliderDisabledAtStart)
        {
            gameObject.GetComponent<SpriteRenderer>().color = Color.gray;
            boxCollider.enabled = !boxCollider.enabled;
        }
        // Is this object meant to return the same path back to the beginning? If is, make a reverse array of moves to take.
        if (returningObject)
        {
            for (int i = 0; i < moves.Length; i++)
            {
                movesBack[i] = moves[moves.Length - i - 1];
            }
            movesBack.SetValue(startPosition, moves.Length); // Set the starting position to the last value of the array. Does not work without this!

        }

    }

    private void OnDrawGizmosSelected()
    {
        if (gizmoPositionChange)
        {
            Gizmos.color = Color.white;
            if (moves[0] != null)
            {
                Vector2 position;
                Gizmos.DrawLine((Vector2)transform.position, moves[0]);
                Gizmos.DrawSphere(moves[0], 0.2f);
                Gizmos.DrawSphere(transform.position, 0.2f);
                position = moves[0];
                for (int i = 1; i < moves.Length; i++)
                {

                    Gizmos.DrawLine(moves[i - 1], moves[i]);
                    Gizmos.DrawSphere(moves[i], 0.2f);
                    position += moves[i];
                }
                if (loop)
                {
                    Gizmos.DrawLine(moves[moves.Length - 1], transform.position);
                    Gizmos.DrawSphere(moves[moves.Length - 1], 0.2f);
                    Gizmos.DrawSphere(transform.position, 0.2f);
                }
                else
                    Gizmos.DrawSphere(position, 0.2f);
            }
        }

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (gameObject.transform.childCount == 0 && !isChainCut && cuttableChain)
        {
            isChainCut = true;
            DOTween.Kill(rb); // Stops all Tweenings so the object doesn't move after the chain is cut.
            StopAllCoroutines();
            rb.isKinematic = false; // Change the rigidbody to dynamic and set the parameters.
            rb.velocity = new Vector2(0, 0);
            rb.gravityScale = 3f;
            rb.drag = 0.05f;
            rb.constraints = RigidbodyConstraints2D.None;
            rb.constraints = RigidbodyConstraints2D.FreezePositionX;
            rb.freezeRotation = true;
        }
        if (!isWaiting && !changeState)
        {
            Move(moves, moveTime); // Moves the object to the designated destination along the given path.
            StartCoroutine(Wait(moveTime)); // Wait until the path is finished.
            
        }
        if (!isWaiting && changeState && !DOTween.IsTweening(rb) && returningObject) // Is the Tweening done with the path and the object is meant to return?
        {
            MoveBack(movesBack, moveTime); // Uses the reverse array to return.
            StartCoroutine(WaitBack(moveTime));

        }
        else if (changeState && !isWaiting && !DOTween.IsTweening(rb) && loop && !destroyAfterPathComplete) // Looped route and not deostryable object?
            changeState = false;
        else if (changeState && !isWaiting && !DOTween.IsTweening(rb) && !loop && destroyAfterPathComplete) // Doesn't loop and is destroyable object?
            Destroy(gameObject);
    }

    private IEnumerator Wait(float waitTime)
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitTime);
        isWaiting = false;
        changeState = true;
    }

    private IEnumerator WaitBack(float waitTime)
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitTime);
        isWaiting = false;
        changeState = false;
    }

    private void Move(Vector2[] move, float time)
    {
        rb.DOPath(moves, time); // Takes an array of Vectors and follows it to the destination.
        stepCounter++;
    }

    private void MoveBack(Vector2[] move, float time)
    {
        rb.DOPath(movesBack, time);
        stepCounter++;
    }

    // Enable or disable the collider.
    private void EnableDisableBoxCollider()
    {
        boxCollider.enabled = !boxCollider.enabled;
    }

    // Used by BoxChainController script.
    public bool getIsBoxColliderEnabled()
    {
        return boxCollider.isActiveAndEnabled;
    }

    // Used by BoxChainController script.
    public bool getIsCuttableChain()
    {
        return cuttableChain;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Debug.Log(collision.collider.name);
        if (collision.gameObject.tag == "Boss")
        {
            Destroy(gameObject);
        }

        //if (collision.gameObject.tag == "Player" && !isChainCut)
        //{
        //    collision.collider.transform.SetParent(transform);
        //}
    }

    //private void OnCollisionStay2D(Collision2D collision)
    //{
    //    //if (collision.gameObject.tag == "Player" && !isChainCut)
    //    //{
    //    //    collision.collider.transform.SetParent(transform);
    //    //}
    //}

    //private void OnCollisionExit2D(Collision2D collision)
    //{
    //    if (collision.gameObject.tag == "Player" && !isChainCut)
    //    {
    //        collision.collider.transform.SetParent(GameObject.Find("[Gameplay]").transform);
    //    }
    //}
}
