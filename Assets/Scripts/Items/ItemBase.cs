﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBase : ScriptableObject
{
    [SerializeField] string name;
    [SerializeField] string description;
    [SerializeField] Sprite icon;
    [SerializeField] float price;
    [SerializeField] bool isSellable;

    public virtual string Name => name;
    // public string Name {
    //     get => name;
    // }   this means the same thing as above
    public string Description => description;
    // public string Description {
    //     get => description;
    // }   this means the same thing as above
    public Sprite Icon => icon;
    // public Sprite Icon {
    //     get => icon;
    // }   this means the same thing as above
    public float Price => price;
    public bool IsSellable => isSellable;

    public virtual bool Use(Pokemon pokemon)
    {
        return false;   
    }

    public virtual bool IsReusable => false;

    public virtual bool CanUseInBattle => true;
    public virtual bool CanUseOutsideBattle => true;
}
