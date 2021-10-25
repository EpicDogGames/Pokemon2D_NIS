using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// only static data can go into a scriptable object
[CreateAssetMenu(fileName = "Quest", menuName = "Quests/Create new quest", order = 0)]
public class QuestBase : ScriptableObject
{
    [SerializeField] string name;
    [SerializeField] string description;

    [SerializeField] Dialogue startDialogue;
    [SerializeField] Dialogue inProgressDialogue;
    [SerializeField] Dialogue completedDialogue;

    [SerializeField] ItemBase requiredItem;         // if multiple items required, can make this a list
    [SerializeField] ItemBase rewardItem;           // if multiple items rewarded, can make this a list 

    public string Name => name;
    public string Description => description;

    public Dialogue StartDialogue => startDialogue;
    public Dialogue InProgressDialogue => inProgressDialogue?.Lines?.Count > 0 ? inProgressDialogue : startDialogue;  // null conditional (?) so it wont crash if you don't have in progress dialogue
    public Dialogue CompletedDialogue => completedDialogue;

    public ItemBase RequiredItem => requiredItem;
    public ItemBase RewardItem => rewardItem;
}
