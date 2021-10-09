using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PartyScreen : MonoBehaviour
{
    [SerializeField] Text messageText;

    PartyMemberUI[] memberSlots;
    List<Pokemon> pokemons;

    int selection = 0;

    public Pokemon SelectedMember => pokemons[selection];

    // party screen can be called from different states like ActionSelection, RunningTurn, AboutToUse
    public BattleState? CalledFrom { get; set; }   // question mark makes CalledFrom nullable

    public void Init() 
    {
        memberSlots = GetComponentsInChildren<PartyMemberUI>(true);
    }

    public void SetPartyData(List<Pokemon> pokemons)
    {
        this.pokemons = pokemons;

        for (int i=0; i<memberSlots.Length; i++)
        {
            if (i < pokemons.Count)
            {
                memberSlots[i].gameObject.SetActive(true);
                memberSlots[i].SetData(pokemons[i]);
            }
            else
            {
                memberSlots[i].gameObject.SetActive(false);
            }
        }

        UpdateMemberSelection(selection);

        messageText.text = "Choose a Pokemon";
    }

    public void UpdateMemberSelection(int selectedMember)
    {
        for (int i=0; i<pokemons.Count; i++)
        {
            if (i == selectedMember)
            {
                memberSlots[i].SetSelected(true);
            }
            else
            {
                memberSlots[i].SetSelected(false);                
            }
        }
    }

    public void HandleUpdate(Action onSelected, Action onBack)
    {
        var prevSelection = selection;

        var gamepadConnected = GameController.Instance.IsGamepadConnected();
        if (gamepadConnected)
        {
            if ((Gamepad.current.dpad.right.wasReleasedThisFrame) || (Keyboard.current.rightArrowKey.wasReleasedThisFrame))
                ++selection;
            else if ((Gamepad.current.dpad.left.wasReleasedThisFrame) || (Keyboard.current.leftArrowKey.wasReleasedThisFrame))
                --selection;
            else if ((Gamepad.current.dpad.down.wasReleasedThisFrame) || (Keyboard.current.downArrowKey.wasReleasedThisFrame))
                selection += 2;
            else if ((Gamepad.current.dpad.up.wasReleasedThisFrame) || (Keyboard.current.upArrowKey.wasReleasedThisFrame))
                selection -= 2;
        }
        else
        {
            if (Keyboard.current.rightArrowKey.wasReleasedThisFrame)
                ++selection;
            else if (Keyboard.current.leftArrowKey.wasReleasedThisFrame)
                --selection;
            else if (Keyboard.current.downArrowKey.wasReleasedThisFrame)
                selection += 2;
            else if (Keyboard.current.upArrowKey.wasReleasedThisFrame)
                selection -= 2;
        }

        selection = Mathf.Clamp(selection, 0, pokemons.Count - 1);

        if (selection != prevSelection)
            UpdateMemberSelection(selection);

        if (gamepadConnected)
        {
            if ((Gamepad.current.buttonSouth.wasReleasedThisFrame) || (Keyboard.current.enterKey.wasReleasedThisFrame))
            {
                onSelected?.Invoke();
            }
            else if ((Gamepad.current.buttonWest.wasReleasedThisFrame) || (Keyboard.current.escapeKey.wasReleasedThisFrame))
            {
                onBack?.Invoke();
            }
        }
        else
        {
            if (Keyboard.current.enterKey.wasReleasedThisFrame)
            {
                onSelected?.Invoke();
            }
            else if (Keyboard.current.escapeKey.wasReleasedThisFrame)
            {
                onBack?.Invoke();
            }
        }
    }

    public void SetMessageText(string message) 
    {
        messageText.text = message;
    }
}
