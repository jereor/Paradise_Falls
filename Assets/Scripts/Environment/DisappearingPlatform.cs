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

        if (enabledOnStart)
        {
            boxCollider.enabled = true;
            spriteRenderer.sortingLayerName = "Interior Foreground";
            transitionAnimator.SetBool("AppearedOnStartBlock", true);
        }
        else
        {
            boxCollider.enabled = false;
            spriteRenderer.sortingLayerName = "Interior Near Background";
            transitionAnimator.SetBool("DisappearedOnStartBlock", true);
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
            if(boxCollider.enabled)
            {
                transitionAnimator.SetBool("Disappear", true);
                transitionAnimator.SetBool("Appear", false);
            }
            else
            {
                transitionAnimator.SetBool("Appear", true);
                transitionAnimator.SetBool("Disappear", false);
            }

            counter = 0;
        }
    }

    // Enable or disable the collider.
    public void EnableDisableBoxCollider()
    {
        if(boxCollider.enabled)
            spriteRenderer.sortingLayerName = "Interior Near Background";
        else
            spriteRenderer.sortingLayerName = "Interior Foreground";

        boxCollider.enabled = !boxCollider.enabled;
    }

    public void Work()
    {
        if (boxCollider.enabled)
        {
            transitionAnimator.SetBool("Disappear", true);
            transitionAnimator.SetBool("Appear", false);
        }
        else
        {
            transitionAnimator.SetBool("Appear", true);
            transitionAnimator.SetBool("Disappear", false);
        }
    }

    public void RiotRoomWork()
    {
        if (gameObject.activeInHierarchy)
        {
            transitionAnimator.SetBool("Disappear", true);
            transitionAnimator.SetBool("Appear", false);
            StartCoroutine(Transition(1));
        }
        else
        {
            gameObject.SetActive(true);
            transitionAnimator.SetBool("Appear", true);
            transitionAnimator.SetBool("Disappear", false);

        }
    }

    private IEnumerator Transition(float waitT)
    {
        yield return new WaitForSeconds(waitT);
        gameObject.SetActive(false);
    }
}
