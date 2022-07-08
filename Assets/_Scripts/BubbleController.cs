using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class BubbleController : MonoBehaviour
{
    [BoxGroup("Main")]
    [SerializeField] GameObject spriteObj;
    PlayerController playerController;

    [BoxGroup("Control")]
    [SerializeField] float moveSpeed;

    private void Start()
    {
        playerController = FindObjectOfType<PlayerController>();
    }

    private void Update()
    {
        gameObject.transform.Translate(Vector2.up * moveSpeed);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
        {
            playerController.isBubbling = false;
            gameObject.SetActive(false);
        }
    }
}
