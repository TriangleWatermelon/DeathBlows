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

    void FixedUpdate()
    {
        if (isDead)
            return;

        // This switch controls the various update loops that occur for each state.
        switch (motionState)
        {
            case state.idle:
                bodyAnimator.SetBool("isMoving", false);

                RaycastHit2D pHit = Physics2D.Raycast(transform.position, lookDirection, pursuingDistance, ~attackLayerMask);
                if (pHit.collider != null)
                {
                    if (pHit.collider.gameObject.CompareTag("Player"))
                    {
                        motionState = state.pursuing;
                        bodyAnimator.SetBool("isMoving", true);

                        CheckSpriteDirection();
                    }
                }
                break;
            case state.pursuing:
                if (!isHit)
                {
                    if (isRight)
                        Move(Vector2.right);
                    else
                        Move(Vector2.left);

                    RaycastHit2D aHit = Physics2D.Raycast(transform.position, lookDirection, attackDistance, ~attackLayerMask);
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
                    hitTimer += Time.deltaTime;
                    if (hitTimer >= stunTime)
                        isHit = false;
                }

                CheckSpriteDirection();
                break;
            case state.attacking:
                attackDelay += Time.deltaTime;
                if (leapDelay < attackDelay && attackDelay < 0.5f)
                    rb2d.AddForce((lookDirection * leapDistance) * 30);
                else if (attackDelay > 1.5f)
                    motionState = state.pursuing;
                break;
            case state.frozen:
                transform.position = brookEffectPosition;
                brookEffectTimer += Time.deltaTime;
                if (brookEffectTimer >= 1)
                {
                    entityCollider.enabled = true;
                    TakeDamage(brookEffectDamage);
                    brookEffectActive = false;

                    motionState = state.pursuing;
                }
                break;
        }

        //Are we standing on someting?
        isGrounded = CheckGround.CheckForGround(groundCheck.position, groundCheckRadius, groundLayer, gameObject);
    }

    protected override void Die()
    {
        base.Die();

        // Stop physics and movement
        entityCollider.enabled = false;
        rb2d.isKinematic = true;
        rb2d.velocity = Vector2.zero;
        bodyAnimator.SetBool("isMoving", false);
    }
}
