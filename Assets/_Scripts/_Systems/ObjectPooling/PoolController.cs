using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class PoolController : MonoBehaviour
{
    [BoxGroup("Control")]
    [SerializeField] int poolThisMany;
    [BoxGroup("Control")]
    [SerializeField] PooledObject pooledObjectPrefab;

    List<PooledObject> objectPool = new List<PooledObject>();

    private void Start()
    {
        for (int i = 0; i < poolThisMany; i++)
        {
            PooledObject newObject = Instantiate(pooledObjectPrefab);
            objectPool.Add(newObject);
            newObject.gameObject.SetActive(false);
        }
    }

    public PooledObject PullFromPool()
    {
        int index = 0;

        for (int i = 0; i < objectPool.Count; i++) {
            if (!objectPool[i].inUse)
            {
                index = i;
                break;
            }
        }

        PooledObject obj = objectPool[index];
        obj.inUse = true;
        obj.gameObject.SetActive(true);

        return obj;
    }

    public void ReturnToPool(PooledObject obj)
    {
        obj.inUse = false;
        obj.gameObject.SetActive(false);
    }

    public void ReturnAllToPool()
    {
        foreach(var obj in objectPool)
        {
            obj.inUse = false;
            obj.gameObject.SetActive(false);
        }
    }
}
