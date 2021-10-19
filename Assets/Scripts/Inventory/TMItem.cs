using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new TM or HM")]
// this is inheritance from ItemBase and will be a scriptable object as well
public class TMItem : ItemBase
{
    [SerializeField] MoveBase move;         // removed from inventory after use
    [SerializeField] bool isHM;             // not removed from inventory after use

    public override string Name => base.Name + $": {move.Name}";

    public override bool Use(Pokemon pokemon)
    {
        // learning move handled from InventoryUI, if it was learned will return true
        return pokemon.HasMove(move);
    }

    public bool CanBeTaught(Pokemon pokemon)
    {
        return pokemon.Base.LearnableByItems.Contains(Move);
    }

    public override bool CanUseInBattle => false;

    public override bool IsReusable => isHM;

    public MoveBase Move => move;
    public bool IsHM => isHM;
}
