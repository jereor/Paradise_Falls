using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class RiotBossBoxMover : MonoBehaviour
{
    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    [SerializeField] private float speed;
    [SerializeField] private float firstStepTime;
    [SerializeField] private float firstWaitTime;
    [SerializeField] private float secondStepTime;
    [SerializeField] private float secondWaitTime;
    [SerializeField] private float thirdStepTime;
    [SerializeField] private float moveLength;

    private bool changeState = false;
    private bool isWaiting = false;
    private bool isChainCut = false;

    [SerializeField] private Mover state;
    // Start is called before the first frame update
    void Start()
    {
        gameObject.GetComponent<SpriteRenderer>().color = Color.gray;
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        state = Mover.FirstStep;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Debug.Log(state);
        if(gameObject.transform.childCount == 0 && !isChainCut)
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
            state = Mover.Idle;
        }

        switch (state)
        {
            case Mover.Idle:
                break;

            case Mover.FirstStep:
                HandleFirstStep();
                break;

            case Mover.FirstWait:
                HandleFirstWait();
                break;

            case Mover.SecondStep:
                HandleSecondStep();
                break;

            case Mover.SecondWait:
                HandleSecondWait();
                break;

            case Mover.ThirdStep:
                HandleThirdStep();
                break;
        }

    }

    private void HandleFirstStep()
    {
        if(!isWaiting && !changeState)
        {
            Move(moveLength, firstStepTime);
            StartCoroutine(Wait(firstStepTime));
        }
        if (changeState)
        {
            state = Mover.FirstWait;
            changeState = false;
        }

    }

    private void HandleFirstWait()
    {
        if (!isWaiting && !changeState)
        {
            gameObject.GetComponent<SpriteRenderer>().color = Color.white;
            EnableDisableBoxCollider();
            StartCoroutine(Wait(firstWaitTime));
        }
        if (changeState)
        {

            state = Mover.SecondStep;
            changeState = false;
        }
    }

    private void HandleSecondStep()
    {
        if (!isWaiting && !changeState)
        {
            Move(11, secondStepTime);
            StartCoroutine(Wait(secondStepTime));
        }
        if (changeState)
        {
            state = Mover.SecondWait;
            changeState = false;
        }
    }

    private void HandleSecondWait()
    {
        if (!isWaiting && !changeState)
        {
            gameObject.GetComponent<SpriteRenderer>().color = Color.grey;
            EnableDisableBoxCollider();
            StartCoroutine(Wait(secondWaitTime));
        }
        if (changeState)
        {

            state = Mover.ThirdStep;
            changeState = false;
        }
    }

    private void HandleThirdStep()
    {
        if (!isWaiting && !changeState)
        {
            Move(moveLength, thirdStepTime);
            StartCoroutine(Wait(thirdStepTime));
        }
        if (changeState)
        {
            changeState = false;
            Destroy(gameObject);
        }
    }

    private IEnumerator Wait(float waitTime)
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitTime);
        isWaiting = false;
        changeState = true;
    }

    private void Move(float moveValue, float time)
    {
        //rb.AddForce(new Vector2(1 * speed * Time.deltaTime, 0));
        rb.DOMove(new Vector2(transform.position.x + moveValue, transform.position.y), time);
    }

    private void EnableDisableBoxCollider()
    {
        boxCollider.enabled = !boxCollider.enabled;
    }

    public enum Mover
    {
        Idle,
        FirstStep,
        FirstWait,
        SecondStep,
        SecondWait,
        ThirdStep

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
}
