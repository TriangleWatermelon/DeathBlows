using UnityEngine;
using Sirenix.OdinInspector;

public class TwoWayEnemyController : Entity
{
    #region Visuals
    [BoxGroup("Visuals")]
    [PreviewField(70, ObjectFieldAlignment.Left)]
    [SerializeField] Sprite sprite;
    SpriteRenderer spriteRenderer;
    #endregion

    #region Control
    [BoxGroup("Control/Stats")]
    [SerializeField] float damage;
    [BoxGroup("Control/Movement")]
    [SerializeField] float moveSpeed;
    Vector2 horizontal = new Vector2 (1, 0);
    bool isGrounded;
    [BoxGroup("Control/Movement")]
    [Tooltip("This is an empty GameObject placed at the bottom of the enemy's collider")]
    [SerializeField] Transform groundCheck;
    const float groundCheckRadius = 0.2f;
    [BoxGroup("Control/Movement")]
    [Tooltip("Whatever layer you use for the ground")]
    [SerializeField] LayerMask groundLayer;
    [BoxGroup("Control")]
    [SerializeField] bool isRight = true;
    bool isDead = false;
    Vector2 corpsePosition;
    #endregion

    private void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
    }

    private void Update()
    {
        if (!isDead)
        {
            if (!isHit)
            {
                if (isRight)
                    Move(horizontal);
                else
                    Move(-horizontal);
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
        }
        else
        {
            if(corpsePosition != Vector2.zero)
                transform.position = corpsePosition;
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

    void FlipSprite()
    {
        isRight = !isRight;

        Vector3 flipScale = transform.localScale;
        flipScale.x *= -1;
        transform.localScale = flipScale;
    }

    public void Die()
    {
        if (!isDead)
        {
            spriteRenderer.gameObject.transform.Rotate(0, 0, -90);
            isDead = true;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.gameObject.GetComponent<PlayerController>().TakeDamage(damage);
        }
        else if (!collision.gameObject.CompareTag("Ground"))
        {
            isRight = !isRight;
        }
        if(isDead && collision.gameObject.CompareTag("Ground"))
        {
            corpsePosition = collision.collider.ClosestPoint(transform.position);
            this.GetComponent<Collider2D>().enabled = false;
        }
    }
}
