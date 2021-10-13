using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new pokeball")]
// this is inheritance from ItemBase and will be a scriptable object as well
public class PokeballItem : ItemBase
{
    [SerializeField] float catchRateModifier = 1;

    public override bool Use(Pokemon pokemon)
    {
        // check to determine if in a battle
        // if not, if inventory UI pulled up, pokeballs can't be used
        if (GameController.Instance.State == GameState.Battle)
            return true;

        return false;
    }

    public float CatchRateModifier => catchRateModifier; 
}
