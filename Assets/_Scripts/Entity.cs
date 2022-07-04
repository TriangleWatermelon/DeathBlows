using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;

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

    public UnityEvent OnDeath;

    public void TakeDamage(float damage)
    {
        health -= damage;
        isHit = true;
        if(health <= 0)
        {
            OnDeath.Invoke();
        }
    }
}
