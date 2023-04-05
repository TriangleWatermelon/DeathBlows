using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class GameManager : MonoBehaviour
{
    [TitleGroup("Debug")]
    [SerializeField] bool isDebug = false;

    [TitleGroup("Important")]
    [SerializeField] PlayerController player;

    private void Start()
    {
        StartCoroutine(WaitForReferences());
    }

    IEnumerator WaitForReferences()
    {
        yield return new WaitForSeconds(0.5f);
        player.SetDebug(isDebug);
    }
}
