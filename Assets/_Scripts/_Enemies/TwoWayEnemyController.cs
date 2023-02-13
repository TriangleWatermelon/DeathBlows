using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;

public class TwoWayEnemyController : Entity
{
    [TitleGroup("Mistake")]
    [BoxGroup("Mistake/Attacking")]
    [SerializeField] float leapDistance;
    [BoxGroup("Mistake/Attacking")]
    [SerializeField] float leapDelay;
    float attackDelay;

    [BoxGroup("Mistake/Components")]
    [SerializeField] ParticleSystem smokeParticles;

    private void Update()
    {
        if (isDead)
        {
            rb2d.velocity = Vector2.zero;
            return;
        }

        // This switch controls the various update loops that occur for each state.
        switch (motionState)
        {
            case state.idle:
                bodyAnimator.SetBool("isMoving", false);

                RaycastHit2D pHit = Physics2D.Raycast(transform.position, lookDirection, pursuingDistance);
                if (pHit.collider != null)
                {
                    if (pHit.collider.gameObject.CompareTag("Player"))
                    {
                        motionState = state.pursuing;
                        bodyAnimator.SetBool("isMoving", true);
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
                    if (aHit.collider != null)
                    {
                        if (aHit.collider.gameObject.CompareTag("Player"))
                        {
                            motionState = state.attacking;
                            rb2d.velocity = Vector2.zero;
                            attackDelay = 0;
                            smokeParticles.Play();
                            bodyAnimator.SetTrigger("isAttacking");
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

                CheckSpriteDirection();
                break;
            case state.attacking:
                attackDelay += Time.deltaTime;
                if (leapDelay < attackDelay && attackDelay < 0.5f)
                    rb2d.AddForce(lookDirection * leapDistance);
                else if (attackDelay > 1.5f)
                    motionState = state.pursuing;
                break;
            case state.frozen:
                transform.position = brookEffectPosition;
                break;
        }

        if (brookEffectActive)
        {
            brookEffectTimer += Time.deltaTime;
            if (brookEffectTimer >= 1)
            {
                entityCollider.enabled = true;
                TakeDamage(brookEffectDamage);
                brookEffectActive = false;

                motionState = state.pursuing;
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
        if (isDead)
            return;

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
