using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class DialogueReader : MonoBehaviour
{
    public List<string> dialogueOptions;

    //In-Progress
    public string GetDialogue(int _index)
    {
        return dialogueOptions[_index];
    }
}
