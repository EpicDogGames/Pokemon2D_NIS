using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapArea : MonoBehaviour
{
    [SerializeField] List<PokemonEncounterRecord> wildPokemons;

    [HideInInspector]
    [SerializeField] int totalChance = 0;

    private void OnValidate() 
    {
        // calculate upper and lower chance
        // sum of chances of the pokemon must equal 100 .. this is done in the inspector
        totalChance = 0;
        foreach (var record in wildPokemons)
        {
            record.chanceLower = totalChance;
            record.chanceUpper = totalChance + record.chancePercentage;

            totalChance = totalChance + record.chancePercentage;
        }
    }

    private void Start() 
    {
       
    }

    public Pokemon GetRandomWildPokemon()
    {
        int randomValue = Random.Range(1, 101);
        var pokemonRecord = wildPokemons.First(p => randomValue >= p.chanceLower && randomValue <= p.chanceUpper);

        var levelRange = pokemonRecord.levelRange;
        int level = levelRange.y == 0 ? levelRange.x : Random.Range(levelRange.x, levelRange.y+1);

        var wildPokemon = new Pokemon(pokemonRecord.pokemon, level);
        wildPokemon.Init();
        return wildPokemon;
    }
}

[System.Serializable]
public class PokemonEncounterRecord
{
    public PokemonBase pokemon;
    public Vector2Int levelRange;           // use Vector2Int because you can enter a range of values
    public int chancePercentage;

    public int chanceLower { get; set; }
    public int chanceUpper { get; set; }
}
