using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    Canvas canvas;

    #region Health
    [BoxGroup("Health")]
    [SerializeField] GameObject heartObj;
    [BoxGroup("Health")]
    [SerializeField] GameObject heartParent;

    [BoxGroup("Health")]
    [SerializeField] List<HeartContainer> heartContainers = new List<HeartContainer>();
    Vector3 heartContainerOffset;
    #endregion

    [BoxGroup("Abilities")]
    [SerializeField] Slider dashSlider;
    [BoxGroup("Abilities")]
    [SerializeField] GameObject bubbleTypeObj;
    Animator bubbleTypeAnim;
    Coroutine stopBubble;

    [BoxGroup("Damage Effects")]
    [SerializeField] GameObject impactObj;

    [BoxGroup("After Death")]
    [SerializeField] TextMeshProUGUI respawnText;

    #region Controller Buttons
    [BoxGroup("Controller Buttons")]
    [SerializeField] GameObject faceNorth;
    [BoxGroup("Controller Buttons")]
    [SerializeField] GameObject faceEast;
    [BoxGroup("Controller Buttons")]
    [SerializeField] GameObject faceSouth;
    [BoxGroup("Controller Buttons")]
    [SerializeField] GameObject faceWest;
    [BoxGroup("Controller Buttons")]
    [SerializeField] GameObject triggerRight;
    [BoxGroup("Controller Buttons")]
    [SerializeField] GameObject triggerLeft;
    List<GameObject> controllerButtons = new List<GameObject>();
    #endregion

    private void Awake()
    {
        canvas = GetComponent<Canvas>();
        heartObj.SetActive(false);
        bubbleTypeAnim = bubbleTypeObj.GetComponent<Animator>();
        bubbleTypeObj.SetActive(false);

        controllerButtons.Add(faceNorth);
        controllerButtons.Add(faceEast);
        controllerButtons.Add(faceSouth);
        controllerButtons.Add(faceWest);
        controllerButtons.Add(triggerRight);
        controllerButtons.Add(triggerLeft);
        DisableControllerButtons();

        DisplayHitEffect(false);
        DisplayDeathElements(false);
    }

    /// <summary>
    /// Takes the players max health and creates the correct amount of heart containers on the canvas.
    /// </summary>
    /// <param name="_maxHealth"></param>
    public void SetPlayerHealthUI(float _maxHealth)
    {
        for (int i = 0; i < _maxHealth; i++)
        {
            GameObject heartClone = Instantiate(heartObj, heartParent.transform);
            heartClone.SetActive(true);
            HeartContainer heartContainer = new HeartContainer(heartClone, heartClone.GetComponent<Image>(), i, heartClone.GetComponent<Animator>());
            heartContainers.Add(heartContainer);
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
    public void DisplayHitEffect(bool _isShown, Vector3? _screenPos = null)
    {
        if(_screenPos != null)
            impactObj.transform.position = (Vector3)_screenPos;
        impactObj.SetActive(_isShown);
    }

    /// <summary>
    /// Adjusts the value of the dash cooldown slider based on the supplied float.
    /// </summary>
    /// <param name="dashTime"></param>
    public void AdjustDashTimer(float dashTime)
    {
        dashSlider.value = dashTime;
    }

    /// <summary>
    /// Toggles all the elements needed when the player dies.
    /// </summary>
    /// <param name="_isShown"></param>
    public void DisplayDeathElements(bool _isShown)
    {
        respawnText.gameObject.SetActive(_isShown);
    }

    /// <summary>
    /// Disables any active button prompts before enabling the requested prompt.
    /// </summary>
    /// <param name="_direction"></param>
    public void ShowControllerButtons(string _direction)
    {
        DisableControllerButtons();

        switch (_direction)
        {
            case "North":
                faceNorth.SetActive(true);
                break;
            case "East":
                faceEast.SetActive(true);
                break;
            case "South":
                faceSouth.SetActive(true);
                break;
            case "West":
                faceWest.SetActive(true);
                break;
            case "RightTrigger":
                triggerRight.SetActive(true);
                break;
            case "LeftTrigger":
                triggerLeft.SetActive(true);
                break;
        }
    }

    /// <summary>
    /// Disables all face button prompts.
    /// </summary>
    public void DisableControllerButtons()
    {
        foreach (var b in controllerButtons)
            b.SetActive(false);
    }

    /// <summary>
    /// Triggers the Bubble Type UI animations when the type is changed.
    /// </summary>
    /// <param name="_type"></param>
    public void SetBubbleTypeUI(BubbleController.BubbleType _type)
    {
        bubbleTypeObj.SetActive(true);
        switch (_type)
        {
            case BubbleController.BubbleType.Basic:
                bubbleTypeAnim.SetTrigger("SetBasic");
                break;
            case BubbleController.BubbleType.Frozen:
                bubbleTypeAnim.SetTrigger("SetFreeze");
                break;
            case BubbleController.BubbleType.Sticky:
                bubbleTypeAnim.SetTrigger("SetSticky");
                break;
            case BubbleController.BubbleType.Anti:
                bubbleTypeAnim.SetTrigger("SetAnti");
                break;
        }
        if(stopBubble == null)
            stopBubble = StartCoroutine(DisableBubbleTypeUI());
        else
        {
            StopCoroutine(stopBubble);
            stopBubble = StartCoroutine(DisableBubbleTypeUI());
        }
    }

    /// <summary>
    /// Disables the bubble type UI when the selection is done being made.
    /// </summary>
    /// <returns></returns>
    IEnumerator DisableBubbleTypeUI()
    {
        yield return new WaitForSeconds(0.75f);
        bubbleTypeObj.SetActive(false);
    }

    #region Debugging
    [TitleGroup("Debug")]
    [SerializeField] TextMeshProUGUI respawnTimerText;
    [SerializeField] TextMeshProUGUI fpsText;

    public void SetRespawnTimer(float _timer) => respawnTimerText.text = Mathf.FloorToInt(_timer).ToString();

    public void SetDebugMode(bool _isDebug)
    {
        respawnTimerText.gameObject.SetActive(_isDebug);
        fpsText.gameObject.SetActive(_isDebug);
    }
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
