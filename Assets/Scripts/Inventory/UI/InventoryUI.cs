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

    [SerializeField] Text categoryText;
    [SerializeField] Image itemIcon;
    [SerializeField] Text itemDescription;

    [SerializeField] Image upArrow;
    [SerializeField] Image downArrow;

    [SerializeField] Color selectedTextColor;
    [SerializeField] Color unselectedTextColor;
    [SerializeField] Color selectedImageColor;
    [SerializeField] Color unselectedImageColor;

    [SerializeField] PartyScreen partyScreen;

    Action<ItemBase> onItemUsed; 

    int selectedItem = 0;
    int selectedCategory = 0;

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

        inventory.OnUpdated += UpdateItemList;  
    }

    private void UpdateItemList()
    {
        Debug.Log("Calling UpdateItemList");
        // clear all existing items
        foreach (Transform child in itemList.transform)
            Destroy(child.gameObject);

        // this will reset the selections to the first one of the list
        // it will be highlighted
        selectedItem = 0;
        
        slotUIList = new List<ItemSlotUI>();
        foreach (var itemSlot in inventory.GetSlotsByCategory(selectedCategory))
        {
            var slotUIObj = Instantiate(itemSlotUI, itemList.transform);
            slotUIObj.SetData(itemSlot);

            slotUIList.Add(slotUIObj);
        }

        UpdateItemSelection();
    }

    public void HandleUpdate(Action onBack, Action<ItemBase> onItemUsed=null)
    {
        this.onItemUsed = onItemUsed;

        if (state == InventoryUIState.ItemSelection)
        {
            int previousSelection = selectedItem;
            int previousCategory = selectedCategory;

            var gamepadConnected = GameController.Instance.IsGamepadConnected();
            if (gamepadConnected)
            {
                if ((Gamepad.current.dpad.down.wasReleasedThisFrame) || (Keyboard.current.downArrowKey.wasReleasedThisFrame))
                    ++selectedItem;
                else if ((Gamepad.current.dpad.up.wasReleasedThisFrame) || (Keyboard.current.upArrowKey.wasReleasedThisFrame))
                    --selectedItem;
                else if ((Gamepad.current.dpad.right.wasReleasedThisFrame) || (Keyboard.current.rightArrowKey.wasReleasedThisFrame))
                    ++selectedCategory;
                else if ((Gamepad.current.dpad.left.wasReleasedThisFrame) || (Keyboard.current.leftArrowKey.wasReleasedThisFrame))
                    --selectedCategory; 
            }
            else
            {
                if (Keyboard.current.downArrowKey.wasReleasedThisFrame)
                    ++selectedItem;
                else if (Keyboard.current.upArrowKey.wasReleasedThisFrame)
                    --selectedItem;
                else if (Keyboard.current.rightArrowKey.wasReleasedThisFrame)
                    ++selectedCategory;
                else if (Keyboard.current.leftArrowKey.wasReleasedThisFrame)
                    --selectedCategory;
            }

            //selectedCategory = Mathf.Clamp(selectedCategory, 0, Inventory.ItemCategories.Count - 1);
            if (selectedCategory > Inventory.ItemCategories.Count - 1)
                selectedCategory = 0;
            else if (selectedCategory < 0)
                selectedCategory = Inventory.ItemCategories.Count - 1;
            selectedItem = Mathf.Clamp(selectedItem, 0, inventory.GetSlotsByCategory(selectedCategory).Count - 1);

            if (previousCategory != selectedCategory)
            {
                ResetSelection();
                categoryText.text = Inventory.ItemCategories[selectedCategory];
                UpdateItemList();
            }
            else if (previousSelection != selectedItem)
            {
                UpdateItemSelection();
            }

            if (gamepadConnected)
            {
                if ((Gamepad.current.buttonSouth.wasReleasedThisFrame) || (Keyboard.current.enterKey.wasReleasedThisFrame))
                {
                    ItemSelected();
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
                    ItemSelected();
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
                StartCoroutine(UseItem());
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
        var slots = inventory.GetSlotsByCategory(selectedCategory);

        selectedItem = Mathf.Clamp(selectedItem, 0, slots.Count - 1);

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

        if (slots.Count > 0) 
        {
            var item = slots[selectedItem].Item;
            itemIcon.sprite = item.Icon;
            itemDescription.text = item.Description;
        }

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

    void ResetSelection()
    {
        selectedItem = 0;

        upArrow.gameObject.SetActive(false);
        downArrow.gameObject.SetActive(false);

        itemIcon.sprite = null;
        itemDescription.text = "";
    }

    void ItemSelected()
    {
        if (selectedCategory == (int)ItemCategory.Pokeballs)
        {
            StartCoroutine(UseItem());
        }
        else
        {
            OpenPartyScreen();
        }
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

    private IEnumerator UseItem()
    {
        state = InventoryUIState.Busy;

        var usedItem = inventory.UseItem(selectedItem, partyScreen.SelectedMember, selectedCategory);
        if (usedItem != null)
        {
            if (!usedItem is PokeballItem)
                yield return DialogueManager.Instance.ShowDialogueText($"The player used {usedItem.Name}");

            onItemUsed?.Invoke(usedItem);
        }
        else
        {
            yield return DialogueManager.Instance.ShowDialogueText($"It won't have any affect!");            
        }

        ClosePartyScreen();
    }
}
