using UnityEngine;

public class CheckGround
{
    /// <summary>
    /// Casts a circle collider to see if the entity is touching the ground or not.
    /// </summary>
    /// <param name="_groundCheckPos"></param>
    /// <param name="_groundCheckRadius"></param>
    /// <param name="_layerMask"></param>
    /// <param name="_gameObject"></param>
    /// <returns></returns>
    public static bool CheckForGround(Vector2 _groundCheckPos, float _groundCheckRadius, LayerMask _layerMask, GameObject _gameObject)
    {
        bool isGrounded = false;

        // The player is grounded if a circlecast to the groundCheck position hits anything designated on the ground layer
        Collider2D[] colliders = Physics2D.OverlapCircleAll(_groundCheckPos, _groundCheckRadius, _layerMask);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != _gameObject)
            {
                isGrounded = true;
            }
        }

        return isGrounded;
    }
}
