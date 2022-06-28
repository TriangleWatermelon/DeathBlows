using UnityEngine;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;
using System;

public class PlayerController : MonoBehaviour
{
    Camera playerCam;
    [BoxGroup("Camera")]
    [SerializeField] Vector3 cameraOffset;
    #region Visuals
    [TitleGroup("Main")]
    [HorizontalGroup("Main")]
    [VerticalGroup("Main")]
    [BoxGroup("Main/Visuals")]
    [PreviewField(70, ObjectFieldAlignment.Left)]
    [SerializeField] Sprite playerSprite;
    SpriteRenderer spriteRenderer;
    bool isFacingRight = true;
    #endregion
    #region STATS
    [VerticalGroup("Main")]
    [BoxGroup("Main/Stats")]
    [SerializeField] float maxHealth;
    private float health;
    [VerticalGroup("Main")]
    [BoxGroup("Main/Stats")]
    [SerializeField] float moveSpeed;
    [VerticalGroup("Main")]
    [BoxGroup("Main/Stats")]
    [SerializeField] float jumpHeight;
    #endregion
    #region Control
    Rigidbody2D rb2d;
    PlayerActions playerActions;
    Vector2 moveDir;
    bool isJumping = false;
    bool isAttacking = false;
    bool isBubbling = false;
    [TitleGroup("Control")]
    [BoxGroup("Control/Raycasting")]
    [SerializeField] float rayDistance;
    #endregion

    private void Awake()
    {
        playerCam = FindObjectOfType<Camera>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        spriteRenderer.sprite = playerSprite;
        playerActions = new PlayerActions();
        rb2d = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        health = maxHealth;

        playerActions.Gameplay.Move.performed += ctx => moveDir = ctx.ReadValue<Vector2>();
        playerActions.Gameplay.Move.performed += ctx => OnMove();
        playerActions.Gameplay.Move.canceled += ctx => moveDir = -moveDir;
        playerActions.Gameplay.Move.canceled += ctx => OnMove();
        playerActions.Gameplay.Jump.performed += ctx => OnJump();
    }

    void OnMove()
    {
        moveDir.y = 0;
        rb2d.AddForce(moveDir * moveSpeed);
    }

    void OnJump()
    {
        RaycastHit hit;
        if(Physics.Raycast(transform.position, Vector2.down, out hit, rayDistance))
        {
            Debug.Log("Hit!");
        }
        if (!isJumping)
        {
            rb2d.AddForce(Vector2.up * jumpHeight);
        }
    }

    void Update()
    {
        playerCam.transform.position = transform.position + cameraOffset;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        string colTag = collision.gameObject.tag;
        switch (colTag)
        {
            case "Ground":
                isJumping = false;
                break;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        string colTag = collision.gameObject.tag;
        switch (colTag)
        {
            case "Ground":
                isJumping = true;
                break;
        }
    }

    private void OnEnable()
    {
        playerActions.Enable();
    }

    private void OnDisable()
    {
        playerActions.Disable();
    }
}
