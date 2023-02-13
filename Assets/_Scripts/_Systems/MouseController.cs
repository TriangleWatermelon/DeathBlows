using UnityEngine;

public class MouseController : MonoBehaviour
{
    private void Start()
    {
        ToggleMouseState(false);
    }

    public static void ToggleMouseState(bool _state) => Cursor.visible = _state;
}
