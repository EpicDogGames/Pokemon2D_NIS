using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ShopState { Menu, Buying, Selling, Busy }

public class ShopController : MonoBehaviour
{
    [SerializeField] InventoryUI inventoryUI;
    [SerializeField] WalletUI walletUI;
    [SerializeField] CountSelectorUI countSelectorUI;

    public event Action OnStartShopping;
    public event Action OnFinishShopping;

    ShopState state;

    Inventory inventory;

    public static ShopController Instance { get; private set; }

    private void Awake() 
    {
        Instance = this;    
    }

    private void Start() 
    {
        inventory = Inventory.GetInventory();   
    }

    public IEnumerator StartTrading(Merchant merchant)
    {
        OnStartShopping?.Invoke();
        yield return StartMenuState();
    }

    private IEnumerator StartMenuState()
    {
        state = ShopState.Menu;

        int selectedChoice = 0;
        yield return DialogueManager.Instance.ShowDialogueText("How may I serve you?", waitForInput: false, choices: new List<string>() { "Buy", "Sell", "Quit" }, onChoiceSelected: choiceIndex => selectedChoice = choiceIndex);

        if (selectedChoice == 0)
        {
            // buy
        }
        else if (selectedChoice == 1)
        {
            // sell
            state = ShopState.Selling;
            inventoryUI.gameObject.SetActive(true);
        }
        else if (selectedChoice == 2)
        {
            // quit
            OnFinishShopping?.Invoke();
            yield break;
        }
    }

    public void HandleUpdate()
    {
        if (state == ShopState.Selling)
        {
            inventoryUI.HandleUpdate(OnBackFromSelling, (selectedItem) => StartCoroutine(SellItem(selectedItem)));
        }
    }

    void OnBackFromSelling()
    {
        inventoryUI.gameObject.SetActive(false);
        StartCoroutine(StartMenuState());
    }

    private IEnumerator SellItem(ItemBase item) 
    {
        state = ShopState.Busy;

        if (!item.IsSellable)
        {
            yield return DialogueManager.Instance.ShowDialogueText("You can't sell that!");
            state = ShopState.Selling;
            yield break;
        }

        walletUI.Show();

        float sellingPrice = Mathf.Round(item.Price / 2);
        int countToSell = 1;

        int itemCount = inventory.GetItemCount(item);
        if (itemCount > 1)
        {
            yield return DialogueManager.Instance.ShowDialogueText($"How many would you like to sell?", waitForInput: false, autoClose: false);

            yield return countSelectorUI.ShowSelector(itemCount, sellingPrice, (selectedCount) => countToSell = selectedCount);

            DialogueManager.Instance.CloseDialogue();
        }

        sellingPrice = sellingPrice * countToSell;

        int selectedChoice = 0;
        yield return DialogueManager.Instance.ShowDialogueText($"I can give {sellingPrice} for that. Would you like to sell?", waitForInput: false, choices: new List<string>() { "Yes", "No" }, onChoiceSelected: choiceIndex => selectedChoice = choiceIndex);

        if (selectedChoice == 0)
        {
            // yes
            inventory.RemoveItem(item, countToSell);
            // add selling price to player's wallet
            Wallet.Instance.AddMoney(sellingPrice);
            
            yield return DialogueManager.Instance.ShowDialogueText($"Turned over {item.Name} and received {sellingPrice}");
        }
        else if (selectedChoice == 1)
        {
            // no
            yield return DialogueManager.Instance.ShowDialogueText("I'm always interested in bartering. Come back when you are ready to sell something.");
        }

        walletUI.Close();

        state = ShopState.Selling;
    }
}
