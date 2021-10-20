using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] GameObject dialogueBox;
    [SerializeField] Text dialogueText;
    [SerializeField] float lettersPerSecond;

    bool gamepadConnected;

    public bool IsShowing { get; private set; }

    public event Action OnShowDialogue;
    public event Action OnCloseDialogue;

    public static DialogueManager Instance { get; private set; }

    private void Awake() 
    {
        Instance = this;
        gamepadConnected = GameController.Instance.IsGamepadConnected();
    }

    public void CloseDialogue()
    {
        dialogueBox.SetActive(false);
        IsShowing = false;   
    }

    public IEnumerator ShowDialogueText(string text, bool waitForInput=true, bool autoClose=true) 
    {
        IsShowing = true;
        dialogueBox.SetActive(true);  

        yield return TypeDialogue(text);
        if (waitForInput)
        {
            if (gamepadConnected)
                yield return new WaitUntil(() => ((Gamepad.current.buttonSouth.wasReleasedThisFrame) || (Keyboard.current.enterKey.wasReleasedThisFrame)));
            else
                yield return new WaitUntil(() => (Keyboard.current.enterKey.wasReleasedThisFrame));
        }

        if (autoClose)
        {
            CloseDialogue();
        }
    }

    public IEnumerator ShowDialogue(Dialogue dialogue)
    {
        yield return new WaitForEndOfFrame();

        OnShowDialogue?.Invoke();
        IsShowing = true;
        dialogueBox.SetActive(true);

        foreach (var line in dialogue.Lines)
        {
            yield return TypeDialogue(line);
            if (gamepadConnected)
                yield return new WaitUntil(() => ((Gamepad.current.buttonSouth.wasReleasedThisFrame) || (Keyboard.current.enterKey.wasReleasedThisFrame)));
            else
                yield return new WaitUntil(() => (Keyboard.current.enterKey.wasReleasedThisFrame));   
        }

        dialogueBox.SetActive(false);
        IsShowing = false;
        OnCloseDialogue?.Invoke();
    }

    public void HandleUpdate()
    {

    }

    public IEnumerator TypeDialogue(string line)
    {
        dialogueText.text = "";
        foreach (var letter in line.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(1f/lettersPerSecond);
        }
    }
}
