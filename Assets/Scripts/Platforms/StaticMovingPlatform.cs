using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class StaticMovingPlatform : MonoBehaviour
{
    private Rigidbody2D rb;
    private Rigidbody2D playerRB;
    private BoxCollider2D boxCollider;
    public SpriteRenderer[] platformRenderers;

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
    [SerializeField] private bool loop = true;
    [SerializeField] private bool destroyAfterPathComplete = true;

    private Vector2 startPosition;
    private Vector2 currentStartPosition;
    [SerializeField] private int stepCounter = 0;

    private bool canChangeCurrentStartPosition = true;
    private bool changeState = false;
    private bool gizmoPositionChange = true;

    private BoxCollider2D playerCollider;
    public float yOffset;
    public Transform leftSideClimbTransform;
    public Transform rightSideClimbTransform;
    private void Awake()
    {
        // Get size of collider to calculate the positions for climbing this platform (should scale with any sizes)
        playerCollider = GameObject.FindGameObjectWithTag("Player").GetComponent<BoxCollider2D>();
        rightSideClimbTransform.localPosition = ((Vector2)gameObject.GetComponent<BoxCollider2D>().size / 2 - Vector2.zero) + (new Vector2(-playerCollider.size.x * (1 / gameObject.transform.localScale.x), playerCollider.size.y * (1 / gameObject.transform.localScale.y)) / 2 + new Vector2(0f, yOffset));

        leftSideClimbTransform.localPosition = ((Vector2)gameObject.GetComponent<BoxCollider2D>().size * new Vector2(-1f, 1f) / 2 - Vector2.zero) + (new Vector2(playerCollider.size.x * (1 / gameObject.transform.localScale.x), playerCollider.size.y * (1 / gameObject.transform.localScale.y)) / 2 + new Vector2(0f, yOffset));
    }

    // Start is called before the first frame update
    void Start()
    {
        movesBack = new List<Vector2>(); // Assign the array with the length of moves-array. Needed to script work properly!!
        rb = GetComponent<Rigidbody2D>();
        playerRB = GameObject.Find("Player").GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        gizmoPositionChange = false;
        startPosition = transform.position;

        // Is this object meant to return the same path back to the beginning? If is, make a reverse array of moves to take.

        if (loop)
        {
            Vector2 returnVector = new Vector2(0, 0);
            for (int i = 0; i < moves.Count; i++)
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
        if (collision.gameObject.tag == "Player" && rb.isKinematic == false && (transform.position.y - playerRB.transform.position.y) > 0)
        {
            PlayerPushback();
        }
    }

    // Return if player will fit on top of moving platform example: moving upward and ceiling would block our climb or we would be squashed
    public bool getWillPlayerFit()
    {
        // Check both positions we should not make boxes that only block on the other side
        if (Physics2D.Raycast((Vector2)rightSideClimbTransform.position + new Vector2(playerCollider.size.x / 2, 0f), new Vector2(0f, 1f), playerCollider.size.y * 0.7f, gameObject.layer)
            && Physics2D.Raycast((Vector2)rightSideClimbTransform.position - new Vector2(playerCollider.size.x / 2, 0f), new Vector2(0f, 1f), playerCollider.size.y * 0.7f, gameObject.layer)
            && Physics2D.Raycast((Vector2)leftSideClimbTransform.position + new Vector2(playerCollider.size.x / 2, 0f), new Vector2(0f, 1f), playerCollider.size.y * 0.7f, gameObject.layer)
            && Physics2D.Raycast((Vector2)leftSideClimbTransform.position - new Vector2(playerCollider.size.x / 2, 0f), new Vector2(0f, 1f), playerCollider.size.y * 0.7f, gameObject.layer))
            return false;

        return true;
    }

    public Vector2 getLeftClimbPos()
    {
        return (Vector2)leftSideClimbTransform.position;
    }

    public Vector2 getRightClimbPos()
    {
        return (Vector2)rightSideClimbTransform.position;
    }
}
