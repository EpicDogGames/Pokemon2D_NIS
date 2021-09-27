﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum BattleState { Start, ActionSelection, MoveSelection, RunningTurn, Busy, PartyScreen, BattleOver }
public enum BattleAction { Move, SwitchPokemon, UseItem, Run }

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScreen partyScreen;

    public event Action<bool> OnBattleOver;

    BattleState state;
    BattleState? prevState;   // question mark makes prevState nullable
    int currentAction;
    int currentMove;
    int currentMember;

    PokemonParty playerParty;
    Pokemon wildPokemon;

    public void StartBattle(PokemonParty playerParty, Pokemon wildPokemon) 
    {
        this.playerParty = playerParty;
        this.wildPokemon = wildPokemon;
        StartCoroutine(SetupBattle());    
    }

    public void HandleUpdate() 
    {
        if (state == BattleState.ActionSelection)
        {
            HandleActionSelection();
        } 
        else if (state == BattleState.MoveSelection)
        {
            HandleMoveSelection();
        }
        else if (state == BattleState.PartyScreen)  
        {
            HandlePartySelection();
        } 
    }

    private void ActionSelection()
    {
        currentAction = 0;
        state = BattleState.ActionSelection;
        // StartCoroutine(dialogBox.TypeDialog("Choose an action"));   use if you want animated text
        dialogBox.SetDialog("Choose an action");                    // use if you dont wanted animated text
        dialogBox.EnableActionSelector(true);
    }

    private void MoveSelection()
    {
        currentMove = 0;
        state = BattleState.MoveSelection;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }

    private void OpenPartyScreen()
    {
        state = BattleState.PartyScreen;
        partyScreen.SetPartyData(playerParty.Pokemons);  
        partyScreen.gameObject.SetActive(true); 
    }

    private void HandleActionSelection()
    {
        if ((Gamepad.current.dpad.right.wasReleasedThisFrame) || (Keyboard.current.rightArrowKey.wasReleasedThisFrame))
            ++currentAction;
        else if ((Gamepad.current.dpad.left.wasReleasedThisFrame) || (Keyboard.current.leftArrowKey.wasReleasedThisFrame))
            --currentAction;
        else if ((Gamepad.current.dpad.down.wasReleasedThisFrame) || (Keyboard.current.downArrowKey.wasReleasedThisFrame))
            currentAction += 2;
        else if ((Gamepad.current.dpad.up.wasReleasedThisFrame) || (Keyboard.current.upArrowKey.wasReleasedThisFrame))
            currentAction -= 2;

        currentAction = Mathf.Clamp(currentAction, 0, 3);

        dialogBox.UpdateActionSelection(currentAction);

        if ((Gamepad.current.buttonSouth.wasReleasedThisFrame) || (Keyboard.current.enterKey.wasReleasedThisFrame))
        {
            if (currentAction == 0)
            {
                MoveSelection();    // fight
            }
            else if (currentAction == 1)
            { 

            } // bag
            else if (currentAction == 2)
            {
                prevState = state;
                OpenPartyScreen();  // pokemon
            }
            else if (currentAction == 3)
            { 

            } // run
        }
    }

    private void HandleMoveSelection()
    {
        if ((Gamepad.current.dpad.right.wasReleasedThisFrame) || (Keyboard.current.rightArrowKey.wasReleasedThisFrame))
            ++currentMove;
        else if ((Gamepad.current.dpad.left.wasReleasedThisFrame) || (Keyboard.current.leftArrowKey.wasReleasedThisFrame))
            --currentMove;
        else if ((Gamepad.current.dpad.down.wasReleasedThisFrame) || (Keyboard.current.downArrowKey.wasReleasedThisFrame))
            currentMove += 2;
        else if ((Gamepad.current.dpad.up.wasReleasedThisFrame) || (Keyboard.current.upArrowKey.wasReleasedThisFrame))
            currentMove -= 2;

        currentMove = Mathf.Clamp(currentMove, 0, playerUnit.Pokemon.Moves.Count - 1);

        dialogBox.UpdateMoveSelection(currentMove, playerUnit.Pokemon.Moves[currentMove]);

        if ((Gamepad.current.buttonSouth.wasReleasedThisFrame) || (Keyboard.current.enterKey.wasReleasedThisFrame))
        {
            var move = playerUnit.Pokemon.Moves[currentMove];
            if (move.PP == 0)  return;
                     
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(RunTurns(BattleAction.Move));
        }
        else if ((Gamepad.current.buttonWest.wasReleasedThisFrame) || (Keyboard.current.escapeKey.wasReleasedThisFrame))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            ActionSelection();           
        }
    }

    private void HandlePartySelection()
    {
        if ((Gamepad.current.dpad.right.wasReleasedThisFrame) || (Keyboard.current.rightArrowKey.wasReleasedThisFrame))
            ++currentMember;
        else if ((Gamepad.current.dpad.left.wasReleasedThisFrame) || (Keyboard.current.leftArrowKey.wasReleasedThisFrame))
            --currentMember;
        else if ((Gamepad.current.dpad.down.wasReleasedThisFrame) || (Keyboard.current.downArrowKey.wasReleasedThisFrame))
            currentMember += 2;
        else if ((Gamepad.current.dpad.up.wasReleasedThisFrame) || (Keyboard.current.upArrowKey.wasReleasedThisFrame))
            currentMember -= 2;

        currentMember = Mathf.Clamp(currentMember, 0, playerParty.Pokemons.Count - 1);

        partyScreen.UpdateMemberSelection(currentMember);

        if ((Gamepad.current.buttonSouth.wasReleasedThisFrame) || (Keyboard.current.enterKey.wasReleasedThisFrame))
        {
            var selectedMember = playerParty.Pokemons[currentMember];
            if (selectedMember.HP <= 0)
            {
                partyScreen.SetMessageText("You can't send out a fainted pokemon");
                return;
            }
            if (selectedMember == playerUnit.Pokemon)
            {
                partyScreen.SetMessageText("You can't switch with the same pokemon");
                return;
            }

            partyScreen.gameObject.SetActive(false);

            if (prevState == BattleState.ActionSelection)
            {
                prevState = null;
                StartCoroutine(RunTurns(BattleAction.SwitchPokemon));
            }
            else
            {
                state = BattleState.Busy;
                StartCoroutine(SwitchPokemon(selectedMember));
            }
        }
        else if ((Gamepad.current.buttonWest.wasReleasedThisFrame) || (Keyboard.current.escapeKey.wasReleasedThisFrame))
        {
            partyScreen.gameObject.SetActive(false);
            ActionSelection();
        }     
    }

    private bool CheckIfMoveHits(Move move, Pokemon source, Pokemon target)
    {
        if (move.Base.AlwaysHits)
            return true;

        float moveAccuracy = move.Base.Accuracy;

        int accuracy = source.StatBoosts[Stat.Accuracy];
        int evasion = target.StatBoosts[Stat.Evasion];

        var boostValues = new float[] { 1f, 4f/3f, 5f/3f, 2f, 7f/3f, 8f/3f, 3f };

        if (accuracy > 0)
            moveAccuracy *= boostValues[accuracy];
        else
            moveAccuracy /= boostValues[-accuracy];

        if (evasion > 0)
            moveAccuracy /= boostValues[evasion];
        else
            moveAccuracy *= boostValues[-evasion];

        return UnityEngine.Random.Range(1, 101) <= moveAccuracy;
    }

    private void BattleOver(bool won)
    {
        state = BattleState.BattleOver;
        playerParty.Pokemons.ForEach(p => p.OnBattleOver());
        OnBattleOver(won);
    }

    private void CheckForBattleOver(BattleUnit faintedUnit)
    {
        if (faintedUnit.IsPlayerUnit)
        {
            var nextPokemon = playerParty.GetHealthyPokemon();
            if (nextPokemon != null)
                OpenPartyScreen();    
            else
                BattleOver(false);
        }
        else 
            BattleOver(true);
    }

    public IEnumerator SetupBattle()
    {
        playerUnit.Setup(playerParty.GetHealthyPokemon());
        enemyUnit.Setup(wildPokemon);

        partyScreen.Init();

        dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);

        // wait for this coroutine to complete before execution goes down
        yield return StartCoroutine(dialogBox.TypeDialog($"A wild {enemyUnit.Pokemon.Base.Name} appeared."));

        ActionSelection();
    }

    private IEnumerator RunTurns(BattleAction playerAction)
    {
        state = BattleState.RunningTurn;

        if (playerAction == BattleAction.Move) 
        {
            playerUnit.Pokemon.CurrentMove = playerUnit.Pokemon.Moves[currentMove];  
            enemyUnit.Pokemon.CurrentMove = enemyUnit.Pokemon.GetRandomMove(); 

            int playerMovePriority = playerUnit.Pokemon.CurrentMove.Base.Priority;
            int enemyMovePriority = enemyUnit.Pokemon.CurrentMove.Base.Priority;

            // Check to see who goes first
            bool playerGoesFirst = true;
            if (enemyMovePriority > playerMovePriority)
                playerGoesFirst = false;
            else if (enemyMovePriority == playerMovePriority)
            { 
                 playerGoesFirst = playerUnit.Pokemon.Speed >= enemyUnit.Pokemon.Speed;
            }

            var firstUnit = (playerGoesFirst) ? playerUnit : enemyUnit;
            var secondUnit = (playerGoesFirst) ? enemyUnit : playerUnit;

            var secondPokemon = secondUnit.Pokemon;

            // First Turn
            yield return RunMove(firstUnit, secondUnit, firstUnit.Pokemon.CurrentMove);
            yield return RunAfterTurn(firstUnit);
            if (state == BattleState.BattleOver)  yield break;

            if (secondPokemon.HP > 0)
            {
                // Second Turn
                yield return RunMove(secondUnit, firstUnit, secondUnit.Pokemon.CurrentMove);
                yield return RunAfterTurn(secondUnit);
                if (state == BattleState.BattleOver)  yield break;
            }
        }
        else
        {
            if (playerAction  == BattleAction.SwitchPokemon)
            {
                var selectedPokemon = playerParty.Pokemons[currentMember];
                state = BattleState.Busy;
                yield return SwitchPokemon(selectedPokemon);
            }

            // Enemy Turn
            var enemyMove = enemyUnit.Pokemon.GetRandomMove();
            yield return RunMove(enemyUnit, playerUnit, enemyMove);
            yield return RunAfterTurn(enemyUnit);
            if (state == BattleState.BattleOver)  yield break;
        }

        if (state != BattleState.BattleOver)
            ActionSelection();
    }

    private IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        bool canRunMove = sourceUnit.Pokemon.OnBeforeMove();
        if (!canRunMove)
        {
            yield return ShowStatusChanges(sourceUnit.Pokemon);
            yield return sourceUnit.HUD.UpdateHP();
            yield break;
        }
        yield return ShowStatusChanges(sourceUnit.Pokemon);

        move.PP--;
        yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name} used {move.Base.Name}");

        if (CheckIfMoveHits(move, sourceUnit.Pokemon, targetUnit.Pokemon))
        {
            sourceUnit.PlayAttackAnimation();
            yield return new WaitForSeconds(1f);
            targetUnit.PlayHitAnimation();

            if (move.Base.Category == MoveCategory.Status)
            {
                yield return RunMoveEffects(move.Base.Effects, sourceUnit.Pokemon, targetUnit.Pokemon, move.Base.Target);
            }
            else
            {
                var damageDetails = targetUnit.Pokemon.TakeDamage(move, sourceUnit.Pokemon);
                yield return targetUnit.HUD.UpdateHP();
                yield return ShowDamageDetails(damageDetails);            
            }

            if (move.Base.SecondaryEffects != null && move.Base.SecondaryEffects.Count > 0 && targetUnit.Pokemon.HP > 0)
            {
                foreach (var secondaryEffect in move.Base.SecondaryEffects)
                {
                    var rnd = UnityEngine.Random.Range(1, 101);
                    if (rnd <= secondaryEffect.Chance)
                        yield return RunMoveEffects(secondaryEffect, sourceUnit.Pokemon, targetUnit.Pokemon, secondaryEffect.Target);                        
                }
            }


            if (targetUnit.Pokemon.HP <= 0)
            {
               yield return dialogBox.TypeDialog($"{targetUnit.Pokemon.Base.Name} Fainted");
                targetUnit.PlayFaintAnimation();
                yield return new WaitForSeconds(2f);

                CheckForBattleOver(targetUnit);
            } 
        }
        else
        {
            yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name}'s attack missed!");            
        } 
    }

    private IEnumerator RunAfterTurn(BattleUnit sourceUnit)
    {
        if (state == BattleState.BattleOver)  yield break;
        // this is here to handle if the pokemon from the player's side on the second turn has already fainted,
        //    allows for a new pokemon to be selected and its move will be used instead
        yield return new WaitUntil(() => state == BattleState.RunningTurn);

        // statuses like psn or brn will hurt the pokemon after the turn
        sourceUnit.Pokemon.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit.Pokemon);
        yield return sourceUnit.HUD.UpdateHP();
        if (sourceUnit.Pokemon.HP <= 0)
        {
            yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name} Fainted");
            sourceUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);

            CheckForBattleOver(sourceUnit);
        }
    }

    private IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        if (damageDetails.Critical > 1f)
            yield return dialogBox.TypeDialog("A critical hit!");
        
        if (damageDetails.TypeEffectiveness > 1f)
            yield return dialogBox.TypeDialog("It's super effective!");
        else if (damageDetails.TypeEffectiveness < 1f)
            yield return dialogBox.TypeDialog("It's not very effective");
    }

    private IEnumerator SwitchPokemon(Pokemon newPokemon)
    {
        if (playerUnit.Pokemon.HP > 0)
        {
            yield return dialogBox.TypeDialog($"Come back {playerUnit.Pokemon.Base.Name}");
            playerUnit.PlaySwitchAnimation();
            yield return new WaitForSeconds(1f);
        }

        playerUnit.Setup(newPokemon);
        dialogBox.SetMoveNames(newPokemon.Moves);
        yield return dialogBox.TypeDialog($"Go {newPokemon.Base.Name}!");

        state = BattleState.RunningTurn;
    }

    private IEnumerator RunMoveEffects(MoveEffects effects, Pokemon source, Pokemon target, MoveTarget moveTarget)
    {
        // stat boosting
        if (effects.Boosts != null)
        {
            if (moveTarget == MoveTarget.Self) 
                source.ApplyBoosts(effects.Boosts);
            else
                target.ApplyBoosts(effects.Boosts);
        }

        // status condition
        if (effects.Status != ConditionID.none) 
        {
            target.SetStatus(effects.Status);
        }

        // volatile status condition
        if (effects.VolatileStatus != ConditionID.none) 
        {
            target.SetVolatileStatus(effects.VolatileStatus);
        }

        yield return ShowStatusChanges(source);
        yield return ShowStatusChanges(target);        
    }

    private IEnumerator ShowStatusChanges(Pokemon pokemon)
    {
        while (pokemon.StatusChanges.Count > 0)
        {
            var message = pokemon.StatusChanges.Dequeue();
            yield return dialogBox.TypeDialog(message);
        }
    }
}
