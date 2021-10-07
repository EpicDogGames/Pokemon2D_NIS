using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] string name;
    [SerializeField] Sprite sprite;

    private Vector2 movementInput;

    private Character character;

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
                Interact();   
            }
        }
        else
        {
            if (Keyboard.current.enterKey.wasReleasedThisFrame)
            {
                Interact();
            }    
        }
    }

    private void Interact()
    {
        var facingDir = new Vector3(character.Animator.MoveX, character.Animator.MoveY);
        var interactPos = transform.position + facingDir;

        // set up a collider to find out if an npc is there 
        // the ? prevents crashes due to nulls
        var collider = Physics2D.OverlapCircle(interactPos, 0.3f, GameLayers.Instance.InteractableLayer);
        if (collider != null)
        {
            //collider.GetComponent<NPCController>()?.Interact(transform);
            collider.GetComponent<Interactable>()?.Interact(transform);
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
        foreach (var collider in colliders)
        {
            var triggerable = collider.GetComponent<IPlayerTriggerable>();
            if (triggerable != null)
            {
                triggerable.OnPlayerTriggered(this);
                break;
            }
        }
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
}
