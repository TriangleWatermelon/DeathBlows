using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;

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
                float distanceBetween = (twinLeft.transform.position - twinRight.transform.position).magnitude;
                if (distanceBetween > maxDistanceBetween)
                {
                    if (twinLeft.isGrounded && !twinRight.isGrounded)
                    {
                        twinRight.Pull((twinLeft.transform.position - twinRight.transform.position).normalized);
                    }
                    else if (!twinLeft.isGrounded && twinRight.isGrounded)
                    {
                        twinLeft.Pull((twinRight.transform.position - twinLeft.transform.position).normalized);
                    }
                    else if (!twinLeft.isGrounded && !twinRight.isGrounded)
                    {
                        float velocityLeft = twinLeft.rb2d.velocity.sqrMagnitude;
                        float velocityRight = twinLeft.rb2d.velocity.sqrMagnitude;
                        Debug.Log($"Velocity Left: {velocityLeft} \nVelocity Right: {velocityRight}");

                        if (velocityLeft > velocityRight)
                        {
                            twinRight.Pull((twinLeft.transform.position - twinRight.transform.position).normalized);
                        }
                        else if (velocityLeft < velocityRight)
                        {
                            twinLeft.Pull((twinRight.transform.position - twinLeft.transform.position).normalized);
                        }
                        else
                        {
                            twinLeft.Pull((twinRight.transform.position - twinLeft.transform.position).normalized);
                            twinRight.Pull((twinLeft.transform.position - twinRight.transform.position).normalized);
                        }
                    }
                }
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
            StartCoroutine(BringTogether());
        }
    }

    //In-Progress
    void ToggleCombine(bool _state)
    {
        if (_state)
        {
            ShareHealth();
            connectionState = State.connected;

            twinLeft.transform.position = transform.position;
            twinRight.transform.position = transform.position;
            twinLeft.transform.rotation = transform.rotation;
            twinRight.transform.rotation = transform.rotation;

            rope.segmentLength = 0.01f;
        }
        else
        {
            connectionState = State.disconnected;

            rope.segmentLength = 0.25f;

            Vector2 dir = (twinLeft.transform.position - twinRight.transform.position).normalized;
            twinLeft.KnockbackEntity(-dir * 5);
            twinRight.KnockbackEntity(dir * 5);
        }

        combinedSpriteObj.SetActive(_state);
        col.enabled = _state;
        rb2d.simulated = _state;

        //twinLeft.ToggleIndividuality(!_state);
        //twinRight.ToggleIndividuality(!_state);
        twinLeft.gameObject.SetActive(!_state);
        twinRight.gameObject.SetActive(!_state);
    }

    IEnumerator BringTogether()
    {
        float segLength = rope.segmentLength;
        Vector3 leftPos = twinLeft.transform.position;
        Vector3 rightPos = twinRight.transform.position;
        while (segLength > 0.01f)
        {
            yield return new WaitForFixedUpdate();
            segLength -= Time.deltaTime / 12; //12 is used here for a ~3 second transition going from 0.25 to 0.01.
            rope.segmentLength = segLength;
            twinLeft.transform.position = leftPos;
            twinRight.transform.position = rightPos;
        }
        transform.position = twinLeft.transform.position;
        ToggleCombine(true);
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
