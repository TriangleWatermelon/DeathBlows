using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VFX;
using Sirenix.OdinInspector;
using Cinemachine;
using UnityEngine.UIElements;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    #region Game Components
    [TitleGroup("Game Control")]
    [BoxGroup("Game Control/Components")]
    [SerializeField] Camera mainCamera;
    CinemachineBrain camBrain;
    [BoxGroup("Game Control/Components")]
    [SerializeField] GameObject mapCamera;
    #endregion

    #region Visuals
    [TitleGroup("Main")]
    [BoxGroup("Main/Visuals")]
    [PreviewField(70, ObjectFieldAlignment.Left)]
    [SerializeField] Sprite playerSprite;

    [Space]
    [BoxGroup("Main/Visuals")]
    [SerializeField] GameObject playerSpriteObj;
    SpriteRenderer playerSpriteRenderer;

    [Space]
    [BoxGroup("Main/Visuals")]
    [SerializeField] VisualEffect bodyVFX;
    Vector2 idleParticleDirection = new Vector2 (0 , 10);
    float idleParticleSpeed = 1;
    [BoxGroup("Main/Visuals")]
    [SerializeField] VisualEffect healVFX;
    [BoxGroup("Main/Visuals")]
    [SerializeField] ParticleSystem summonParticles;

    [Space]
    [BoxGroup("Main/Visuals")]
    [PreviewField(70, ObjectFieldAlignment.Left)]
    [SerializeField] Sprite attackSprite;

    [Space]
    [BoxGroup("Main/Visuals")]
    [SerializeField] GameObject attackObj;
    SpriteRenderer attackSpriteRenderer;

    [Space]
    [BoxGroup("Main/Visuals")]
    [PreviewField(70, ObjectFieldAlignment.Left)]
    [SerializeField] Sprite bubbleSprite;

    [Space]
    [BoxGroup("Main/Visuals")]
    [SerializeField] GameObject bubbleObj;
    SpriteRenderer bubbleSpriteRenderer;

    [Space]
    [BoxGroup("Main/Visuals")]
    [PreviewField(70, ObjectFieldAlignment.Left)]
    [SerializeField] Sprite impactSprite;

    [Space]
    [BoxGroup("Main/Visuals")]
    [SerializeField] GameObject impactObj;
    SpriteRenderer impactSpriteRenderer;

    [Space]
    [BoxGroup("Main/Visuals")]
    [SerializeField] Animator animator;

    [Space]
    [BoxGroup("Main/Visuals")]
    [SerializeField] LineRenderer lineRenderer;

    Material playerSpriteMat;
    #endregion

    #region STATS
    [BoxGroup("Main/Stats")]
    [SerializeField] float maxHealth;
    [HideInInspector]
    public float health { get; private set; }
    [BoxGroup("Main/Stats")]
    [SerializeField] float moveSpeed;
    [BoxGroup("Main/Stats")]
    [SerializeField] float jumpHeight;
    [BoxGroup("Main/Stats")]
    public float damage;
    #endregion

    #region Movement Control
    [TitleGroup("Control")]
    [BoxGroup("Control/Movement")]
    [Range(0, 3)]
    [Tooltip("This will slow the player's movement in the air by dividing the input value")]
    [SerializeField] float airSpeedDivider;
    PlayerActions playerActions;
    Vector2 moveDir;
    [BoxGroup("Control/Movement")]
    [SerializeField] float movementSmoothing;
    Vector3 velocity = Vector3.zero;
    float coyoteTime;
    bool isGrounded;
    bool isJumping = false;
    [BoxGroup("Control/Movement")]
    [Tooltip("This is an empty GameObject placed at the bottom of the player's collider")]
    [SerializeField] Transform groundCheck;
    const float groundCheckRadius = 0.2f;
    [BoxGroup("Control/Movement")]
    [Tooltip("Whatever layer you use for the ground")]
    [SerializeField] LayerMask groundLayer;
    Rigidbody2D rb2d;
    bool isFacingRight = true;
    [BoxGroup("Control/Movement")]
    [SerializeField] float stunTime;
    float hitTimer;
    [BoxGroup("Control/Movement")]
    [SerializeField] float dashCooldown;
    [BoxGroup("Control/Movement")]
    [SerializeField] float dashForce;
    Vector2 dashDir;
    bool isDashing = false;
    float dashTimer;
    public bool canMove { get; set; }
    [BoxGroup("Control/Movement")]
    [SerializeField] float maxGrappleDistance;
    [BoxGroup("Control/Movement")]
    [SerializeField] float grappleSpeed;
    Coroutine moveGrappleCoroutine;
    Coroutine moveToGrappleCoroutine;
    Coroutine removeGrappleCoroutine;
    bool isGrappling = false;
    Vector3 grappleDir;
    [BoxGroup("Control/Movement")]
    [SerializeField] GameObject grappleCheckObj;
    GrapplePoint grapplePoint = null;
    #endregion

    #region Combat Control
    [BoxGroup("Control/Combat")]
    [SerializeField] LayerMask attackLayerMask;
    [BoxGroup("Control/Combat")]
    [SerializeField] float slashDistance;
    [BoxGroup("Control/Combat")]
    [SerializeField] float attackRadius;
    bool isHit = false;
    bool hasAttacked = false;
    bool isSlashing = false;
    Coroutine slashCheck;
    [BoxGroup("Control/Combat")]
    [SerializeField] float attackCooldown;
    float attackTimer;
    [BoxGroup("Control/Combat")]
    [SerializeField] float knockbackForce;
    Vector2 slashPos;
    Vector3 circleStartOffset;
    #endregion

    #region Bubble Control
    [HideInInspector]
    public bool isBubbling = false;
    Vector2 bubblePos;
    Vector2 bubbleOffset = new Vector2(2, 0);
    BubbleController bubbleController;
    Vector2 bubbleDir;
    #endregion

    #region Flag Control
    Vector3 roomStartPosition;
    [BoxGroup("Control/Respawn Flags")]
    [SerializeField] int maxFlags;
    [BoxGroup("Control/Respawn Flags")]
    [SerializeField] float flagPlacementTime;
    float flagPlacementTimer;
    bool isPlacingFlag;
    [BoxGroup("Control/Respawn Flags")]
    [SerializeField] RespawnFlagController respawnFlagController;
    RespawnFlag lastTouchedFlag;
    #endregion

    #region UI Control
    bool isMap;
    bool isTalking;
    DialogueHost d_host;
    bool canTalk;
    #endregion

    [Space]
    public UnityEvent OnDeath;

    PlayerUI playerUI;

    #region Public Variables
    [HideInInspector]
    public Vector3 lastPlaceBeforeJump { get; private set; }
    [HideInInspector]
    public int tomatoCount { get; set; }
    #endregion

    bool isDebug = false;

    void Awake()
    {
        // Sprites 'n Things
        playerSpriteRenderer = playerSpriteObj.GetComponent<SpriteRenderer>();
        playerSpriteRenderer.sprite = playerSprite;
        attackObj = Instantiate(attackObj, transform);
        attackObj.SetActive(false);
        attackSpriteRenderer = attackObj.GetComponent<SpriteRenderer>();
        attackSpriteRenderer.sprite = attackSprite;
        bubbleObj = Instantiate(bubbleObj);
        bubbleObj.SetActive(false);
        bubbleController = bubbleObj.GetComponent<BubbleController>();
        bubbleSpriteRenderer = bubbleObj.GetComponentInChildren<SpriteRenderer>();
        bubbleSpriteRenderer.sprite = bubbleSprite;
        impactObj = Instantiate(impactObj, transform);
        impactObj.SetActive(false);
        impactSpriteRenderer = impactObj.GetComponent<SpriteRenderer>();
        impactSpriteRenderer.sprite = impactSprite;
        playerSpriteMat = playerSpriteRenderer.material;

        // Physics
        rb2d = GetComponent<Rigidbody2D>();

        //Camera
        camBrain = mainCamera.GetComponent<CinemachineBrain>();

        // Base Values
        health = maxHealth;
        if (isFacingRight) circleStartOffset = -Vector2.right;
        else circleStartOffset = Vector2.right;
        respawnFlagController.SetMaxFlags(maxFlags);
        RespawnManager.SetRoomRespawnPosition(transform.position);
        RespawnManager.SetPlayerRespawnPosition(transform.position);
        roomStartPosition = transform.position;
        canMove = true;
        grappleCheckObj.SetActive(false);
        isTalking = false;
        canTalk = false;

        // Input Stuff
        playerActions = new PlayerActions();
        playerActions.Gameplay.Jump.performed += ctx => OnJump();
        playerActions.Gameplay.Jump.canceled += ctx => StopJump();
        playerActions.Gameplay.Slash.performed += ctx => OnSlash();
        playerActions.Gameplay.Bubble.performed += ctx => OnBubble();
        playerActions.Gameplay.ChangeBubble.performed += ctx => OnChangeBubble();
        playerActions.Gameplay.Dash.performed += ctx => OnDash();
        playerActions.Gameplay.PlaceFlag.performed += ctx => OnFlagPress();
        playerActions.Gameplay.PlaceFlag.canceled += ctx => OnFlagRelease();
        playerActions.Gameplay.ToggleMap.performed += ctx => OnMap();
        playerActions.Gameplay.ToggleMap.canceled += ctx => OnMap();
    }

    // Everything in Start needs to be here to avoid racing
    private void Start()
    {
        // UI Stuff
        playerUI = GetComponentInChildren<PlayerUI>();
        playerUI.SetPlayerHealthUI(maxHealth);
        playerUI.AdjustDashTimer(dashCooldown);
    }

    void Update()
    {
        if (isHit)
        {
            hitTimer += Time.deltaTime;
            if (hitTimer > 0.1f)
                playerUI.DisplayHitEffect(false);
            if(hitTimer >= stunTime)
                isHit = false;
        }

        if (!isGrounded)
            coyoteTime += Time.deltaTime;

        if (isDashing)
        {
            dashTimer += Time.deltaTime;

            playerUI.AdjustDashTimer(dashTimer);

            if (dashTimer >= dashCooldown)
                isDashing = false;
        }

        // Attack Delay
        if (hasAttacked)
        {
            attackTimer += Time.deltaTime;
            if(attackTimer >= 0.1f)
            {
                impactObj.SetActive(false);
            }
            if(attackTimer >= 0.2f)
            {
                isSlashing = false;
                StopCoroutine(slashCheck);
                attackObj.SetActive(false);
            }
            if (attackTimer >= attackCooldown)
            {
                hasAttacked = false;
            }
        }

        if (isPlacingFlag)
        {
            flagPlacementTimer += Time.deltaTime;

            if(isDebug)
                playerUI.SetRespawnTimer(flagPlacementTimer);

            if (flagPlacementTimer >= flagPlacementTime)
            {
                PlaceFlag();
                isPlacingFlag = false;
            }
        }

        #region Debug Controls
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Equals))
            Heal(1);
        if (Input.GetKeyDown(KeyCode.R))
        {
            Entity[] entities = FindObjectsOfType<Entity>();
            foreach (var e in entities)
                e.ResetEntity();
        }
#endif
        #endregion
    }

    void FixedUpdate()
    {
        // Input & Movement
        moveDir = playerActions.Gameplay.Move.ReadValue<Vector2>();
        Move(moveDir);

        isGrounded = false;

        // The player is grounded if a circlecast to the groundCheck position hits anything designated on the ground layer
        Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheck.position, groundCheckRadius, groundLayer);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
            {
                if (colliders[i].gameObject.tag != "Bubble")
                    lastPlaceBeforeJump = transform.position;
                isGrounded = true;
                coyoteTime = 0;
            }
        }
    }

    // Body VFX follow the player motion so wait for all movement calculations to happen before effects change
    private void LateUpdate()
    {
        // Body VFX
        if (moveDir.x == 0 && moveDir.y == 0)
        {
            bodyVFX.SetVector3("PlayerDirection", idleParticleDirection);
            bodyVFX.SetFloat("PlayerSpeed", idleParticleSpeed);

            playerSpriteMat.SetVector("_Direction", idleParticleDirection);
        }
        else
        {
            bodyVFX.SetVector3("PlayerDirection", -rb2d.velocity);
            bodyVFX.SetFloat("PlayerSpeed", rb2d.velocity.x / 4);

            playerSpriteMat.SetVector("_Direction", rb2d.velocity);
        }
    }

    /// <summary>
    /// Sets the Dialogue Host and sets the talking state to true.
    /// </summary>
    /// <param name="_host"></param>
    public void SetDialogueHost(DialogueHost _host)
    {
        d_host = _host;
        canTalk = true;
    }

    /// <summary>
    /// Sets the talking state to false and nulls the Dialogue Host.
    /// </summary>
    public void ClearDialogueHost()
    {
        isTalking = false;
        d_host = null;
    }

    /// <summary>
    /// Triggered when the player hits the attack button.
    /// Handles the player attack (sprites and effects) based on the direction of the controller (-1 to 1 on XY axis).
    /// </summary>
    void OnSlash()
    {
        // Add any guards here
        #region Guards
        if (isHit)
            return;

        if (hasAttacked)
            return;

        if (isMap)
            return;

        if (isPlacingFlag)
            return;

        if (isTalking)
        {
            d_host.CycleNextDialogue();
            return;
        }
        #endregion

        if (Mathf.Abs(moveDir.x) <= 0.2f && Mathf.Abs(moveDir.y) <= 0.2f)
        {
            if (isFacingRight)
                moveDir = Vector2.right;
            else
                moveDir = -Vector2.right;

            rb2d.AddForce(moveDir * (knockbackForce * 10));
        }
        slashPos = moveDir * slashDistance;

        // Slash Sprite Position
        attackObj.transform.localPosition = slashPos;

        isSlashing = true;
        slashCheck = StartCoroutine(Slashing());
        attackTimer = 0;
        hasAttacked = true;
        attackObj.SetActive(true);

        // Slash Sprite Rotation
        attackObj.transform.localEulerAngles = new Vector3(0, 0, MathHelper.FindDegreesForRotation(moveDir));

        // Player Slash Animation
        if (isFacingRight)
            animator.SetTrigger("AttackRight");
        else
            animator.SetTrigger("AttackLeft");
    }

    /// <summary>
    /// Loops the CircleCast that checks for a hit the entire time the slash animation is visible
    /// or a hit is registered.
    /// </summary>
    /// <returns></returns>
    IEnumerator Slashing()
    {
        while (isSlashing)
        {
            yield return new WaitForEndOfFrame();

            // Does it hit? Doing this twice just in case the first one misses something.
            RaycastHit2D[] hits = Physics2D.CircleCastAll(
                transform.position + circleStartOffset,
                attackRadius,
                moveDir,
                slashDistance,
                ~attackLayerMask
                );

            foreach (var hit in hits)
            {
                if (hit.collider != null)
                {
                    // Impact Sprite Position
                    impactObj.transform.position = hit.collider.ClosestPoint(transform.position);
                    impactObj.transform.rotation = Quaternion.Euler(0, 0, UnityEngine.Random.Range(0, 360));
                    impactObj.SetActive(true);

                    if (hit.collider.GetComponent<Entity>() != null)
                    {
                        Entity enemy = hit.collider.GetComponent<Entity>();
                        enemy.TakeDamage(damage);
                        enemy.KnockbackEntity(moveDir);
                        impactObj.transform.position = hit.collider.transform.position;
                    }

                    // Knockback the player on successful contact.
                    KnockbackPlayer();
                    StopCoroutine(slashCheck);
                }
            }
        }
    }

    /// <summary>
    /// Takes the D-Pad (or arrow key) input to determine the bubble type.
    /// </summary>
    private void OnChangeBubble()
    {
        bubbleDir = playerActions.Gameplay.ChangeBubble.ReadValue<Vector2>();

        switch (bubbleDir)
        {
            case Vector2 v when v.Equals(Vector2.up):
                ChangeBubbleType(BubbleController.BubbleType.Basic);
                break;
            case Vector2 v when v.Equals(Vector2.right):
                ChangeBubbleType(BubbleController.BubbleType.Frozen);
                break;
            case Vector2 v when v.Equals(Vector2.down):
                ChangeBubbleType(BubbleController.BubbleType.Anti);
                break;
            case Vector2 v when v.Equals(Vector2.left):
                ChangeBubbleType(BubbleController.BubbleType.Sticky);
                break;
        }
    }

    /// <summary>
    /// Tells the BubbleController to change the bubble type.
    /// </summary>
    /// <param name="_type"></param>
    void ChangeBubbleType(BubbleController.BubbleType _type)
    {
        if (isBubbling)
            return;

        if (isMap)
            return;

        bubbleController.SetBubbleType(_type);
        playerUI.SetBubbleTypeUI(_type);
    }


    /// <summary>
    /// Spawns a bubble and handles the positioning of said bubble.
    /// </summary>
    void OnBubble()
    {
        if (isTalking)
        {
            d_host.CyclePreviousDialogue();
            return;
        }

        if (!isBubbling)
        {
            bubblePos = transform.position + new Vector3(moveDir.x, 0);

            if (isFacingRight) bubblePos += bubbleOffset;
            else bubblePos -= bubbleOffset;

            bubbleObj.SetActive(true);
            bubbleObj.transform.position = bubblePos;
            isBubbling = true;
        }
        else
        {
            // If the player is close enough, let them pop the bubble.
            if (Vector3.Distance(transform.position, bubbleObj.transform.position) < 1.85f)
                bubbleController.Pop();
        }
    }

    /// <summary>
    /// Checks for the Brook Effect and then applies force in the direction the player
    /// is facing.
    /// </summary>
    void OnDash()
    {
        #region Guards
        if (isDashing)
            return;

        if (isMap)
            return;

        if (isTalking)
            return;
        #endregion

        // Set the directional force. If the player is in the air we want to give them a slight push upward.
        if (isGrounded)
        {
            if (isFacingRight)
                dashDir = Vector2.right * (dashForce * 1000);
            else
                dashDir = -Vector2.right * (dashForce * 1000);
        }
        else
        {
            if (isFacingRight)
                dashDir = new Vector2(1, 0.1f) * (dashForce * 1000);
            else
                dashDir = new Vector2(-1, 0.1f) * (dashForce * 1000);
        }

        CheckForBrookEffect();

        // Add the directional force but stop any motion first to avoid crazy dash.
        rb2d.velocity = Vector2.zero;
        rb2d.AddForce(dashDir);

        dashTimer = 0;
        isDashing = true;
    }

    /// <summary>
    /// Checks the space in front of the player for entities.
    /// If they exist, activate the Brook effect on them.
    /// </summary>
    void CheckForBrookEffect()
    {
        // Are there enemies in my way?
        RaycastHit2D[] hits = Physics2D.CircleCastAll(
            transform.position + circleStartOffset,
            2,
            dashDir,
            5,
            ~attackLayerMask
            );

        foreach (var hit in hits)
        {
            if (hit.collider != null)
            {
                if (hit.collider.GetComponent<Entity>() != null)
                {
                    Entity enemy = hit.collider.GetComponent<Entity>();
                    enemy.ActivateBrookEffect(damage);
                }
            }
        }
    }

    /// <summary>
    /// Makes the player jump.
    /// </summary>
    void OnJump()
    {
        if (isMap)
            return;

        if (!isGrounded && coyoteTime > 0.1f)
        {
            if(moveDir != Vector2.zero)
                TryGrapple();
            return;
        }

        isGrounded = false;
        isJumping = true;

        rb2d.AddForce(new Vector2(0f, jumpHeight * 100));
    }

    /// <summary>
    /// Stops the player from jumping when they release the jump button.
    /// </summary>
    void StopJump()
    {
        if (!isGrounded && isJumping && rb2d.velocity.y > 0)
        {
            rb2d.velocity = new Vector2(rb2d.velocity.x, rb2d.velocity.y/3);
            isJumping = false;
        }

        if (isGrappling)
        {
            StopCoroutine(moveToGrappleCoroutine);
            StopCoroutine(removeGrappleCoroutine);
            ToggleGrappleComponents(false);

            isGrappling = false;
        }
        else if(moveGrappleCoroutine != null)
        {
            StopCoroutine(moveGrappleCoroutine);
            ToggleGrappleComponents(false);
        }
    }

    /// <summary>
    /// Sets all values for the grapple hook and starts the coroutine to move the hook.
    /// </summary>
    void TryGrapple()
    {
        if (isGrappling)
            return;

        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, transform.position);

        grappleDir = moveDir;

        grappleCheckObj.transform.position = transform.position;
        grappleCheckObj.transform.localEulerAngles = new Vector3(0, 0, MathHelper.FindDegreesForRotation(grappleDir));

        ToggleGrappleComponents(true);

        if (moveGrappleCoroutine != null)
            StopCoroutine(moveGrappleCoroutine);
        moveGrappleCoroutine = StartCoroutine(MoveGrappleHook());
    }

    /// <summary>
    /// Moves the grapple hook from the player's body in the direction supplied when TryGrapple() was called.
    /// Stops itself when the maxGrappleDistance is reached.
    /// </summary>
    /// <returns></returns>
    IEnumerator MoveGrappleHook()
    {
        while(Vector3.Distance(lineRenderer.GetPosition(0), lineRenderer.GetPosition(1)) < maxGrappleDistance)
        {
            lineRenderer.SetPosition(1, transform.position);
            lineRenderer.SetPosition(0, lineRenderer.GetPosition(0) + (grappleDir * 0.75f));
            grappleCheckObj.transform.position = lineRenderer.GetPosition(0);
            yield return new WaitForFixedUpdate();
        }
        if (!isGrappling)
            ToggleGrappleComponents(false);
        StopCoroutine(moveGrappleCoroutine);
    }

    /// <summary>
    /// Starts the coroutine to move the player towards the grapple point.
    /// </summary>
    void Grapple()
    {
        moveToGrappleCoroutine = StartCoroutine(MoveToGrapplePoint());
    }

    /// <summary>
    /// Adds to the player's velocity towards the grapple point.
    /// </summary>
    /// <returns></returns>
    IEnumerator MoveToGrapplePoint()
    {
        while (isGrappling)
        {
            Vector3 launchDir = (grapplePoint.position - transform.position).normalized;
            Vector3 targetVelocity = launchDir * grappleSpeed;
            rb2d.velocity = Vector3.SmoothDamp(rb2d.velocity, targetVelocity, ref velocity, movementSmoothing);
            yield return new WaitForFixedUpdate();
        }
        StopCoroutine(moveToGrappleCoroutine);
    }

    /// <summary>
    /// Gives the player controller a grapple point to move to.
    /// </summary>
    /// <param name="_point"></param>
    public void SetGrapplePoint(GrapplePoint _point)
    {
        if (moveGrappleCoroutine != null)
            StopCoroutine(moveGrappleCoroutine);

        isGrappling = true;

        grapplePoint = _point;
        lineRenderer.SetPosition(0, grapplePoint.position);
        grappleCheckObj.transform.position = grapplePoint.position;

        Grapple();
        removeGrappleCoroutine = StartCoroutine(RemoveGrappleHook());
    }

    /// <summary>
    /// Sets the position of the lineRenderer to follow the player while they grapple.
    /// </summary>
    /// <returns></returns>
    IEnumerator RemoveGrappleHook()
    {
        while (Vector3.Distance(transform.position, grapplePoint.position) >= 1.5f)
        {
            lineRenderer.SetPosition(1, transform.position);
            grappleCheckObj.transform.position = grapplePoint.position;
            yield return new WaitForFixedUpdate();
        }
        isGrappling = false;
        ToggleGrappleComponents(false);
        StopCoroutine(removeGrappleCoroutine);
    }

    /// <summary>
    /// Toggles the grapple visual components.
    /// </summary>
    /// <param name="_state"></param>
    void ToggleGrappleComponents(bool _state)
    {
        lineRenderer.enabled = _state;
        grappleCheckObj.SetActive(_state);
    }

    /// <summary>
    /// Moves the player based on moveSpeed and the direction the controller is aiming (between -1 and 1 on XY axis).
    /// </summary>
    /// <param name="moveDir"></param>
    void Move(Vector2 moveDir)
    {
        if (!canMove)
            return;

        if (isMap)
            return;

        if (isTalking)
            return;

        if (isPlacingFlag)
            return;

        if (moveDir.x < 0 && isFacingRight)
            FlipSprite();
        if (moveDir.x > 0 && !isFacingRight)
            FlipSprite();

        if (!isGrounded)
            moveDir = moveDir / airSpeedDivider;

        if (Mathf.Abs(moveDir.x) < 0.2f)
        {
            //Sets the target velocity to zero to stop the player's movement.
            Vector3 targetVelocity = Vector2.zero + new Vector2(0, rb2d.velocity.y);
            rb2d.velocity = Vector3.SmoothDamp(rb2d.velocity, targetVelocity, ref velocity, 0.1f);
        }
        else
        {
            // Move the character by finding the target velocity
            // And then smoothing it out and applying it to the character
            Vector3 targetVelocity = new Vector2((moveDir.x * moveSpeed), rb2d.velocity.y);
            rb2d.velocity = Vector3.SmoothDamp(rb2d.velocity, targetVelocity, ref velocity, movementSmoothing);
        }
    }

    /// <summary>
    /// Toggles the simulation state of the rigidbody and canMove boolean.
    /// This also changed the update method for the camera to prevent jittering.
    /// </summary>
    /// <param name="_state"></param>
    public void ToggleRigidBody(bool _state)
    {
        rb2d.simulated = _state;
        canMove = _state;
        if (!_state)
            camBrain.m_UpdateMethod = CinemachineBrain.UpdateMethod.FixedUpdate;
        else
            camBrain.m_UpdateMethod = CinemachineBrain.UpdateMethod.SmartUpdate;
    }

    /// <summary>
    /// Flips the player sprite depending on if they are facing right or not.
    /// Also adjusts any values specific to the direction the sprite is facing.
    /// </summary>
    void FlipSprite()
    {
        isFacingRight = !isFacingRight;

        Vector3 flipScale = playerSpriteObj.transform.localScale;
        flipScale.x *= -1;
        playerSpriteObj.transform.localScale = flipScale;

        if (isFacingRight) circleStartOffset = -Vector2.right;
        else circleStartOffset = Vector2.right;
    }

    /// <summary>
    /// Applies damage to the entity.
    /// Starts the hit timer to prevent multiple hits in a single attack.
    /// Invokes death if health is less than 0 after applying damage.
    /// </summary>
    /// <param name="_damage"></param>
    public void TakeDamage(float _damage)
    {
        if (isHit)
            return;

        OnFlagRelease();

        hitTimer = 0;
        isHit = true;
        health -= _damage;
        if (health <= 0)
        {
            Die();
            OnDeath.Invoke();
        }
        else
            animator.SetTrigger("TookDamage");

        // Knock the player back when they take damage
        KnockbackPlayer();

        playerUI.AdjustHealth(health);
        playerUI.DisplayHitEffect(true, mainCamera.WorldToScreenPoint(transform.position));
    }

    /// <summary>
    /// Applies a knockback force to the player.
    /// </summary>
    void KnockbackPlayer()
    {
        rb2d.velocity = Vector2.zero;
        rb2d.velocity = -moveDir * knockbackForce;
    }

    /// <summary>
    /// Heals the player by the value provided as _additionalHealth.
    /// </summary>
    /// <param name="_additionalHealth"></param>
    public void Heal(float _additionalHealth)
    {
        health += _additionalHealth;
        if (health > maxHealth)
            health = maxHealth;

        animator.SetTrigger("GotHealed");
        healVFX.Play();

        playerUI.AdjustHealth(health);
    }

    /// <summary>
    /// Places the player at the given position.
    /// </summary>
    /// <param name="newPos"></param>
    public void RepositionPlayer(Vector3 newPos)
    {
        transform.position = newPos;
    }

    /// <summary>
    /// When the user presses down on the flag button a timer will begin and an animation will start.
    /// </summary>
    private void OnFlagPress()
    {
        if (!isGrounded)
            return;

        if (canTalk)
        {
            isTalking = true;
            d_host.StartDialogue();
            return;
        }

        if (lastTouchedFlag != null)
        {
            lastTouchedFlag.RemoveFlag();
            return;
        }

        flagPlacementTimer = 0;
        isPlacingFlag = true;
        animator.SetBool("isPlacingFlag", true);
        summonParticles.Play();
    }

    /// <summary>
    /// When the user releases the flag button the animation and timer will stop.
    /// </summary>
    private void OnFlagRelease()
    {
        animator.SetBool("isPlacingFlag", false);
        isPlacingFlag = false;
        summonParticles.Stop();

        if(isDebug)
            playerUI.SetRespawnTimer(0);
    }

    /// <summary>
    /// Places a Respawn Flag at the player's current position;
    /// </summary>
    private void PlaceFlag()
    {
        respawnFlagController.PlaceFlag(transform.position);

        if (isDebug)
            playerUI.SetRespawnTimer(0);
    }

    void Die()
    {
        if (respawnFlagController.AnyActiveFlags())
        {
            RepositionPlayer(RespawnManager.GetPlayerRespawnPoint());
            ChoseRespawnPoint();
        }
        else
        {
            RepositionPlayer(roomStartPosition);
            ChoseRespawnPoint();
        }
    }

    /// <summary>
    /// When the player has chosen their respawn point, all the enemies and the
    /// player's health will reset.
    /// </summary>
    public void ChoseRespawnPoint()
    {
        Entity[] entities = FindObjectsOfType<Entity>();
        foreach (var e in entities)
            e.ResetEntity();

        Soul[] souls = FindObjectsOfType<Soul>();
        foreach (var s in souls)
            s.gameObject.SetActive(false);

        FullHealPlayer();
    }

    public void FullHealPlayer()
    {
        StartCoroutine(WaitToHeal());
    }

    IEnumerator WaitToHeal()
    {
        yield return new WaitForSeconds(0.1f);
        health = maxHealth;
        playerUI.AdjustHealth(health);
    } 

    private void OnTriggerEnter2D(Collider2D collision)
    {
        string tag = collision.tag;
        switch (tag)
        {
            case "Flag":
                lastTouchedFlag = collision.GetComponent<RespawnFlag>();
                break;
            case "Hazard":
                TakeDamage(1);
                if(health > 0)
                    RepositionPlayer(lastPlaceBeforeJump);
                break;
            case "Trigger":
                collision.GetComponent<TriggerAction>().enterEvent.Invoke();
                break;
            case "Tomato":
                collision.GetComponent<Tomato>().CollectTomato();
                tomatoCount += 1;
                SaveController.instance.SetTomatoesHeldByPlayer(tomatoCount);
                break;
            case "Enemy":
                TakeDamage(1);
                break;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        string tag = collision.tag;
        switch (tag)
        {
            case "Flag":
                lastTouchedFlag = null;
                break;
            case "Trigger":
                collision.GetComponent<TriggerAction>().exitEvent.Invoke();
                break;
        }
    }

    private void OnMap()
    {
        isMap = !isMap;

        mapCamera.SetActive(isMap);
        mainCamera.gameObject.SetActive(!isMap);
    }

    public void SetDebug(bool _isDebug)
    {
        isDebug = _isDebug;
        playerUI.SetDebugMode(_isDebug);
    }

    private void OnEnable()
    {
        playerActions.Enable();
    }
    private void OnDisable()
    {
        playerActions.Disable();
    }
}