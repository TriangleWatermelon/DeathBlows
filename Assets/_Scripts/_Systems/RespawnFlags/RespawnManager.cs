using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    RespawnManager instance;

    static Vector3 roomRespawnPosition;
    static Vector3 playerRespawnPosition;

    private void Start()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }

    /// <summary>
    /// Takes the supplied position and holds it for respawning the player at the beginning of the room.
    /// </summary>
    /// <param name="_pos"></param>
    public static void SetRoomRespawnPosition(Vector3 _pos)
    {
        roomRespawnPosition = _pos;
    }

    /// <summary>
    /// Returns the room's respawn position;
    /// </summary>
    /// <returns></returns>
    public static Vector3 GetRoomRespawnPosition()
    {
        return roomRespawnPosition;
    }

    /// <summary>
    /// Takes the supplied position and holds it for respawning the player where they choose.
    /// </summary>
    /// <param name="_pos"></param>
    public static void SetPlayerRespawnPosition(Vector3 _pos)
    {
        playerRespawnPosition = _pos;
    }

    /// <summary>
    /// Returns the player's respawn position of choice.
    /// </summary>
    /// <returns></returns>
    public static Vector3 GetPlayerRespawnPoint()
    {
        return playerRespawnPosition;
    }
}
