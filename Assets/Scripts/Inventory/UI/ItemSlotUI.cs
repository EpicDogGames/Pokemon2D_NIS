using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlotUI : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text countText;
    [SerializeField] Image selectUnselectBackground;

    RectTransform rectTransform;

    public Text NameText => nameText;
    public Text CountText => countText;
    public Image SelectUnselectBackground => selectUnselectBackground;
    public float Height => rectTransform.rect.height;

    private void Awake() 
    {
        rectTransform = GetComponent<RectTransform>();   
    }

    public void SetData(ItemSlot itemSlot)
    {
        nameText.text = itemSlot.Item.Name;
        countText.text = $"X {itemSlot.Count}";
    }
}
