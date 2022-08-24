using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChoiceText : MonoBehaviour
{
    Text text;

    [SerializeField] Color selectedTextColor;
    [SerializeField] Color unselectedTextColor;

    private void Awake() 
    {
        text = GetComponent<Text>();    
    }

    public void SetSelected(bool selected) 
    {
        // if (selected)
        // { 
        //     text.color = selectedTextColor;
        // }
        // else
        // {
        //     text.color = unselectedTextColor;
        // }

        text.color = (selected) ? selectedTextColor : unselectedTextColor; 
    }

    public Text TextField => text;
}
