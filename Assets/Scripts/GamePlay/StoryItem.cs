﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoryItem : MonoBehaviour, IPlayerTriggerable
{
    [SerializeField] Dialogue dialogue;

    public void OnPlayerTriggered(PlayerController player)
    {
        // stop player from animating when hitting trigger
        player.Character.Animator.IsMoving = false; 
        
        StartCoroutine(DialogueManager.Instance.ShowDialogue(dialogue));
    }

    public bool TriggerRepeatedly => false;
}
