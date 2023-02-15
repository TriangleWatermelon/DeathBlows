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
    Collider2D attackCollider;

    [BoxGroup("Bouncer/Behavior")]
    [SerializeField] float fallSpeed;
    [BoxGroup("Bouncer/Behavior")]
    [SerializeField] float risingTimeMax;
    float risingTimer = 0;

    Vector3 directionToPlayer;
    Vector3 lastAttackObjPosition;
    float risingAttackRotation;

    GameObject playerObj;
    bool playerPositionSet = false;

    private void Start()
    {
        playerObj = FindObjectOfType<PlayerController>().gameObject;
        attackCollider = attackObj.GetComponent<Collider2D>();
        attackCollider.enabled = false;
    }

    private void FixedUpdate()
    {
        if (isDead)
            return;

        // This switch controls the various update loops that occur for each state.
        switch (motionState)
        {
            case state.idle:
                RaycastHit2D pHit = Physics2D.Raycast(transform.position, lookDirection, pursuingDistance, ~attackLayerMask);
                if (pHit.collider != null)
                {
                    if (pHit.collider.gameObject.CompareTag("Player"))
                    {
                        AdjustGravity(0);
                        lastAttackObjPosition = attackObj.transform.position;
                        attackCollider.enabled = false;
                        motionState = state.pursuing;
                    }
                }
                break;
            case state.pursuing:
                rb2d.velocity = Vector3.up * moveSpeed;

                risingTimer += Time.deltaTime;
                if (risingTimer >= risingTimeMax)
                {
                    attackCollider.enabled = true;
                    motionState = state.attacking;
                    AdjustGravity(1);
                }

                attackObj.transform.position = Vector3.Lerp(lastAttackObjPosition,
                    new Vector3(transform.position.x, transform.position.y - 0.8f), risingTimer / risingTimeMax);
                float zAngle = Mathf.SmoothDampAngle(attackObj.transform.eulerAngles.z, -90, ref risingAttackRotation, risingTimeMax / 2.5f);
                attackObj.transform.rotation = Quaternion.Euler(0, 0, zAngle);
                break;
            case state.attacking:
                attackTimer += Time.deltaTime;
                if (attackTimer > attackDelay)
                {
                    SetAttackPosition();
                    transform.position += directionToPlayer * (attackSpeed / 10);
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
                    lastAttackObjPosition = attackObj.transform.position;
                    attackCollider.enabled = false;
                    motionState = state.pursuing;
                }
                break;
            case state.frozen:
                transform.position = brookEffectPosition;
                brookEffectTimer += Time.deltaTime;
                if (brookEffectTimer >= 1)
                {
                    entityCollider.enabled = true;
                    TakeDamage(brookEffectDamage);
                    brookEffectActive = false;

                    risingTimer = 0;
                    lastAttackObjPosition = attackObj.transform.position;
                    motionState = state.pursuing;
                }
                break;
        }
    }

    private void SetAttackPosition()
    {
        if (playerPositionSet)
            return;

        playerPositionSet = true;
        directionToPlayer = (playerObj.transform.position - transform.position).normalized;

        attackObj.transform.localPosition = directionToPlayer / 1.2f;

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

    public override void ActivateBrookEffect(float _damage)
    {
        base.ActivateBrookEffect(_damage);
        entityCollider.enabled = false;
    }

    public override void OnCollisionEnter2D(Collision2D collision)
    {
        base.OnCollisionEnter2D(collision);

        switch (motionState)
        {
            case state.pursuing:
                motionState = state.attacking;
                AdjustGravity(1);
                break;
            case state.attacking:
                if (collision.gameObject.CompareTag("Bubble"))
                {
                    if (isRight)
                        KnockbackEntity(Vector2.right * 10);
                    else
                        KnockbackEntity(-Vector2.right * 10);
                }
                ResetAttackStates();
                motionState = state.waiting;
                break;
            case state.dying:
                if (collision.gameObject.CompareTag("Ground"))
                {
                    attackCollider.enabled = false;
                    this.GetComponent<Collider2D>().enabled = false;
                    rb2d.velocity = Vector2.zero;
                    rb2d.isKinematic = true;
                }
                break;
        }
    }

    protected override void Die()
    {
        base.Die();
        rb2d.AddForce(Vector2.up);
        AdjustGravity(1);
    }
}