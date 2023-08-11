using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[RequireComponent(typeof(DialogueReader))]
public class DialogueHost : MonoBehaviour
{
    DialogueReader d_Reader;
    DialoguePresenter d_Presenter;

    int dialogueIndex = 0;

    [BoxGroup("NPC Details")]
    [SerializeField] string npcName;

    private void Awake()
    {
        d_Reader = GetComponent<DialogueReader>();
        d_Presenter = GetComponentInChildren<DialoguePresenter>();
        CycleNextDialogue();
    }

    //In-Progress
    public void StartDialogue()
    {
        d_Presenter.ShowDialogueBox(true, npcName);
    }

    //In-Progress
    public void EndDialogue()
    {
        d_Presenter.ShowDialogueBox(false);
    }

    //In-Progress
    public void CycleNextDialogue()
    {
        d_Presenter.SetDialogueText(d_Reader.GetDialogue(dialogueIndex));
        dialogueIndex++;
    }

    //In-Progress
    public void CyclePreviousDialogue()
    {
        dialogueIndex--;
        d_Presenter.SetDialogueText(d_Reader.GetDialogue(dialogueIndex));
    }
}
