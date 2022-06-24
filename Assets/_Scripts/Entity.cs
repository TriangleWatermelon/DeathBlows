using UnityEngine;

public class Entity : MonoBehaviour
{
    public float health;

    public void TakeDamage(float damage)
    {
        health -= damage;
    }

    public void DealDamage(Entity entity, float damage)
    {
        entity.health -= damage;
    }
}
