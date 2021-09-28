using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed;
    public LayerMask solidObjectsLayer;
    public LayerMask interactableLayer;
    public LayerMask grassLayer;

    public event Action OnEncountered;

    public bool isMoving;
    private Vector2 movementInput;

    private Animator animator;

    // input actions
    PlayerInputControls inputControls;

    private void Awake() 
    {
        animator = GetComponent<Animator>(); 
        inputControls = new PlayerInputControls();
        inputControls.PlayerControls.Move.performed += ctx => movementInput = ctx.ReadValue<Vector2>();   
    }

    public void HandleUpdate() 
    {
        if (!isMoving)
        {
            float h = movementInput.x;
            float v = movementInput.y;

            // prevent diagonal movement
            if (h != 0) v = 0;

            if (movementInput != Vector2.zero)
            {
                animator.SetFloat("moveX", h);
                animator.SetFloat("moveY", v);

                var targetPos = transform.position;
                targetPos.x += h;
                targetPos.y += v;

                if (IsWalkable(targetPos))
                {
                    StartCoroutine(Move(targetPos));
                }
            }
        }

        animator.SetBool("isMoving", isMoving);

        if ((Gamepad.current.buttonSouth.wasReleasedThisFrame) || (Keyboard.current.enterKey.wasReleasedThisFrame))
        {
            Interact();   
        }
    }

    private void Interact()
    {
        var facingDir = new Vector3(animator.GetFloat("moveX"), animator.GetFloat("moveY"));
        var interactPos = transform.position + facingDir;

        //Debug.DrawLine(transform.position, interactPos, Color.red, 0.5f);

        // set up a collider to find out if an npc is there 
        // the ? prevents crashes due to nulls
        var collider = Physics2D.OverlapCircle(interactPos, 0.3f, interactableLayer);
        if (collider != null)
        {
            collider.GetComponent<NPCController>()?.Interact();
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

    private bool IsWalkable(Vector3 targetPos)
    {
        // bitwise combine when you have two or more objects to check
        if (Physics2D.OverlapCircle(targetPos, 0.2f, solidObjectsLayer | interactableLayer) != null)
        {
            return false;
        }
        return true;
    }

    private void CheckForEncounters()
    {
        if (Physics2D.OverlapCircle(transform.position, 0.2f, grassLayer) != null)
        {
            if (UnityEngine.Random.Range(1, 101) <= 10)
            {
                animator.SetBool("isMoving", false);
                OnEncountered();
            }
        }
    }

    IEnumerator Move(Vector3 targetPos)
    {
        isMoving = true;

        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;

        isMoving = false;

        CheckForEncounters();
    }
}
