using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Healer : MonoBehaviour
{
    public IEnumerator Heal(Transform player, Dialogue dialogue)
    {
        int selectedChoice = 0;

        yield return DialogueManager.Instance.ShowDialogueText("You look tired? Would you like to rest here?", 
          choices: new List<string>() { "Yes please", "No thanks"}, 
          onChoiceSelected: (choiceIndex) => selectedChoice = choiceIndex);

        if (selectedChoice == 0)
        {
            // Yes
            yield return Fader.Instance.FadeIn(0.5f);

            var playerParty = player.GetComponent<PokemonParty>();
            playerParty.Pokemons.ForEach(p => p.Heal());
            playerParty.PartyUpdated();

            yield return Fader.Instance.FadeOut(0.5f);

            yield return DialogueManager.Instance.ShowDialogueText($"Your pokemon should be fully healed!");
        } 
        else if (selectedChoice == 1)
        {
            // No
            yield return DialogueManager.Instance.ShowDialogueText($"Okay! Come back if you change your mind.");
        }
    }
}
