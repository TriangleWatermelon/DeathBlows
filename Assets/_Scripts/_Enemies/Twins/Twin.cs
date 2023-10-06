using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;

public class Twin : Entity
{
    [BoxGroup("Visual")]
    [SerializeField] GameObject spriteObj;

    Twins twinController;
    Twin twin;

    PlayerController player;

    public bool canAttack = false;

    private void Start()
    {
        twinController = GetComponentInParent<Twins>();
        player = FindObjectOfType<PlayerController>();
    }

    public void SetTwin(Twin _otherTwin) => twin = _otherTwin;

    private void FixedUpdate()
    {
        switch (motionState)
        {
            case state.idle:
                motionState = state.attacking;
                break;
            case state.walking:

                break;
            case state.attacking:
                if (canAttack)
                {
                    StartCoroutine(AttackDelay());
                    canAttack = false;
                }
                break;
        }

        isGrounded = false;
    }

    IEnumerator AttackDelay()
    {
        Pull((player.transform.position - transform.position).normalized * 3);
        if (!twin.isGrounded)
            twin.Pull((transform.position - twin.transform.position).normalized * 2);
        yield return new WaitForSeconds(4);
        twinController.CheckToAttack();
    }

    /// <summary>
    /// Enables and disables the object holding the twin sprite.
    /// </summary>
    /// <param name="_state"></param>
    void ToggleSprite(bool _state) => spriteObj.SetActive(_state);

    /// <summary>
    /// Toggles the collider used by the individual twin.
    /// </summary>
    /// <param name="_state"></param>
    void ToggleCollider(bool _state) => entityCollider.enabled = _state;

    /// <summary>
    /// Toggles the rigidbody used by the individual twin.
    /// </summary>
    /// <param name="_state"></param>
    void ToggleRB(bool _state) => rb2d.simulated = _state;

    //In-Progress
    public void ToggleIndividuality(bool _state)
    {
        ToggleSprite(_state);
        ToggleCollider(_state);
        ToggleRB(_state);
    }

    //In-Progress
    public void Pull(Vector2 _dir)
    {
        rb2d.velocity = _dir * moveSpeed;
    }

    protected override void Die()
    {
        if (isDead)
            return;

        isDead = true;
    }

    protected override void FlipSprite()
    {
        //We don't want any sprites to flip.
    }

    /// <summary>
    /// This has been removed from the original Entity Die method to allow the
    /// Twin controller to eject souls when both twins die.
    /// </summary>
    public void EjectSouls() => EjectSoul(soulsToDrop);

    public override void ResetEntity()
    {
        transform.position = startPosition;
        if (isRight != isRightReset)
            FlipSprite();
        health = healthReset;
        isDead = false;
        motionState = state.idle;
        bodyAnimator.SetBool("isDead", false);
        for (int i = 0; i < spriteSegments.Length; i++)
        {
            spriteSegments[i].transform.position = spriteSegmentStartPositions[i];
            spriteSegments[i].transform.rotation = new Quaternion(0, 0, 0, 0);
        }
        rb2d.isKinematic = false;
    }

    public override void OnCollisionEnter2D(Collision2D collision)
    {
        base.OnCollisionEnter2D(collision);
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = true;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = true;
    }
}
