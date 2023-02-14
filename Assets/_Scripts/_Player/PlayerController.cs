using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.VFX;
using Sirenix.OdinInspector;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    #region Game Components
    [TitleGroup("Game Control")]
    [BoxGroup("Game Control/Components")]
    [SerializeField] Camera mainCamera;
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
    #endregion

    #region STATS
    [BoxGroup("Main/Stats")]
    [Tooltip("I'll never die!")]
    [SerializeField] float maxHealth;
    [HideInInspector]
    public float health { get; private set; }
    [BoxGroup("Main/Stats")]
    [Tooltip("ZOOOOOOOOOM!!!")]
    [SerializeField] float moveSpeed;
    [BoxGroup("Main/Stats")]
    [Tooltip("...How high?")]
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
    #endregion

    [Space]
    public UnityEvent OnDeath;

    PlayerUI playerUI;

    #region Public Variables
    [HideInInspector]
    public Vector3 lastPlaceBeforeJump { get; private set; }
    #endregion

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

        // Physics
        rb2d = GetComponent<Rigidbody2D>();

        // Base Values
        health = maxHealth;

        // Input Stuff
        playerActions = new PlayerActions();
        playerActions.Gameplay.Jump.performed += ctx => OnJump();
        playerActions.Gameplay.Jump.canceled += ctx => StopJump();
        playerActions.Gameplay.Slash.performed += ctx => OnSlash();
        playerActions.Gameplay.Bubble.performed += ctx => OnBubble();
        playerActions.Gameplay.Dash.performed += ctx => OnDash();
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
        // Input & Movement
        moveDir = playerActions.Gameplay.Move.ReadValue<Vector2>();
        Move(moveDir, moveSpeed);

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
            if(attackTimer >= 0.39f)
            {
                attackObj.SetActive(false);
            }
            if (attackTimer >= attackCooldown)
            {
                hasAttacked = false;
            }
        }
        #region Debug Controls
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Equals))
            Heal(1);
        if (Input.GetKeyDown(KeyCode.R))
            SceneController.ReloadCurrentScene();
#endif
        #endregion
    }

    void FixedUpdate()
    {
        bool wasGrounded = isGrounded;
        isGrounded = false;

        // The player is grounded if a circlecast to the groundCheck position hits anything designated on the ground layer
        Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheck.position, groundCheckRadius, groundLayer);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
            {
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
        }
        else
        {
            bodyVFX.SetVector3("PlayerDirection", -rb2d.velocity);
            bodyVFX.SetFloat("PlayerSpeed", rb2d.velocity.x / 4);
        }
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
        #endregion

        if (Mathf.Abs(moveDir.x) <= 0.2f && Mathf.Abs(moveDir.y) <= 0.2f)
        {
            if (isFacingRight)
                moveDir = Vector2.right;
            else
                moveDir = -Vector2.right;
        }
        slashPos = moveDir * slashDistance;

        // Slash Sprite Position
        attackObj.transform.localPosition = slashPos;

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
            }
            else
            {
                // Gives the player a little push forward if they don't hit anything.
                rb2d.AddForce(moveDir * (knockbackForce * 10));
            }
        }
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
    /// Spawns a bubble and handles the positioning of said bubble.
    /// </summary>
    void OnBubble()
    {
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
        if (isDashing)
            return;

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
        if (!isGrounded && coyoteTime > 0.1f)
            return;

        isGrounded = false;
        isJumping = true;

        lastPlaceBeforeJump = transform.position;

        rb2d.AddForce(new Vector2(0f, jumpHeight * 100));
    }

    /// <summary>
    /// Stops the player from jumping when they release the jump button.
    /// </summary>
    void StopJump()
    {
        if (!isGrounded && isJumping && rb2d.velocity.y > 0)
        {
            rb2d.velocity = new Vector2(rb2d.velocity.x, rb2d.velocity.y/2);
            isJumping = false;
        }
    }

    /// <summary>
    /// Moves the player based on moveSpeed and the direction the controller is aiming (between -1 and 1 on XY axis).
    /// </summary>
    /// <param name="moveDir"></param>
    /// <param name="moveSpeed"></param>
    void Move(Vector2 moveDir, float moveSpeed)
    {
        if (moveDir.x < 0 && isFacingRight)
            FlipSprite();
        if (moveDir.x > 0 && !isFacingRight)
            FlipSprite();

        if (!isGrounded)
            moveDir = moveDir / airSpeedDivider;

        if (Mathf.Abs(moveDir.x) < 0.2f)
        {
            // Move the character by finding the target velocity
            Vector3 targetVelocity = Vector2.zero + new Vector2(0, rb2d.velocity.y);
            // And then smoothing it out and applying it to the character
            rb2d.velocity = Vector3.SmoothDamp(rb2d.velocity, targetVelocity, ref velocity, 0.1f);
        }
        else
        {
            // Move the character by finding the target velocity
            Vector3 targetVelocity = new Vector2((moveDir.x * moveSpeed), rb2d.velocity.y);
            // And then smoothing it out and applying it to the character
            rb2d.velocity = Vector3.SmoothDamp(rb2d.velocity, targetVelocity, ref velocity, movementSmoothing);
        }
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

    void Die()
    {
        SceneController.ReloadCurrentScene();
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