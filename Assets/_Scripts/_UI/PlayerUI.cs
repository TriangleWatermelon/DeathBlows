using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    Canvas canvas;

    [BoxGroup("Health")]
    [SerializeField] GameObject heartObj;
    [BoxGroup("Health")]
    [SerializeField] GameObject heartParent;

    [BoxGroup("Health")]
    [SerializeField] List<HeartContainer> heartContainers = new List<HeartContainer>();
    Vector3 heartContainerOffset;

    [BoxGroup("Abilities")]
    [SerializeField] Slider dashSlider;

    [BoxGroup("Damage Effects")]
    [SerializeField] GameObject impactObj;

    [BoxGroup("After Death")]
    [SerializeField] TextMeshProUGUI respawnText;

    [BoxGroup("Face Buttons")]
    [SerializeField] GameObject faceNorth;
    [BoxGroup("Face Buttons")]
    [SerializeField] GameObject faceEast;
    [BoxGroup("Face Buttons")]
    [SerializeField] GameObject faceSouth;
    [BoxGroup("Face Buttons")]
    [SerializeField] GameObject faceWest;
    List<GameObject> faceButtons = new List<GameObject>();

    private void Awake()
    {
        canvas = GetComponent<Canvas>();
        heartObj.SetActive(false);

        faceButtons.Add(faceNorth);
        faceButtons.Add(faceEast);
        faceButtons.Add(faceSouth);
        faceButtons.Add(faceWest);
        DisableFaceButtons();

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
    public void ShowFaceButton(string _direction)
    {
        DisableFaceButtons();

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
        }
    }

    /// <summary>
    /// Disables all face button prompts.
    /// </summary>
    public void DisableFaceButtons()
    {
        foreach (var b in faceButtons)
            b.SetActive(false);
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
