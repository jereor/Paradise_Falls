using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisappearingPlatform : MonoBehaviour
{
    private Collider2D boxCollider;
    private SpriteRenderer spriteRenderer;

    //[SerializeField] private Sprite transitionSprite;
    [SerializeField] private Sprite appearedBlock;
    [SerializeField] private Sprite disappearedBlock;

    [SerializeField] private Animator transitionAnimator;

    [SerializeField] private bool onTimer;
    [SerializeField] private bool enabledOnStart;

    private float counter;
    [SerializeField] private float timeBeforeTransition;

    // Start is called before the first frame update
    void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        transitionAnimator = GetComponent<Animator>();

        if(enabledOnStart)
        {
            boxCollider.enabled = true;
        }
        else
        {
            boxCollider.enabled = false;
            //spriteRenderer.color = Color.grey;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(onTimer)
        {
            counter += Time.deltaTime;
        }

        if(counter > timeBeforeTransition && onTimer)
        {
            EnableDisableBoxCollider();

            transitionAnimator.Play("DisappearingPlatform");

            if (boxCollider.enabled == true)
                spriteRenderer.sprite = appearedBlock;
            else if (boxCollider.enabled == false)
                spriteRenderer.sprite = disappearedBlock;

            counter = 0;
        }
    }

    // Enable or disable the collider.
    public void EnableDisableBoxCollider()
    {
        boxCollider.enabled = !boxCollider.enabled;
        //if (boxCollider.enabled)
        //{
        //    gameObject.GetComponent<SpriteRenderer>().color = Color.white;
        //}
        //else
        //{
        //    gameObject.GetComponent<SpriteRenderer>().color = Color.grey;
        //}
    }
}
