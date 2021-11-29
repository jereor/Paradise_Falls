using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class AttackVineController : MonoBehaviour
{
    private BigPlantController plantController;

    private GameObject plantBoss;

    private GameObject target;
    private Rigidbody2D targetRB;

    private Vector2 velocityPlayer;

    [SerializeField] private float attackVineStretchDuration;
    [SerializeField] private float attackVineRotateDuration;
    [SerializeField] private float attackVineMoveDuration;
    [SerializeField] private float attackVineWaitTime;
    [SerializeField] private float attackVineStretchAmount;
    [SerializeField] private float vineSpeed;

    public bool isAttackVineActivated = false;
    public bool isRotatingTowardsTarget = false;

    [SerializeField] private float knockbackForce;
    private bool knockbackOnCooldown = false;
    // Start is called before the first frame update
    void Start()
    {
        plantBoss = GameObject.Find("PlantBoss");
        plantController = plantBoss.GetComponent<BigPlantController>();
        target = GameObject.Find("Player");
        targetRB = target.GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isAttackVineActivated)
        {
            StartCoroutine(SpawnVine());
            transform.rotation = new Quaternion(0, 0, -180, 0);
            isRotatingTowardsTarget = true;
        }

        if (isRotatingTowardsTarget)
            RotateVineTowardsTheTarget();
    }

    // Cooldown for the player knockback.
    private IEnumerator KnockbackCooldown()
    {
        knockbackOnCooldown = true;
        yield return new WaitForSeconds(0.5f);
        knockbackOnCooldown = false;
    }

    // Pushbacks the player when hit with riot drone collider. Uses velocity for the knockback instead of force.
    public void PlayerPushback()
    {
        velocityPlayer = new Vector2(target.transform.position.x - transform.position.x > 0 ? knockbackForce * 1 : knockbackForce * -1, knockbackForce / 3);
        targetRB.MovePosition(targetRB.position + velocityPlayer * Time.deltaTime);
        StartCoroutine(KnockbackCooldown());
    }

    private void RotateVineTowardsTheTarget()
    {
        // Rotating object to point player
        Vector3 vectorToTarget = target.transform.position - transform.position;
        float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg;
        Quaternion q = Quaternion.AngleAxis(angle - 90, Vector3.forward);
        transform.rotation = Quaternion.Slerp(transform.rotation, q, Time.deltaTime);
    }

    private IEnumerator SpawnVine()
    {
        vineSpeed = plantController.GetVineSpeed();
        isAttackVineActivated = true;
        transform.position = new Vector2(Random.Range(plantBoss.transform.position.x - 5, plantBoss.transform.position.x + 5), plantBoss.transform.position.y + 15);
        transform.DOMoveY(transform.position.y - 5, attackVineMoveDuration * vineSpeed);
        yield return new WaitForSeconds(attackVineRotateDuration * vineSpeed);
        gameObject.GetComponent<SpriteRenderer>().color = Color.red;
        isRotatingTowardsTarget = false;
        Vector2 attackDirection = target.transform.position;
        yield return new WaitForSeconds(attackVineWaitTime * vineSpeed);
        transform.DOScaleY(attackVineStretchAmount, attackVineStretchDuration * vineSpeed);
        yield return new WaitForSeconds(attackVineStretchDuration * vineSpeed);
        transform.DOScaleY(1, attackVineStretchDuration * vineSpeed);
        yield return new WaitForSeconds(attackVineStretchDuration * vineSpeed);
        gameObject.GetComponent<SpriteRenderer>().color = Color.white;
        transform.DOMoveY(transform.position.y + 5, attackVineMoveDuration * vineSpeed);

        yield return new WaitForSeconds(attackVineMoveDuration * vineSpeed);
        isAttackVineActivated = false;
        plantController.attackVineInstances.Remove(gameObject);
        Destroy(gameObject);
    }

    //private void OnCollisionEnter2D(Collision2D collision)
    //{
    //    if (collision.collider.gameObject.name == "PlantBoss")
    //    {
    //        Debug.Log("OUCH!");
    //        plantController.state = BigPlantController.PlantState.Stunned;
    //    }

    //    if (collision.collider.gameObject.name == "Player")
    //    {
    //        PlayerPushback();
    //    }
    //}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.name == "PlantBoss" && plantController.state != BigPlantController.PlantState.Stunned)
        {
            Debug.Log("OUCH!");
            plantController.state = BigPlantController.PlantState.Stunned;
        }

        if (collision.transform.name == "Player")
        {
            PlayerPushback();
        }
    }
}
