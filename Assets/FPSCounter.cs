using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FPSCounter : MonoBehaviour
{
    TextMeshProUGUI text;

    private void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
        InvokeRepeating(nameof(GetFPS), 1, 1);
    }

    private void GetFPS()
    {
        float fps = (int)(1f / Time.unscaledDeltaTime);
        text.text = fps.ToString();
    }
}
