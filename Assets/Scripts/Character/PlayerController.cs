using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] string name;
    [SerializeField] Sprite sprite;

    const float offsetY = 0.3f;

    public event Action OnEncountered;
    public event Action<Collider2D> OnEnterTrainersView;

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

        if ((Gamepad.current.buttonSouth.wasReleasedThisFrame) || (Keyboard.current.enterKey.wasReleasedThisFrame))
        {
            Interact();   
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
        CheckForEncounters();
        CheckIfInTrainersView();
    }

    private void CheckForEncounters()
    {
        if (Physics2D.OverlapCircle(transform.position - new Vector3(0, offsetY), 0.2f, GameLayers.Instance.GrassLayer) != null)
        {
            if (UnityEngine.Random.Range(1, 101) <= 10)
            {
                character.Animator.IsMoving = false;
                OnEncountered();
            }
        }
    }
    
    private void CheckIfInTrainersView()
    {
        var collider = Physics2D.OverlapCircle(transform.position, 0.2f, GameLayers.Instance.FovLayer);

        if (collider != null)
        {
            character.Animator.IsMoving = false;
            OnEnterTrainersView?.Invoke(collider);    
        }
    }

    public string Name {
        get => name;
    }

    public Sprite Sprite {
        get => sprite;
    }
}
