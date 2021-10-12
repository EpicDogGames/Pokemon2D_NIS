using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new recovery item")]
// this is inheritance from ItemBase and will be a scriptable object as well
public class RecoveryItem : ItemBase
{
    [Header("HP")]
    [SerializeField] int hpAmount;
    [SerializeField] bool restoreMaxHP;

    [Header("PP")]
    [SerializeField] int ppAmount;
    [SerializeField] bool restoreMaxPP;

    [Header("Status Conditions")]
    [SerializeField] ConditionID status;
    [SerializeField] bool recoverAllStatus;
    
    [Header("Revive")]
    [SerializeField] bool revive;
    [SerializeField] bool maxRevive;

    public override bool Use(Pokemon pokemon)
    {
        // revive
        if (revive || maxRevive)
        {
            if (pokemon.HP > 0)
                return false;
            
            if (revive)
                pokemon.IncreaseHP(pokemon.MaxHp / 2);
            else
                pokemon.IncreaseHP(pokemon.MaxHp);
            
            pokemon.CureStatus();

            return true;
        }

        // indicates a fainted pokemon
        // if fainted, no other item than revive can be used on it
        if (pokemon.HP == 0)
            return false;

        // restore HP
        if (restoreMaxHP || hpAmount > 0)
        {
            if (pokemon.HP == pokemon.MaxHp)
                return false;

            if (restoreMaxHP)
                pokemon.IncreaseHP(pokemon.MaxHp);
            else
                pokemon.IncreaseHP(hpAmount);
        }

        // recover status
        if (recoverAllStatus || status != ConditionID.none) 
        {
            if (pokemon.Status == null && pokemon.VolatileStatus != null) 
                return false; 

            if (recoverAllStatus)
            {
                pokemon.CureStatus();
                pokemon.CureVolatileStatus();
            }
            else
            {
                if (pokemon.Status.ID == status)
                    pokemon.CureStatus();
                else if (pokemon.VolatileStatus.ID == status)
                    pokemon.CureVolatileStatus();
                else
                    return false;
            }  
        }

        // restore PP
        if (restoreMaxPP)
            pokemon.Moves.ForEach(m => m.IncreasePP(m.Base.PP));
        else if (ppAmount > 0)
            pokemon.Moves.ForEach(m => m.IncreasePP(ppAmount));            

        return true;
    }
}
