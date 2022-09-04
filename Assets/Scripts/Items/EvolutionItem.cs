using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new evolution item")]
public class EvolutionItem : ItemBase
{
    // logic has to be handled in the inventoryUI script
    public override bool Use(Pokemon pokemon)
    {
        return true;
    }
}
