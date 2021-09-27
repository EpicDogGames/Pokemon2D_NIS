using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Pokemon", menuName = "Pokemon/Create new pokemon", order = 0)]
public class PokemonBase : ScriptableObject 
{
    [SerializeField] string name;

    [TextArea]
    [SerializeField] string description;

    [SerializeField] Sprite frontSprite;
    [SerializeField] Sprite backSprite; 

    [SerializeField] PokemonType type1;
    [SerializeField] PokemonType type2;

    // Base Stats
    [SerializeField] int maxHp;
    [SerializeField] int attack;
    [SerializeField] int defense;
    [SerializeField] int spAttack;
    [SerializeField] int spDefense;
    [SerializeField] int speed;

    [SerializeField] List<LearnableMove> learnableMoves;

    public string Name 
    {
        get { return name; }
    }

    public string Description
    {
        get { return description; }
    }

    public Sprite FrontSprite
    {
        get { return frontSprite; }
    }

    public Sprite BackSprite
    {
        get { return backSprite; }
    }

    public PokemonType Type1
    {
        get { return type1; }
    }

    public PokemonType Type2
    {
        get { return type2; }
    }

    public int MaxHp
    {
        get { return maxHp; }
    }

    public int Attack
    {
        get { return attack; }
    }

    public int SpAttack
    {
        get { return spAttack; }
    }

    public int Defense
    {
        get { return defense; }
    }

    public int SpDefense
    {
        get { return spDefense; }
    }

    public int Speed
    {
        get { return speed; }
    }

    public List<LearnableMove> LearnableMoves 
    {
        get { return learnableMoves; }
    }

}

[System.Serializable]
public class LearnableMove
{
    [SerializeField] MoveBase moveBase;
    [SerializeField] int level; 

    public MoveBase Base 
    {
        get { return moveBase; }
    }

    public int Level
    {
        get { return level; }
    } 
}

public enum PokemonType
{
    None,
    Normal,
    Fire,
    Water,
    Electric,
    Grass,
    Ice,
    Fighting,
    Poison,
    Ground,
    Flying,
    Psychic,
    Bug,
    Rock,
    Ghost,
    Dragon,
    Dark,
    Steel,
    Fai
}

public class TypeChart
{
    static float[][] chart = 
    {
        //                    NOR  FIR   WAT   ELE   GRS   ICE   FIG   POI  GRO  FLY  PSY  BUG  ROC  GHO  DRA  DAR  STE  FAI
        /*NOR*/ new float[] { 1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,  1f,  1f,  1f,  1f, 0.5f,  0,  1f,  1f, 0.5f, 1f },
        /*FIR*/ new float[] { 1f,  0.5f, 0.5f,  1f,   2f,   2f,   1f,   1f,  1f,  1f,  1f,  2f, 0.5f, 1f, 0.5f, 1f,   2f, 1f },
        /*WAT*/ new float[] { 1f,   2f,  0.5f,  2f,  0.5f,  1f,   1f,   1f,  2f,  1f,  1f,  1f,  2f,  1f, 0.5f, 1f,  1f,  1f },
        /*ELE*/ new float[] { 1f,   1f,   2f,  0.5f, 0.5f,  2f,   1f,   1f,  0f,  2f,  1f,  1f,  1f,  1f, 0.5f, 1f,  1f,  1f },
        /*GRS*/ new float[] { 1f,  0.5f,  2f,   2f,  0.5f,  1f,   1f,  0.5f, 2f, 0.5f, 1f, 0.5f, 2f,  1f, 0.5f, 1f, 0.5f, 1f },
        /*ICE*/ new float[] { 1f,  0.5f, 0.5f,  1f,   2f,  0.5f,  1f,   1f,  2f,  2f,  1f,  1f,  1f,  1f,  2f,  1f, 0.5f, 1f },
        /*FIG*/ new float[] { 2f,   1f,   1f,   1f,   1f,   2f,   1f,  0.5f, 1f, 0.5f, 0.5f, 0.5f, 2f,  0f,  1f,  2f,   2f, 0.5f },
        /*POI*/ new float[] { 1f,   1f,   1f,   1f,   2f,   1f,   1f,  0.5f, 0.5f, 1f, 1f,  1f, 0.5f, 0.5f, 1f,  1f,   0f, 2f },
        /*GRO*/ new float[] { 1f,   2f,   1f,   2f,  0.5f,  1f,   1f,   2f,  1f,  0f,  1f, 0.5f,  2f, 1f,  1f,  1f,  2f,  1f },
        /*FLY*/ new float[] { 1f,   1f,   1f,  0.5f,  2f,   1f,   2f,   1f,  1f,  1f,  1f,  2f, 0.5f, 1f,  1f,  1f, 0.5f, 1f },
        /*PSY*/ new float[] { 1f,   1f,   1f,   1f,   1f,   1f,   2f,   2f,  1f,  1f, 0.5f, 1f,  1f,  1f,  1f,  0f, 0.5f, 1f },
        /*BUG*/ new float[] { 1f,  0.5f,  1f,   1f,   2f,   1f,  0.5f, 0.5f, 1f, 0.5f, 2f,  1f,  1f, 0.5f, 1f,  2f, 0.5f, 0.5f },
        /*ROC*/ new float[] { 1f,   2f,   1f,   1f,   1f,   2f,  0.5f,  1f, 0.5f, 2f,  1f,  2f,  1f,  1f,  1f,  1f, 0.5f, 1f },
        /*GHO*/ new float[] { 0f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,  1f,  1f, 0.5f, 1f,  1f,  2f,  1f, 0.5f, 1f,  1f },
        /*DRA*/ new float[] { 1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,  1f,  1f,  1f,  1f,  1f,  1f,  2f,  1f, 0.5f, 0f },
        /*DAR*/ new float[] { 1f,   1f,   1f,   1f,   1f,   1f,  0.5f,  1f,  1f,  1f,  2f,  1f,  1f,  2f,  1f, 0.5f, 1f, 0.5f },
        /*STE*/ new float[] { 1f,  0.5f, 0.5f, 0.5f,  1f,   2f,   1f,   1f,  1f,  1f,  1f,  2f, 0.5f, 1f,  1f,  1f, 0.5f, 2f },
        /*FAI*/ new float[] { 1f,  0.5f,  1f,   1f,   1f,   1f,   2f,  0.5f,  1f, 1f,  1f,  1f,  1f,  1f,  2f,  2f, 0.5f, 1f}
    };

    public static float GetEffectiveness(PokemonType attackType, PokemonType defenseType)
    {
        if (attackType == PokemonType.None || defenseType == PokemonType.None)
            return 1;

        // have to match PokemonType array to TypeChart array 
        // since the first row(0) of PokemonType is none and first row(0) of TypeChart is normal,
        //   must subtract 1 to get the two to match up
        int row = (int)attackType - 1;
        int col = (int)defenseType - 1;

        return chart[row][col];
    }
}

public enum Stat
{
    Attack,
    Defense,
    SpAttack,
    SpDefense,
    Speed,
    // these are not actual stats .. they just modify moveAccuracy
    Accuracy,
    Evasion
}
