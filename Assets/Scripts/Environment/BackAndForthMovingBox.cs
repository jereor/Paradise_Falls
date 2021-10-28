using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BackAndForthMovingBox : MonoBehaviour
{
    private Rigidbody2D rb;
    private Rigidbody2D playerRB;
    private BoxCollider2D boxCollider;
    private Transform chain;
    [SerializeField] private float stepTime;
    [SerializeField] private float waitTime;
    [SerializeField] private Vector2[] moves;
    private Vector2 startPosition;
    [SerializeField] private int stepCounter = 0;
    [SerializeField] private Vector2 velocityPlayer;
    [SerializeField] private float knockbackForce;


    private bool changeState = false;
    private bool canStep = true;
    private bool returning = false;
    private bool shutScript = false;

    private bool isWaiting = false;
    private bool isChainCut = false;
    [SerializeField] private bool colliderDisabledAtStart = false;
    [SerializeField] private bool comesAndGoesFromBackground = false;
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

        rb = GetComponent<Rigidbody2D>();
        playerRB = GameObject.Find("Player").GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
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
            boxCollider.enabled = !boxCollider.enabled;
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
                Gizmos.DrawSphere((Vector2)transform.position + moves[0], 0.2f);
                position = (Vector2)transform.position + moves[0];
                for (int i = 1; i < moves.Length; i++)
                {

                    Gizmos.DrawLine(position, position + moves[i]);
                    Gizmos.DrawSphere(position, 0.2f);
                    position += moves[i];
                }
                if (loop)
                {
                    Gizmos.DrawLine(position, transform.position);
                    Gizmos.DrawSphere(position, 0.2f);
                    Gizmos.DrawSphere(transform.position, 0.2f);
                }
                else
                    Gizmos.DrawSphere(position, 0.2f);
            }
        }
        else
        {
            Gizmos.color = Color.red;
            if (moves[0] != null)
            {
                Vector2 position;
                Gizmos.DrawLine(startPosition, startPosition + moves[0]);
                Gizmos.DrawSphere(startPosition + moves[0], 0.2f);
                position = startPosition + moves[0];
                for (int i = 1; i < moves.Length; i++)
                {

                    Gizmos.DrawLine(position, position + moves[i]);
                    Gizmos.DrawSphere(position, 0.2f);
                    position += moves[i];
                }
                if (loop)
                {
                    Gizmos.DrawLine(position, startPosition);
                    Gizmos.DrawSphere(position, 0.2f);
                    Gizmos.DrawSphere(startPosition, 0.2f);
                }
                else
                    Gizmos.DrawSphere(position, 0.2f);
            }
        }

    }
    private void Update()
    {

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
            else if (!canStep && !returning)
                HandleWait();

            if (returning)
                HandleReturnStep();
        }
    }

    private IEnumerator Wait(float waitTime)
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitTime);
        isWaiting = false;
        changeState = true;
    }

    private void Move(Vector2 move, float time)
    {
        rb.DOMove((Vector2)transform.position + move, time);


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


    private void HandleWait()
    {
        if (!isWaiting && !changeState)
        {

            // Change the box and chain's color when enabled or disabled.
            if (comesAndGoesFromBackground)
            {
                if (boxCollider.enabled)
                {
                    gameObject.GetComponent<SpriteRenderer>().color = Color.grey;
                    transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.grey;
                }
                else if (!cuttableChain)
                {
                    gameObject.GetComponent<SpriteRenderer>().color = Color.white;
                    transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.black;
                }
                else if(cuttableChain)
                {
                    gameObject.GetComponent<SpriteRenderer>().color = Color.white;
                    transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.red;
                }

                EnableDisableBoxCollider();
            }

            StartCoroutine(Wait(waitTime)); // Waits the given time until moves again.
        }

        if (changeState && stepCounter == moves.Length && !returning)
        {
            if (destroyAfterPathComplete) // If the object needs to be destroyed after the path is complete.
                Destroy(gameObject);
            else if (loop) // In other cases loops around the given parameters.
            {
                changeState = false;
                returning = true;
            }
            else // If not looped, shut the script.
                shutScript = true;

        }
        else if (changeState) // Object is made to return to startPosition after getting to destination.
        {
            returning = false;
            changeState = false;
            canStep = true;

        }
    }

    // Handles all the steps the box takes during it's adventure.
    private void HandleStep()
    {
        if (!isWaiting && !changeState)
        {
            //if (moves[stepCounter] == null)
            //{
            //    return;
            //}
            StartCoroutine(Wait(stepTime));
            Move(moves[stepCounter], stepTime); // Moves the object to next waypoint.
            Debug.Log(stepCounter);
            stepCounter++;
        }
        if (changeState) // Checks if the step is done.
        {

            changeState = false;
            canStep = false;
        }
    }

    // Own function for handling the return to startPosition.
    private void HandleReturnStep()
    {
        if (!isWaiting && !changeState)
        {
            Move(startPosition, stepTime);
            StartCoroutine(Wait(stepTime));
        }
        if (changeState)
        {
            returning = false; // When at destination, not returning anymore.
            canStep = false;
            stepCounter = 0; // Reset the stepCounter so it starts from beginning again.
            changeState = false;
        }
    }
}
