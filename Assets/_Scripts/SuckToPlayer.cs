using UnityEngine;
using UnityEngine.VFX;
using System.Collections;
using Sirenix.OdinInspector;

public class SuckToPlayer : MonoBehaviour
{
    GameObject player;

    bool isActive = false;
    float distanceToPlayer;
    Vector3 lerpStartPos;
    Vector3 playerLatestPos;

    private void Update()
    {
        // When active, we move towards the plaer over time then deactivate.
        if (isActive)
        {
            distanceToPlayer += Time.deltaTime;
            transform.position = Vector3.Lerp(lerpStartPos, playerLatestPos, distanceToPlayer);

            // If we finish the lerp and haven't collided with the player, get new points.
            if (distanceToPlayer >= 1)
            {
                distanceToPlayer = 0;
                lerpStartPos = transform.position;
                playerLatestPos = player.transform.position;
            }
        }
    }

    public void Activate()
    {
        player = FindObjectOfType<PlayerController>().gameObject;
        lerpStartPos = transform.position;
        playerLatestPos = player.transform.position;
        isActive = true;
    }

    private void ResetSuck()
    {
        isActive = false;
        distanceToPlayer = 0;
        gameObject.SetActive(false);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            ResetSuck();
    }
}
