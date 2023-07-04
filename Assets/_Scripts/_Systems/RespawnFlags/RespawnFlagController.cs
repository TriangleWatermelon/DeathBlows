using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;

public class RespawnFlagController : MonoBehaviour
{
    [TitleGroup("Flags")]
    [SerializeField] GameObject flagPrefab;
    List<GameObject> flags = new List<GameObject>();

    /// <summary>
    /// Instantiates the given number of flags and adds them to a list.
    /// </summary>
    /// <param name="_maxFlags"></param>
    public void SetMaxFlags(int _maxFlags)
    {
        for (int i = 0; i < _maxFlags; i++)
        {
            flags.Add(Instantiate(flagPrefab));
        }
    }

    /// <summary>
    /// Takes a respawn flag from the pool and enables it at the current position.
    /// </summary>
    /// <param name="_position"></param>
    /// <returns></returns>
    public RespawnFlag PlaceFlag(Vector3 _position)
    {
        for (int i = 0; i < flags.Count; i++)
        {
            RespawnFlag flag = flags[i].GetComponent<RespawnFlag>();
            if (!flag.isActive)
            {
                flag.transform.position = _position;
                RespawnManager.SetPlayerRespawnPosition(_position);
                flag.gameObject.SetActive(true);
                flag.ToggleActiveState(true);

                SaveController.instance.SetFlagPosition(_position);
                return flag;
            }
        }
        return null;
    }

    //In-Progress
    public bool AnyActiveFlags()
    {
        for (int i = 0; i < flags.Count; i++)
        {
            RespawnFlag flag = flags[i].GetComponent<RespawnFlag>();
            if (flag.isActive)
                return true;
        }
        return false;
    }
}