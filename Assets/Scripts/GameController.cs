﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum GameState { FreeRoam, Battle, Dialogue, Menu, PartyScreen, Bag, Cutscene, Paused }

public class GameController : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] InventoryUI inventoryUI;

    GameState state;
    GameState prevState;

    public SceneDetails CurrentScene { get; private set; }
    public SceneDetails PreviousScene { get; private set; }

    TrainerController trainer;

    public static GameController Instance { get; private set; } 

    MenuController menuController;

    private void Awake() 
    {
        Instance = this;

        menuController = GetComponent<MenuController>();

        // this disables the mouse so that it can be used or seen in the game
        // Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;

        PokemonDB.Init();
        MoveDB.Init();
        ConditionsDB.Init();    
    }

    private void Start() 
    {
        battleSystem.OnBattleOver += EndBattle;

        partyScreen.Init();

        DialogueManager.Instance.OnShowDialogue += () =>
        {
            prevState = state;
            state = GameState.Dialogue;
        };

        DialogueManager.Instance.OnCloseDialogue += () =>
        {
            if (state == GameState.Dialogue)
                state = prevState;
        };

        menuController.onBack += () =>
        {
            state = GameState.FreeRoam;   
        };

        menuController.onMenuSelected += OnMenuSelected;
    }

    private void Update() 
    {
        if (state == GameState.FreeRoam)    
        {
            playerController.HandleUpdate();

            var gamepadConnected = IsGamepadConnected();
            if (gamepadConnected)
            {
                if ((Gamepad.current.startButton.wasReleasedThisFrame) || (Keyboard.current.pKey.wasReleasedThisFrame))
                {
                    menuController.OpenMenu();
                    state = GameState.Menu;
                }
            }
            else
            {
                if (Keyboard.current.pKey.wasReleasedThisFrame)
                {
                    menuController.OpenMenu();
                    state = GameState.Menu;
                }
            }
        }
        else if (state == GameState.Battle)
        {
            battleSystem.HandleUpdate();
        }
        else if (state == GameState.Dialogue)
        {
            DialogueManager.Instance.HandleUpdate();
        }
        else if (state == GameState.Menu)
        {
            menuController.HandleUpdate();
        }
        else if (state == GameState.PartyScreen)
        {
            Action onSelected = () =>
            {
                // goto summary screen
                Debug.Log("Summary screen of pokemon to be created still");
            };
            Action onBack = () =>
            {
                partyScreen.gameObject.SetActive(false);
                state = GameState.FreeRoam;
            };

            partyScreen.HandleUpdate(onSelected, onBack);    
        }
        else if (state == GameState.Bag) 
        {
            Action onBack = () =>
            {
                inventoryUI.gameObject.SetActive(false);
                state = GameState.FreeRoam;
            };

            inventoryUI.HandleUpdate(onBack);
        }
    }

    public bool IsGamepadConnected()
    {
        var gamepad = Gamepad.current;
        if (gamepad != null)
            return true;
        return false;        
    }

    public void StartBattle()
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);  
        worldCamera.gameObject.SetActive(false);

        var playerParty = playerController.GetComponent<PokemonParty>();
        var wildPokemon = CurrentScene.GetComponent<MapArea>().GetRandomWildPokemon();

        var wildPokemonCopy = new Pokemon(wildPokemon.Base, wildPokemon.Level);

        battleSystem.StartBattle(playerParty, wildPokemonCopy);
    }

    public void StartTrainerBattle(TrainerController trainer)
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);  
        worldCamera.gameObject.SetActive(false);

        this.trainer = trainer;
        var playerParty = playerController.GetComponent<PokemonParty>();
        var trainerParty = trainer.GetComponent<PokemonParty>();

        battleSystem.StartTrainerBattle(playerParty, trainerParty);
    }

    public void OnEnterTrainersView(TrainerController trainer)
    {
        state = GameState.Cutscene;
        StartCoroutine(trainer.TriggerTrainerBattle(playerController));
    }

    public void PauseGame(bool pause)
    {
        if (pause) 
        {
            prevState = state;
            state = GameState.Paused;
        }
        else
            state = prevState;
    }

    public void SetCurrentScene(SceneDetails currScene)
    {
        PreviousScene = CurrentScene;
        CurrentScene = currScene;
    }

    private void EndBattle(bool won) 
    {
        if (trainer != null && won == true)
        {
            trainer.BattleLost();
            trainer = null;
        }

        state = GameState.FreeRoam; 
        battleSystem.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);   
    }

    private void OnMenuSelected(int selectedItem)
    {
        if (selectedItem == 0)
        {
            // Pokemon
            partyScreen.gameObject.SetActive(true);
            state = GameState.PartyScreen;
        }
        else if (selectedItem == 1)
        {
            // Bag
            inventoryUI.gameObject.SetActive(true);
            state = GameState.Bag;
        }
        else if (selectedItem == 2)
        {
            // Save
            SavingSystem.i.Save("saveSlot1");
            state = GameState.FreeRoam;
        }
        else if (selectedItem == 3)
        {
            // Load
            SavingSystem.i.Load("saveSlot1");
            state = GameState.FreeRoam;
        }
    }

    public GameState State => state;
}
