using UnityEngine;

public class MathHelper : MonoBehaviour
{
    /// <summary>
    /// Finds the degrees of rotation for a sprite based on a Vector2.
    /// </summary>
    /// <param name="_direction"></param>
    /// <returns></returns>
    public static float FindDegreesForRotation(Vector2 _direction)
    {
        float rads = Mathf.Atan2(_direction.y, _direction.x);
        float degrees = rads * Mathf.Rad2Deg;

        return degrees;
    }
}
