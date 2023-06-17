using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;

public class MoveBetween : MonoBehaviour
{
    [BoxGroup("GameObject Points")]
    [SerializeField] GameObject point1, point2;
    bool moving = false;
    bool isPoint1 = false;

    [BoxGroup("Graphics Renderers")]
    [SerializeField] LineRenderer lineRenderer;

    [BoxGroup("Control")]
    [SerializeField] float speed;
    float timeToMove;

    GameObject player;
    PlayerController playerController;

    bool waiting = false;
    float timeToWait;

    private void Start()
    {
        playerController = FindObjectOfType<PlayerController>();
        player = playerController.gameObject;

        lineRenderer.SetPosition(0, point1.transform.position);
        lineRenderer.SetPosition(1, point2.transform.position);
    }

    private void FixedUpdate()
    {
        if (moving)
        {
            MoveToOtherPoint(isPoint1);
            timeToMove += Time.deltaTime * (speed / 10);
            if (timeToMove >= 1)
                StopMoving();
        }
        if (waiting)
        {
            timeToWait += Time.deltaTime;
            if (timeToWait >= 3)
                waiting = false;
        }
    }

    /// <summary>
    /// Disables the player's rigidbody and begins moving from one point to the other.
    /// </summary>
    /// <param name="_isPoint1"></param>
    public void StartMoving(bool _isPoint1)
    {
        if (waiting)
            return;

        timeToMove = 0;
        isPoint1 = _isPoint1;
        moving = true;
        playerController.ToggleRigidBody(false);
    }

    /// <summary>
    /// Re-enables the player's rigidbody and stops their movement from one point to the other.
    /// </summary>
    public void StopMoving()
    {
        if (!moving)
            return;

        timeToWait = 0;
        waiting = true;
        moving = false;
        playerController.ToggleRigidBody(true);
    }

    /// <summary>
    /// Lerps the player's position between two points.
    /// </summary>
    /// <param name="_isPoint1"></param>
    void MoveToOtherPoint(bool _isPoint1)
    {
        Vector3 newPos;

        if (_isPoint1)
            newPos = Vector3.Lerp(point1.transform.position, point2.transform.position, timeToMove);
        else
            newPos = Vector3.Lerp(point2.transform.position, point1.transform.position, timeToMove);

        playerController.RepositionPlayer(newPos);
    }
}
