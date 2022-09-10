using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;

[RequireComponent(typeof(Rigidbody2D))]
public class Entity : MonoBehaviour
{
    [BoxGroup("Control/Stats")]
    public float health;
    [HideInInspector]
    public bool isHit;
    [HideInInspector]
    public float hitTimer = 0;
    [BoxGroup("Control/Movement")]
    public float stunTime;
    [HideInInspector]
    public Rigidbody2D rb2d;

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
        }
        //Debug.Log(gameObject.name + " Ouch!");
    }
}
