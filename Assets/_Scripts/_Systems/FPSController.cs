using UnityEngine;

public class FPSController : MonoBehaviour
{
    FPSController instance;

    void Start()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }

    public static void AdjustTargetFrameRate(int _desiredFPS) => Application.targetFrameRate = _desiredFPS;
}
