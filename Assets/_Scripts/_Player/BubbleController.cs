using UnityEngine;
using Sirenix.OdinInspector;

public class BubbleController : MonoBehaviour
{
    [BoxGroup("Main")]
    [SerializeField] GameObject spriteObj;
    PlayerController playerController;

    [BoxGroup("Control")]
    [SerializeField] float moveSpeed;
    [BoxGroup("Control")]
    [SerializeField] float maxBubbleTime;
    float bubbleTimer;
    [BoxGroup("Control")]
    [SerializeField] float rotationSpeed;

    private void Start()
    {
        playerController = FindObjectOfType<PlayerController>();
    }

    private void Update()
    {
        gameObject.transform.Translate(Vector2.up * moveSpeed * Time.deltaTime);
        spriteObj.transform.Rotate(0, 0, rotationSpeed/50);

        if (playerController.isBubbling)
        {
            bubbleTimer += Time.deltaTime;
            if(bubbleTimer >= maxBubbleTime)
            {
                Pop();
            }
        }
    }

    public void Pop()
    {
        playerController.isBubbling = false;
        gameObject.SetActive(false);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
        {
            Pop();
        }
    }

    private void OnEnable()
    {
        bubbleTimer = 0;
    }
}
