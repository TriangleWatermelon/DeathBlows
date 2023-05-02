using System;
using UnityEngine;
using Sirenix.OdinInspector;

public class Tomato : MonoBehaviour
{
    [BoxGroup("Control")]
    [SerializeField] float minutesToGrow;

    [HideInInspector]
    public bool isCollected { get; private set; }
    private DateTime collectionTime;

    public void CollectTomato()
    {
        isCollected = true;
        collectionTime = DateTime.Now;
        gameObject.SetActive(false);
    }

    public void CheckGrowTime()
    {
        if (DateTime.Now.Second - collectionTime.Second > (60 * minutesToGrow))
            GrowTomato();
    }

    private void GrowTomato()
    {
        isCollected = false;
        gameObject.SetActive(true);
    }
}
