using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MapCameraController : MonoBehaviour
{
    Camera self;

    PlayerActions actions;

    private void Awake()
    {
        self = GetComponent<Camera>();

        actions = new PlayerActions();
    }

    private void FixedUpdate()
    {
        Move();
        Zoom();
    }

    /// <summary>
    /// Adds the supplied value to the orthographic size of the camera.
    /// </summary>
    public void AdjustOrthographicSize(float _amountToAdjust)
    {
        self.orthographicSize += _amountToAdjust;
    }

    /// <summary>
    /// Adjust the camera position based on player input;
    /// </summary>
    private void Move()
    {
        Vector2 dir = actions.Gameplay.Move.ReadValue<Vector2>();
        transform.position += (Vector3)dir;
    }

    /// <summary>
    /// Adjust the camera zoom based on player input;
    /// </summary>
    private void Zoom()
    {
        if (actions.Gameplay.ZoomInMap.ReadValue<float>() > 0.5f)
            AdjustOrthographicSize(-0.1f);
        if (actions.Gameplay.ZoomOutMap.ReadValue<float>() > 0.5f)
            AdjustOrthographicSize(0.1f);
    }

    private void OnEnable()
    {
        actions.Enable();
    }
    private void OnDisable()
    {
        actions.Disable();
    }
}
