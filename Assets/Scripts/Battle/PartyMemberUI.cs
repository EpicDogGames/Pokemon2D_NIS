using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text lvlText;
    [SerializeField] HPBar hpBar;
    [SerializeField] Image selectUnselectBackground;

    //[SerializeField] Color highlightedColor;
    [SerializeField] Color selectedTextColor;
    [SerializeField] Color unselectedTextColor;
    [SerializeField] Color selectedImageColor;
    [SerializeField] Color unselectedImageColor;

    Pokemon _pokemon;

    public void Init(Pokemon pokemon)
    {
        _pokemon = pokemon;
        UpdateData();

        _pokemon.OnHPChanged += UpdateData;
    }

    void UpdateData()
    {
        nameText.text = _pokemon.Base.Name;
        lvlText.text = "Lvl " + _pokemon.Level;
        hpBar.SetHP((float)_pokemon.HP / _pokemon.MaxHp);
    }

    public void SetSelected(bool selected) 
    {
        if (selected) 
        {
            //nameText.color = highlightedColor;   
            nameText.color = selectedTextColor;
            selectUnselectBackground.color = selectedImageColor; 
        }
        else
        {
            //nameText.color = Color.black;
            nameText.color = unselectedTextColor;
            selectUnselectBackground.color = unselectedImageColor;
        }
    }
}
