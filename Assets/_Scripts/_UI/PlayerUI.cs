using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Collections;

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

    [BoxGroup("Abilities")]
    [SerializeField] Slider dashSlider;

    [BoxGroup("Damage Effects")]
    [SerializeField] GameObject impactObj;

    private void Awake()
    {
        float sizeX = heartObj.GetComponent<RectTransform>().rect.width;
        heartContainerOffset = new Vector3(sizeX + (sizeX / 2), 0, 0);

        DisplayHitEffect(false);
    }

    /// <summary>
    /// Takes the players max health and creates the correct amount of heart containers on the canvas.
    /// </summary>
    /// <param name="_maxHealth"></param>
    public void SetPlayerHealthUI(float _maxHealth)
    {
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

    /// <summary>
    /// Toggles the active state of the hit effect as well as positioning it over the player.
    /// </summary>
    /// <param name="isShown"></param>
    /// <param name="screenPos"></param>
    public void DisplayHitEffect(bool isShown, Vector3? screenPos = null)
    {
        if(screenPos != null)
            impactObj.transform.position = (Vector3)screenPos;
        impactObj.SetActive(isShown);
    }

    /// <summary>
    /// Adjusts the value of the dash cooldown slider based on the supplied float.
    /// </summary>
    /// <param name="dashTime"></param>
    public void AdjustDashTimer(float dashTime)
    {
        dashSlider.value = dashTime;
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
