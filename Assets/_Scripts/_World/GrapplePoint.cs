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

    private void Start()
    {
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
