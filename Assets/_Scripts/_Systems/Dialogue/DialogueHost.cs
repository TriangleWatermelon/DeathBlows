using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[RequireComponent(typeof(DialogueReader))]
public class DialogueHost : MonoBehaviour
{
    DialogueReader d_Reader;
    DialoguePresenter d_Presenter;

    PlayerController player;

    int dialogueIndex = 0;
    int dialogueMax;

    [BoxGroup("NPC Details")]
    [SerializeField] string npcName;

    [BoxGroup("Visuals")]
    [SerializeField] GameObject interactObj;

    private void Awake()
    {
        d_Reader = GetComponent<DialogueReader>();
        dialogueMax = d_Reader.dialogueOptions.Count - 1;
        d_Presenter = GetComponentInChildren<DialoguePresenter>();
        d_Presenter.SetDialogueText(d_Reader.GetDialogue(0));
        player = FindObjectOfType<PlayerController>();
    }

    //In-Progress
    public void PromptDialogue()
    {
        interactObj.SetActive(true);
        player.SetDialogueHost(this);
    }

    //In-Progress
    public void StartDialogue()
    {
        interactObj.SetActive(false);
        d_Presenter.ShowDialogueBox(true, npcName);
    }

    //In-Progress
    public void EndDialogue()
    {
        interactObj.SetActive(false);
        d_Presenter.ShowDialogueBox(false);
        player.ClearDialogueHost();
    }

    //In-Progress
    public void CycleNextDialogue()
    {
        dialogueIndex++;
        if (dialogueIndex > dialogueMax)
        {
            EndDialogue();
            return;
        }

        d_Presenter.SetDialogueText(d_Reader.GetDialogue(dialogueIndex));
    }

    //In-Progress
    public void CyclePreviousDialogue()
    {
        if (dialogueIndex > 0)
            dialogueIndex--;

        d_Presenter.SetDialogueText(d_Reader.GetDialogue(dialogueIndex));
    }
}
