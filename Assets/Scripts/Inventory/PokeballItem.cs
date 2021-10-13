using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new pokeball")]
// this is inheritance from ItemBase and will be a scriptable object as well
public class PokeballItem : ItemBase
{
    public override bool Use(Pokemon pokemon)
    {
        return true;
    }
}
