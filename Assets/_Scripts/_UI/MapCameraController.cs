using UnityEngine;
using Sirenix.OdinInspector;

public class MapCameraController : MonoBehaviour
{
    Camera self;
    PlayerActions actions;

    [BoxGroup("Components")]
    [SerializeField] GameObject mainCam;
    [BoxGroup("Components")]
    [SerializeField] ReticleSelector reticle;

    [BoxGroup("Settings")]
    [SerializeField] float zoomSpeed = 1;

    private void Awake()
    {
        self = GetComponent<Camera>();

        actions = new PlayerActions();
        actions.Gameplay.Slash.performed += ctx => OnSelect();
        actions.Gameplay.Jump.performed += ctx => OnSelect();
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
            AdjustOrthographicSize(-0.1f * zoomSpeed);
        if (actions.Gameplay.ZoomOutMap.ReadValue<float>() > 0.5f)
            AdjustOrthographicSize(0.1f * zoomSpeed);
    }

    /// <summary>
    /// THe event fired off by Jump and Attack buttons.
    /// </summary>
    private void OnSelect()
    {
        if (reticle.lastFlagTouched != null)
            reticle.lastFlagTouched.RespawnPlayerHere();
    }

    private void OnEnable()
    {
        actions.Enable();
        transform.position = mainCam.transform.position;
    }
    private void OnDisable()
    {
        actions.Disable();
    }
}
