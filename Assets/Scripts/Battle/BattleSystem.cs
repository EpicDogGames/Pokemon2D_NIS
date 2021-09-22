using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum BattleState { Start, ActionSelection, MoveSelection, PerformMove, Busy, PartyScreen, BattleOver }
public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScreen partyScreen;

    public event Action<bool> OnBattleOver;

    BattleState state;
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
        // StartCoroutine(dialogBox.TypeDialog("Choose an action"));  use if you want animated text
        dialogBox.SetDialog("Choose an action");            // use if you dont wanted animatad text
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
                MoveSelection();    // fight
            else if (currentAction == 1)
            { } // bag
            else if (currentAction == 2)
                OpenPartyScreen();  // pokemon
            else if (currentAction == 3)
            { } // run
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
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(PlayerMove());
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
            state = BattleState.Busy;
            StartCoroutine(SwitchPokemon(selectedMember));
        }
        else if ((Gamepad.current.buttonWest.wasReleasedThisFrame) || (Keyboard.current.escapeKey.wasReleasedThisFrame))
        {
            partyScreen.gameObject.SetActive(false);
            ActionSelection();
        }     
    }

    private void BattleOver(bool won)
    {
        state = BattleState.BattleOver;
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

    private IEnumerator PlayerMove()
    {
        state = BattleState.PerformMove;

        var move = playerUnit.Pokemon.Moves[currentMove];
        yield return RunMove(playerUnit, enemyUnit, move);

        // if the battle state was not changed by RunMove, then go to next step
        if (state == BattleState.PerformMove)
            StartCoroutine(EnemyMove());
    }

    private IEnumerator EnemyMove()
    {
        state = BattleState.PerformMove;

        var move = enemyUnit.Pokemon.GetRandomMove();
        yield return RunMove(enemyUnit, playerUnit, move);

        // if the battle state was not changed by RunMove, then go to next step
        if (state == BattleState.PerformMove)
            ActionSelection();
    }

    private IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        move.PP--;
        yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name} used {move.Base.Name}");

        sourceUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(1f);

        targetUnit.PlayHitAnimation();
        var damageDetails = targetUnit.Pokemon.TakeDamage(move, sourceUnit.Pokemon);
        yield return targetUnit.HUD.UpdateHP();
        yield return ShowDamageDetails(damageDetails);

        if (damageDetails.Fainted)
        {
            yield return dialogBox.TypeDialog($"{targetUnit.Pokemon.Base.Name} Fainted");
            targetUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);

            CheckForBattleOver(targetUnit);
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

        StartCoroutine(EnemyMove());
    }
}
