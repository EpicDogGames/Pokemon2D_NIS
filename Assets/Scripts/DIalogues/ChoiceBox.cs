using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ChoiceBox : MonoBehaviour
{
    [SerializeField] ChoiceText choiceTextPrefab;
    bool choiceSelected = false;

    List<ChoiceText> choiceTexts;
    int currentChoice;

    public IEnumerator ShowChoices(List<string> choices, Action<int> onChoiceSelected)
    {
        choiceSelected = false;
        currentChoice = 0;

        gameObject.SetActive(true);

        // delete all existing choices in choice box
        foreach (Transform child in transform) 
            Destroy(child.gameObject);   

        // create a choice text prefab for all the choices
        choiceTexts = new List<ChoiceText>();
        foreach (var choice in choices)
        {
            var choiceTextObject = Instantiate(choiceTextPrefab, transform);
            choiceTextObject.TextField.text = choice;
            choiceTexts.Add(choiceTextObject);
        }

        yield return new WaitUntil(() => choiceSelected == true);

        onChoiceSelected?.Invoke(currentChoice);
        gameObject.SetActive(false);
    }

    private void Update() 
    {
        var gamepadConnected = GameController.Instance.IsGamepadConnected();
        if (gamepadConnected)
        {
            if ((Gamepad.current.dpad.down.wasReleasedThisFrame) || (Keyboard.current.downArrowKey.wasReleasedThisFrame))
                ++currentChoice;
            else if ((Gamepad.current.dpad.up.wasReleasedThisFrame) || (Keyboard.current.upArrowKey.wasReleasedThisFrame))
                --currentChoice;
        }
        else
        {
            if (Keyboard.current.downArrowKey.wasReleasedThisFrame)
                ++currentChoice;
            else if (Keyboard.current.upArrowKey.wasReleasedThisFrame)
                --currentChoice;
        }

        currentChoice = Mathf.Clamp(currentChoice, 0, choiceTexts.Count-1);

        for (int i=0; i<choiceTexts.Count; i++)
        {
            choiceTexts[i].SetSelected(i == currentChoice);
        }

        if (gamepadConnected)
        {
            if ((Gamepad.current.buttonSouth.wasReleasedThisFrame) || (Keyboard.current.enterKey.wasReleasedThisFrame))
                choiceSelected = true;
        }
        else
        {
            if (Keyboard.current.enterKey.wasReleasedThisFrame)
                choiceSelected = true;
        }

    }
}
