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
    [BoxGroup("Stats")]
    public float health;
    [HideInInspector]
    public bool isHit;
    [HideInInspector]
    public float hitTimer = 0;
    [BoxGroup("Movement")]
    public float stunTime;
    [HideInInspector]
    public Rigidbody2D rb2d;
    [BoxGroup("Movement")]
    public bool isRight = true;
    [BoxGroup("Visual")]
    public GameObject spriteParentObj;
    [BoxGroup("Visual")]
    public Animator animator;

    public UnityEvent OnDeath;

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
        }
        //Debug.Log(gameObject.name + " Ouch!");
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
}
