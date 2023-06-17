using UnityEngine;
using Sirenix.OdinInspector;

public class GrapplePoint : MonoBehaviour
{
    bool canAttach = true;

    [BoxGroup("Components")]
    [SerializeField] GameObject canAttachObj;
    [BoxGroup("Components")]
    [SerializeField] GameObject attachedObj;

    PlayerController player;

    private void Start()
    {
        player = FindObjectOfType<PlayerController>();
        attachedObj.SetActive(false);
        Deactivate();
    }

    //In-Progress
    public void Activate()
    {
        canAttachObj.SetActive(true);
        canAttach = true;
    }

    //In-Progress
    public void Deactivate()
    {
        canAttachObj.SetActive(false);
        canAttach = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("GrappleCheck"))
            player.SetGrapplePoint(this);

        Debug.Log($"Entered trigger: {collision.gameObject.name}");
    }
}
