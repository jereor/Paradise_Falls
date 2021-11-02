using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BackAndForthMovingBox : MonoBehaviour
{
    private Rigidbody2D rb;
    private Rigidbody2D playerRB;
    private BoxCollider2D boxCollider;
    private CircleCollider2D circleCollider;
    private Transform chain;

    [Header("Speed and waypoint detection Radius")]
    [SerializeField] private float waitTime;
    [SerializeField] private float speed;
    [SerializeField] private float circleSize = 0.15f;

    [Header("Lists to control waypoints")]
    [SerializeField] private List<Vector2> moves;
    [SerializeField] private List<bool> stepsWhenColliderChanged;

    [Header("Player knockback control")]
    [SerializeField] private Vector2 velocityPlayer;
    [SerializeField] private float knockbackForce;

    [Header("Bools to control Platform Movement")]
    [SerializeField] private bool colliderDisabledAtStart = false;
    [SerializeField] private bool cuttableChain = false;
    [SerializeField] private bool loop = true;
    [SerializeField] private bool teleportToStartAfterPathComplete = true;

    private Vector2 startPosition;
    private Vector2 currentStartPosition;
    private int stepCounter = 0;

    private bool canChangeCurrentStartPosition = true;
    private bool changeState = false;
    private bool canStep = true;
    private bool returning = false;
    private bool shutScript = false;
    private bool isWaiting = false;
    private bool isChainCut = false;
    private bool gizmoPositionChange = true;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerRB = GameObject.Find("Player").GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        circleCollider = GetComponent<CircleCollider2D>();
        chain = GetComponentInChildren<Transform>();
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
            gameObject.GetComponent<SpriteRenderer>().color = Color.grey;
            transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.grey;
            boxCollider.enabled = false;
        }

        if (loop && !teleportToStartAfterPathComplete)
        {
            Vector2 returnVector = new Vector2(0, 0);
            for (int i = 0; i < moves.Count; i++)
            {
                returnVector += moves[i];
            }
            moves.Add((startPosition - returnVector) - startPosition);
            stepsWhenColliderChanged.Add(false);
        }
    }

    // Gizmos for the path the object takes.
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
                if (loop && !teleportToStartAfterPathComplete)
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
                if (loop && !teleportToStartAfterPathComplete)
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
            Debug.Log("test");
            isChainCut = true;

            rb.isKinematic = false; // Change the rigidbody to dynamic and set the parameters.
            rb.velocity = new Vector2(0, 0);
            rb.useAutoMass = true;
            rb.gravityScale = 3f;
            rb.drag = 0.05f;
            rb.constraints = RigidbodyConstraints2D.None;
            rb.constraints = RigidbodyConstraints2D.FreezePositionX;
            rb.freezeRotation = true;
            DOTween.Kill(rb); // Stops all Tweenings so the object doesn't move after the chain is cut.
            StopAllCoroutines();
        }

        if (!shutScript || chain != null || gameObject.transform.childCount != 0) // Prevents the script to progress if chain is cut.
        {
            if (canStep && !returning)
                HandleStep();

            if (returning)
                HandleReturnStep();
        }
    }

    private IEnumerator Wait(float waitTime)
    {
        isWaiting = true;
        rb.velocity = new Vector2(0, 0);
        yield return new WaitForSeconds(waitTime);
        EnableDisableBoxCollider();
        yield return new WaitForSeconds(waitTime);
        stepCounter++;
        canChangeCurrentStartPosition = true;
        isWaiting = false;
        changeState = true;
    }

    // Moves the object with rigidbody velocity to the next waypoint direction.
    private void Move(Vector2 move)
    {
        rb.velocity = move.normalized * speed * Time.deltaTime;
        if (ArrivedToDestination(move))
        {

            if (stepsWhenColliderChanged[stepCounter])
            {

                StartCoroutine(Wait(waitTime));
            }
            else
            {
                changeState = true;
                rb.velocity = new Vector2(0, 0);
                stepCounter++;
                canChangeCurrentStartPosition = true;
            }           
        }
    }

    // Moves the object back to it's original position.
    private void MoveBack(Vector2 move)
    {
        rb.velocity = move.normalized * speed * Time.deltaTime;
        if (ArrivedToDestination(move))
        {
            rb.velocity = new Vector2(0, 0);
            canChangeCurrentStartPosition = true;
            changeState = true;
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
        if(boxCollider.enabled && cuttableChain)
        {
            gameObject.GetComponent<SpriteRenderer>().color = Color.white;
            transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.red;
        }
        else if(boxCollider.enabled && !cuttableChain)
        {
            gameObject.GetComponent<SpriteRenderer>().color = Color.white;
            transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.black;
        }
        else
        {
            gameObject.GetComponent<SpriteRenderer>().color = Color.grey;
            transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.grey;
        }
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
        if (collision.gameObject.tag == "Boss" && rb.isKinematic == false)
        {
            Destroy(gameObject);
        }
        if(collision.gameObject.tag == "Player" && rb.isKinematic == false && rb.velocity.y < -1 && (transform.position.y - playerRB.transform.position.y) > 0)
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

    // Handles all the steps the box takes during it's adventure.
    private void HandleStep()
    {
        if (!changeState && !isWaiting) // Is the platform in moving state and not waiting?
        {
            if (canChangeCurrentStartPosition) // Changes the "point of view" for the object so it gets the right vector for next waypoint.
            {
                currentStartPosition = transform.position;
                canChangeCurrentStartPosition = false;
            }
            Move(moves[stepCounter]); // Moves the object to next waypoint.

        }
        // Everything regarding the end possibilities of the object.
        if (changeState && stepCounter == moves.Count && !returning)
        {
            // Disable the boxCollider for the gameobject for start. Disabling done here since no other viable solution for the disabling was found.
            if (boxCollider.enabled)
                EnableDisableBoxCollider();

            if (teleportToStartAfterPathComplete) // If the object needs to be teleported back to original position after the path is complete.
            {
                transform.position = startPosition;
                if (loop) // If object is supposed to loop, start from the beginning.
                {
                    changeState = false;
                    canStep = true;
                    stepCounter = 0;
                }
            }

            else if (loop && !teleportToStartAfterPathComplete) // In other cases loops around the given parameters. Does not teleport but moves to the original pos.
            {
                changeState = false;
                returning = true;
            }
            else // If not looped, shut the script. We done here.
                shutScript = true;

        }
        else if (changeState) // Starts the movement to the next waypoint since script was not in the end yet.
        {
            returning = false;
            changeState = false;
            canStep = true;

        }
    }

    // Own function for handling the return to startPosition.
    private void HandleReturnStep()
    {
        if (!changeState)
        {
            if (canChangeCurrentStartPosition)
            {
                currentStartPosition = transform.position;
                canChangeCurrentStartPosition = false;
            }
            MoveBack(startPosition - currentStartPosition); // Moves the platform back to original position.
        }
        if (changeState)
        {
            returning = false; // When at destination, not returning anymore.
            stepCounter = 0; // Reset the stepCounter so it starts from beginning again.
            changeState = false;
        }
    }
}
