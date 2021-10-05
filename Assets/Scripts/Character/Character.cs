using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    public float moveSpeed;
    public bool IsMoving { get; private set; }
    public float OffsetY { get; private set; } = 0.3f;

    CharacterAnimator animator;

    private void Awake() 
    {
        animator = GetComponent<CharacterAnimator>();
        SetPositionAndSnapToTile(transform.position);
    }

    public void SetPositionAndSnapToTile(Vector2 pos)
    {
        // 2.3 -> Floor -> 2 -> 2.5  ... places it in the center of the tile
        pos.x = Mathf.Floor(pos.x) + 0.5f;
        pos.y = Mathf.Floor(pos.y) + 0.5f + OffsetY;

        transform.position = pos;
    }

    public CharacterAnimator Animator {
        get => animator;
    }

    public void HandleUpdate()
    {
        animator.IsMoving = IsMoving;
    }

    public void LookTowards(Vector3 targetPos)
    {
        var xdiff = Mathf.Floor(targetPos.x) - Mathf.Floor(transform.position.x);
        var ydiff = Mathf.Floor(targetPos.y) - Mathf.Floor(transform.position.y);  

        if (xdiff == 0 || ydiff == 0)
        {
            animator.MoveX = Mathf.Clamp(xdiff, -1f, 1f);
            animator.MoveY = Mathf.Clamp(ydiff, -1f, 1f);           
        }
        else
            Debug.LogError("Error in Look Towards. You can't ask the character to look diagonally");

    }

    private bool IsWalkable(Vector3 targetPos)
    {
        // bitwise combine when you have two or more objects to check
        if (Physics2D.OverlapCircle(targetPos, 0.2f, GameLayers.Instance.SolidObjectsLayer | GameLayers.Instance.InteractableLayer) != null)
        {
            Debug.Log("Is not walkable");
            return false;
        }
        return true;
    }

    private bool IsPathClear(Vector3 targetPos)
    {
        var diff = targetPos - transform.position;
        var dir = diff.normalized;

        if (Physics2D.BoxCast(transform.position + dir, new Vector2(0.2f, 0.2f), 0f, dir, diff.magnitude - 1, GameLayers.Instance.SolidObjectsLayer | GameLayers.Instance.InteractableLayer | GameLayers.Instance.PlayerLayer) == true)
        {
            return false;
        }

        return true;
    }

    public IEnumerator Move(Vector2 moveVector, Action OnMoveOver=null)
    {
        animator.MoveX = Mathf.Clamp(moveVector.x, -1f, 1f);
        animator.MoveY = Mathf.Clamp(moveVector.y, -1f, 1f);;

        var targetPos = transform.position;
        targetPos.x += moveVector.x;
        targetPos.y += moveVector.y;

        if (!IsPathClear(targetPos))
            yield break;

        IsMoving = true;

        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;

        IsMoving = false;

        OnMoveOver?.Invoke();
    }
}
