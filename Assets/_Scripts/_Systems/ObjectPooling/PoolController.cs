using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class PoolController : MonoBehaviour
{
    [BoxGroup("Control")]
    [SerializeField] int poolThisMany;
    [BoxGroup("Control")]
    [SerializeField] GameObject pooledObjectPrefab;

    List<GameObject> objectPool = new List<GameObject>();

    private void Start()
    {
        for (int i = 0; i < poolThisMany; i++)
        {
            GameObject newObject = Instantiate(pooledObjectPrefab);
            objectPool.Add(newObject);
            newObject.gameObject.SetActive(false);
        }
    }

    public void AdjustPooledAmount(int _newAmount)
    {
        poolThisMany = _newAmount;

        for (int i = 0; i < poolThisMany; i++)
        {
            GameObject newObject = Instantiate(pooledObjectPrefab);
            objectPool.Add(newObject);
            newObject.gameObject.SetActive(false);
        }
    }

    public GameObject PullFromPool(Vector3 position)
    {
        for (int i = 0; i < objectPool.Count; i++) {
            if (!objectPool[i].activeInHierarchy)
            {
                objectPool[i].transform.position = position;
                objectPool[i].SetActive(true);
                return objectPool[i];
            }
        }

        return null;
    }

    public void ReturnToPool(GameObject obj)
    {
        obj.gameObject.SetActive(false);
    }

    public void ReturnAllToPool()
    {
        foreach(var obj in objectPool)
        {
            obj.gameObject.SetActive(false);
        }
    }
}
