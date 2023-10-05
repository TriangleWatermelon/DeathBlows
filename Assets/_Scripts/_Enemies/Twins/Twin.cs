using UnityEngine;
using Sirenix.OdinInspector;

public class Twin : Entity
{
    [BoxGroup("Visual")]
    [SerializeField] GameObject spriteObj;

    Twins twinController;

    private void Start()
    {
        twinController = GetComponentInParent<Twins>();
    }

    private void FixedUpdate()
    {
        switch (motionState)
        {
            case state.idle:

                break;
            case state.walking:

                break;
            case state.attacking:

                break;
        }

        //Are we standing on someting?
        isGrounded = CheckGround.CheckForGround(groundCheck.position, groundCheckRadius, groundLayer, gameObject);
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

    protected override void Die()
    {
        if (isDead)
            return;

        isDead = true;
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
}
