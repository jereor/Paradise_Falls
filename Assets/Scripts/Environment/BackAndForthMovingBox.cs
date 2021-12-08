using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BackAndForthMovingBox : MonoBehaviour
{
    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    public BoxCollider2D knockbackTrigger;

    [Header("Box SpriteRenderers")]
    [SerializeField] private List<SpriteRenderer> boxRenderers = new List<SpriteRenderer>();

    [Header("Chain controller")]
    [SerializeField] private BoxChainController chainController;

    [Header("Knockback trigger")]
    [SerializeField] private BoxKnockback knockbackScript;

    [Header("Speed and waypoint detection Radius")]
    [SerializeField] private float waitTime;
    [SerializeField] private float speed;
    [SerializeField] private float circleSize = 0.15f;

    [Header("Lists to control waypoints")]
    [SerializeField] private List<Vector2> moves;
    [SerializeField] private List<bool> stepsWhenColliderChanged;

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
    private bool canCount = false;
    private bool gizmoPositionChange = true;

    private BoxCollider2D playerCollider;
    public float yOffset;
    public Transform leftSideClimbTransform;
    public Transform rightSideClimbTransform;
    private void Awake()
    {
        // Get size of collider to calculate the positions for climbing this platform (should scale with any sizes)
        playerCollider = GameObject.FindGameObjectWithTag("Player").GetComponent<BoxCollider2D>();
        rightSideClimbTransform.localPosition = ((Vector2)gameObject.GetComponent<BoxCollider2D>().size / 2 - Vector2.zero) + (new Vector2(-playerCollider.size.x * (1 / gameObject.transform.localScale.x), playerCollider.size.y * (1 / gameObject.transform.localScale.y)) / 2 + new Vector2(0f, yOffset) + gameObject.GetComponent<BoxCollider2D>().offset);

        leftSideClimbTransform.localPosition = ((Vector2)gameObject.GetComponent<BoxCollider2D>().size * new Vector2(-1f, 1f) / 2 - Vector2.zero) + (new Vector2(playerCollider.size.x * (1 / gameObject.transform.localScale.x), playerCollider.size.y * (1 / gameObject.transform.localScale.y)) / 2 + new Vector2(0f, yOffset) + gameObject.GetComponent<BoxCollider2D>().offset);
    }


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        gizmoPositionChange = false;
        startPosition = transform.position;
        knockbackTrigger.enabled = false;

        // Is the chain cuttable by melee weapon
        // Is the chain cuttable by melee weapon
        if (!cuttableChain)
        {
            foreach (GameObject chain in chainController.getChains())
            {
                chain.GetComponent<SpriteRenderer>().color = Color.black;
                chain.GetComponent<BoxCollider2D>().enabled = false;
            }
        }
        else
        {
            foreach (GameObject chain in chainController.getChains())
            {
                chain.GetComponent<SpriteRenderer>().color = Color.red;
            }
        }

        // Sets the collider inactive if bool is true.
        if (colliderDisabledAtStart)
        {
            chainController.clawPart.GetComponent<SpriteRenderer>().sortingLayerName = "Interior Near Background";
            chainController.chainMount.GetComponent<SpriteRenderer>().sortingLayerName = "Interior Near Background";
            // Box sprites
            foreach (SpriteRenderer renderer in boxRenderers)
            {
                renderer.color = Color.grey;
                renderer.sortingLayerName = "Interior Near Background";
            }
            boxCollider.enabled = false;
            // Chains
            foreach (GameObject chain in chainController.getChains())
            {
                chain.GetComponent<SpriteRenderer>().sortingLayerName = "Interior Near Background";
                chain.GetComponent<SpriteRenderer>().color = Color.grey;
                chain.GetComponent<BoxCollider2D>().enabled = false;
            }
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
        if (chainController.getIfCut() && !isChainCut && cuttableChain)
        {
            // If chain is cut idle sound should not play anymore
            if (TryGetComponent(out AudioSourceMute script))
            {
                script.playIdleSound = false;
                script.ToggleLoop(false);
            }

            isChainCut = true;
            StopAllCoroutines();
            rb.isKinematic = false; // Change the rigidbody to dynamic and set the parameters.
            rb.velocity = new Vector2(0, 0);
            rb.useAutoMass = true;
            rb.gravityScale = 3f;
            rb.drag = 0.05f;
            rb.constraints = RigidbodyConstraints2D.None;
            rb.constraints = RigidbodyConstraints2D.FreezePositionX;
            rb.freezeRotation = true;

            knockbackTrigger.enabled = true;

            gameObject.tag = "Box";
            knockbackScript.setFalling(true);
        }

        if (!isChainCut) // Prevents the script to progress if chain is cut.
        {
            if (returning)
                HandleReturnStep();

            if (canStep && !returning)
                HandleStep();
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
        canCount = true;
        canChangeCurrentStartPosition = true;
        isWaiting = false;
        changeState = true;
    }

    // Moves the object with rigidbody velocity to the next waypoint direction.
    private void Move(Vector2 move)
    {
        rb.velocity = move.normalized * speed * Time.deltaTime;
        if (Vector2.Distance(currentStartPosition + moves[stepCounter], (Vector2)transform.position) < circleSize)
        {
            transform.position = currentStartPosition + moves[stepCounter];
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
                canCount = true;
            }           
        }
    }

    // Moves the object back to it's original position.
    private void MoveBack(Vector2 move)
    {
        rb.velocity = move.normalized * speed * Time.deltaTime;
        if (Vector2.Distance(startPosition - currentStartPosition, (Vector2)transform.position) < circleSize)
        {
            transform.position = startPosition - currentStartPosition;
            rb.velocity = new Vector2(0, 0);
            canChangeCurrentStartPosition = true;
            changeState = true;
        }
    }

    // Enable or disable the collider.
    private void EnableDisableBoxCollider()
    {
        boxCollider.enabled = !boxCollider.enabled;
        if(boxCollider.enabled && cuttableChain)
        {
            chainController.clawPart.GetComponent<SpriteRenderer>().sortingLayerName = "Interior Foreground";
            chainController.chainMount.GetComponent<SpriteRenderer>().sortingLayerName = "Interior Foreground";
            chainController.chainMount.GetComponent<SpriteRenderer>().sortingOrder = 1;
            // Box sprites
            foreach (SpriteRenderer renderer in boxRenderers)
            {
                renderer.color = Color.white;
                renderer.sortingLayerName = "Interior Foreground";
            }
            // Chains
            foreach (GameObject chain in chainController.getChains())
            {
                if (chain != null)
                {
                    chain.GetComponent<SpriteRenderer>().sortingLayerName = "Interior Foreground";
                    chain.GetComponent<SpriteRenderer>().color = Color.red;
                    chain.GetComponent<BoxCollider2D>().enabled = true;
                }
            }
        }
        else if(boxCollider.enabled && !cuttableChain)
        {
            chainController.clawPart.GetComponent<SpriteRenderer>().sortingLayerName = "Interior Foreground";
            chainController.chainMount.GetComponent<SpriteRenderer>().sortingLayerName = "Interior Foreground";
            chainController.chainMount.GetComponent<SpriteRenderer>().sortingOrder = 1;
            // Box sprites
            foreach (SpriteRenderer renderer in boxRenderers)
            {
                renderer.color = Color.white;
                renderer.sortingLayerName = "Interior Foreground";
            }
            // Chains
            foreach (GameObject chain in chainController.getChains())
            {
                if (chain != null)
                {
                    chain.GetComponent<SpriteRenderer>().sortingLayerName = "Interior Foreground";
                    chain.GetComponent<SpriteRenderer>().color = Color.black;
                    chain.GetComponent<BoxCollider2D>().enabled = false;
                }
            }
        }
        else
        {
            chainController.clawPart.GetComponent<SpriteRenderer>().sortingLayerName = "Interior Near Background";
            chainController.chainMount.GetComponent<SpriteRenderer>().sortingLayerName = "Interior Near Background";
            chainController.chainMount.GetComponent<SpriteRenderer>().sortingOrder = 1;
            // Box sprites
            foreach (SpriteRenderer renderer in boxRenderers)
            {
                renderer.color = Color.grey;
                renderer.sortingLayerName = "Interior Near Background";
            }
            // Chains
            foreach (GameObject chain in chainController.getChains())
            {
                if (chain != null)
                {
                    chain.GetComponent<SpriteRenderer>().sortingLayerName = "Interior Near Background";
                    chain.GetComponent<SpriteRenderer>().color = Color.grey;
                    chain.GetComponent<BoxCollider2D>().enabled = false;
                }
            }
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

    //private void OnCollisionEnter2D(Collision2D collision)
    //{
    //    if (collision.gameObject.tag == "Boss" && rb.isKinematic == false)
    //    {
    //        Destroy(gameObject);
    //    }
    //}

    // Handles all the steps the box takes during it's adventure.
    private void HandleStep()
    {
        //if(stepCounter == moves.Count - 1)
        //{
        //    returning = true;
        //}
        if (!changeState && !isWaiting && !returning) // Is the platform in moving state and not waiting?
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
                stepCounter = 0;
                //returning = true;
            }
            else // If not looped, shut the script. We done here.
                shutScript = true;

        }
        else if (changeState) // Starts the movement to the next waypoint since script was not in the end yet.
        {
            //returning = false;
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
            Debug.Log(startPosition - currentStartPosition);
            MoveBack(startPosition - currentStartPosition); // Moves the platform back to original position.
        }
        if (changeState)
        {
            returning = false; // When at destination, not returning anymore.
            stepCounter = 0; // Reset the stepCounter so it starts from beginning again.
            changeState = false;
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
        return leftSideClimbTransform.position;
    }

    public Vector2 getRightClimbPos()
    {
        return rightSideClimbTransform.position;
    }
}
