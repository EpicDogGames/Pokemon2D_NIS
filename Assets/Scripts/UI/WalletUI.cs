using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WalletUI : MonoBehaviour
{
    [SerializeField] Text moneyText;

    private void Start() 
    {
        Wallet.Instance.OnMoneyChanged += SetMoneyText;     
    }
    
    public void Show()
    {
        gameObject.SetActive(true);
        SetMoneyText();
    }

    public void Close() 
    {
        gameObject.SetActive(false);    
    }

    private void SetMoneyText()
    {
        moneyText.text = "$" + Wallet.Instance.Money;
    }
}
