using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class CountSelectorUI : MonoBehaviour
{
    [SerializeField] Text countText;
    [SerializeField] Text priceText;

    bool selected;
    int currentCount;

    int maxCount;
    float pricePerUnit;

    public IEnumerator ShowSelector(int maxCount, float pricePerUnit, Action<int> onCountSelected)
    {
        this.maxCount = maxCount;
        this.pricePerUnit = pricePerUnit;

        selected = false;
        currentCount = 1;

        gameObject.SetActive(true);
        SetValues();

        yield return new WaitUntil(() => selected == true);

        onCountSelected?.Invoke(currentCount);
        gameObject.SetActive(false);
    }

    private void Update() 
    {
        int previousCount = currentCount;

        var gamepadConnected = GameController.Instance.IsGamepadConnected();
        if (gamepadConnected)
        {
            if ((Gamepad.current.dpad.up.wasReleasedThisFrame) || (Keyboard.current.upArrowKey.wasPressedThisFrame))
            {
                ++currentCount;
            }
            else if ((Gamepad.current.dpad.down.wasReleasedThisFrame) || (Keyboard.current.downArrowKey.wasPressedThisFrame))
            {
                --currentCount;
            }
        }
        else
        {
            if (Keyboard.current.upArrowKey.wasPressedThisFrame)
            {
                ++currentCount;
            }
            else if (Keyboard.current.downArrowKey.wasPressedThisFrame)
            {
                --currentCount;
            }
        }

        currentCount = Mathf.Clamp(currentCount, 1, maxCount);

        if (currentCount != previousCount)
            SetValues();

        if (gamepadConnected)
        {
            if ((Gamepad.current.buttonSouth.wasReleasedThisFrame) || (Keyboard.current.enterKey.wasReleasedThisFrame))
                selected = true;
        }
        else
        {
            if (Keyboard.current.enterKey.wasReleasedThisFrame)
                selected = true;
        }
    }

    void SetValues()
    {
        countText.text = "x " + currentCount;  
        priceText.text = "$" + pricePerUnit * currentCount;
    }
}
