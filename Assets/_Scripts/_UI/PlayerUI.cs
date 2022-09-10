using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using System.Collections.Generic;

public class PlayerUI : MonoBehaviour
{
    [BoxGroup("Health")]
    [SerializeField] GameObject heartObj;

    List<GameObject> heartContainers = new List<GameObject>();
    Vector3 heartContainerOffset = new Vector3(35, 0, 0);

    /// <summary>
    /// Takes the players max health and populates the correct amount of heart containers on the canvas.
    /// </summary>
    /// <param name="_maxHealth"></param>
    public void ActivatePlayerUI(float _maxHealth)
    {
        int heartContainersNeeded = Mathf.FloorToInt(_maxHealth / 20);

        for(int i = 0; i < heartContainersNeeded; i++)
        {
            GameObject heartClone = Instantiate(heartObj, gameObject.transform);
            heartClone.SetActive(true);
            heartContainers.Add(heartClone);
            if (i == 0)
                heartContainers[i].transform.position = new Vector3(35, Screen.height - 35, 0);
            else
                heartContainers[i].transform.position = heartContainers[i - 1].transform.position + heartContainerOffset;
        }
    }
}
