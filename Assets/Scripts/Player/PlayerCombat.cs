using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [Header("Player Variables")]
    [SerializeField] private float meleeDamage; // Normal hits currentComboHit 1, 2
    [SerializeField] private float meleeComboLastDamage; // Last hit currentComboHit 3
    [SerializeField] private float knockbackForceNormal;
    [SerializeField] private float knockbackForceLast;


    [SerializeField] private float meleeAttackRate; // Time between combo attacks
    [SerializeField] private float comboCooldown; // Timer of when we need to start new combo
    [SerializeField] private float comboBetweenTimer; // If this runs out we set combo on cooldown

    [Header("Throwing")]
    [SerializeField] private GameObject throwIndicator;
    [SerializeField] private float maxXScale;
    //[SerializeField] private float minXScale;
    [SerializeField] private float maxYScale;
    //[SerializeField] private float minYScale;
    [SerializeField] private float defaultThrowingForce;
    [SerializeField] private float maxChargeTime;
    [SerializeField] private float maxDistance;
    [SerializeField] private float minDistance;

    private Vector3 defaultScale; // Used to reset throwIndicator

    [Header("Attack Detection Variables")]
    public LayerMask enemyLayer;
    public Transform attackPoint; // Center of the hit point box we draw to check collisions
    public float attackRangeX; // Width of the check box
    public float attackRangeY; // Heigth

    [Header("Weapon")]
    [SerializeField] private GameObject meleeWeaponPrefab;
    private GameObject weaponInstance;
    [SerializeField] private bool isWeaponWielded;

    // State variables
    //private float? meleeButtonPressedTime;
    private float comboEndTime = 0; // Set here to zero so we are sure it starts from zero
    private float lastTimeMeleed = 0;
    private bool meleeThrow; // True if we are holdling Throw input
    private int currentComboHit = 1; // Combo counter 1 normal, 2 normal, 3 last hit -> comboCooldown -> Combo can be done again

    private float? throwButtonPressedTime; // Time when we start throwing
    private float throwChargeMaxTime; // Time when we reach maximum charge
    private float ratio; // Ratio float to grow desired parameters in same ratio. "One ratio to rule them all"

    private bool comboOnCooldown; // boolean to check if we can attack
    private bool comboOnGoing; // Used in ComboCounter() calculates if we aren't attacking before comboBetweenTimer then we reset combo


    Mouse mouse = Mouse.current;

    Camera mainCamera;
    Ray mousePosRay;
    // Start is called before the first frame update
    void Start()
    {
        // We don't have to wait cooldown on start
        comboEndTime = comboEndTime - comboCooldown;
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

        // Save scale here
        defaultScale = throwIndicator.transform.localScale;
    }

    private void Update()
    {
        //AdjustIndicatorScale();
    }

    private void FixedUpdate()
    {
        //AdjustIndicatorScale();
        // Calculates when we can combo and if combo will end
        ComboCounter();

        RotateIndicator();

        Throwing();
    }

    private void AdjustIndicatorScale()
    {
        Debug.Log(throwIndicator.transform.localScale.x);
        Debug.Log(gameObject.transform.localScale.x);
        if (throwIndicator.transform.localScale.x != gameObject.transform.localScale.x)
        {
            Debug.Log("Scale");
            throwIndicator.transform.localScale = new Vector3(throwIndicator.transform.localScale.x * -1, throwIndicator.transform.localScale.y, throwIndicator.transform.localScale.z);
        }
    }

    private void RotateIndicator()
    {
        if (throwIndicator.activeInHierarchy)
        {
            //AdjustIndicatorScale();
            Vector3 vectorToTarget = new Vector2(mousePosRay.origin.x - gameObject.transform.position.x, mousePosRay.origin.y - gameObject.transform.position.y);
            float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg;
            //Debug.Log(angle);

            throwIndicator.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
          
        }
    }

    private void Throwing()
    {
        // If we are throwing do these
        if (meleeThrow)
        {
            // Get mouse position from mainCamera ScreenPointToRay
            mousePosRay = mainCamera.ScreenPointToRay(mouse.position.ReadValue());
            // We are holding throw/melee button down
            if (Time.time - throwButtonPressedTime <= maxChargeTime)
            {
                //Debug.Log("Charging");
                throwChargeMaxTime = Time.time;

                ratio = (throwChargeMaxTime - (float)throwButtonPressedTime) / maxChargeTime;
                if(ratio * maxDistance >= minDistance)
                    throwIndicator.transform.localScale = new Vector3(ratio * maxXScale * gameObject.transform.localScale.x, ratio * maxYScale, throwIndicator.transform.localScale.z);
            }
            //else
            //{
            //    AdjustIndicatorScale();
            //}
        }
    }

    private void ComboCounter()
    {
        if (Time.time - comboEndTime > comboCooldown)
        {
            if (comboOnCooldown)
                comboOnCooldown = false;
            
            if (comboOnGoing)
            {
                if (Time.time - lastTimeMeleed > comboBetweenTimer)
                {
                    //Debug.Log("End Combo due no attack");
                    ResetCombo();
                }
            }
        }
    }

    private bool CheckAttackRate()
    {
        return (Time.time - lastTimeMeleed >= meleeAttackRate);
    }

    public void Melee(InputAction.CallbackContext context)
    {
        // Start Throwing
        if (context.performed && isWeaponWielded && meleeThrow && meleeWeaponPrefab)
        {
            // Set time here since we start charging
            throwButtonPressedTime = Time.time;
            Debug.Log((minDistance / maxDistance) * maxXScale);
            throwIndicator.transform.localScale = new Vector3((minDistance / maxDistance) * maxXScale, (minDistance / maxDistance) * maxYScale, 1);
            throwIndicator.SetActive(true);
            
        }
        // Melee
        else if (context.performed && isWeaponWielded && CheckAttackRate() && !meleeThrow && !comboOnCooldown)
        {
            // We start combo if we havent one started
            if(!comboOnGoing)
                comboOnGoing = true;

            // Draws box and if this overlaps with object on enemyLayer, stores their colliders in array
            Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(attackPoint.position, new Vector2(attackRangeX, attackRangeY), 0f, enemyLayer);

            // First and second attack
            if (currentComboHit < 3)
            {
                Debug.Log("Combo hit: " + currentComboHit);

                // If there is elements on hitEnemies array go through it
                foreach (Collider2D enemy in hitEnemies)
                {
                    // Error check if there isn't Health script attached don't do damage
                    if (enemy.GetComponent<Health>() != null)
                    {
                        // Deal damage
                        enemy.GetComponent<Health>().TakeDamage(meleeDamage);
                        // Knockback enemy
                        Knockback(enemy.gameObject, gameObject, knockbackForceNormal);
                    }
                }
                // Next combo attack
                currentComboHit++;
            }
            // Third attack or if bug happens (currentComboHit = 4) this will deal 3. damage and reset combo
            else
            {
                Debug.Log("Last hit: " + currentComboHit);

                foreach (Collider2D enemy in hitEnemies)
                {
                    if (enemy.GetComponent<Health>() != null)
                    {
                        enemy.GetComponent<Health>().TakeDamage(meleeComboLastDamage);
                        Knockback(enemy.gameObject, gameObject, knockbackForceLast);
                    }
                }
                //Debug.Log("End Combo due last attack");
                ResetCombo();
            }
            //meleeButtonPressedTime = null;
            lastTimeMeleed = Time.time;
        }
        // Pull weapon if thrown
        else if(context.performed && !isWeaponWielded && CheckAttackRate() && weaponInstance != null)
        {
            Debug.Log("Trying to pull weapon");
            weaponInstance.GetComponent<MeleeWeapon>().PullWeapon(gameObject);          
        }

        // Throw
        if(context.canceled && isWeaponWielded && meleeThrow && meleeWeaponPrefab)
        {
            // Instantiate meleeWeaponPrefab on attackPoint
            weaponInstance = Instantiate(meleeWeaponPrefab, attackPoint.position, Quaternion.identity);
            // Give force to weaponInstance to throw
            if (ratio * maxDistance >= minDistance)
            {
                if (weaponInstance.GetComponent<MeleeWeapon>().getMaxDistance() != maxDistance)
                    weaponInstance.GetComponent<MeleeWeapon>().setMaxDistance(maxDistance * ratio); // Favor distance set in this script easier upgrade handling
                else
                    weaponInstance.GetComponent<MeleeWeapon>().setMaxDistance(weaponInstance.GetComponent<MeleeWeapon>().getMaxDistance() * ratio);
            }
            else
            {
                Debug.Log("MinDis");
                weaponInstance.GetComponent<MeleeWeapon>().setMaxDistance(minDistance);
            }

            // Give force to vector mousePosRay - gameObjectPos, use default force and adjust length of throw iva MaxDistance calculations above
            weaponInstance.GetComponent<Rigidbody2D>().AddForce(new Vector2(mousePosRay.origin.x - gameObject.transform.position.x, mousePosRay.origin.y - gameObject.transform.position.y).normalized * defaultThrowingForce, ForceMode2D.Impulse);
            
            // We don't have weapon anymore
            isWeaponWielded = false;
            throwButtonPressedTime = null;

            throwIndicator.transform.localScale = defaultScale;
            throwIndicator.SetActive(false);
        }
    }

    public void MeleeAimThrowing(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            meleeThrow = true;
            //throwIndicator.SetActive(true);
            //Debug.Log("Throw ini");
        }

        if (context.canceled)
        {
            meleeThrow = false;
            //throwIndicator.SetActive(false);
            //Debug.Log("Throw cancel");
        }
    }

    // Debug info draws hit point in this case cube
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireCube(attackPoint.position, new Vector3(attackRangeX, attackRangeY, 0f));
    }

    // Sets correct variables to disable melee attacks
    private void ResetCombo()
    {
        comboOnCooldown = true;
        comboOnGoing = false;
        currentComboHit = 1;
        comboEndTime = Time.time;
    }

    // Small knockback to the target when too close to the enemy unit. Knockback knocks slightly upwards so the friction doesn't stop the target right away.
    private void Knockback(GameObject target, GameObject from, float knockbackForce)
    {
        float pushbackX = target.transform.position.x - from.transform.position.x;
        Vector2 knockbackDirection = new Vector2(pushbackX, Mathf.Abs(pushbackX / 4)).normalized;
        target.GetComponent<Rigidbody2D>().AddForce(knockbackDirection * knockbackForce);
    }

    // Called from Weapon script if pulled or we Interact with weapon
    public void PickUpWeapon()
    {
        //Debug.Log("Picked weapon");
        isWeaponWielded = true;
    }

    public bool getWeaponWielded()
    {
        return isWeaponWielded;
    }
}