using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SpikyDeathWallController : MonoBehaviour
{
    private Vector2 endPosition;
    private bool playerSurvived = false;

    private bool knockbackOnCooldown = false;
    Vector2 velocityPlayer;
    GameObject target;
    Rigidbody2D targetRB;

    [SerializeField] private float spikyWallRiseTime; // How long will the wall rise.
    [SerializeField] private float spikyWallRiseHeight;
    [SerializeField] private float wallSpeed; // How fast the wall rises.

    private float counter = 0;

    [SerializeField] float knockbackForce;
    // Start is called before the first frame update
    void Start()
    {
        target = GameObject.Find("Player");
        targetRB = target.GetComponent<Rigidbody2D>();

        // Shakes the different parts of the wall at different intervals, making it seem more realistic.
        transform.GetChild(0).DOShakePosition(spikyWallRiseTime, 0.2f, 10, 90, false, false);
        transform.GetChild(1).DOShakePosition(spikyWallRiseTime, 0.4f, 8, 90, false, false);
        transform.GetChild(2).DOShakePosition(spikyWallRiseTime, 0.3f, 6, 90, false, false);
    }

    // Update is called once per frame
    void Update()
    {
        // Rises the wall for a given time.
        if(counter < spikyWallRiseTime && !playerSurvived)
            transform.position = Vector2.MoveTowards(transform.position, new Vector2(transform.position.x, transform.position.y + spikyWallRiseHeight), Time.deltaTime * wallSpeed);

        // if the player escapes, kill the shake tween.
        if (playerSurvived)
        {
            transform.DOMove(new Vector2(transform.position.x, transform.position.y - 20), 10);
        }
    }

    // Cooldown for the player knockback.
    private IEnumerator KnockbackCooldown()
    {
        knockbackOnCooldown = true;
        yield return new WaitForSeconds(1);
        knockbackOnCooldown = false;
    }

    public void SetPlayerSurvived(bool b)
    {
        playerSurvived = b;
    }

    public bool GetPlayerSurvived()
    {
        return playerSurvived;
    }

    public void StopTheWall()
    {
        DOTween.Kill(transform);

    }

    // Pushbacks the player when hit with riot drone collider. Uses velocity for the knockback instead of force.
    public void PlayerPushback()
    {
        velocityPlayer = new Vector2(0, knockbackForce);
        targetRB.MovePosition(targetRB.position + velocityPlayer * Time.deltaTime);
        StartCoroutine(KnockbackCooldown());
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.name == "Player" && !knockbackOnCooldown)
        {
            PlayerPushback();
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.transform.name == "Player" && !knockbackOnCooldown)
        {
            PlayerPushback();
        }
    }
}
