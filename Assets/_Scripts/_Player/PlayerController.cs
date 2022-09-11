using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using UnityEngine.VFX;
using System;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    #region Visuals
    [TitleGroup("Main")]
    [BoxGroup("Main/Visuals")]
    [PreviewField(70, ObjectFieldAlignment.Left)]
    [SerializeField] Sprite playerSprite;
    [Space]
    [TitleGroup("Main")]
    [BoxGroup("Main/Visuals")]
    [SerializeField] GameObject playerSpriteObj;
    SpriteRenderer playerSpriteRenderer;
    [Space]
    [TitleGroup("Main")]
    [BoxGroup("Main/Visuals")]
    [SerializeField] VisualEffect bodyVFX;
    Vector2 idleParticleDirection = new Vector2 (0 , 10);
    float idleParticleSpeed = 1;
    [Space]
    [TitleGroup("Main")]
    [BoxGroup("Main/Visuals")]
    [PreviewField(70, ObjectFieldAlignment.Left)]
    [SerializeField] Sprite attackSprite;
    [Space]
    [TitleGroup("Main")]
    [BoxGroup("Main/Visuals")]
    [SerializeField] GameObject attackObj;
    SpriteRenderer attackSpriteRenderer;
    [Space]
    [TitleGroup("Main")]
    [BoxGroup("Main/Visuals")]
    [PreviewField(70, ObjectFieldAlignment.Left)]
    [SerializeField] Sprite bubbleSprite;
    [Space]
    [TitleGroup("Main")]
    [BoxGroup("Main/Visuals")]
    [SerializeField] GameObject bubbleObj;
    SpriteRenderer bubbleSpriteRenderer;
    [Space]
    [TitleGroup("Main")]
    [BoxGroup("Main/Visuals")]
    [PreviewField(70, ObjectFieldAlignment.Left)]
    [SerializeField] Sprite impactSprite;
    [Space]
    [TitleGroup("Main")]
    [BoxGroup("Main/Visuals")]
    [SerializeField] GameObject impactObj;
    SpriteRenderer impactSpriteRenderer;
    #endregion

    #region STATS
    [TitleGroup("Main")]
    [BoxGroup("Main/Stats")]
    [Tooltip("I'll never die!")]
    [SerializeField] float maxHealth;
    private float health;
    [TitleGroup("Main")]
    [BoxGroup("Main/Stats")]
    [Tooltip("ZOOOOOOOOOM!!!")]
    [SerializeField] float moveSpeed;
    [TitleGroup("Main")]
    [BoxGroup("Main/Stats")]
    [Tooltip("...How high?")]
    [SerializeField] float jumpHeight;
    [TitleGroup("Main")]
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
    [TitleGroup("Control")]
    [BoxGroup("Control/Movement")]
    [SerializeField] float movementSmoothing;
    Vector3 velocity = Vector3.zero;
    bool isGrounded;
    bool isJumping = false;
    [TitleGroup("Control")]
    [BoxGroup("Control/Movement")]
    [Tooltip("This is an empty GameObject placed at the bottom of the player's collider")]
    [SerializeField] Transform groundCheck;
    const float groundCheckRadius = 0.2f;
    [TitleGroup("Control")]
    [BoxGroup("Control/Movement")]
    [Tooltip("Whatever layer you use for the ground")]
    [SerializeField] LayerMask groundLayer;
    Rigidbody2D rb2d;
    bool isFacingRight = true;
    [TitleGroup("Control")]
    [BoxGroup("Control/Movement")]
    [SerializeField] float stunTime;
    float hitTimer;
    #endregion

    #region Combat Control
    [TitleGroup("Control")]
    [BoxGroup("Control/Combat")]
    [SerializeField] float slashDistance;
    bool isHit = false;
    bool hasAttacked = false;
    [TitleGroup("Control")]
    [BoxGroup("Control/Combat")]
    [SerializeField] float attackCooldown;
    float attackTimer;
    [TitleGroup("Control")]
    [BoxGroup("Control/Combat")]
    [SerializeField] float knockbackForce;
    Vector2 slashPos;
    #endregion

    #region Bubble Control
    [HideInInspector]
    public bool isBubbling = false;
    Vector2 bubblePos;
    Vector2 bubbleOffset = new Vector2(2, 0);
    #endregion

    [Space]
    public UnityEvent OnDeath;

    PlayerUI playerUI;

    void Awake()
    {
        //Sprites 'n Things
        playerSpriteRenderer = playerSpriteObj.GetComponent<SpriteRenderer>();
        playerSpriteRenderer.sprite = playerSprite;
        attackObj = Instantiate(attackObj, transform);
        attackObj.SetActive(false);
        attackSpriteRenderer = attackObj.GetComponent<SpriteRenderer>();
        attackSpriteRenderer.sprite = attackSprite;
        bubbleObj = Instantiate(bubbleObj);
        bubbleObj.SetActive(false);
        bubbleSpriteRenderer = bubbleObj.GetComponentInChildren<SpriteRenderer>();
        bubbleSpriteRenderer.sprite = bubbleSprite;
        impactObj = Instantiate(impactObj, transform);
        impactObj.SetActive(false);
        impactSpriteRenderer = impactObj.GetComponent<SpriteRenderer>();
        impactSpriteRenderer.sprite = impactSprite;

        //Physics
        rb2d = GetComponent<Rigidbody2D>();

        health = maxHealth;

        //Input Stuff
        playerActions = new PlayerActions();
        playerActions.Gameplay.Jump.performed += ctx => OnJump();
        playerActions.Gameplay.Jump.canceled += ctx => StopJump();
        playerActions.Gameplay.Slash.performed += ctx => HoldSlash();
        playerActions.Gameplay.Slash.canceled += ctx => OnSlash();
        playerActions.Gameplay.Bubble.performed += ctx => OnBubble();
    }

    //Everything in Start needs to be here to avoid racing
    private void Start()
    {
        //UI Stuff
        playerUI = GetComponentInChildren<PlayerUI>();
        playerUI.AdjustPlayerHealthUI(maxHealth);
    }

    void Update()
    {
        //Input & Movement
        if (!isHit)
        {
            moveDir = playerActions.Gameplay.Move.ReadValue<Vector2>();
            Move(moveDir, moveSpeed);
        }
        else
        {
            hitTimer += Time.deltaTime;
            if(hitTimer >= stunTime)
            {
                isHit = false;
            }
        }

        //Attack Delay
        if (hasAttacked)
        {
            attackTimer += Time.deltaTime;
            if(attackTimer >= 0.1f)
            {
                impactObj.SetActive(false);
            }
            if(attackTimer >= 0.2f)
            {
                attackObj.SetActive(false);
            }
            if(attackTimer >= attackCooldown)
            {
                hasAttacked = false;
            }
        }

        //Body VFX
        if (moveDir.x == 0 && moveDir.y == 0)
        {
            bodyVFX.SetVector3("PlayerDirection", idleParticleDirection);
            bodyVFX.SetFloat("PlayerSpeed", idleParticleSpeed);
        }
        else
        {
            bodyVFX.SetVector3("PlayerDirection", -rb2d.velocity);
            bodyVFX.SetFloat("PlayerSpeed", rb2d.velocity.x/4);
        }

        if(Input.GetKeyDown(KeyCode.Equals))
        {
            Heal(1);
        }
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
            }
        }
    }

    /// <summary>
    /// Triggered when the player hits the attack button.
    /// Handles attack warmup effects and animations.
    /// </summary>
    void HoldSlash()
    {
        //Do stuff
    }

    /// <summary>
    /// Triggered when the player releases the attack button.
    /// Handles the player attack (sprites and effects) based on the direction of the controller (-1 to 1 on XY axis).
    /// </summary>
    void OnSlash()
    {
        if (!hasAttacked)
        {
            if(Mathf.Abs(moveDir.x) <= 0.2f && Mathf.Abs(moveDir.y) <= 0.2f)
            {
                if (isFacingRight)
                {
                    moveDir = Vector2.right;
                }
                else
                {
                    moveDir = -Vector2.right;
                }
            }
            slashPos = moveDir * slashDistance;

            //Slash Sprite Position
            attackObj.transform.localPosition = new Vector2(slashPos.x, slashPos.y);

            //Does it hit?
            RaycastHit2D hit = Physics2D.Raycast(transform.position, slashPos, slashDistance);
            if (hit.collider != null)
            {
                //Slash and Impact Sprite Positions
                attackObj.transform.position = hit.collider.ClosestPoint(transform.position);
                impactObj.transform.position = hit.collider.ClosestPoint(transform.position);
                impactObj.transform.rotation = Quaternion.Euler(0, 0, UnityEngine.Random.Range(0, 360));
                impactObj.SetActive(true);

                if (hit.collider.GetComponent<Entity>() != null)
                {
                    hit.collider.GetComponent<Entity>().TakeDamage(damage);
                    impactObj.transform.position = hit.collider.transform.position;
                }

                //Knockback the player on successful contact
                rb2d.velocity = (rb2d.velocity / 2) + (-moveDir * knockbackForce);
            }
            attackTimer = 0;
            hasAttacked = true;
            attackObj.SetActive(true);

            //Slash Sprite Rotation
            float x = moveDir.x;
            float y = moveDir.y;
            float rads = Mathf.Atan2(y, x);
            float degrees = rads * Mathf.Rad2Deg;
            attackObj.transform.localEulerAngles = new Vector3(0, 0, degrees);
        }
    }


    /// <summary>
    /// Spawns a bubble and handles the positioning of said bubble.
    /// </summary>
    void OnBubble()
    {
        if (!isBubbling)
        {
            bubblePos = transform.position + new Vector3(moveDir.x, 0);
            if (isFacingRight)
            {
                bubblePos += bubbleOffset;
            }
            else
            {
                bubblePos -= bubbleOffset;
            }
            bubbleObj.SetActive(true);
            bubbleObj.transform.position = bubblePos;
            isBubbling = true;
        }
    }

    /// <summary>
    /// Makes the player jump.
    /// </summary>
    void OnJump()
    {
        if (isGrounded)
        {
            isGrounded = false;
            isJumping = true;
            rb2d.AddForce(new Vector2(0f, jumpHeight * 100));
        }
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
    /// </summary>
    void FlipSprite()
    {
        isFacingRight = !isFacingRight;

        Vector3 flipScale = playerSpriteObj.transform.localScale;
        flipScale.x *= -1;
        playerSpriteObj.transform.localScale = flipScale;
    }

    /// <summary>
    /// Applies damage to the entity.
    /// Starts the hit timer to prevent multiple hits in a single attack.
    /// Invokes death if health is less than 0 after applying damage.
    /// </summary>
    /// <param name="_damage"></param>
    public void TakeDamage(float _damage)
    {
        if (!isHit)
        {
            health -= _damage;
            isHit = true;
            if (health <= 0)
            {
                Die();
                OnDeath.Invoke();
            }

            playerUI.AdjustHealth(health);
        }
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

        playerUI.AdjustHealth(health);
    }

    void Die()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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