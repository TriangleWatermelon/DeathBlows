using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class DialogueReader : MonoBehaviour
{
    [TitleGroup("NPC Dialogue Options")]
    public List<string> dialogueOptions;

    //In-Progress
    public string GetDialogue(int _index)
    {
        return dialogueOptions[_index];
    }
}
