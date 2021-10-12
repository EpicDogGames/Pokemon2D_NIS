using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using DG.Tweening;

public enum BattleState { Start, ActionSelection, MoveSelection, RunningTurn, Busy, Bag, PartyScreen, AboutToUse, MoveToForget, BattleOver }
public enum BattleAction { Move, SwitchPokemon, UseItem, Run }

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] Image playerImage;
    [SerializeField] Image trainerImage;
    [SerializeField] GameObject pokeballSprite;
    [SerializeField] MoveSelectionUI moveSelectionUI;
    [SerializeField] InventoryUI inventoryUI;

    public event Action<bool> OnBattleOver;

    BattleState state;
    int currentAction;
    int currentMove;
    bool aboutToUseChoice = true;

    PokemonParty playerParty;
    PokemonParty trainerParty;
    Pokemon wildPokemon;

    bool isTrainerBattle = false;
    PlayerController player;
    TrainerController trainer;

    int escapeAttempts;
    MoveBase moveToLearn;

    public void StartBattle(PokemonParty playerParty, Pokemon wildPokemon) 
    {
        this.playerParty = playerParty;
        this.wildPokemon = wildPokemon;
        player = playerParty.GetComponent<PlayerController>();
        isTrainerBattle = false;
        
        StartCoroutine(SetupBattle());    
    }

    public void StartTrainerBattle(PokemonParty playerParty, PokemonParty trainerParty) 
    {
        this.playerParty = playerParty;
        this.trainerParty = trainerParty;

        isTrainerBattle = true;
        player = playerParty.GetComponent<PlayerController>();
        trainer = trainerParty.GetComponent<TrainerController>();

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
        else if (state == BattleState.Bag)
        {
            Action onBack = () =>
            {
                inventoryUI.gameObject.SetActive(false);
                state = BattleState.ActionSelection;
            };

            Action onItemUsed = ()  =>
            {
                state = BattleState.Busy;
                inventoryUI.gameObject.SetActive(false);
                StartCoroutine(RunTurns(BattleAction.UseItem));
            };

            inventoryUI.HandleUpdate(onBack, onItemUsed);
        }
        else if (state == BattleState.AboutToUse)
        {
            HandleAboutToUse();
        }
        else if (state == BattleState.MoveToForget)
        {
            Action<int> onMoveSelected = (moveIndex) =>
            {
                moveSelectionUI.gameObject.SetActive(false);
                if (moveIndex == PokemonBase.MaxNumOfMoves)
                {
                    // don't learn the new move
                    StartCoroutine(dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} did not learn {moveToLearn.Name}"));
                }
                else
                {
                    // forget selected move and learn new move
                    var selectedMove = playerUnit.Pokemon.Moves[moveIndex].Base;
                    StartCoroutine(dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} forget {selectedMove.Name} and learned {moveToLearn.Name}"));

                    playerUnit.Pokemon.Moves[moveIndex] = new Move(moveToLearn);
                }

                moveToLearn = null;
                state = BattleState.RunningTurn;
            };

            moveSelectionUI.HandleMoveSelection(onMoveSelected);    
        }
    }

    private void ActionSelection()
    {
        // resets the action to the first one in the list at beginning of player's turn vs what was last selected
        currentAction = 0;          
        state = BattleState.ActionSelection;
        // StartCoroutine(dialogBox.TypeDialog("Choose an action"));   use if you want animated text
        dialogBox.SetDialog("Choose an action");                    // use if you dont wanted animated text
        dialogBox.EnableActionSelector(true);
    }

    private void MoveSelection()
    {
        // resets the move to the first one in the list at beginning of player's turn vs what was last selected
        currentMove = 0;
        state = BattleState.MoveSelection;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }

    private void OpenBag()
    {
        state = BattleState.Bag;
        inventoryUI.gameObject.SetActive(true);   
    }

    private void OpenPartyScreen()
    {
        partyScreen.CalledFrom = state;
        state = BattleState.PartyScreen;
        partyScreen.gameObject.SetActive(true); 
    }

    private void HandleActionSelection()
    {
        var gamepadConnected = GameController.Instance.IsGamepadConnected();
        if (gamepadConnected)
        {
            if ((Gamepad.current.dpad.right.wasReleasedThisFrame) || (Keyboard.current.rightArrowKey.wasReleasedThisFrame))
                ++currentAction;
            else if ((Gamepad.current.dpad.left.wasReleasedThisFrame) || (Keyboard.current.leftArrowKey.wasReleasedThisFrame))
                --currentAction;
            else if ((Gamepad.current.dpad.down.wasReleasedThisFrame) || (Keyboard.current.downArrowKey.wasReleasedThisFrame))
                currentAction += 2;
            else if ((Gamepad.current.dpad.up.wasReleasedThisFrame) || (Keyboard.current.upArrowKey.wasReleasedThisFrame))
                currentAction -= 2;
        }
        else
        {
            if (Keyboard.current.rightArrowKey.wasReleasedThisFrame)
                ++currentAction;
            else if (Keyboard.current.leftArrowKey.wasReleasedThisFrame)
                --currentAction;
            else if (Keyboard.current.downArrowKey.wasReleasedThisFrame)
                currentAction += 2;
            else if (Keyboard.current.upArrowKey.wasReleasedThisFrame)
                currentAction -= 2;            
        }

        currentAction = Mathf.Clamp(currentAction, 0, 3);

        dialogBox.UpdateActionSelection(currentAction);

        if (gamepadConnected)
        {
            if ((Gamepad.current.buttonSouth.wasReleasedThisFrame) || (Keyboard.current.enterKey.wasReleasedThisFrame))
            {
                if (currentAction == 0)
                {
                    // fight
                    MoveSelection();
                }
                else if (currentAction == 1)
                {
                    // bag 
                    OpenBag();  
                }
                else if (currentAction == 2)
                {
                    // pokemon
                    OpenPartyScreen();  
                }
                else if (currentAction == 3)
                {
                    // run
                    StartCoroutine(RunTurns(BattleAction.Run));
                } 
            }
        }
        else
        {
            if (Keyboard.current.enterKey.wasReleasedThisFrame)
            {
                if (currentAction == 0)
                {
                    // fight
                    MoveSelection();
                }
                else if (currentAction == 1)
                {
                    // bag
                    OpenBag();
                }
                else if (currentAction == 2)
                {
                    // pokemon
                    partyScreen.CalledFrom = state;
                    OpenPartyScreen();
                }
                else if (currentAction == 3)
                {
                    // run
                    StartCoroutine(RunTurns(BattleAction.Run));
                }
            }            
        }
    }

    private void HandleMoveSelection()
    {
        var gamepadConnected = GameController.Instance.IsGamepadConnected();
        if (gamepadConnected)
        {
            if ((Gamepad.current.dpad.right.wasReleasedThisFrame) || (Keyboard.current.rightArrowKey.wasReleasedThisFrame))
                ++currentMove;
            else if ((Gamepad.current.dpad.left.wasReleasedThisFrame) || (Keyboard.current.leftArrowKey.wasReleasedThisFrame))
                --currentMove;
            else if ((Gamepad.current.dpad.down.wasReleasedThisFrame) || (Keyboard.current.downArrowKey.wasReleasedThisFrame))
                currentMove += 2;
            else if ((Gamepad.current.dpad.up.wasReleasedThisFrame) || (Keyboard.current.upArrowKey.wasReleasedThisFrame))
                currentMove -= 2;
        }
        else
        {
            if (Keyboard.current.rightArrowKey.wasReleasedThisFrame)
                ++currentMove;
            else if (Keyboard.current.leftArrowKey.wasReleasedThisFrame)
                --currentMove;
            else if (Keyboard.current.downArrowKey.wasReleasedThisFrame)
                currentMove += 2;
            else if (Keyboard.current.upArrowKey.wasReleasedThisFrame)
                currentMove -= 2;            
        }

        currentMove = Mathf.Clamp(currentMove, 0, playerUnit.Pokemon.Moves.Count - 1);

        dialogBox.UpdateMoveSelection(currentMove, playerUnit.Pokemon.Moves[currentMove]);

        if (gamepadConnected)
        {
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
        else
        {
            if (Keyboard.current.enterKey.wasReleasedThisFrame)
            {
                var move = playerUnit.Pokemon.Moves[currentMove];
                if (move.PP == 0) return;

                dialogBox.EnableMoveSelector(false);
                dialogBox.EnableDialogText(true);
                StartCoroutine(RunTurns(BattleAction.Move));
            }
            else if (Keyboard.current.escapeKey.wasReleasedThisFrame)
            {
                dialogBox.EnableMoveSelector(false);
                dialogBox.EnableDialogText(true);
                ActionSelection();
            }            
        }
    }

    private void HandlePartySelection()
    {
        var gamepadConnected = GameController.Instance.IsGamepadConnected();

        Action onSelected = () =>
        {
            if (gamepadConnected)
            {
                if ((Gamepad.current.buttonSouth.wasReleasedThisFrame) || (Keyboard.current.enterKey.wasReleasedThisFrame))
                {
                    var selectedMember = partyScreen.SelectedMember;
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

                    if (partyScreen.CalledFrom == BattleState.ActionSelection)
                    {
                        StartCoroutine(RunTurns(BattleAction.SwitchPokemon));
                    }
                    else
                    {
                        state = BattleState.Busy;
                        bool isTrainerAboutToUse = partyScreen.CalledFrom == BattleState.AboutToUse;
                        StartCoroutine(SwitchPokemon(selectedMember, isTrainerAboutToUse));
                    }
                    partyScreen.CalledFrom = null;
                }
            }
            else
            {
                if (Keyboard.current.enterKey.wasReleasedThisFrame)
                {
                    var selectedMember = partyScreen.SelectedMember;
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
                    if (partyScreen.CalledFrom == BattleState.ActionSelection)
                    {
                        StartCoroutine(RunTurns(BattleAction.SwitchPokemon));
                    }
                    else
                    {
                        state = BattleState.Busy;
                        StartCoroutine(SwitchPokemon(selectedMember));
                    }
                    partyScreen.CalledFrom = null;
                }
            }
        };

        Action onBack = () =>
        {
            if (gamepadConnected)
            {
                if ((Gamepad.current.buttonWest.wasReleasedThisFrame) || (Keyboard.current.escapeKey.wasReleasedThisFrame))
                {
                    if (playerUnit.Pokemon.HP <= 0)
                    {
                        partyScreen.SetMessageText("You have to choose a pokemon to continue");
                        return;
                    }

                    partyScreen.gameObject.SetActive(false);

                    if (partyScreen.CalledFrom == BattleState.AboutToUse)
                    {
                        StartCoroutine(SendNextTrainerPokemon());
                    }
                    else
                        ActionSelection();

                    partyScreen.CalledFrom = null;
                }
            }
            else
            {
                if (Keyboard.current.escapeKey.wasReleasedThisFrame)
                {
                    if (playerUnit.Pokemon.HP <= 0)
                    {
                        partyScreen.SetMessageText("You have to choose a pokemon to continue");
                        return;
                    }

                    partyScreen.gameObject.SetActive(false);

                    if (partyScreen.CalledFrom == BattleState.AboutToUse)
                    {
                        StartCoroutine(SendNextTrainerPokemon());
                    }
                    else
                        ActionSelection();

                    partyScreen.CalledFrom = null;
                }
            }
        };

        partyScreen.HandleUpdate(onSelected, onBack);    
    }

    private void HandleAboutToUse()
    {
        var gamepadConnected = GameController.Instance.IsGamepadConnected();
        if (gamepadConnected)
        {
            if ((Gamepad.current.dpad.down.wasReleasedThisFrame) || (Gamepad.current.dpad.up.wasReleasedThisFrame) || (Keyboard.current.downArrowKey.wasReleasedThisFrame) || (Keyboard.current.upArrowKey.wasReleasedThisFrame))
                aboutToUseChoice = !aboutToUseChoice;

            dialogBox.UpdateChoiceBox(aboutToUseChoice);

            if ((Gamepad.current.buttonSouth.wasReleasedThisFrame) || (Keyboard.current.enterKey.wasReleasedThisFrame))
            {
                dialogBox.EnableChoiceBox(false);
                if (aboutToUseChoice == true) 
                {
                    OpenPartyScreen();
                }
                else
                {
                    StartCoroutine(SendNextTrainerPokemon());
                }            
            }
            else if ((Gamepad.current.buttonWest.wasReleasedThisFrame) || (Keyboard.current.escapeKey.wasReleasedThisFrame))
            {
                dialogBox.EnableChoiceBox(false);
                StartCoroutine(SendNextTrainerPokemon());
            }
        }
        else
        {
            if ((Keyboard.current.downArrowKey.wasReleasedThisFrame) || (Keyboard.current.upArrowKey.wasReleasedThisFrame))
                aboutToUseChoice = !aboutToUseChoice;

            dialogBox.UpdateChoiceBox(aboutToUseChoice);

            if (Keyboard.current.enterKey.wasReleasedThisFrame)
            {
                dialogBox.EnableChoiceBox(false);
                if (aboutToUseChoice == true)
                {
                    partyScreen.CalledFrom = BattleState.AboutToUse;
                    OpenPartyScreen();
                }
                else
                {
                    StartCoroutine(SendNextTrainerPokemon());
                }
            }
            else if (Keyboard.current.escapeKey.wasReleasedThisFrame)
            {
                dialogBox.EnableChoiceBox(false);
                StartCoroutine(SendNextTrainerPokemon());
            }
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
            if (!isTrainerBattle)
                BattleOver(true);
            else
            {
                var nextPokemon = trainerParty.GetHealthyPokemon();
                if (nextPokemon != null)
                    StartCoroutine(AboutToUse(nextPokemon));
                else
                    BattleOver(true);
            }
    }

    public IEnumerator SetupBattle()
    {
        playerUnit.Clear();
        enemyUnit.Clear();

        if (!isTrainerBattle)
        {
            // wild pokemon battle
            playerUnit.Setup(playerParty.GetHealthyPokemon());
            enemyUnit.Setup(wildPokemon);

            dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);
            // wait for this coroutine to complete before execution goes down
            yield return StartCoroutine(dialogBox.TypeDialog($"A wild {enemyUnit.Pokemon.Base.Name} appeared."));
        }
        else
        {
            // trainer battle

            // show trainer and player sprites
            playerUnit.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(false);

            playerImage.gameObject.SetActive(true);
            trainerImage.gameObject.SetActive(true);
            playerImage.sprite = player.Sprite;
            trainerImage.sprite = trainer.Sprite;

            yield return dialogBox.TypeDialog($"{trainer.Name} wants to battle");

            // send out first pokemon of the trainer
            trainerImage.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(true);
            var enemyPokemon = trainerParty.GetHealthyPokemon();
            enemyUnit.Setup(enemyPokemon);
            yield return dialogBox.TypeDialog($"{trainer.Name} sent out {enemyPokemon.Base.Name}");
            // send out first pokemon of the player
            playerImage.gameObject.SetActive(false);
            playerUnit.gameObject.SetActive(true);
            var playerPokemon = playerParty.GetHealthyPokemon();
            playerUnit.Setup(playerPokemon);
            yield return dialogBox.TypeDialog($"Go {playerPokemon.Base.Name}!");
            dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);
        }

        escapeAttempts = 0;

        partyScreen.Init();
        ActionSelection();
    }

    private IEnumerator AboutToUse(Pokemon newPokemon)
    {
        state = BattleState.Busy;
        yield return dialogBox.TypeDialog($"{trainer.Name} is about to use {newPokemon.Base.Name}. Do you want to switch pokemon?");

        state = BattleState.AboutToUse;
        dialogBox.EnableChoiceBox(true);
    }

    private IEnumerator ChooseMoveToForget(Pokemon pokemon, MoveBase newMove)
    {
        state = BattleState.Busy;
        yield return dialogBox.TypeDialog($"Choose a move you want to forget");
        moveSelectionUI.gameObject.SetActive(true);
        moveSelectionUI.SetMoveData(pokemon.Moves.Select(x => x.Base).ToList(), newMove);
        moveToLearn = newMove;

        state = BattleState.MoveToForget;
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
                var selectedPokemon = partyScreen.SelectedMember;
                state = BattleState.Busy;
                yield return SwitchPokemon(selectedPokemon);
            }
            else if (playerAction == BattleAction.UseItem)
            {
                // this os handled from item screen, so do nothing and skip to enemy move
                dialogBox.EnableActionSelector(false);
            }
            else if (playerAction == BattleAction.Run)
            {
                yield return TryToEscape();
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
            yield return sourceUnit.HUD.WaitForHPUpdate();
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
                yield return targetUnit.HUD.WaitForHPUpdate();
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
                yield return HandlePokemonFainted(targetUnit);
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
        yield return sourceUnit.HUD.WaitForHPUpdate();
        if (sourceUnit.Pokemon.HP <= 0)
        {
            yield return HandlePokemonFainted(sourceUnit);
            yield return new WaitUntil(() => state == BattleState.RunningTurn);
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

    private IEnumerator SwitchPokemon(Pokemon newPokemon, bool isTrainerAboutToUse=false)
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

        if (isTrainerAboutToUse)
            StartCoroutine(SendNextTrainerPokemon());
        else
            state = BattleState.RunningTurn;
    }

    private IEnumerator SendNextTrainerPokemon()
    {
        state = BattleState.Busy;

        var nextPokemon = trainerParty.GetHealthyPokemon();

        enemyUnit.Setup(nextPokemon);
        yield return dialogBox.TypeDialog($"{trainer.Name} sent out {nextPokemon.Base.Name}!");
        
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

    private IEnumerator HandlePokemonFainted(BattleUnit faintedUnit)
    {
        yield return dialogBox.TypeDialog($"{faintedUnit.Pokemon.Base.Name} Fainted");
        faintedUnit.PlayFaintAnimation();
        yield return new WaitForSeconds(2f);

        if (!faintedUnit.IsPlayerUnit)
        {
            // exp gained
            int expYield = faintedUnit.Pokemon.Base.ExpYield;
            int enemyLevel = faintedUnit.Pokemon.Level;
            float trainerBonus = (isTrainerBattle) ? 1.5f : 1f;

            int expGain = Mathf.FloorToInt((expYield * enemyLevel ) * trainerBonus) / 7;
            playerUnit.Pokemon.Exp += expGain;
            yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} gained {expGain} exp");
            yield return playerUnit.HUD.SetExpSmooth();

            // check level up
            while (playerUnit.Pokemon.CheckForLevelUp())
            {
                playerUnit.HUD.SetLevel();
                yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} grew to level {playerUnit.Pokemon.Level}");

                // try to learn a new move
                var newMove = playerUnit.Pokemon.GetLearnableMoveAtCurrLevel();
                if (newMove != null)
                {
                    if (playerUnit.Pokemon.Moves.Count < PokemonBase.MaxNumOfMoves)
                    {
                        // learn a new move
                        playerUnit.Pokemon.LearnMove(newMove);
                        yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} learned {newMove.Base.Name}");
                        dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);
                    }
                    else
                    {
                        // option to forget a move to get a new move
                        yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} trying to learn {newMove.Base.Name}");
                        yield return dialogBox.TypeDialog($"But it cannot learn more than {PokemonBase.MaxNumOfMoves} moves");
                        yield return ChooseMoveToForget(playerUnit.Pokemon, newMove.Base);
                        yield return new WaitUntil(() => state != BattleState.MoveToForget);
                        yield return new WaitForSeconds(2f);
                    }
                }
                yield return playerUnit.HUD.SetExpSmooth(true);
            }

            yield return new WaitForSeconds(1f);
        }

        CheckForBattleOver(faintedUnit);
    }

    private IEnumerator ThrowPokeball()
    {
        state = BattleState.Busy;

        if (isTrainerBattle)
        {
            yield return dialogBox.TypeDialog($"You can't steal the trainer's pokemon!");
            state = BattleState.RunningTurn;
            yield break;
        }

        yield return dialogBox.TypeDialog($"{player.Name} used POKEBALL");

        var pokeballObj = Instantiate(pokeballSprite, playerUnit.transform.position - new Vector3(2, 0), Quaternion.identity);
        var pokeball = pokeballObj.GetComponent<SpriteRenderer>();

        // animations
        yield return pokeball.transform.DOJump(enemyUnit.transform.position + new Vector3(0, 2), 2f, 1, 1f).WaitForCompletion();
        yield return enemyUnit.PlayCaptureAnimation();
        yield return pokeball.transform.DOMoveY(enemyUnit.transform.position.y - 1, 0.5f).WaitForCompletion();

        int shakeCount = TryToCatchPokemon(enemyUnit.Pokemon);

        for (int i=0; i<Mathf.Min(shakeCount, 3); ++i)
        {
            yield return new WaitForSeconds(0.5f);
            yield return pokeball.transform.DOPunchRotation(new Vector3(0, 0, 10f), 0.8f).WaitForCompletion();
        }

        if (shakeCount == 4)
        {
            // pokemon caught
            yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} was caught");
            yield return pokeball.DOFade(0, 1.5f).WaitForCompletion();

            playerParty.AddPokemon(enemyUnit.Pokemon);
            yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} has been added to your party");

            Destroy(pokeball);
            BattleOver(true);
        }
        else
        {
            // pokemon broke out
            yield return new WaitForSeconds(1f);
            pokeball.DOFade(0, 0.2f);
            yield return enemyUnit.PlayBreakoutAnimation();

            if (shakeCount < 2)
                yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} broke free");
            else
                yield return dialogBox.TypeDialog($"Almost caught it");

            Destroy(pokeball);
            state = BattleState.RunningTurn;
        }
    }

    private int TryToCatchPokemon(Pokemon pokemon)
    {
        float a = (3 * pokemon.MaxHp - 2 * pokemon.HP) * pokemon.Base.CatchRate * ConditionsDB.GetStatusBonus(pokemon.Status) / (3 * pokemon.MaxHp);
 
        if (a >= 255)
            return 4;

        float b = 1048560 / Mathf.Sqrt(Mathf.Sqrt(16711680 / a));

        int shakeCount = 0;
        while (shakeCount < 4)
        {
            if (UnityEngine.Random.Range(0, 65535) >= b) 
                break;

            ++shakeCount;
        }

        return shakeCount;
    }

    private IEnumerator TryToEscape()
    {
        state = BattleState.Busy;

        if (isTrainerBattle)
        {
            yield return dialogBox.TypeDialog($"You can't run from trainer battles!");
            state = BattleState.RunningTurn;
            yield break;
        }

        ++escapeAttempts;

        int playerSpeed = playerUnit.Pokemon.Speed;
        int enemySpeed = enemyUnit.Pokemon.Speed;

        if (enemySpeed < playerSpeed)
        {
            yield return dialogBox.TypeDialog($"Ran away safely!");
            BattleOver(true);
        }
        else 
        {
            float f = (playerSpeed * 128) / enemySpeed + 30 * escapeAttempts;
            f = f % 256;

            if (UnityEngine.Random.Range(0, 256) < f)    
            {
                yield return dialogBox.TypeDialog($"Ran away safely!");
                BattleOver(true);                    
            }
            else
            {
                yield return dialogBox.TypeDialog($"Can't escape!");
                state = BattleState.RunningTurn;              
            }
        }
    }
}
