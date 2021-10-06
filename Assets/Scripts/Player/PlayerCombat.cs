using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [Header("Player Variables")]
    [SerializeField] private float meleeDamage; // Normal hits currentComboHit 1, 2 and 2*normal damage to last hit
    //[SerializeField] private float meleeComboLastDamage; // Last hit currentComboHit 3
    [SerializeField] private float knockbackForceNormal;
    [SerializeField] private float knockbackForceLast;


    [SerializeField] private float meleeAttackRate; // Time between combo attacks
    [SerializeField] private float comboCooldown; // Timer of when we need to start new combo
    [SerializeField] private float comboBetweenTimer; // If this runs out we set combo on cooldown

    [Header("Throwing")]
    [SerializeField] private GameObject throwIndicator; // Object used to rotate throwPoint and pointPrefabs
    [SerializeField] private Transform throwPoint; // Point where the weapon will be Instantiated
    [SerializeField] private float defaultThrowingForce;
    [SerializeField] private float maxChargeTime;
    [SerializeField] private float maxDistance;
    [SerializeField] private float minDistance;

    [Header("Projection points")]
    public GameObject pointPrefab;
    GameObject[] points; // Array of pointsPrefabs Instantiated
    private int numberOfPoints; // Amount should be same as maxDistance we can throw, 1 point = 1 distance unit
    [SerializeField] private float spaceBetweenPoints;
    [SerializeField] private int pointsShown;
    [SerializeField] private float pointScaleRatio;

    [Header("Attack Detection Variables")]
    public LayerMask enemyLayer;
    public Transform attackPoint; // Center of the hit point box we draw to check collisions
    public float attackRangeX; // Width of the check box
    public float attackRangeY; // Height

    [Header("Weapon")]
    [SerializeField] private GameObject meleeWeaponPrefab;
    private GameObject weaponInstance;
    [SerializeField] private bool isWeaponWielded;

    // State variables
    //private float? meleeButtonPressedTime;
    private float comboEndTime = 0; // Set here to zero so we are sure it starts from zero
    private float lastTimeMeleed = 0;
    private bool throwAim; // True if we are holdling Throw input
    private int currentComboHit = 1; // Combo counter 1 normal, 2 normal, 3 last hit -> comboCooldown -> Combo can be done again

    private float? throwButtonPressedTime; // Time when we start throwing
    private float throwChargeStartTime; // Time when we start charging
    private float ratio; // Ratio float to grow desired parameters in same ratio. "One ratio to rule them all"

    private bool comboOnCooldown; // boolean to check if we can attack
    private bool comboOnGoing; // Used in ComboCounter() calculates if we aren't attacking before comboBetweenTimer then we reset combo

    Mouse mouse = Mouse.current; // Mouse in use on Unity player

    Camera mainCamera;  // MainCamera
    Ray mousePosRay; // Ray from MainCamera to mouse position in screen (endpoint aka .origin vector is what we can use)

    Vector2 vectorToTarget;

    // Start is called before the first frame update
    void Start()
    {
        // We don't have to wait cooldown on start
        comboEndTime = comboEndTime - comboCooldown;
        // MainCamera
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

        numberOfPoints = (int)maxDistance;
        // Instantiate points and adjust their look
        InitPoints();
    }

    private void Update()
    {
        
    }

    private void FixedUpdate()
    {
        // If we updated maxDistance via pick up or debuff set numberOfPoints to same, aka show 1 pointPrefab per 1 distance unit 
        if (numberOfPoints != (int)maxDistance)
        {
            numberOfPoints = (int)maxDistance;
            InitPoints();
        }

        ComboCounter();

        RotateIndicator();

        Throwing();
    }

    // --- INPUT FUNCITONS ---

    public void Melee(InputAction.CallbackContext context)
    {
        // Start Throwing
        if (context.performed && isWeaponWielded && throwAim && meleeWeaponPrefab)
        {
            // Set time here since we start charging
            throwButtonPressedTime = Time.time;
        }
        // Melee
        else if (context.performed && isWeaponWielded && CheckAttackRate() && !throwAim && !comboOnCooldown)
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
                        enemy.GetComponent<Health>().TakeDamage(meleeDamage * 2);
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

        // Throw on button release
        if(context.canceled && isWeaponWielded && throwAim && meleeWeaponPrefab && throwButtonPressedTime != null)
        {
            // Instantiate meleeWeaponPrefab on attackPoint
            weaponInstance = Instantiate(meleeWeaponPrefab, throwPoint.position, Quaternion.identity);

            weaponInstance.transform.right = vectorToTarget.normalized;

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
                weaponInstance.GetComponent<MeleeWeapon>().setMaxDistance(minDistance);
            }

            // Give force to vector mousePosRay - gameObjectPos, use default force and adjust length of throw iva MaxDistance calculations above
            weaponInstance.GetComponent<Rigidbody2D>().AddForce(new Vector2(mousePosRay.origin.x - gameObject.transform.position.x, mousePosRay.origin.y - gameObject.transform.position.y).normalized * defaultThrowingForce, ForceMode2D.Impulse);
            
            // We don't have weapon anymore
            isWeaponWielded = false;
            throwButtonPressedTime = null;

            // Weapon is thrown hide points
            HideAllProjPoints();
        }
    }

    public void MeleeAimThrowing(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            throwAim = true;

            // Show minDistance amount of points
            ShowProjPoints((int)minDistance);
        }

        if (context.canceled)
        {
            throwAim = false;

            // We release aim button hide points
            HideAllProjPoints();
        }
    }


    // --- MELEE ---

    private void ComboCounter()
    {
        // If we can start new combo
        if (Time.time - comboEndTime > comboCooldown)
        {
            // Combo is not on cooldown anymore
            if (comboOnCooldown)
                comboOnCooldown = false;

            // Check if we need to reset combo due no attacks
            if (comboOnGoing)
            {
                // We dont melee between lastTimeMeleed and lastTimeMeleed + comboBetweenTimer
                if (Time.time - lastTimeMeleed > comboBetweenTimer)
                {
                    // No attacks 
                    ResetCombo();
                }
            }
        }
    }

    private bool CheckAttackRate()
    {
        return (Time.time - lastTimeMeleed >= meleeAttackRate);
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


    // --- THROWING ---

    // Instantiate throwing projection points and scale, color, modify them here
    private void InitPoints()
    {
        points = new GameObject[numberOfPoints];

        // Go through the array and set colors of different red and scales
        for (int i = 0; i < numberOfPoints; i++)
        {
            // Instantiate and set parent
            points[i] = Instantiate(pointPrefab, throwPoint.position, Quaternion.identity);
            points[i].transform.parent = throwIndicator.transform;

            // Naming easier to read hierarchy
            points[i].name = points[i].name.Replace("(Clone)", "(" + i + ")");

            // Color
            Color pointC = new Color(1f, 1f - 1f * i / (numberOfPoints - 1), 1f - 1f * i / (numberOfPoints - 1));
            points[i].GetComponent<SpriteRenderer>().color = pointC;

            // Scale 
            if (i != 0)
                points[i].transform.localScale = points[i - 1].transform.localScale * pointScaleRatio;

            points[i].SetActive(false);
        }
    }

    // Calculate the position of next point
    Vector2 PointPosition(float t, Vector2 toTarget)
    {
        // Please don't ask the logic of this calculation it just works via trial and error
        Vector2 position = (Vector2)throwPoint.position + (toTarget.normalized * defaultThrowingForce * t) + 0.5f * Physics2D.gravity * (t * t);
        return position;
    }

    // Rotates indicator to mouse position and adjusts the positions of points[] gameObjects
    private void RotateIndicator()
    {
        if (throwAim)
        {
            // Get mouse position from mainCamera ScreenPointToRay
            mousePosRay = mainCamera.ScreenPointToRay(mouse.position.ReadValue());

            vectorToTarget = new Vector2(mousePosRay.origin.x - gameObject.transform.position.x, mousePosRay.origin.y - gameObject.transform.position.y);

            // Positions of points[]
            for (int i = 0; i < numberOfPoints; i++)
            {
                points[i].transform.position = PointPosition(i * spaceBetweenPoints, vectorToTarget);
            }

            // Check if we need to flip vector, player flip angles are different
            if (gameObject.transform.localScale.x == 1)
                throwIndicator.transform.right = vectorToTarget;
            else
                throwIndicator.transform.right = -vectorToTarget;
        }
    }

    private void Throwing()
    {
        // If we are throwing do these
        if (throwAim)
        {
            // We are holding throw and melee button down
            if (Time.time - throwButtonPressedTime <= maxChargeTime)
            {
                throwChargeStartTime = Time.time;

                // Ratio calculation tap ratio = 0 and hold for maxChargeTime = 1
                ratio = (throwChargeStartTime - (float)throwButtonPressedTime) / maxChargeTime;

                // Floor ratio to int to show correct amount of points
                int pointsToShow = Mathf.FloorToInt(ratio * numberOfPoints);
                // Check if we need to show more points since ratio is growing
                if (pointsShown < pointsToShow + 1)
                {
                    points[pointsShown].SetActive(true);
                    pointsShown++;
                }
            }
        }
    }

    // Show amount of points
    private void ShowProjPoints(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            points[i].SetActive(true);
            pointsShown++;
        }
    }

    // Hide
    private void HideAllProjPoints()
    {
        for (int i = 0; i < numberOfPoints; i++)
        {
            points[i].SetActive(false);
        }
        // Show minDistance amount of projections when aiming
        pointsShown = 0;
    }

    // Called from Weapon script if pulled or we Interact with weapon
    public void PickUpWeapon()
    {
        //Debug.Log("Picked weapon");
        isWeaponWielded = true;
    }


    // --- GET / SET ---

    public bool getWeaponWielded()
    {
        return isWeaponWielded;
    }

    public void setWeaponWielded(bool wield)
    {
        isWeaponWielded = wield;
    }

    public float getMaxDistance()
    {
        return maxDistance;
    }

    public void setMaxDistance(float d)
    {
        maxDistance = d;
    }

    public float getMeleeDamage()
    {
        return meleeDamage;
    }

    public void setMeleeDamage(float dmg)
    {
        meleeDamage = dmg;
    }
}