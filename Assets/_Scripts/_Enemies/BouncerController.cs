using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;

public class BouncerController : Entity
{
    [TitleGroup("Bouncer")]
    [BoxGroup("Bouncer/Attacking")]
    [SerializeField] float attackDelay;
    float attackTimer = 0;
    [BoxGroup("Bouncer/Attacking")]
    [SerializeField] float attackSpeed;
    [BoxGroup("Bouncer/Attacking")]
    [SerializeField] float attackWaitTime;
    float waitTimer = 0;
    [BoxGroup("Bouncer/Attacking")]
    [SerializeField] GameObject attackObj;

    [BoxGroup("Bouncer/Behavior")]
    [SerializeField] float fallSpeed;
    [BoxGroup("Bouncer/Behavior")]
    [SerializeField] float risingTimeMax;
    float risingTimer = 0;

    Vector3 directionToPlayer;

    GameObject playerObj;
    bool playerPositionSet = false;

    private void Start()
    {
        playerObj = FindObjectOfType<PlayerController>().gameObject;
    }

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
                    }
                }
                break;
            case state.pursuing:
                if (!isHit)
                {
                    rb2d.velocity = Vector3.up * moveSpeed;

                    risingTimer += Time.deltaTime;
                    if (risingTimer >= risingTimeMax)
                    {
                        motionState = state.attacking;
                        AdjustGravity(1);
                    }
                }
                else
                {
                    hitTimer += Time.deltaTime;
                    if (hitTimer >= stunTime)
                        isHit = false;
                }
                break;
            case state.attacking:
                attackTimer += Time.deltaTime;
                if (attackTimer > attackDelay)
                {
                    SetAttackPosition();
                    transform.position += directionToPlayer * (attackSpeed / 1000);
                    RaycastHit2D dHit = Physics2D.Raycast(groundCheck.transform.position, Vector2.down, 10, ~groundLayer);
                    RaycastHit2D lHit = Physics2D.Raycast(groundCheck.transform.position, -Vector2.right, 10, ~groundLayer);
                    RaycastHit2D rHit = Physics2D.Raycast(groundCheck.transform.position, Vector2.right, 10, ~groundLayer);
                    if (dHit || lHit || rHit)
                        rb2d.velocity = Vector2.zero;
                }
                else
                {
                    rb2d.velocity = Vector2.zero;
                }
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

        attackObj.transform.localPosition = new Vector2(directionToPlayer.x / 2, directionToPlayer.y / 2);

        attackObj.transform.localEulerAngles = new Vector3(0, 0, MathHelper.FindDegreesForRotation(directionToPlayer));

        if (directionToPlayer.x > 0 && !isRight)
            FlipSprite();
        else if (directionToPlayer.x < 0 && isRight)
            FlipSprite();
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

    public override void TakeDamage(float _damage)
    {
        base.TakeDamage(_damage);
        if (motionState == state.pursuing)
            risingTimer = risingTimeMax;
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