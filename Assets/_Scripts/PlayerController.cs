using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using UnityEngine.VFX;

public class PlayerController : MonoBehaviour
{
    #region Visuals
    [TitleGroup("Main")]
    [BoxGroup("Main/Visuals")]
    [PreviewField(70, ObjectFieldAlignment.Left)]
    [SerializeField] Sprite playerSprite;
    SpriteRenderer spriteRenderer;
    [TitleGroup("Main")]
    [BoxGroup("Main/Visuals")]
    [SerializeField] VisualEffect bodyVFX;
    Vector2 idleParticleDirection = new Vector2 (0 , 10);
    float idleParticleSpeed = 1;
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
    #endregion

    #region Control
    [TitleGroup("Control")]
    [BoxGroup("Control/Movement")]
    [Range(0, 3)]
    [Tooltip("This will slow the player's movement in the air by dividing the input value")]
    [SerializeField] float airSpeedDivider;
    PlayerActions playerActions;
    Vector2 moveDir;
    bool isAttacking = false;
    bool isBubbling = false;
    [TitleGroup("Control")]
    [BoxGroup("Control/Movement")]
    [SerializeField] float movementSmoothing;
    Vector3 velocity = Vector3.zero;
    bool isGrounded;
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
    #endregion

    public UnityEvent OnLandEvent;

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        spriteRenderer.sprite = playerSprite;

        playerActions = new PlayerActions();

        rb2d = GetComponent<Rigidbody2D>();

        if (OnLandEvent == null)
            OnLandEvent = new UnityEvent();
    }

    void Start()
    {
        health = maxHealth;

        playerActions.Gameplay.Jump.performed += ctx => OnJump();
    }

    private void Update()
    {
        //Input & Movement
        moveDir = playerActions.Gameplay.Move.ReadValue<Vector2>();
        Move(moveDir, moveSpeed);

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
    }

    void FixedUpdate()
    {
        bool wasGrounded = isGrounded;
        isGrounded = false;

        // The player is grounded if a circlecast to the groundcheck position hits anything designated on the ground layer
        Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheck.position, groundCheckRadius, groundLayer);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
            {
                isGrounded = true;
                if (!wasGrounded)
                    OnLandEvent.Invoke();
            }
        }
    }

    void OnJump()
    {
        if (isGrounded)
        {
            isGrounded = false;
            rb2d.AddForce(new Vector2(0f, jumpHeight * 100));
        }
    }

    public void Move(Vector2 moveDir, float moveSpeed)
    {
        if (moveDir.x < 0 && isFacingRight)
        {
            FlipSprite();
        }
        if (moveDir.x > 0 && !isFacingRight)
        {
            FlipSprite();
        }
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

    void FlipSprite()
    {
        isFacingRight = !isFacingRight;

        Vector3 flipScale = transform.localScale;
        flipScale.x *= -1;
        transform.localScale = flipScale;
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
