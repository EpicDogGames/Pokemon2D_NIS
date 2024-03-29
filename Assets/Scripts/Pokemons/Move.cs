﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move
{
    public MoveBase Base { get; set; }
    public int PP { get; set; }

    public Move(MoveBase pBase) 
    {
        Base = pBase;
        PP = pBase.PP;    
    }

    public Move(MoveSaveData saveData)
    {
        Base = MoveDB.GetObjectByName(saveData.name);
        PP = saveData.pp;
    }

    public MoveSaveData GetSaveData()
    {
        var saveData = new MoveSaveData()
        {
            name = Base.name,               // have to use Name not name because it will grab the scriptable object's Name field and not the scriptable object's name (like what you see in the folder directory)
            pp = PP
        };

        return saveData;
    }

    public void IncreasePP(int amount)
    {
        Debug.Log($"Called to increase PP by {amount}");
        PP = Mathf.Clamp(PP + amount, 0, Base.PP);
    }
}

[System.Serializable]                // this is because you only need to save portions of the move class
public class MoveSaveData
{
    public string name;
    public int pp;
}
