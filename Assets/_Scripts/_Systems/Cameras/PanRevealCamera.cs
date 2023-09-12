using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;

public class PanRevealCamera : MonoBehaviour
{
    [TitleGroup("Main Camera")]
    public GameObject mainCamObj;
    Camera mainCam;
    float mainCamOrthoSize;

    [TitleGroup("Pan Reveal Camera")]
    public GameObject panCamObj;
    Camera panCam;
    float panCamOrthoSize;
    Vector3 panCamPosition;

    Coroutine panToRoutine;
    float panTimer;
    [BoxGroup("Pan Values")]
    [SerializeField] float maxPanTime;

    [BoxGroup("Pan Values")]
    [SerializeField] float maxPanHoldTime;
    float panHoldTimer;

    private void Start()
    {
        //Get the cameras from their parent objects.
        mainCam = mainCamObj.GetComponent<Camera>();
        mainCamOrthoSize = mainCam.orthographicSize;
        panCam = panCamObj.GetComponent<Camera>();
        panCamOrthoSize = panCam.orthographicSize;
        panCamPosition = panCamObj.transform.position;

        //Stops the update loop.
        panHoldTimer = maxPanHoldTime;
    }

    private void Update()
    {
        //Used just to time the hold on the frame that is panned to.
        if (panHoldTimer < maxPanHoldTime)
        {
            panHoldTimer += Time.deltaTime;
            if (panHoldTimer > maxPanHoldTime)
                StartCameraPan(false);
        }
    }

    /// <summary>
    /// Sets the pan camera position and size to match the main camera and toggles which (of the two) is enabled.
    /// </summary>
    /// <param name="_state"></param>
    public void ChangeCameraToMain(bool _state)
    {
        panCamObj.transform.position = mainCamObj.transform.position;
        panCam.orthographicSize = mainCamOrthoSize;
        panCamObj.SetActive(!_state);
        mainCamObj.SetActive(_state);

        Debug.Log($"Camera switched: {mainCamObj.transform.position} /n{panCamObj.transform.position}");
    }

    /// <summary>
    /// Takes two points and starts a coroutine to move the camera between those two points.
    /// </summary>
    /// <param name="pointA"></param>
    /// <param name="pointB"></param>
    /// <param name="orthoSize"></param>
    /// <param name="panAwayFromMain"></param>
    public void StartCameraPan(bool panAwayFromMain)
    {
        panTimer = 0;
        if (panAwayFromMain)
        {
            ChangeCameraToMain(false);
            panToRoutine = StartCoroutine(PanTo(mainCamObj.transform.position, panCamPosition, panAwayFromMain));
            Debug.Log("Panning away from main.");
        }
        else
        {
            panToRoutine = StartCoroutine(PanTo(panCamPosition, mainCamObj.transform.position, panAwayFromMain));
            Debug.Log("Panning to main.");
        }
    }

    /// <summary>
    /// Lerps the camera between two points. Lerps the camera size between two values.
    /// </summary>
    /// <param name="pointA"></param>
    /// <param name="pointB"></param>
    /// <param name="orthoSize"></param>
    /// <param name="panAwayFromMain"></param>
    /// <returns></returns>
    IEnumerator PanTo(Vector3 pointA, Vector3 pointB, bool panAwayFromMain)
    {
        while (panTimer < 1)
        {
            yield return new WaitForFixedUpdate();
            panTimer += (Time.deltaTime / maxPanTime);
            Vector3 pos = Vector3.Lerp(pointA, pointB, panTimer);
            panCamObj.transform.position = pos;
            if (panAwayFromMain)
            {
                float camOrtho = Mathf.Lerp(mainCamOrthoSize, panCamOrthoSize, panTimer);
                panCam.orthographicSize = camOrtho;
            }
            else
            {
                float camOrtho = Mathf.Lerp(panCamOrthoSize, mainCamOrthoSize, panTimer);
                panCam.orthographicSize = camOrtho;
            }
        }
        if (panAwayFromMain)
            panHoldTimer = 0;
        else
            ChangeCameraToMain(true);
        StopCoroutine(panToRoutine);
    }
}
