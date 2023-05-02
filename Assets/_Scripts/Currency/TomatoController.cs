using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class TomatoController : MonoBehaviour
{
    [BoxGroup("GameObjects")]
    [SerializeField] List<Tomato> tomatoes = new List<Tomato>();

    public void CheckGrowTimes()
    {
        foreach (var tomato in tomatoes)
            tomato.CheckGrowTime();
    }
}
