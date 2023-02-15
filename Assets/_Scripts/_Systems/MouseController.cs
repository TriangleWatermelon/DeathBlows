using UnityEngine;

public class MouseController : MonoBehaviour
{
    MouseController instance;

    private void Start()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);

        ToggleMouseState(false);
    }

    public static void ToggleMouseState(bool _state) => Cursor.visible = _state;
}
