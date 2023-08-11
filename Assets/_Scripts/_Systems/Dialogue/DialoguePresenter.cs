using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;

public class DialoguePresenter : MonoBehaviour
{
    [BoxGroup("Visuals")]
    [SerializeField] GameObject dialogueBoxObj;

    [BoxGroup("Text")]
    [SerializeField] TextMeshProUGUI dialogueText;
    [BoxGroup("Text")]
    [SerializeField] TextMeshProUGUI nameText;

    public void ShowDialogueBox(bool _state, string? _name = null)
    {
        if(_name != null)
            nameText.text = _name;
        dialogueBoxObj.SetActive(_state);
    }

    //In-Progress
    public void SetDialogueText(string _input)
    {
        dialogueText.text = _input;
    }
}
