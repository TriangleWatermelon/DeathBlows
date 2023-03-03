using UnityEngine;

public class RespawnFlag : MonoBehaviour
{
    public bool isActive { get; private set; }
    PlayerController player;
    public Vector3 position { get; private set; }

    private void Start()
    {
        player = FindObjectOfType<PlayerController>();
        ToggleActiveState(false);
    }

    /// <summary>
    /// Changes the active state of the Respawn Flag.
    /// </summary>
    /// <param name="_state"></param>
    public void ToggleActiveState(bool _state)
    {
        isActive = _state;
        gameObject.SetActive(_state);
    }

    //In-Progress
    public void RemoveFlag()
    {
        ToggleActiveState(false);
    }

    /// <summary>
    /// Repositions the player to this flag.
    /// </summary>
    public void RespawnPlayerHere()
    {
        player.RepositionPlayer(position);
        RespawnManager.SetPlayerRespawnPosition(position);
    }

    private void OnEnable()
    {
        position = transform.position;
    }
}
