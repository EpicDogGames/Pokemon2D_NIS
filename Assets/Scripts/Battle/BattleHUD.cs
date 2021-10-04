using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BattleHUD : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text lvlText;
    [SerializeField] Text statusText;
    [SerializeField] HPBar hpBar;
    [SerializeField] GameObject expBar;

    [SerializeField] Color psnColor;
    [SerializeField] Color brnColor;
    [SerializeField] Color frzColor;
    [SerializeField] Color parColor;
    [SerializeField] Color slpColor;

    Pokemon _pokemon;
    Dictionary<ConditionID, Color> statusColors;

    public void SetData(Pokemon pokemon)
    {
        _pokemon = pokemon;

        nameText.text = pokemon.Base.Name;
        SetLevel();
        hpBar.SetHP((float) pokemon.HP / pokemon.MaxHp); 
        SetExp();

        statusColors = new Dictionary<ConditionID, Color>()
        {
            {ConditionID.psn, psnColor},
            {ConditionID.brn, brnColor},
            {ConditionID.frz, frzColor},
            {ConditionID.par, parColor},
            {ConditionID.slp, slpColor}
        };

        SetStatusText();
        _pokemon.OnStatusChanged += SetStatusText;
    }

    private void SetStatusText()
    {
        if (_pokemon.Status  == null)
            statusText.text = "";
        else
        {
            statusText.text = _pokemon.Status.ID.ToString().ToUpper();
            statusText.color = statusColors[_pokemon.Status.ID];
        }
    }

    public void SetLevel()
    {
        lvlText.text = "Lvl " + _pokemon.Level;        
    }

    public IEnumerator UpdateHP()
    {
        if (_pokemon.HpChanged)
        {
            yield return hpBar.SetHPSmooth((float)_pokemon.HP / _pokemon.MaxHp);
            _pokemon.HpChanged = false;
        }
    }

    public void SetExp()
    {
        if (expBar == null)  return;

        float normalizedExp = GetNormalizedExp();
        expBar.transform.localScale = new Vector3(normalizedExp, 1, 1);
    }

    public IEnumerator SetExpSmooth(bool reset=false)
    {
        if (expBar == null)  yield break;

        if (reset) 
        {
            expBar.transform.localScale = new Vector3(0, 1, 1);    
        }

        float normalizedExp = GetNormalizedExp();
        yield return expBar.transform.DOScaleX(normalizedExp, 1.5f).WaitForCompletion();
    }
        
    private float GetNormalizedExp()
    {
        int currentLevelExp = _pokemon.Base.GetExpForLevel(_pokemon.Level);
        int nextLevelExp = _pokemon.Base.GetExpForLevel(_pokemon.Level + 1);

        float normalizedExp = (float)(_pokemon.Exp - currentLevelExp) / (nextLevelExp - currentLevelExp);   
        return Mathf.Clamp01(normalizedExp);
    }
}
