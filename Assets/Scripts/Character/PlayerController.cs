using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour, ISavable
{
    [SerializeField] string name;
    [SerializeField] Sprite sprite;

    private Vector2 movementInput;

    private Character character;

    private IPlayerTriggerable currentlyInTrigger;

    // input actions
    PlayerInputControls inputControls;

    private void Awake() 
    {
        character = GetComponent<Character>();
        inputControls = new PlayerInputControls();
        inputControls.PlayerControls.Move.performed += ctx => movementInput = ctx.ReadValue<Vector2>();   
    }

    public void HandleUpdate() 
    {
        if (!character.IsMoving)
        {
            float h = movementInput.x;
            float v = movementInput.y;

            // prevent diagonal movement
            if (h != 0) v = 0;

            if (movementInput != Vector2.zero)
            {
                StartCoroutine(character.Move(movementInput, OnMoveOver));
            }
        }

        character.HandleUpdate();

        var gamepadConnected = GameController.Instance.IsGamepadConnected();
        if (gamepadConnected)
        {
            if ((Gamepad.current.buttonSouth.wasReleasedThisFrame) || (Keyboard.current.enterKey.wasReleasedThisFrame))
            {
                StartCoroutine(Interact());   
            }
        }
        else
        {
            if (Keyboard.current.enterKey.wasReleasedThisFrame)
            {
               StartCoroutine(Interact());
            }    
        }
    }

    private IEnumerator Interact()
    {
        var facingDir = new Vector3(character.Animator.MoveX, character.Animator.MoveY);
        var interactPos = transform.position + facingDir;

        // set up a collider to find out if an npc is there 
        // the ? prevents crashes due to nulls
        var collider = Physics2D.OverlapCircle(interactPos, 0.3f, GameLayers.Instance.InteractableLayer);
        if (collider != null)
        {
            yield return collider.GetComponent<Interactable>()?.Interact(transform);
        }
    }

    private void OnEnable() 
    {
        inputControls.Enable();    
    }

    private void OnDisable() 
    {
        inputControls.Disable();    
    }
  
    private void OnMoveOver()
    {
        var colliders = Physics2D.OverlapCircleAll(transform.position - new Vector3(0, character.OffsetY), 0.2f, GameLayers.Instance.TriggerableLayers);

        IPlayerTriggerable triggerable = null;
        foreach (var collider in colliders)
        {
            triggerable = collider.GetComponent<IPlayerTriggerable>();
            if (triggerable != null)
            {
                if (triggerable == currentlyInTrigger && !triggerable.TriggerRepeatedly)
                    break;

                triggerable.OnPlayerTriggered(this);
                currentlyInTrigger = triggerable;
                break;
            }
        }

        if (colliders.Count() == 0 || triggerable != currentlyInTrigger)
            currentlyInTrigger = null;

    }

    public string Name {
        get => name;
    }

    public Sprite Sprite {
        get => sprite;
    }

    public Character Character {
        get => character;
    }

    public object CaptureState()
    {
        var saveData = new PlayerSaveData()
        {
            position = new float[] { transform.position.x, transform.position.y },
            pokemons = GetComponent<PokemonParty>().Pokemons.Select(p => p.GetSaveData()).ToList()
        };

        return saveData;
    }

    public void RestoreState(object state) 
    {
        var saveData = (PlayerSaveData)state;

        var pos = saveData.position;
        transform.position = new Vector3(pos[0], pos[1]);

        GetComponent<PokemonParty>().Pokemons = saveData.pokemons.Select(s => new Pokemon(s)).ToList();
    }
}

[Serializable]
public class PlayerSaveData
{
    public float[] position;
    public List<PokemonSaveData> pokemons;
}
