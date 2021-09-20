using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum BattleState { Start, PlayerAction, PlayerMove, EnemyMove, Busy }
public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleHUD playerHUD;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleHUD enemyHUD;
    [SerializeField] BattleDialogBox dialogBox;

    public event Action<bool> OnBattleOver;

    BattleState state;
    int currentAction;
    int currentMove;

    public void StartBattle() 
    {
        StartCoroutine(SetupBattle());    
    }

    public void HandleUpdate() 
    {
        if (state == BattleState.PlayerAction)
        {
            HandleActionSelection();
        } 
        else if (state == BattleState.PlayerMove)
        {
            HandleMoveSelection();
        }   
    }

    private void PlayerAction()
    {
        state = BattleState.PlayerAction;
        StartCoroutine(dialogBox.TypeDialog("Choose an action"));
        dialogBox.EnableActionSelector(true);
    }

    private void PlayerMove()
    {
        state = BattleState.PlayerMove;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }

    private void HandleActionSelection()
    {
        if ((Gamepad.current.dpad.down.wasReleasedThisFrame) || (Keyboard.current.downArrowKey.wasReleasedThisFrame))
        {
            if (currentAction < 1)
                ++currentAction;
        }
        else if ((Gamepad.current.dpad.up.wasReleasedThisFrame) || (Keyboard.current.upArrowKey.wasReleasedThisFrame))
        {
            if (currentAction > 0)
                --currentAction;
        }

        dialogBox.UpdateActionSelection(currentAction);

        if ((Gamepad.current.buttonSouth.wasReleasedThisFrame) || (Keyboard.current.enterKey.wasReleasedThisFrame))
        {
            if (currentAction == 0)
            {
                // fight
                PlayerMove();
            }
            else if (currentAction == 1)
            {
                // run
            }
        }
    }

    private void HandleMoveSelection()
    {
        if ((Gamepad.current.dpad.right.wasReleasedThisFrame) || (Keyboard.current.rightArrowKey.wasReleasedThisFrame))
        {
            if (currentMove < playerUnit.Pokemon.Moves.Count - 1)
                ++currentMove;
        }
        else if ((Gamepad.current.dpad.left.wasReleasedThisFrame) || (Keyboard.current.leftArrowKey.wasReleasedThisFrame))
        {
            if (currentMove > 0)
                --currentMove;
        }
        else if ((Gamepad.current.dpad.down.wasReleasedThisFrame) || (Keyboard.current.downArrowKey.wasReleasedThisFrame))
        {
            if (currentMove < playerUnit.Pokemon.Moves.Count - 1)
                currentMove += 2;

        }
        else if ((Gamepad.current.dpad.up.wasReleasedThisFrame) || (Keyboard.current.upArrowKey.wasReleasedThisFrame))
        {
            if (currentMove > 1)
                currentMove -= 2;
        }

        dialogBox.UpdateMoveSelection(currentMove, playerUnit.Pokemon.Moves[currentMove]);

        if ((Gamepad.current.buttonSouth.wasReleasedThisFrame) || (Keyboard.current.enterKey.wasReleasedThisFrame))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(PerformPlayerMove());
        }
    }

    public IEnumerator SetupBattle()
    {
        playerUnit.Setup();
        playerHUD.SetData(playerUnit.Pokemon);
        enemyUnit.Setup();
        enemyHUD.SetData(enemyUnit.Pokemon);

        dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);

        // wait for this coroutine to complete before execution goes down
        yield return StartCoroutine(dialogBox.TypeDialog($"A wild {enemyUnit.Pokemon.Base.Name} appeared."));

        PlayerAction();
    }

    private IEnumerator PerformPlayerMove()
    {
        state = BattleState.Busy;

        var move = playerUnit.Pokemon.Moves[currentMove];
        yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} used {move.Base.Name}");

        playerUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(1f);

        enemyUnit.PlayHitAnimation();

        var damageDetails = enemyUnit.Pokemon.TakeDamage(move, playerUnit.Pokemon);
        yield return enemyHUD.UpdateHP();
        yield return ShowDamageDetails(damageDetails);

        if (damageDetails.Fainted)
        {
            yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} Fainted");
            enemyUnit.PlayFaintAnimation();

            yield return new WaitForSeconds(2f);
            OnBattleOver(true);
        }
        else
        {
            StartCoroutine(EnemyMove());
        }
    }

    private IEnumerator EnemyMove()
    {
        state = BattleState.EnemyMove;

        var move = enemyUnit.Pokemon.GetRandomMove();
        yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} used {move.Base.Name}");

        enemyUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(1f);

        playerUnit.PlayHitAnimation();

        var damageDetails = playerUnit.Pokemon.TakeDamage(move, enemyUnit.Pokemon);
        yield return playerHUD.UpdateHP();
        yield return ShowDamageDetails(damageDetails);

        if (damageDetails.Fainted)
        {
            yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} Fainted");
            playerUnit.PlayFaintAnimation();

            yield return new WaitForSeconds(2f);
            OnBattleOver(false);
        }
        else
        {
            PlayerAction();
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
}
