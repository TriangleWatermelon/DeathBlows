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
                    Move(Vector2.left);
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

        //Are we standing on someting?
        isGrounded = CheckGround.CheckForGround(groundCheck.position, groundCheckRadius, groundLayer, gameObject);
    }
}
