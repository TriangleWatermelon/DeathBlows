using UnityEngine;
using Sirenix.OdinInspector;

[RequireComponent(typeof(PhysicsRope))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class Twins : MonoBehaviour
{
    [BoxGroup("Siblings")]
    [SerializeField] Twin twinLeft, twinRight;
    Vector3 twinLeftPos, twinRightPos;
    [BoxGroup("Siblings")]
    [SerializeField] GameObject combinedSpriteObj;

    [BoxGroup("Control")]
    [SerializeField] float moveSpeed;
    float damage;
    bool isRight;
    [BoxGroup("Control")]
    [SerializeField] float maxDistanceBetween;
    Rigidbody2D rb2d;
    CircleCollider2D col;
    [BoxGroup("Control")]
    [SerializeField] Transform groundCheck;
    float groundCheckRadius = 0.2f;
    bool isGrounded;
    LayerMask groundLayer;

    PhysicsRope rope;
    LineRenderer lineRenderer;

    public enum State
    {
        connected,
        disconnected
    }
    State connectionState;

    bool bothDead = false;
    float combinedHealth;
    bool canCombine = true;

    private void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        col = GetComponent<CircleCollider2D>();
        rope = GetComponent<PhysicsRope>();
        lineRenderer = GetComponent<LineRenderer>();

        connectionState = State.connected;

        groundLayer = twinLeft.groundLayer;
        damage = twinLeft.damage;

        CheckTwinHealth();
        ToggleCombine(true);
    }

    private void FixedUpdate()
    {
        switch (connectionState)
        {
            case State.connected:
                if (isRight)
                    Move(Vector2.right);
                else
                    Move(Vector2.left);

                isGrounded = CheckGround.CheckForGround(groundCheck.position, groundCheckRadius, groundLayer, gameObject);
                break;
            case State.disconnected:

                break;
        }
    }

    void Move(Vector2 moveDir)
    {
        if (isGrounded)
            rb2d.velocity = moveDir * moveSpeed;
    }

    //In-Progress
    public void CheckTwinHealth()
    {
        if (twinLeft.health > 0 && twinRight.health > 0)
            canCombine = true;
        else
            canCombine = false;

        combinedHealth = twinLeft.health + twinRight.health;

        if (combinedHealth <= 0)
        {
            Die();
        }
    }

    //In-Progress
    void ShareHealth()
    {
        float splitHealth = combinedHealth / 2;
        twinLeft.health = splitHealth;
        twinRight.health = splitHealth;

        twinLeft.isDead = false;
        twinRight.isDead = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            ToggleCombine(false);
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            ToggleCombine(true);
        }
    }

    //In-Progress
    void ToggleCombine(bool _state)
    {
        if (_state)
        {
            ShareHealth();
            connectionState = State.connected;

            rope.segmentLength = 0.01f;
        }
        else
        {
            connectionState = State.disconnected;

            rope.segmentLength = 0.25f;

            Vector2 dir = (twinLeft.transform.position - twinRight.transform.position).normalized;
            twinLeft.KnockbackEntity(dir);
            twinRight.KnockbackEntity(-dir);
        }

        combinedSpriteObj.SetActive(_state);
        col.enabled = _state;
        rb2d.simulated = _state;

        //twinLeft.ToggleIndividuality(!_state);
        //twinRight.ToggleIndividuality(!_state);
        twinLeft.gameObject.SetActive(!_state);
        twinRight.gameObject.SetActive(!_state);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.gameObject.GetComponent<PlayerController>().TakeDamage(damage);
        }
        else if (!collision.gameObject.CompareTag("Ground"))
        {
            isRight = !isRight;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        string tag = collision.tag;

        if (tag != "Hazard" && tag != "Trigger")
            isRight = !isRight;

        if (tag == "Hazard")
            TakeDamage(combinedHealth);
    }

    public void TakeDamage(float _damage)
    {
        combinedHealth -= _damage;

        if (combinedHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        bothDead = true;
        if (connectionState == State.connected)
        {
            connectionState = State.disconnected;
            ToggleCombine(false);
        }

        twinLeft.TakeDamage(combinedHealth);
        twinLeft.EjectSouls();
        twinRight.TakeDamage(combinedHealth);
        twinRight.EjectSouls();
    }
}
