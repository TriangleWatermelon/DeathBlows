using UnityEngine;
using Sirenix.OdinInspector;

public class Pacer : Entity
{
    private void FixedUpdate()
    {
        switch (motionState)
        {
            case state.idle:
                if (isRight)
                    Move(Vector2.right);
                else
                    Move(-Vector2.right);
                break;
            case state.frozen:
                transform.position = brookEffectPosition;
                brookEffectTimer += Time.deltaTime;
                if (brookEffectTimer >= 1)
                {
                    entityCollider.enabled = true;
                    TakeDamage(brookEffectDamage);
                    brookEffectActive = false;

                    motionState = state.idle;
                }
                break;
        }

        bool wasGrounded = isGrounded;
        isGrounded = false;

        // The player is grounded if a circlecast to the groundCheck position hits anything designated on the ground layer
        Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheck.position, groundCheckRadius, groundLayer);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
            {
                isGrounded = true;
                latestGroundObj = colliders[i].gameObject;
            }
        }
    }
}
