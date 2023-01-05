using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;

public class TwoWayEnemyController : Entity
{
    [TitleGroup("Two-Way Specific")]
    [BoxGroup("Two-Way Specific/Attacking")]
    [SerializeField] float leapDistance;
    [BoxGroup("Two-Way Specific/Attacking")]
    [SerializeField] float leapDelay;
    float attackDelay;

    Vector2 lookDirection;
    bool isDead = false;

    GameObject latestGroundObj;
    [BoxGroup("Two-Way Specific/Components")]
    [SerializeField] ParticleSystem smokeParticles;

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
            if (isRight)
                lookDirection = Vector2.right;
            else
                lookDirection = -Vector2.right;

            // This switch controls the various update loops that occur for each state.
            switch (motionState)
            {
                case state.idle:
                    animator.SetBool("isMoving", false);

                    RaycastHit2D pHit = Physics2D.Raycast(transform.position, lookDirection, pursuingDistance);
                    Debug.DrawRay(transform.position, lookDirection * pursuingDistance, Color.blue);
                    if (pHit.collider != null)
                    {
                        if (pHit.collider.gameObject.CompareTag("Player"))
                        {
                            motionState = state.pursuing;
                            animator.SetBool("isMoving", true);
                        }
                    }
                    break;
                case state.pursuing:
                    if (!isHit)
                    {
                        if (isRight)
                            Move(Vector2.right);
                        else
                            Move(-Vector2.right);

                        RaycastHit2D aHit = Physics2D.Raycast(transform.position, lookDirection, attackDistance);
                        Debug.DrawRay(transform.position, lookDirection * attackDistance, Color.red);
                        if (aHit.collider != null)
                        {
                            if (aHit.collider.gameObject.CompareTag("Player"))
                            {
                                motionState = state.attacking;
                                rb2d.velocity = Vector2.zero;
                                attackDelay = 0;
                                smokeParticles.Play();
                                animator.SetTrigger("isAttacking");
                            }
                        }
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
                    break;
                case state.attacking:
                    attackDelay += Time.deltaTime;
                    if (leapDelay < attackDelay && attackDelay < 0.5f)
                        rb2d.AddForce(lookDirection * leapDistance);
                    else if (attackDelay > 1.5f)
                        motionState = state.idle;
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
                latestGroundObj = colliders[i].gameObject;
            }
        }
    }

    public void Die()
    {
        if (!isDead)
        {
            isDead = true;

            // Stop physics and movement
            this.GetComponent<Collider2D>().enabled = false;
            rb2d.isKinematic = true;
            rb2d.velocity = Vector2.zero;

            // Match ground rotation
            transform.rotation = latestGroundObj.transform.rotation;

            // Flip for animation to look more convincing with rotation
            if (latestGroundObj.transform.rotation.z > 0 && isRight)
                FlipSprite();
            else if (latestGroundObj.transform.rotation.x < 0 && !isRight)
                FlipSprite();
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

    // This handles the edge interactions
    private void OnTriggerEnter2D(Collider2D collision)
    {
        FlipSprite();
    }
}
