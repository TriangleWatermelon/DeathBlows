using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;

public class BouncerController : Entity
{
    [TitleGroup("Bouncer")]
    [BoxGroup("Bouncer/Attacking")]
    [SerializeField] float attackDelay;
    float attackTimer;
    [BoxGroup("Bouncer/Attacking")]
    [SerializeField] float attackSpeed;
    [BoxGroup("Bouncer/Attacking")]
    [SerializeField] float attackWaitTime;
    float waitTimer;

    [BoxGroup("Bouncer/Behavior")]
    [SerializeField] float fallSpeed;
    [BoxGroup("Bouncer/Behavior")]
    [SerializeField] float risingTimeMax;
    float risingTimer;

    Vector3 directionToPlayer;

    Vector2 risingDirectionRight = new Vector2(0.3f, 1);
    Vector2 risingDirectionLeft = new Vector2(-0.3f, 1);

    GameObject playerObj;
    bool playerPositionSet = false;

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
                RaycastHit2D pHit = Physics2D.Raycast(transform.position, lookDirection, pursuingDistance);
                if (pHit.collider != null)
                {
                    if (pHit.collider.gameObject.CompareTag("Player"))
                    {
                        AdjustGravity(0);
                        motionState = state.pursuing;
                        playerObj = pHit.collider.gameObject;
                    }
                }
                break;
            case state.pursuing:
                if (!isHit)
                {
                    if (isRight)
                        transform.position += (Vector3)risingDirectionRight * (moveSpeed / 1000);
                    else
                        transform.position += (Vector3)risingDirectionLeft * (moveSpeed / 1000);

                    risingTimer += Time.deltaTime;
                    if (risingTimer >= risingTimeMax)
                    {
                        motionState = state.attacking;
                        AdjustGravity(1);
                    }
                }
                else
                {
                    transform.position -= Vector3.down * (fallSpeed / 1000);
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
                attackTimer += Time.deltaTime;
                if (attackTimer > attackDelay)
                {
                    SetAttackPosition();
                    transform.position += directionToPlayer * (attackSpeed / 1000);
                    RaycastHit2D dHit = Physics2D.Raycast(groundCheck.transform.position, Vector2.down, 1, ~groundLayer);
                    RaycastHit2D lHit = Physics2D.Raycast(groundCheck.transform.position, -Vector2.right, 1, ~groundLayer);
                    RaycastHit2D rHit = Physics2D.Raycast(groundCheck.transform.position, Vector2.right, 1, ~groundLayer);
                    if (dHit || lHit || rHit)
                        rb2d.velocity = Vector2.zero;
                }
                else
                    rb2d.velocity = Vector2.zero;
                break;
            case state.waiting:
                waitTimer += Time.deltaTime;
                if (waitTimer >= attackWaitTime)
                {
                    AdjustGravity(0);
                    motionState = state.pursuing;
                }
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

                motionState = state.idle;
            }
        }
    }

    private void SetAttackPosition()
    {
        if (playerPositionSet)
            return;

        playerPositionSet = true;
        directionToPlayer = (playerObj.transform.position - transform.position).normalized;
    }

    private void AdjustGravity(float _val) => rb2d.gravityScale = _val;

    private void ResetAttackStates()
    {
        playerPositionSet = false;
        AdjustGravity(1);
        attackTimer = 0;
        waitTimer = 0;
        risingTimer = 0;
    }

    public override void OnCollisionEnter2D(Collision2D collision)
    {
        base.OnCollisionEnter2D(collision); // Make sure we keep the base functionality.

        switch (motionState)
        {
            case state.pursuing:
                motionState = state.attacking;
                AdjustGravity(1);
                break;
            case state.attacking:
                ResetAttackStates();
                motionState = state.waiting;
                break;
        }
    }
}