using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class StartSequenceController : MonoBehaviour
{
    [SerializeField] private GameObject additionalDisappearingGround;
    private Player playerControls;
    private Health health;
    private bool canStartBossFight = true;
    // Start is called before the first frame update
    void Start()
    {
        additionalDisappearingGround = GameObject.Find("AdditionalDisappearingGround");
        playerControls = GameObject.Find("Player").GetComponent<Player>();
        health = GetComponent<Health>();
    }

    // Update is called once per frame
    void Update()
    {
        if(health.GetHealth() <= 0 && canStartBossFight)
        {
            StartCoroutine(StartBossFight());
            canStartBossFight = false;
        }
    }

    private IEnumerator StartBossFight()
    {
        playerControls.HandleAllPlayerControlInputs(false);
        transform.DOMove(new Vector2(transform.position.x, transform.position.y - 4), 2);
        yield return new WaitForSeconds(3);
        gameObject.GetComponent<SpriteRenderer>().enabled = false;
        // Screen shakeeeeeee
        PlayerCamera.Instance.CameraShake(1f, 3);
        yield return new WaitForSeconds(3);
        // Ground collapse
        additionalDisappearingGround.SetActive(false);
        yield return new WaitForSeconds(2);
        playerControls.HandleAllPlayerControlInputs(true);
    }

}
