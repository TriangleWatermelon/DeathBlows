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

    //In-Progress
    public RespawnFlag PlaceFlag(Vector3 _position)
    {
        for (int i = 0; i < flags.Count; i++)
        {
            RespawnFlag flag = flags[i].GetComponent<RespawnFlag>();
            if (!flag.isActive)
            {
                flag.transform.position = _position;
                flag.gameObject.SetActive(true);
                flag.ToggleActiveState(true);
                return flag;
            }
        }

        RemoveFlags();
        return null;
    }

    //In-Progress
    private void RemoveFlags()
    {
        //This will activate a UI that shows the location of all the respawn flags.
        //Once the user picks which flag to replace, they can try to place a flag again.
        Debug.Log("No more flags to place. Someday this will actually do something!");
    }
}