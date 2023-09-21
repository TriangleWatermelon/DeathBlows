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
    BubbleType type = BubbleType.Basic;

    [BoxGroup("Visual Components")]
    [SerializeField] GameObject frozenBubbleVFX;

    public enum BubbleType
    {
        Basic = 0,
        Frozen = 1,
        Sticky = 2,
        Anti = 3
    }

    private void Start()
    {
        playerController = FindObjectOfType<PlayerController>();
        SetBubbleType(BubbleType.Basic);
    }

    private void Update()
    {
        MoveBubble();

        if (playerController.isBubbling)
        {
            bubbleTimer += Time.deltaTime;
            if(bubbleTimer >= maxBubbleTime)
            {
                Pop();
            }
        }
    }

    private void MoveBubble()
    {
        switch (type)
        {
            case BubbleType.Basic:
                gameObject.transform.Translate(Vector2.up * moveSpeed * Time.deltaTime);
                spriteObj.transform.Rotate(0, 0, rotationSpeed / 50);
                break;
            case BubbleType.Sticky:
                gameObject.transform.Translate(Vector2.up * (moveSpeed * 0.5f) * Time.deltaTime);
                spriteObj.transform.Rotate(0, 0, rotationSpeed / 75);
                break;
            case BubbleType.Anti:
                gameObject.transform.Translate(Vector2.up * -moveSpeed * Time.deltaTime);
                spriteObj.transform.Rotate(0, 0, -rotationSpeed / 50);
                break;
        }
    }

    /// <summary>
    /// Changes the BubbleType.
    /// </summary>
    /// <param name="_type"></param>
    public void SetBubbleType(BubbleType _type)
    {
        type = _type;

        switch (type)
        {
            case BubbleType.Basic:
                frozenBubbleVFX.SetActive(false);
                break;
            case BubbleType.Frozen:
                frozenBubbleVFX.SetActive(true);
                break;
        }
        Debug.Log($"Bubble Type: {type}");
    }

    /// <summary>
    /// Disables the bubble object.
    /// </summary>
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
