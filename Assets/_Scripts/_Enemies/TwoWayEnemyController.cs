using UnityEngine;
using Sirenix.OdinInspector;

public class TwoWayEnemyController : Entity
{
    #region Control
    [TitleGroup("Control")]
    [BoxGroup("Control/Stats")]
    [SerializeField] float damage;
    [BoxGroup("Control/Movement")]
    [SerializeField] float moveSpeed;
    bool isGrounded;
    [BoxGroup("Control/Movement")]
    [Tooltip("This is an empty GameObject placed at the bottom of the enemy's collider")]
    [SerializeField] Transform groundCheck;
    const float groundCheckRadius = 0.2f;
    [BoxGroup("Control/Movement")]
    [Tooltip("Whatever layer you use for the ground")]
    [SerializeField] LayerMask groundLayer;
    bool isDead = false;
    #endregion

    state motionState;

    private void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();

        if (!isRight)
        {
            FlipSprite();
            isRight = !isRight;
        }

        motionState = state.idle;
    }

    private void Update()
    {
        if (!isDead)
        {
            switch (motionState)
            {
                case state.idle:
                    animator.SetBool("isMoving", false);
                    break;
                case state.walking:
                    if (!isHit)
                    {
                        if (isRight)
                            Move(Vector2.right);
                        else
                            Move(-Vector2.right);
                    }
                    else
                    {
                        rb2d.velocity = new Vector2(rb2d.velocity.x / 4, rb2d.velocity.y / 4);
                        hitTimer += Time.deltaTime;
                        if (hitTimer >= stunTime)
                            isHit = false;
                    }

                    if (rb2d.velocity.x > 0 && !isRight)
                        FlipSprite();
                    if (rb2d.velocity.x < 0 && isRight)
                        FlipSprite();

                    animator.SetBool("isMoving", true);
                    break;
            }
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

    private void Move(Vector2 moveDir)
    {
        if(isGrounded)
            rb2d.velocity = moveDir * moveSpeed;
    }

    public void Die()
    {
        if (!isDead)
        {
            isDead = true;
            this.GetComponent<Collider2D>().enabled = false;
            rb2d.isKinematic = true;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isDead)
        {
            collision.gameObject.gameObject.GetComponent<PlayerController>().TakeDamage(damage);
        }
        else if (!collision.gameObject.CompareTag("Ground") && !isDead)
        {
            FlipSprite();
        }
    }

    // This handles the border interactions
    private void OnTriggerEnter2D(Collider2D collision)
    {
        FlipSprite();
    }
}
