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

    public bool IsShowing { get; private set; }

    public event Action OnShowDialogue;
    public event Action OnDialogueFinished;

    public static DialogueManager Instance { get; private set; }

    private void Awake() 
    {
        Instance = this;
    }

    public IEnumerator ShowDialogueText(string text, bool waitForInput=true, bool autoClose=true) 
    {
        OnShowDialogue?.Invoke();

        var gamepadConnected = GameController.Instance.IsGamepadConnected();
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
                OnDialogueFinished?.Invoke();
    }

    public void CloseDialogue()
    {
        dialogueBox.SetActive(false);
        IsShowing = false;

    }

    public IEnumerator ShowDialogue(Dialogue dialogue)
    {
        var gamepadConnected = GameController.Instance.IsGamepadConnected();
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
        OnDialogueFinished?.Invoke();
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
