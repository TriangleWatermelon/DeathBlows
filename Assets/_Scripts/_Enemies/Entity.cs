using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;

[RequireComponent(typeof(Rigidbody2D))]
public class Entity : MonoBehaviour
{
    public enum state
    {
        idle,
        walking,
        pursuing,
        attacking,
        waiting,
        frozen,
        dying
    }

    [TitleGroup("Entity Base")]
    [BoxGroup("Entity Base/Stats")]
    public float health;
    private float healthReset;

    [BoxGroup("Entity Base/Stats")]
    public float damage;

    [BoxGroup("Entity Base/Stats")]
    public float pursuingDistance;

    [BoxGroup("Entity Base/Stats")]
    public float attackDistance;

    [HideInInspector]
    public bool isHit;
    [HideInInspector]
    public float hitTimer;

    [BoxGroup("Entity Base/Movement")]
    public bool isRight = true;
    private bool isRightReset;

    [BoxGroup("Entity Base/Movement")]
    public float stunTime;
    [HideInInspector]
    public Rigidbody2D rb2d { get; private set; }
    [HideInInspector]
    public Collider2D entityCollider { get; private set; }

    [HideInInspector]
    public bool brookEffectActive;
    [HideInInspector]
    public float brookEffectTimer;
    [HideInInspector]
    public float brookEffectDamage { get; private set; }
    [HideInInspector]
    public Vector3 brookEffectPosition { get; private set; }

    [BoxGroup("Entity Base/Movement")]
    public float moveSpeed;
    [HideInInspector]
    public state motionState;

    [HideInInspector]
    public bool isGrounded;
    [BoxGroup("Entity Base/Movement")]
    [Tooltip("This is an empty GameObject placed at the bottom of the enemy's collider")]
    public Transform groundCheck;
    public const float groundCheckRadius = 0.2f;

    [BoxGroup("Entity Base/Movement")]
    [Tooltip("Whatever layer you use for the ground")]
    public LayerMask groundLayer;
    [HideInInspector]
    public GameObject latestGroundObj;

    [BoxGroup("Entity Base/Movement")]
    public float knockbackForce;
    [HideInInspector]
    public Vector2 lookDirection;

    [BoxGroup("Entity Base/Visual")]
    public GameObject spriteParentObj;

    [BoxGroup("Entity Base/Visual")]
    public Animator bodyAnimator;
    [BoxGroup("Entity Base/Visual")]
    public Animator iceAnimator;

    [BoxGroup("Entity Base/Attack")]
    public LayerMask attackLayerMask;

    [BoxGroup("Entity Base/Souls")]
    public int soulsToDrop;

    public UnityEvent OnDeath;
    [HideInInspector]
    public bool isDead = false;

    private Vector3 startPosition;

    PoolController poolController;

    private void Awake()
    {
        poolController = FindObjectOfType<PoolController>();
        rb2d = GetComponent<Rigidbody2D>();
        entityCollider = GetComponent<Collider2D>();

        if (!isRight)
        {
            FlipSprite();
            isRight = !isRight;
        }

        motionState = state.idle;
        isRightReset = isRight;
        healthReset = health;
        startPosition = transform.position;

        if (isRight)
            lookDirection = Vector2.right;
        else
            lookDirection = -Vector2.right;
    }

    /// <summary>
    /// Applies damage to the entity.
    /// Starts the hit timer to prevent multiple hits in a single attack.
    /// Invokes death if health is less than 0 after applying damage.
    /// </summary>
    /// <param name="_damage"></param>
    public virtual void TakeDamage(float _damage)
    {
        if (isDead)
            return;

        health -= _damage;
        hitTimer = 0;
        isHit = true;
        bodyAnimator.SetTrigger("TookDamage");
        if (health <= 0)
        {
            motionState = state.dying;
            OnDeath.Invoke();
            bodyAnimator.SetBool("isDead", true);
            Die();
        }
    }

    protected void EjectSoul(int _numberOfSouls)
    {
        for (int i = 0; i < _numberOfSouls; i++)
        {
            GameObject soul = poolController.PullFromPool(transform.position);
            soul.GetComponent<SuckToPlayer>().Activate();
        }
    }

    protected void CheckSpriteDirection()
    {
        if (rb2d.velocity.x > 0 && !isRight)
            FlipSprite();
        if (rb2d.velocity.x < 0 && isRight)
            FlipSprite();
    }

    /// <summary>
    /// Flips the entity sprite
    /// </summary>
    protected void FlipSprite()
    {
        isRight = !isRight;

        Vector3 flipScale = spriteParentObj.transform.localScale;
        flipScale.x *= -1;
        spriteParentObj.transform.localScale = flipScale;

        if (isRight)
            lookDirection = Vector2.right;
        else
            lookDirection = -Vector2.right;
    }

    /// <summary>
    /// Moves the entity in the provided direction
    /// </summary>
    /// <param name="moveDir"></param>
    protected void Move(Vector2 moveDir)
    {
        if (isGrounded)
            rb2d.velocity = moveDir * moveSpeed;
    }

    /// <summary>
    /// Applies knockback in the direction supplied.
    /// </summary>
    /// <param name="dir"></param>
    public void KnockbackEntity(Vector2 dir)
    {
        rb2d.velocity = (rb2d.velocity / 2) + (dir * knockbackForce);
    }

    /// <summary>
    /// Holds the entity in place and plays a random varient of the freeze animation.
    /// Holds value to apply damage later.
    /// </summary>
    /// <param name="_damage"></param>
    public virtual void ActivateBrookEffect(float _damage)
    {
        // Saving the position here because the rigidbody sinks without the collider enabled.
        brookEffectPosition = transform.position;
        motionState = state.frozen;
        rb2d.velocity = Vector2.zero;

        brookEffectDamage = _damage;
        brookEffectTimer = 0;
        brookEffectActive = true;
        entityCollider.enabled = false;

        // Increase the max of the range for each aniation type added.
        iceAnimator.SetTrigger($"Type {Random.Range(1, 3)}");
    }

    public virtual void OnCollisionEnter2D(Collision2D collision)
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
    public void OnTriggerEnter2D(Collider2D collision)
    {
        string tag = collision.tag;

        if (motionState != state.attacking && tag != "Hazard")
            FlipSprite();

        if (tag == "Hazard")
            TakeDamage(healthReset);
    }

    protected virtual void Die()
    {
        if (isDead)
            return;

        isDead = true;
        EjectSoul(soulsToDrop);
    }

    public virtual void ResetEntity()
    {
        transform.position = startPosition;
        isRight = isRightReset;
        if (!isRight)
        {
            FlipSprite();
            isRight = !isRight;
        }
        health = healthReset;
        isDead = false;
        motionState = state.idle;
        bodyAnimator.SetBool("isDead", false);
        entityCollider.enabled = true;
        rb2d.isKinematic = false;
    }
}
