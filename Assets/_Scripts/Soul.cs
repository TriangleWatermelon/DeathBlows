using UnityEngine;
using UnityEngine.VFX;
using Sirenix.OdinInspector;

public class Soul : MonoBehaviour
{
    [BoxGroup("Components")]
    [SerializeField] VisualEffect VFX;

    Vector3 previousPosition;
    Vector3 randomPosition;

    private void Update()
    {
        Vector3 direction = previousPosition - transform.position;

        // Use the movement direction to create a trail.
        VFX.SetVector3("Direction", direction);

        // Use the movement speed to set particle speed.
        float speed = direction.magnitude;
        VFX.SetFloat("Speed", speed);

        // Create a random position around the soul where new particles can be created.
        float randomPositionValue = Random.Range(-0.3f, 0.3f);
        randomPosition = new Vector3(randomPositionValue, randomPositionValue, randomPositionValue);
        VFX.SetVector3("RandPos", randomPosition);

        previousPosition = transform.position;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
            collision.gameObject.GetComponent<PlayerController>().Heal(1);
    }
}
