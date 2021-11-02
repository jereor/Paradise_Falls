using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class StaticMovingBox : MonoBehaviour
{
    private Rigidbody2D rb;
    private Rigidbody2D playerRB;
    private BoxCollider2D boxCollider;
    private CircleCollider2D circleCollider;
    [SerializeField] private float moveTime;
    [SerializeField] private float speed;
    [SerializeField] private float circleSize = 0.15f;
    [SerializeField] private List<Vector2> moves;
    [SerializeField] private List<Vector2> movesBack;
    private Vector2 startPosition;
    private Vector2 currentStartPosition;
    [SerializeField] private int stepCounter = 0;
    [SerializeField] private Vector2 velocityPlayer;
    [SerializeField] private float knockbackForce;

    private bool canChangeCurrentStartPosition = true;
    [SerializeField]private bool changeState = false;
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

        movesBack = new List<Vector2>(); // Assign the array with the length of moves-array. Needed to script work properly!!
        rb = GetComponent<Rigidbody2D>();
        playerRB = GameObject.Find("Player").GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        circleCollider = GetComponent<CircleCollider2D>();
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

        if(loop)
        {
            Vector2 returnVector = new Vector2(0,0);
            for(int i = 0; i < moves.Count; i++)
            {
                returnVector += moves[i];
            }
            moves.Add((startPosition - returnVector) - startPosition);
        }

        if (returningObject)
        {
            for (int i = 0; i < moves.Count; i++)
            {
                movesBack.Add(-moves[moves.Count - i - 1]);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (gizmoPositionChange)
        {
            Gizmos.color = Color.red;
            if (moves[0] != null)
            {
                Vector2 position;
                Gizmos.DrawLine((Vector2)transform.position, (Vector2)transform.position + moves[0]);
                Gizmos.DrawSphere((Vector2)transform.position + moves[0], circleSize);
                position = (Vector2)transform.position + moves[0];
                for (int i = 1; i < moves.Count; i++)
                {

                    Gizmos.DrawLine(position, position + moves[i]);
                    Gizmos.DrawSphere(position, circleSize);
                    position += moves[i];
                }
                if (loop)
                {
                    Gizmos.DrawLine(position, transform.position);
                    Gizmos.DrawSphere(position, circleSize);
                    Gizmos.DrawSphere(transform.position, circleSize);
                }
                else
                    Gizmos.DrawSphere(position, circleSize);
            }
        }
        else
        {
            Gizmos.color = Color.red;
            if (moves[0] != null)
            {
                Vector2 position;
                Gizmos.DrawLine(startPosition, startPosition + moves[0]);
                Gizmos.DrawSphere(startPosition + moves[0], circleSize);
                position = startPosition + moves[0];
                for (int i = 1; i < moves.Count; i++)
                {

                    Gizmos.DrawLine(position, position + moves[i]);
                    Gizmos.DrawSphere(position, circleSize);
                    position += moves[i];
                }
                if (loop)
                {
                    Gizmos.DrawLine(position, startPosition);
                    Gizmos.DrawSphere(position, circleSize);
                    Gizmos.DrawSphere(startPosition, circleSize);
                }
                else
                    Gizmos.DrawSphere(position, circleSize);
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
            rb.useAutoMass = true;
            rb.gravityScale = 3f;
            rb.drag = 0.05f;
            rb.constraints = RigidbodyConstraints2D.None;
            rb.constraints = RigidbodyConstraints2D.FreezePositionX;
            rb.freezeRotation = true;
        }
        if (!changeState)
        {
            if (canChangeCurrentStartPosition)
            {
                currentStartPosition = transform.position;
                canChangeCurrentStartPosition = false;
            }
            Move(); // Moves the object to the designated destination along the given path.
            
        }
        if (changeState && returningObject) // Is the game object meant to return to original position?
        {
            if (canChangeCurrentStartPosition)
            {
                currentStartPosition = transform.position;
                canChangeCurrentStartPosition = false;
            }
            MoveBack(); // Uses the reverse List to return.

        }
        else if (changeState && loop && !destroyAfterPathComplete) // Looped route and not destroyable object?
        {
            changeState = false;
            stepCounter = 0;
        }

        else if (changeState && !loop && destroyAfterPathComplete) // Doesn't loop and is destroyable object?
            Destroy(gameObject);
    }

    // Moves the game object with given Vectors to position. Moves it a one vector at time until the end is reached.
    private void Move()
    {
        rb.velocity = ((((Vector2)transform.position + moves[stepCounter]) - (Vector2)transform.position).normalized) * speed * Time.deltaTime;
        if (ArrivedToDestination(moves[stepCounter]))
        {
            canChangeCurrentStartPosition = true;
            rb.velocity = new Vector2(0, 0);
            stepCounter++;
            if (stepCounter == moves.Count)
            {
                changeState = true;
                stepCounter = 0;
            }
        }
    }

    // Same behaviour as Move(), but in reverse order.
    private void MoveBack()
    {
        rb.velocity = movesBack[stepCounter].normalized * speed * Time.deltaTime;
        if (ArrivedToDestination(movesBack[stepCounter]))
        {
            canChangeCurrentStartPosition = true;
            rb.velocity = new Vector2(0, 0);
            stepCounter++;
            if (stepCounter == moves.Count)
            {
                changeState = false; 
                stepCounter = 0;
            }
        }
    }

    // OverlapCircle to check if the moving object is in the desired position radius. Does not give the best possible result with high GameObject speeds as might not be able to detect the coming object.
    private bool ArrivedToDestination(Vector2 move)
    {
        if (circleCollider == Physics2D.OverlapCircle(currentStartPosition + move, circleSize))
        {
            transform.position = currentStartPosition + move; // Snaps the game object to the exact desired position for better accuracy.
            return true;
        }
        else
            return false;
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

    void PlayerPushback()
    {
        velocityPlayer = new Vector2(playerRB.position.x - transform.position.x > 0 ? knockbackForce * 1 : knockbackForce * -1, 0);
        playerRB.MovePosition(playerRB.position + velocityPlayer * Time.deltaTime);
        //StartCoroutine(KnockbackCooldown());
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Debug.Log(collision.collider.name);
        if (collision.gameObject.tag == "Boss")
        {
            Destroy(gameObject);
        }

        if (collision.gameObject.tag == "Player" && rb.isKinematic == false && rb.velocity.y < -1 && (transform.position.y - playerRB.transform.position.y) > 0)
        {
            PlayerPushback();
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player" && rb.isKinematic == false  && (transform.position.y - playerRB.transform.position.y) > 0)
        {
            PlayerPushback();
        }
    }
}
