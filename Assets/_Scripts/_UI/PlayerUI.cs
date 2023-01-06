using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using System.Collections.Generic;

public class PlayerUI : MonoBehaviour
{
    [BoxGroup("Health")]
    [SerializeField] GameObject heartObj;
    [BoxGroup("Health")]
    [SerializeField] Color goodHeartColor;
    [BoxGroup("Health")]
    [SerializeField] Color badHeartColor;

    List<HeartContainer> heartContainers = new List<HeartContainer>();
    Vector3 heartContainerOffset;

    private void Awake()
    {
        float sizeX = heartObj.GetComponent<RectTransform>().sizeDelta.x;
        heartContainerOffset = new Vector3(sizeX + (sizeX / 2), 0, 0);
    }

    /// <summary>
    /// Takes the players max health and creates the correct amount of heart containers on the canvas.
    /// </summary>
    /// <param name="_maxHealth"></param>
    public void AdjustPlayerHealthUI(float _maxHealth)
    {;
        for(int i = 0; i < _maxHealth; i++)
        {
            GameObject heartClone = Instantiate(heartObj, gameObject.transform);
            HeartContainer heartContainer = new HeartContainer(heartClone, heartClone.GetComponent<Image>(), i);
            heartContainers.Add(heartContainer);
            if (i == 0)
                heartContainers[i].heartContainerObj.transform.position = new Vector3(heartContainerOffset.x, Screen.height - heartContainerOffset.x, 0);
            else
                heartContainers[i].heartContainerObj.transform.position = heartContainers[i - 1].heartContainerObj.transform.position + heartContainerOffset;
        }
    }

    /// <summary>
    /// Adjusts the amount of hearts in the UI based on the player health
    /// </summary>
    /// <param name="_playerHealth"></param>
    public void AdjustHealth(float _playerHealth)
    {
        foreach(HeartContainer heart in heartContainers)
        {
            if(heart.heartContainerIndex >= _playerHealth)
            {
                heart.heartContainerImage.color = badHeartColor;
            }
            else
            {
                heart.heartContainerImage.color = goodHeartColor;
            }
        }
    }
}

struct HeartContainer
{
    public GameObject heartContainerObj;
    public Image heartContainerImage;
    public int heartContainerIndex;

    public HeartContainer(GameObject _Obj, Image _image, int _index)
    {
        this.heartContainerObj = _Obj;
        this.heartContainerImage = _image;
        this.heartContainerIndex = _index;
    }
}
