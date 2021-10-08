using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class MenuController : MonoBehaviour
{
    [SerializeField] GameObject menu;

    [SerializeField] Color selectedTextColor;
    [SerializeField] Color unselectedTextColor;
    [SerializeField] Color selectedImageColor;
    [SerializeField] Color unselectedImageColor;

    public event Action<int> onMenuSelected;
    public event Action onBack;

    List<Text> menuItems;
    List<Image> menuImages;

    int selectedItem = 0;

    private void Awake() 
    {
        menuItems = menu.GetComponentsInChildren<Text>().ToList();
        menuImages = menu.GetComponentsInChildren<Image>().ToList();
    }

    public void OpenMenu()
    {
        menu.SetActive(true);
        UpdateItemSelection();
    }

    public void CloseMenu()
    {
        menu.SetActive(false);
    }

    public void HandleUpdate() 
    {
        int previousSelection = selectedItem;

        var gamepadConnected = GameController.Instance.IsGamepadConnected();
        if (gamepadConnected)
        {
            if ((Gamepad.current.dpad.down.wasReleasedThisFrame) || (Keyboard.current.downArrowKey.wasReleasedThisFrame))
                ++selectedItem;
            else if ((Gamepad.current.dpad.up.wasReleasedThisFrame) || (Keyboard.current.upArrowKey.wasReleasedThisFrame))
                --selectedItem;
        }
        else
        {
            if (Keyboard.current.downArrowKey.wasReleasedThisFrame)
                ++selectedItem;
            else if (Keyboard.current.upArrowKey.wasReleasedThisFrame)
                --selectedItem;
        }

        selectedItem = Mathf.Clamp(selectedItem, 0, menuItems.Count - 1);

        if (previousSelection != selectedItem)
            UpdateItemSelection();

        if (gamepadConnected)
        {
            if ((Gamepad.current.buttonSouth.wasReleasedThisFrame) || (Keyboard.current.enterKey.wasReleasedThisFrame))
            {
                onMenuSelected?.Invoke(selectedItem);
                CloseMenu();
            }
            else if ((Gamepad.current.buttonEast.wasReleasedThisFrame) || (Keyboard.current.escapeKey.wasReleasedThisFrame))
            {
                onBack?.Invoke();
                CloseMenu();
            }
        }
        else
        {
            if (Keyboard.current.enterKey.wasReleasedThisFrame)
            {
                onMenuSelected?.Invoke(selectedItem);
                CloseMenu();
            }
            else if (Keyboard.current.escapeKey.wasReleasedThisFrame)
            {
                onBack?.Invoke();
                CloseMenu();
            }
        }
    }

    void UpdateItemSelection()
    {
        for (int i=0; i<menuItems.Count; i++)
        {
            if (i == selectedItem)
            {
                menuItems[i].color = selectedTextColor;
                menuImages[i+1].color = selectedImageColor;        // have to add 1 to count because the menu is an image and is counted in the list
            }
            else
            {
                menuItems[i].color = unselectedTextColor;
                menuImages[i+1].color = unselectedImageColor;      // have to add 1 to count because the menu is an image and is counted in the list
            }
        }
    }
}
