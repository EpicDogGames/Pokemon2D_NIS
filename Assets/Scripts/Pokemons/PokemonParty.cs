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

    public IEnumerator CheckForEvolutions()
    {
        foreach (var pokemon in pokemons)
        {
            var evolution = pokemon.CheckForEvolution();
            if (evolution != null)
            {
                // yield return DialogueManager.Instance.ShowDialogueText($"{pokemon.Base.Name} evolved into {evolution.EvolvesInto.Name}");
                // pokemon.Evolve(evolution);
                yield return EvolutionManager.Instance.Evolve(pokemon, evolution);
            }
        }

        OnUpdated?.Invoke();
    }

    public static PokemonParty GetPlayerParty()
    {
        return FindObjectOfType<PlayerController>().GetComponent<PokemonParty>();
    }
}
