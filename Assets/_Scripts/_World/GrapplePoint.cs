using UnityEngine;
using Sirenix.OdinInspector;

public class GrapplePoint : MonoBehaviour
{
    [BoxGroup("Components")]
    [SerializeField] GameObject canAttachObj;
    [BoxGroup("Components")]
    [SerializeField] GameObject attachedObj;

    PlayerController player;

    BoxCollider2D col;

    [HideInInspector]
    public Vector3 position { get; private set; }

    private void Start()
    {
        position = transform.position;
        player = FindObjectOfType<PlayerController>();
        col = GetComponent<BoxCollider2D>();
        attachedObj.SetActive(false);
        Deactivate();
    }

    //In-Progress
    public void Activate()
    {
        canAttachObj.SetActive(true);
        col.enabled = true;
        attachedObj.SetActive(false);
    }

    //In-Progress
    public void Deactivate()
    {
        canAttachObj.SetActive(false);
        col.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("GrappleCheck"))
        {
            player.SetGrapplePoint(this);
            attachedObj.SetActive(true);
            Deactivate();
        }
    }
}
