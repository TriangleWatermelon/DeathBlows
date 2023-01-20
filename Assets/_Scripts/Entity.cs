using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;

[RequireComponent(typeof(Rigidbody2D))]
public class Entity : MonoBehaviour
{
    public enum state
    {
        idle = 0,
        walking = 1,
        pursuing = 2,
        attacking = 3,
        frozen = 4,
        dying = 5
    }

    [TitleGroup("Entity Base")]
    [BoxGroup("Entity Base/Stats")]
    public float health;

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

    public UnityEvent OnDeath;

    PoolController poolController;

    private void Awake()
    {
        poolController = FindObjectOfType<PoolController>();
        rb2d = GetComponent<Rigidbody2D>();
        entityCollider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        
    }

    /// <summary>
    /// Applies damage to the entity.
    /// Starts the hit timer to prevent multiple hits in a single attack.
    /// Invokes death if health is less than 0 after applying damage.
    /// </summary>
    /// <param name="_damage"></param>
    public void TakeDamage(float _damage)
    {
        health -= _damage;
        hitTimer = 0;
        isHit = true;
        bodyAnimator.SetTrigger("TookDamage");
        if(health <= 0)
        {
            OnDeath.Invoke();
            bodyAnimator.SetBool("isDead", true);
            EjectSoul();
        }
    }

    void EjectSoul()
    {
        GameObject soul = poolController.PullFromPool(transform.position);
        soul.GetComponent<SuckToPlayer>().Activate();
    }

    /// <summary>
    /// Flips the entity sprite
    /// </summary>
    public void FlipSprite()
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
    public void Move(Vector2 moveDir)
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

    //In-Progress
    public void ActivateBrookEffect(float _damage)
    {
        // Saving the position here because the rigidbody sinks without the collider enabled.
        brookEffectPosition = transform.position;
        motionState = state.frozen;
        rb2d.velocity = Vector2.zero;

        brookEffectDamage = _damage;
        brookEffectTimer = 0;
        brookEffectActive = true;
        entityCollider.enabled = false;

        iceAnimator.SetTrigger($"Type {Random.Range(1, 3)}");
        Debug.Log("Activating Brook effect");
    }
}
