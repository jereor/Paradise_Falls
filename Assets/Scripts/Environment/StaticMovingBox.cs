using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class StaticMovingBox : MonoBehaviour
{
    private Rigidbody2D rb;
    private Rigidbody2D playerRB;
    private BoxCollider2D boxCollider;
    //private CircleCollider2D circleCollider;

    //public Vector2 velocity = Vector2.zero;
    //private Vector2 _distance;
    //private Vector2 _oldPosition;
    //private PlayerCollision _player;
    //private bool _playerIsOnTop;
    //private float time;

    [Header("Speed and waypoint detection Radius")]
    [SerializeField] private float speed;
    [SerializeField] private float circleSize = 0.15f;

    [Header("Lists to control waypoints")]
    [SerializeField] private List<Vector2> moves;
    [SerializeField] private List<Vector2> movesBack;

    [Header("Player knockback control")]
    [SerializeField] private Vector2 velocityPlayer;
    [SerializeField] private float knockbackForce;

    [Header("Bools to control Platform Movement")]
    [SerializeField] private bool returningObject = false;
    [SerializeField] private bool colliderDisabledAtStart = false;
    [SerializeField] private bool cuttableChain = false;
    [SerializeField] private bool loop = true;
    [SerializeField] private bool destroyAfterPathComplete = true;

    private Vector2 startPosition;
    private Vector2 currentStartPosition;
    [SerializeField]private int stepCounter = 0;

    private bool canChangeCurrentStartPosition = true;
    private bool changeState = false;
    private bool gizmoPositionChange = true;
    //private bool isWaiting = false;
    private bool isChainCut = false;

    private BoxCollider2D playerCollider;
    public float yOffset;
    private Vector2 leftSideClimbPos;
    private Vector2 rightSideClimbPos;
    //public GameObject test;
    //public GameObject test2;
    private void Awake()
    {
        playerCollider = GameObject.FindGameObjectWithTag("Player").GetComponent<BoxCollider2D>();
        rightSideClimbPos = ((Vector2)gameObject.GetComponent<BoxCollider2D>().size / 2 - Vector2.zero) + (new Vector2(-playerCollider.size.x * (1 / gameObject.transform.localScale.x), playerCollider.size.y * (1 / gameObject.transform.localScale.y)) / 2 + new Vector2(0f, yOffset));
        //test.transform.localPosition = rightSideClimbPos;

        leftSideClimbPos = ((Vector2)gameObject.GetComponent<BoxCollider2D>().size * new Vector2(-1f,1f) / 2 - Vector2.zero) + (new Vector2(playerCollider.size.x * (1 / gameObject.transform.localScale.x), playerCollider.size.y * (1 / gameObject.transform.localScale.y)) / 2 + new Vector2(0f, yOffset));
        //test2.transform.localPosition = leftSideClimbPos;
    }

    // Start is called before the first frame update
    void Start()
    {
        movesBack = new List<Vector2>(); // Assign the array with the length of moves-array. Needed to script work properly!!
        rb = GetComponent<Rigidbody2D>();
        playerRB = GameObject.Find("Player").GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        //circleCollider = GetComponent<CircleCollider2D>();
        gizmoPositionChange = false;
        startPosition = transform.position;
        //_oldPosition = rb.position;

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
        // Everything related to chain control. Comment this if-case if not needed.
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
        if (!changeState) // Moves the object through the waypoints without stopping.
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
        rb.velocity = ((moves[stepCounter]).normalized) * speed * Time.fixedDeltaTime; // Changes velocity to move the object.
        if (Vector2.Distance(currentStartPosition + moves[stepCounter], (Vector2)transform.position) < circleSize)
        {
            transform.position = currentStartPosition + moves[stepCounter];
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
        rb.velocity = movesBack[stepCounter].normalized * speed * Time.deltaTime; // Changes velocity to move the object.
        if (Vector2.Distance(currentStartPosition + movesBack[stepCounter], (Vector2)transform.position) < circleSize)
        {
            transform.position = currentStartPosition + movesBack[stepCounter];
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

    public bool getWillPlayerFit()
    {
        if (Physics2D.OverlapBox(rightSideClimbPos, playerCollider.size * 1.5f, 0f, gameObject.layer) || Physics2D.OverlapBox(leftSideClimbPos, new Vector2(-1f,1) * playerCollider.size * 1.5f, 0f, gameObject.layer))
            return false;

        return true;
    }

    public Vector2 getLeftClimbPos()
    {
        return leftSideClimbPos;
    }

    public Vector2 getRightClimbPos()
    {
        return rightSideClimbPos;
    }

    //private void OnCollisionEnter2D(Collision2D other)
    //{
    //    if (!other.collider.CompareTag("Player")) return;
    //    // only when player is on top of the platform
    //    if (!(Vector3.Dot(other.contacts[0].normal, Vector3.down) > 0.5)) return;
    //    // some caching
    //    if (_player == null)
    //    {
    //        // get whatever component used to able to access your player
    //        _player = other.transform.GetComponent<PlayerCollision>();
    //    }

    //    _playerIsOnTop = true;
    //}

    //private void OnCollisionExit2D(Collision2D other)
    //{
    //    if (!other.collider.CompareTag("Player")) return;
    //    _playerIsOnTop = false;
    //}
}
