using UnityEngine;

public class EndOfTheWorld : MonoBehaviour
{
    PlayerController player;

    void Start()
    {
        player = FindObjectOfType<PlayerController>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            player.TakeDamage(1);
            if(player.health != 0)
                player.RepositionPlayer(player.lastPlaceBeforeJump);
        }
    }
}
