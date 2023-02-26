using UnityEngine;
using UnityEngine.VFX;
using System.Collections;
using Sirenix.OdinInspector;

public class SuckToPlayer : MonoBehaviour
{
    [BoxGroup("Control")]
    [SerializeField] float speed = 0.005f;

    GameObject player;

    bool isActive = false;

    private void Start()
    {
        player = FindObjectOfType<PlayerController>().gameObject;
    }

    private void Update()
    {
        // When active, we move towards the plaer over time then deactivate.
        if (isActive)
        {
            Vector3 direction = (player.transform.position - transform.position).normalized;

            transform.position += direction * speed;
        }
    }

    public void Activate()
    {
        isActive = true;
    }

    private void ResetSuck()
    {
        isActive = false;
        gameObject.SetActive(false);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            ResetSuck();
    }
}
