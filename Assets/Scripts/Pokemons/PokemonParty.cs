﻿using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokemonParty : MonoBehaviour
{
    [SerializeField] List<Pokemon> pokemons;

    public event Action OnUpdated;

    public List<Pokemon> Pokemons
    {
        get {
            return pokemons;
        }
        set {
            pokemons = value;
            OnUpdated?.Invoke();
        }
    }

    private void Awake() 
    {
        foreach (var pokemon in pokemons)
        {
            pokemon.Init();   
        }    
    }

    public Pokemon GetHealthyPokemon()
    {
        return pokemons.Where(x => x.HP > 0).FirstOrDefault();
    }

    public void AddPokemon(Pokemon newPokemon)
    {
        if (pokemons.Count < 6)
        {
            pokemons.Add(newPokemon);
            OnUpdated?.Invoke();
        }
        else
        {
            // add to the PC once that's implemented
        }
    }

    public bool CheckForEvolutions()
    {
        // check if any pokemon in our party has an evolution
        return pokemons.Any(p => p.CheckForEvolution() != null);
    }

    public IEnumerator RunEvolutions()
    {
        foreach (var pokemon in pokemons)
        {
            var evolution = pokemon.CheckForEvolution();
            if (evolution != null)
            {
                yield return EvolutionManager.Instance.Evolve(pokemon, evolution);
            }
        }
    }

    public static PokemonParty GetPlayerParty()
    {
        return FindObjectOfType<PlayerController>().GetComponent<PokemonParty>();
    }

    public void PartyUpdated()
    {
        OnUpdated?.Invoke();
    }
}
