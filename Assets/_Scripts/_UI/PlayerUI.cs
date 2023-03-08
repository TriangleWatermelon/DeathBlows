using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    [BoxGroup("Health")]
    [SerializeField] GameObject heartObj;

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
        heartObj.SetActive(false);

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
            heartClone.SetActive(true);
            HeartContainer heartContainer = new HeartContainer(heartClone, heartClone.GetComponent<Image>(), i, heartClone.GetComponent<Animator>());
            heartContainers.Add(heartContainer);
            if (i == 0)
                heartContainers[i].heartObject.transform.position = new Vector3(heartContainerOffset.x, gameObject.GetComponent<Canvas>().pixelRect.height - heartContainerOffset.x, 0);
            else
                heartContainers[i].heartObject.transform.position = heartContainers[i - 1].heartObject.transform.position + heartContainerOffset;
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
            if(heart.index >= _playerHealth)
            {
                heart.SetHeartStatus(false);
            }
            else
            {
                if (!heart.isHealed)
                {
                    heart.SetHeartStatus(true);
                }
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

    #region Debugging
    [TitleGroup("Debug")]
    [SerializeField] TextMeshProUGUI respawnText;
    public void SetRespawnTimer(float _timer) => respawnText.text = Mathf.FloorToInt(_timer).ToString();
    #endregion
}

class HeartContainer
{
    public GameObject heartObject;
    public Image image;
    public int index;

    public bool isHealed = true;
    Animator animator;

    public HeartContainer(GameObject _Obj, Image _image, int _index, Animator _animator)
    {
        this.heartObject = _Obj;
        this.image = _image;
        this.index = _index;
        this.animator = _animator;
    }

    /// <summary>
    /// Changes animation booleans depending on if the heart should heal or hurt.
    /// </summary>
    /// <param name="_isHealed"></param>
    public void SetHeartStatus(bool _isHealed)
    {
        isHealed = _isHealed;

        if (isHealed)
        {
            animator.SetBool("Damaged", false);
            animator.SetBool("Healed", true);
        }
        else
        {
            animator.SetBool("Healed", false);
            animator.SetBool("Damaged", true);
        }
    }
}
