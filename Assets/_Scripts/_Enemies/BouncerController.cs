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

    [BoxGroup("Bouncer/Behavior")]
    [SerializeField] float fallSpeed;
    [BoxGroup("Bouncer/Behavior")]
    [SerializeField] float risingTimeMax;
    float risingTimer;

    Vector3 directionToPlayer;

    Vector2 risingDirectionRight = new Vector2(0.25f, 1);
    Vector2 risingDirectionLeft = new Vector2(-0.25f, 1);

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
                //bodyAnimator

                RaycastHit2D pHit = Physics2D.Raycast(transform.position, lookDirection, pursuingDistance);
                if (pHit.collider != null)
                {
                    if (pHit.collider.gameObject.CompareTag("Player"))
                    {
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
                        motionState = state.attacking;
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

                motionState = state.pursuing;
            }
        }
    }

    private void SetAttackPosition()
    {
        if (!playerPositionSet)
        {
            playerPositionSet = true;
            directionToPlayer = (playerObj.transform.position - transform.position).normalized;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        motionState = state.pursuing;
    }
}
