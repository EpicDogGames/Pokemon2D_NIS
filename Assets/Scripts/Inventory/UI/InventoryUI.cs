using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public enum InventoryUIState { ItemSelection, PartySelection, Busy }

public class InventoryUI : MonoBehaviour
{
    [SerializeField] GameObject itemList;
    [SerializeField] ItemSlotUI itemSlotUI;

    [SerializeField] Image itemIcon;
    [SerializeField] Text itemDescription;

    [SerializeField] Image upArrow;
    [SerializeField] Image downArrow;

    [SerializeField] Color selectedTextColor;
    [SerializeField] Color unselectedTextColor;
    [SerializeField] Color selectedImageColor;
    [SerializeField] Color unselectedImageColor;

    [SerializeField] PartyScreen partyScreen;

    int selectedItem;

    InventoryUIState state;

    const int itemsInViewport = 8;

    List<ItemSlotUI> slotUIList;
    Inventory inventory;
    RectTransform itemListRect;

    private void Awake() 
    {
        inventory = Inventory.GetInventory();
        itemListRect = itemList.GetComponent<RectTransform>();
    }

    private void Start() 
    {
        UpdateItemList();    
    }

    private void UpdateItemList()
    {
        // clear all existing items
        foreach (Transform child in itemList.transform)
            Destroy(child.gameObject);
        
        slotUIList = new List<ItemSlotUI>();
        foreach (var itemSlot in inventory.Slots)
        {
            var slotUIObj = Instantiate(itemSlotUI, itemList.transform);
            slotUIObj.SetData(itemSlot);

            slotUIList.Add(slotUIObj);
        }

        UpdateItemSelection();
    }

    public void HandleUpdate(Action onBack)
    {
        if (state == InventoryUIState.ItemSelection)
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

            selectedItem = Mathf.Clamp(selectedItem, 0, inventory.Slots.Count - 1);

            if (previousSelection != selectedItem)
                UpdateItemSelection();

            if (gamepadConnected)
            {
                if ((Gamepad.current.buttonSouth.wasReleasedThisFrame) || (Keyboard.current.enterKey.wasReleasedThisFrame))
                {
                    OpenPartyScreen();
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
                    OpenPartyScreen();
                }
                else if (Keyboard.current.escapeKey.wasReleasedThisFrame)
                {
                    onBack?.Invoke();
                }
            }
        }
        else if (state == InventoryUIState.PartySelection)
        {
            Action onSelected = () =>
            {
                // use the item on selected pokemon
            };

            Action onBackPartyScreen = () =>
            {
                ClosePartyScreen();
            };

            partyScreen.HandleUpdate(onSelected, onBackPartyScreen);
        }
    }

    void UpdateItemSelection()
    {
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

        var item = inventory.Slots[selectedItem].Item;
        itemIcon.sprite = item.Icon;
        itemDescription.text = item.Description;

        HandleScrolling();
    }

    void HandleScrolling()
    {
        if (slotUIList.Count <= itemsInViewport)  return;
        
        float scrollPos = Mathf.Clamp(selectedItem - itemsInViewport/2, 0, selectedItem) * slotUIList[0].Height;
        // use localPosition because it is a child object
        itemListRect.localPosition = new Vector2(itemListRect.localPosition.x, scrollPos);

        bool showUpArrow = selectedItem > itemsInViewport / 2;
        upArrow.gameObject.SetActive(showUpArrow);
        bool showDownArrow = selectedItem + itemsInViewport / 2 < slotUIList.Count;
        downArrow.gameObject.SetActive(showDownArrow);
    }

    void OpenPartyScreen()
    {
        state = InventoryUIState.PartySelection;
        partyScreen.gameObject.SetActive(true);
    }

    void ClosePartyScreen()
    {
        state = InventoryUIState.ItemSelection;
        partyScreen.gameObject.SetActive(false);
    }
}
