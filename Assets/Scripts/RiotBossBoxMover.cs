using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class RiotBossBoxMover : MonoBehaviour
{
    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    [SerializeField] private float firstStepTime;
    [SerializeField] private float secondWaitTime;
    [SerializeField] private Vector2[] moves;
    private Vector2 startPosition;
    [SerializeField] private int stepCounter = 0;


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
    void Start()
    {

        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        gizmoPositionChange = false;
        startPosition = transform.position;
        if(colliderDisabledAtStart)
        {
            gameObject.GetComponent<SpriteRenderer>().color = Color.gray;
            boxCollider.enabled = !boxCollider.enabled;
        }

    }

    private void OnDrawGizmosSelected()
    {
        if(gizmoPositionChange)
        {
            Gizmos.color = Color.red;
            if (moves[0] != null)
            {
                Vector2 position;
                Gizmos.DrawLine((Vector2)transform.position, (Vector2)transform.position + moves[0]);
                Gizmos.DrawSphere((Vector2)transform.position + moves[0], 0.5f);
                position = (Vector2)transform.position + moves[0];
                for (int i = 1; i < moves.Length; i++)
                {

                    Gizmos.DrawLine(position, position + moves[i]);
                    Gizmos.DrawSphere(position, 0.4f);
                    position += moves[i];
                }
                if(loop)
                {
                    Gizmos.DrawLine(position, transform.position);
                    Gizmos.DrawSphere(position, 0.4f);
                    Gizmos.DrawSphere(transform.position, 0.4f);
                }
            }
        }
        else
        {
            Gizmos.color = Color.red;
            if (moves[0] != null)
            {
                Vector2 position;
                Gizmos.DrawLine(startPosition, startPosition + moves[0]);
                Gizmos.DrawSphere(startPosition + moves[0], 0.5f);
                position = startPosition + moves[0];
                for (int i = 1; i < moves.Length; i++)
                {

                    Gizmos.DrawLine(position, position + moves[i]);
                    Gizmos.DrawSphere(position, 0.4f);
                    position += moves[i];
                }
                if (loop)
                {
                    Gizmos.DrawLine(position, startPosition);
                    Gizmos.DrawSphere(position, 0.4f);
                    Gizmos.DrawSphere(startPosition, 0.4f);
                }
            }
        }

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(gameObject.transform.childCount == 0 && !isChainCut && cuttableChain)
        {
            Debug.Log("test");
            isChainCut = true;
            DOTween.Kill(rb);
            StopAllCoroutines();
            rb.velocity = new Vector2(0, 0);
            rb.gravityScale = 3f;
            rb.constraints = RigidbodyConstraints2D.None;
            rb.constraints = RigidbodyConstraints2D.FreezePositionX;
            rb.freezeRotation = true;
        }
        if(!shutScript)
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
        //rb.AddForce(new Vector2(1 * speed * Time.deltaTime, 0));
        rb.DOMove((Vector2)transform.position + move, time);
        stepCounter++;
    }

    private void EnableDisableBoxCollider()
    {
        boxCollider.enabled = !boxCollider.enabled;
    }

    public bool getIsBoxColliderEnabled()
    {
        return boxCollider.isActiveAndEnabled;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Debug.Log(collision.collider.name);
        if (collision.gameObject.tag == "Boss")
        {
            Destroy(gameObject);
        }
    }

    private void HandleWait()
    {
        if (!isWaiting && !changeState)
        {


            if (comesAndGoesFromBackground)
            {
                if (boxCollider.enabled)
                    gameObject.GetComponent<SpriteRenderer>().color = Color.grey;
                else
                    gameObject.GetComponent<SpriteRenderer>().color = Color.white;

                EnableDisableBoxCollider();
            }

            StartCoroutine(Wait(secondWaitTime));
        }
        if (changeState && stepCounter == moves.Length && !returning)
        {
            if (destroyAfterPathComplete)
                Destroy(gameObject);
            else if (loop)
            {
                changeState = false;
                returning = true;
            }
            else
                shutScript = true;

        }
        else if (changeState)
        {
            returning = false;
            changeState = false;
            canStep = true;

        }          
    }

    private void HandleStep()
    {
        if (!isWaiting && !changeState)
        {
            if (moves[stepCounter] == null)
            {
                return;
            }
            Move(moves[stepCounter], firstStepTime);
            StartCoroutine(Wait(firstStepTime));
        }
        if (changeState)
        {

            changeState = false;
            canStep = false;
        }
    }

    private void HandleReturnStep()
    {
        if (!isWaiting && !changeState)
        {
            rb.DOMove(startPosition, firstStepTime);
            StartCoroutine(Wait(firstStepTime));
        }
        if (changeState)
        {
            returning = false;
            canStep = false;
            stepCounter = 0;
            changeState = false;
        }
    }
}
