using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokemonGiver : MonoBehaviour, ISavable
{
    [SerializeField] Pokemon pokemonToGive;
    [SerializeField] Dialogue dialogue;
    string dialogueText;

    bool used = false;

    public IEnumerator GivePokemon(PlayerController player)
    {
        yield return DialogueManager.Instance.ShowDialogue(dialogue);

        pokemonToGive.Init();
        player.GetComponent<PokemonParty>().AddPokemon(pokemonToGive);

        used = true;

        dialogueText = $"{player.Name} received {pokemonToGive.Base.Name}";

        yield return DialogueManager.Instance.ShowDialogueText(dialogueText);
    }

    public bool CanBeGiven()
    {
        return pokemonToGive != null && !used;
    }

    public object CaptureState()
    {
        return used;
    }

    public void RestoreState(object state)
    {
        used = (bool)state;
    }
}
