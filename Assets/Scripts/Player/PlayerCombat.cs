using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    public static PlayerCombat instance; // Make instance so we can call this script from animations

    [Header("Player Variables")]
    [SerializeField] private float lightDamage; // Light hits 1, 2 and 3 = lightDamage + lightDamage/2 (pyoristettyna ylospain) 
    [SerializeField] private float heavyDamage; // Same as above but with different float
    //[SerializeField] private float meleeComboLastDamage; // Last hit currentComboHit 3
    [SerializeField] private float knockbackForceLight;
    [SerializeField] private float knockbackForceHeavy;

    [Header("Attack Detection Variables")]
    public LayerMask enemyLayer;
    public Transform attackPoint; // Center of the hit point box we draw to check collisions
    public float attackRangeX; // Width of the check box
    public float attackRangeY; // Height

    [Header("Throwing")]
    [SerializeField] private GameObject throwIndicator; // Object used to rotate throwPoint and pointPrefabs
    [SerializeField] private Transform throwPoint; // Point where the weapon will be Instantiated
    [SerializeField] private float defaultThrowingForce;
    [SerializeField] private float maxChargeTime;
    [SerializeField] private float maxDistance;
    [SerializeField] private float minDistance;
    [SerializeField] private float maxDistanceBetween;

    [Header("Throw projection points")]
    public GameObject pointPrefab;
    GameObject[] points; // Array of pointsPrefabs Instantiated
    private int numberOfPoints; // Amount should be same as maxDistance we can throw, 1 point = 1 distance unit
    [SerializeField] private float spaceBetweenPoints;
    [SerializeField] private int pointsShown;
    [SerializeField] private float pointScaleRatio;

    [Header("Weapon")]
    [SerializeField] private GameObject meleeWeaponPrefab;
    private GameObject weaponInstance; // Players weapon in scene after throwing used in pull backs
    [SerializeField] private bool isWeaponWielded;

    // State variables
    private bool throwAimHold; // True if we are holdling Throw input

    private bool heavyHold;

    private float? throwButtonPressedTime; // Time when we start throwing
    private float throwChargeStartTime; // Time when we start charging
    private float ratio; // Ratio float to grow desired parameters in same ratio. "One ratio to rule them all"

    Mouse mouse = Mouse.current; // Mouse in use on Unity player

    Camera mainCamera;  // MainCamera
    Ray mousePosRay; // Ray from MainCamera to mouse position in screen (endpoint aka .origin vector is what we can use)

    Vector2 vectorToTarget;

    [Header("Placeholder anim debugs")]
    public bool onIdle = true;
    public bool onTran1 = false;
    public bool onTran2 = false;
    public bool onTran3 = false;

    Coroutine tranToIdle; // This will be replaced with correct transittion animation
    
    // These will stay w
    public bool canReceiveInput; // If this is true we can melee (no attack animation ongoing)
    public bool inputReceived; // Used in transitions and idle to tell animator to start correct attack if this turns true

    private void Awake()
    {
        instance = this;
        canReceiveInput = true;
    }



    // Start is called before the first frame update
    void Start()
    {
        // MainCamera
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

        numberOfPoints = (int)maxDistance;
        // Instantiate points and adjust their look
        InitPoints();
    }

    // PLACEHOLDER ANIMATOR STATES
    private void Update()
    {
        // Idle
        if (onIdle && inputReceived && !heavyHold)
        {
            // Attacks in air
            //if (TryGetComponent<PlayerMovement>(out var movementScript))
            //{
            //    if (movementScript.GROUNDED();)
            //    airAttackPosition = gameObject.transform.position;
            //    GetComponent<Rigidbody2D>().gravityScale = 0f;
            //    attackingInAir = true;
            //}

            //Debug.Log("Attack1");
            StartCoroutine(PlaceHolderAttack1());
            InputManager();
            onIdle = false;
            inputReceived = false;
        }

        if (onTran1 && inputReceived && !heavyHold)
        {
            //Debug.Log("Attack2");
            StopCoroutine(tranToIdle);
            StartCoroutine(PlaceHolderAttack2());
            InputManager();
            onTran1 = false;
            inputReceived = false;
        }

        if (onTran2 && inputReceived && !heavyHold)
        {
            //Debug.Log("Attack3");
            StopCoroutine(tranToIdle);
            StartCoroutine(PlaceHolderAttack3());
            InputManager();
            onTran2 = false;
            inputReceived = false;
        }

        // Heavy
        if (onIdle && inputReceived && heavyHold)
        {
            // Attacks in air
            //if(TryGetComponent<PlayerMovement>(out var movementScript))
            //{
            //    if (movementScript.GROUNDED();)
            //    airAttackPosition = gameObject.transform.position;
            //    GetComponent<Rigidbody2D>().gravityScale = 0f;
            //    attackingInAir = true;
            //}
            //Debug.Log("Attack1");
            StartCoroutine(PlaceHolderAttackH1());
            InputManager();
            onIdle = false;
            inputReceived = false;
        }

        if (onTran1 && inputReceived && heavyHold)
        {
            //Debug.Log("Attack2");
            StopCoroutine(tranToIdle);
            StartCoroutine(PlaceHolderAttackH2());
            InputManager();
            onTran1 = false;
            inputReceived = false;
        }

        if (onTran2 && inputReceived && heavyHold)
        {
            //Debug.Log("Attack3");
            StopCoroutine(tranToIdle);
            StartCoroutine(PlaceHolderAttackH3());
            InputManager();
            onTran2 = false;
            inputReceived = false;
        }
    }

    private void FixedUpdate()
    {

        CheckIfMaxDistanceChanged();

        RotateIndicator();

        Throwing();

        CheckMaxDistance();
    }

    // --- INPUT FUNCITONS ---

    public void Melee(InputAction.CallbackContext context)
    {
        // Throw and melee
        if (context.performed && isWeaponWielded)
        {
            // Start Throwing
            if (throwAimHold && meleeWeaponPrefab)
            {
                // Set time here since we start charging
                throwButtonPressedTime = Time.time;
            }

            // Input for melee 
            // Melee type checked in Idle and transitions if heavyHold is true or false
            else if (!throwAimHold && canReceiveInput)
            {
                inputReceived = true;
                canReceiveInput = false;
            }
        }

        // Pull (and grappling hook control) 
        else if (context.performed && !isWeaponWielded && weaponInstance != null && weaponInstance.TryGetComponent<MeleeWeapon>(out var weaponScript))
        {
            // Just left click
            if (!throwAimHold)
            {
                Debug.Log("Trying to pull weapon");
                weaponScript.PullWeapon(gameObject);
            }
            // Left click when right is held down
            else if (throwAimHold)
            {
                Debug.Log("DO GRAPPLING HOOK HERE");
                //weaponScript.PullWeapon(gameObject);
            }
        }

        // Throw on button release
        if(context.canceled && isWeaponWielded && throwAimHold && meleeWeaponPrefab && throwButtonPressedTime != null)
        {
            ThrowWeapon();

            // We don't have weapon anymore
            isWeaponWielded = false;
            throwButtonPressedTime = null;

            // Weapon is thrown hide points
            HideAllProjPoints();
        }
    }

    public void HeavyMelee(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            heavyHold = true;
        }

        if (context.canceled)
        {
            heavyHold = false;
        }
    }

    public void MeleeAimThrowing(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            throwAimHold = true;

            // Show weapon throw min distance direction
            if (isWeaponWielded)
            {
                // Show minDistance amount of points
                ShowProjPoints((int)minDistance);
            }
        }

        if (context.canceled)
        {
            throwAimHold = false;

            // We release aim button hide points
            HideAllProjPoints();
        }
    }


    // --- MELEE ---

    IEnumerator TranToIdle(int tranI)
    {
        //Debug.Log("Transition enter");
        yield return new WaitForSecondsRealtime(1f);
        //Debug.Log("Transition exit");
        if (tranI == 1)
            onTran1 = false;
        else if (tranI == 2)
            onTran2 = false;
        else if (tranI == 3)
            onTran3 = false;

        // Allow movement
        //gameObject.GetComponent<PlayerMovement>().enabled = true;

        onIdle = true;

        if (!canReceiveInput)
        {
            InputManager();
        }
        inputReceived = false;
    }

    IEnumerator PlaceHolderAttack1()
    {
        //Debug.Log("Started melee animation");
        // Disable movement
        //gameObject.GetComponent<PlayerMovement>().enabled = false;
        //gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;

        yield return new WaitForSecondsRealtime(1f);

        Debug.Log("Ended light melee animation 1");


        // Light attack animation 1
        DealDamage(1, false);

        canReceiveInput = true;
        onTran1 = true;

        tranToIdle = StartCoroutine(TranToIdle(1));
    }

    IEnumerator PlaceHolderAttack2()
    {
        //Debug.Log("Started melee animation");
        //gameObject.GetComponent<PlayerMovement>().enabled = false;
        //gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;

        yield return new WaitForSecondsRealtime(1f);

        Debug.Log("Ended light melee animation 2");


        // Light attack animation 2
        DealDamage(2, false);

        canReceiveInput = true;
        onTran2 = true;

        tranToIdle = StartCoroutine(TranToIdle(2));
    }

    IEnumerator PlaceHolderAttack3()
    {
        //Debug.Log("Started melee animation");
        //gameObject.GetComponent<PlayerMovement>().enabled = false;
        //gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;

        yield return new WaitForSecondsRealtime(1f);

        Debug.Log("Ended light melee animation 3");


        // Light attack animation 3
        DealDamage(3, false);

        onTran3 = true;

        tranToIdle = StartCoroutine(TranToIdle(3));
    }

    IEnumerator PlaceHolderAttackH1()
    {
        //Debug.Log("Started melee animation");
        // Disable movement
        //gameObject.GetComponent<PlayerMovement>().enabled = false;
        //gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;

        yield return new WaitForSecondsRealtime(1f);

        Debug.Log("Ended heavy melee animation 1");


        // Heavy attack animation 1
        DealDamage(1, true);

        canReceiveInput = true;
        onTran1 = true;

        tranToIdle = StartCoroutine(TranToIdle(1));
    }

    IEnumerator PlaceHolderAttackH2()
    {
        //Debug.Log("Started melee animation");
        //gameObject.GetComponent<PlayerMovement>().enabled = false;
        // gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;

        yield return new WaitForSecondsRealtime(1f);

        Debug.Log("Ended heavy melee animation 2");


        // Heavy attack animation 2
        DealDamage(2, true);

        canReceiveInput = true;
        onTran2 = true;

        tranToIdle = StartCoroutine(TranToIdle(2));
    }

    IEnumerator PlaceHolderAttackH3()
    {
        //Debug.Log("Started melee animation");
        //gameObject.GetComponent<PlayerMovement>().enabled = false;
        //gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;

        yield return new WaitForSecondsRealtime(1f);

        Debug.Log("Ended heavy melee animation 3");
        
        // Heavy attack animation 3
        DealDamage(3, true);

        onTran3 = true;

        tranToIdle = StartCoroutine(TranToIdle(3));
    }


    // Change canReceiveInput boolean to opposite
    public void InputManager()
    {
        if (!canReceiveInput)
        {
            canReceiveInput = true;
        }
        else
        {
            canReceiveInput = false;
        }
    }

    // Call this function on melee animation end when melee visually hits something
    public void DealDamage(int comboIndex, bool heavyHit)
    {
        // Draws a box in scene if objects from enemyLayer overlap with this box store them in hitEnemies array
        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(attackPoint.position, new Vector2(attackRangeX, attackRangeY), 0f, enemyLayer);

        Debug.Log("DEALING DMG!! combo hit: " + comboIndex + " Heavy?: " + heavyHit);

        // Normal combo hits 1 and 2
        if (comboIndex < 3 && !heavyHit)
        {
            // If there is elements on hitEnemies array go through it
            foreach (Collider2D enemy in hitEnemies)
            {
                // Error check if there isn't Health script attached don't do damage
                if (enemy.TryGetComponent<Health>(out var healthScript))
                {
                    // Deal damage
                    healthScript.TakeDamage(lightDamage);
                    
                    // STUN OR KNOCKBACK + DASH?
                    // Knockback enemy
                    //Knockback(enemy.gameObject, gameObject, knockbackForceLight);
                }
            }
        }
        // Normal combo hit 3 aka last hit of combo
        else if (comboIndex == 3 && !heavyHit)
        {
            // If there is elements on hitEnemies array go through it
            foreach (Collider2D enemy in hitEnemies)
            {
                // Error check if there isn't Health script attached don't do damage
                if (enemy.TryGetComponent<Health>(out var healthScript))
                {
                    // Deal damage
                    // Example if lightDamage = 3 --> 3 / 2 = 1.5 --> Floor(1.5) = 1 total damage 4 OR Ceil(1.5) = 2 total damage 5 
                    if (lightDamage % 2 == 1)
                    {
                        healthScript.TakeDamage(lightDamage + Mathf.Floor(lightDamage / 2));
                    }
                    else
                    {
                        healthScript.TakeDamage(lightDamage + Mathf.Ceil(lightDamage / 2));
                    }
                    // Knockback enemy
                    Knockback(enemy.gameObject, gameObject, knockbackForceHeavy);
                }
            }
        }
        // Heavy combo hits 1 and 2
        if (comboIndex < 3 && heavyHit)
        {
            // If there is elements on hitEnemies array go through it
            foreach (Collider2D enemy in hitEnemies)
            {
                // Error check if there isn't Health script attached don't do damage
                if (enemy.TryGetComponent<Health>(out var healthScript))
                {
                    // Deal damage
                    healthScript.TakeDamage(heavyDamage);
                    // Knockback enemy
                    Knockback(enemy.gameObject, gameObject, knockbackForceLight);
                }
            }
        }
        // Heavy combo hit 3 aka last hit of combo
        else if (comboIndex == 3 && heavyHit)
        {
            // If there is elements on hitEnemies array go through it
            foreach (Collider2D enemy in hitEnemies)
            {
                // Error check if there isn't Health script attached don't do damage
                if (enemy.TryGetComponent<Health>(out var healthScript))
                {
                    // Deal damage
                    if (heavyDamage % 2 == 1) // Example if heavyDamage = 3 --> 3 / 2 = 1.5 --> Floor(1.5) = 1 total damage 4 OR Ceil(1.5) = 2 total damage 5 
                    {
                        healthScript.TakeDamage(heavyDamage + Mathf.Floor(heavyDamage / 2));
                    }
                    else
                    {
                        healthScript.TakeDamage(heavyDamage + Mathf.Ceil(heavyDamage / 2));
                    }
                    // Knockback enemy
                    Knockback(enemy.gameObject, gameObject, knockbackForceHeavy);
                }
            }
        }
    }

    // Debug info draws hit point in this case cube
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireCube(attackPoint.position, new Vector3(attackRangeX, attackRangeY, 0f));
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
        if (throwAimHold)
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
        if (throwAimHold)
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

    private void CheckMaxDistance()
    {
        // We have no weapon wielded and we have thrown weapon we know the instance
        if (!isWeaponWielded && weaponInstance != null)
        {
            // If distance between player and weapon is greater than maxDistance
            if ((weaponInstance.transform.position - gameObject.transform.position).magnitude >= maxDistanceBetween)
            {
                weaponInstance.GetComponent<MeleeWeapon>().PullWeapon(gameObject);
            }
        }

        CheckIfMaxDistanceChanged();
    }

    private void CheckIfMaxDistanceChanged()
    {
        // If we updated maxDistance via pick up or debuff set numberOfPoints to same, aka show 1 pointPrefab per 1 distance unit 
        if (numberOfPoints != (int)maxDistance)
        {
            numberOfPoints = (int)maxDistance;
            InitPoints();
        }
    }

    // Instantiate weaponPrefab and launch it to mouse position
    public void ThrowWeapon()
    {
        // Instantiate meleeWeaponPrefab on attackPoint
        weaponInstance = Instantiate(meleeWeaponPrefab, throwPoint.position, Quaternion.identity);

        weaponInstance.transform.right = vectorToTarget.normalized;

        // Give force to weaponInstance to throw
        if (ratio * maxDistance >= minDistance)
        {
            if (weaponInstance.GetComponent<MeleeWeapon>().getMaxDistance() != maxDistance)
            {
                weaponInstance.GetComponent<MeleeWeapon>().setMaxDistance(maxDistance * ratio); // Favor distance set in this script easier upgrade handling
            }
            else
            {
                weaponInstance.GetComponent<MeleeWeapon>().setMaxDistance(weaponInstance.GetComponent<MeleeWeapon>().getMaxDistance() * ratio);
            }
        }
        else
        {
            weaponInstance.GetComponent<MeleeWeapon>().setMaxDistance(minDistance);
        }

        // Give force to vector mousePosRay - gameObjectPos, use default force and adjust length of throw iva MaxDistance calculations above
        weaponInstance.GetComponent<Rigidbody2D>().AddForce(vectorToTarget.normalized * defaultThrowingForce, ForceMode2D.Impulse);
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

    public float getlightDamage()
    {
        return lightDamage;
    }

    public void setlightDamage(float dmg)
    {
        lightDamage = dmg;
    }
}