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
        dying = 4
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
    public float hitTimer = 0;

    [BoxGroup("Entity Base/Movement")]
    public bool isRight = true;

    [BoxGroup("Entity Base/Movement")]
    public float stunTime;
    [HideInInspector]
    public Rigidbody2D rb2d;

    [BoxGroup("Entity Base/Movement")]
    public float moveSpeed;

    [HideInInspector]
    public bool isGrounded;
    [BoxGroup("Entity Base/Movement")]
    [Tooltip("This is an empty GameObject placed at the bottom of the enemy's collider")]
    public Transform groundCheck;
    public const float groundCheckRadius = 0.2f;

    [BoxGroup("Entity Base/Movement")]
    [Tooltip("Whatever layer you use for the ground")]
    public LayerMask groundLayer;

    [BoxGroup("Entity Base/Visual")]
    public GameObject spriteParentObj;

    [BoxGroup("Entity Base/Visual")]
    public Animator animator;

    public UnityEvent OnDeath;

    PoolController poolController;

    private void Awake()
    {
        poolController = FindObjectOfType<PoolController>();
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
        if(health <= 0)
        {
            OnDeath.Invoke();
            animator.SetBool("isDead", true);
            EjectSoul();
        }
    }

    void EjectSoul()
    {
        Debug.Log("Ejecting soul...");
        GameObject soul = poolController.PullFromPool(transform.position);
        soul.GetComponent<SuckToPlayer>().Activate();
        Debug.Log("Soul ejected.");
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
}
