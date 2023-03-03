using UnityEngine;

public class ReticleSelector : MonoBehaviour
{
    public RespawnFlag lastFlagTouched { get; private set; }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Flag"))
            lastFlagTouched = collision.GetComponentInParent<RespawnFlag>();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (lastFlagTouched != null)
            lastFlagTouched = null;
    }
}
