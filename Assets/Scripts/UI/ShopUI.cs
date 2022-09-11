using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class ShopUI : MonoBehaviour
{
    [SerializeField] GameObject itemList;
    [SerializeField] ItemSlotUI itemSlotUI;

    [SerializeField] Image itemIcon;
    [SerializeField] Text itemDescription;

    [SerializeField] Image upArrow;
    [SerializeField] Image downArrow;

    int selectedItem = 0;

    [SerializeField] Color selectedTextColor;
    [SerializeField] Color unselectedTextColor;
    [SerializeField] Color selectedImageColor;
    [SerializeField] Color unselectedImageColor;

    List<ItemBase> availableItems;
    Action<ItemBase> onItemSelected;
    Action onBack;

    List<ItemSlotUI> slotUIList;

    const int itemsInViewport = 8;
    RectTransform itemListRect;

    private void Awake()
    {
        itemListRect = itemList.GetComponent<RectTransform>();
    }

    public void Show(List<ItemBase> availableItems, Action<ItemBase> onItemSelected, Action onBack)
    {
        this.availableItems = availableItems;
        this.onItemSelected = onItemSelected;
        this.onBack = onBack;

        gameObject.SetActive(true);
        UpdateItemList();
    }

    public void Close() 
    {
        gameObject.SetActive(false);
    }

    public void HandleUpdate()
    {
        var previousSelection = selectedItem;

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

        selectedItem = Mathf.Clamp(selectedItem, 0, availableItems.Count - 1);

        if (selectedItem != previousSelection)
            UpdateItemSelection();

        if (gamepadConnected)
        {
            if ((Gamepad.current.buttonSouth.wasReleasedThisFrame) || (Keyboard.current.enterKey.wasReleasedThisFrame))
            {
                onItemSelected?.Invoke(availableItems[selectedItem]);
            }
            else if ((Gamepad.current.buttonEast.wasReleasedThisFrame) || (Keyboard.current.escapeKey.wasReleasedThisFrame))
            {
                onBack?.Invoke();
            }
        }
        else
        {
            if (Keyboard.current.enterKey.wasReleasedThisFrame)
            {
                onItemSelected?.Invoke(availableItems[selectedItem]);
            }
            else if (Keyboard.current.escapeKey.wasReleasedThisFrame)
            {
                onBack?.Invoke();
            }
        }
    }

    private void UpdateItemList()
    {
        // clear all existing items
        foreach (Transform child in itemList.transform)
            Destroy(child.gameObject);

        // this will reset the selections to the first one of the list
        // it will be highlighted
        selectedItem = 0;

        slotUIList = new List<ItemSlotUI>();
        foreach (var item in availableItems)
        {
            var slotUIObj = Instantiate(itemSlotUI, itemList.transform);
            slotUIObj.SetNameAndPrice(item);

            slotUIList.Add(slotUIObj);
        }

        UpdateItemSelection();
    }

    void UpdateItemSelection()
    {
        selectedItem = Mathf.Clamp(selectedItem, 0, availableItems.Count - 1);

        for (int i = 0; i < slotUIList.Count; i++)
        {
            if (i == selectedItem)
            {
                slotUIList[i].NameText.color = selectedTextColor;
                slotUIList[i].CountText.color = selectedTextColor;
                slotUIList[i].SelectUnselectBackground.color = selectedImageColor;
            }
            else
            {
                slotUIList[i].NameText.color = unselectedTextColor;
                slotUIList[i].CountText.color = unselectedTextColor;
                slotUIList[i].SelectUnselectBackground.color = unselectedImageColor;
            }
        }

        if (availableItems.Count > 0)
        {
            var item = availableItems[selectedItem];
            itemIcon.sprite = item.Icon;
            itemDescription.text = item.Description;
        }

        HandleScrolling();
    }

    void HandleScrolling()
    {
        if (slotUIList.Count <= itemsInViewport) return;

        float scrollPos = Mathf.Clamp(selectedItem - itemsInViewport / 2, 0, selectedItem) * slotUIList[0].Height;
        // use localPosition because it is a child object
        itemListRect.localPosition = new Vector2(itemListRect.localPosition.x, scrollPos);

        bool showUpArrow = selectedItem > itemsInViewport / 2;
        upArrow.gameObject.SetActive(showUpArrow);
        bool showDownArrow = selectedItem + itemsInViewport / 2 < slotUIList.Count;
        downArrow.gameObject.SetActive(showDownArrow);
    }
}
