using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainerController : MonoBehaviour, Interactable, ISavable
{
    [SerializeField] string name;
    [SerializeField] Sprite sprite;
    [SerializeField] Dialogue dialogue;
    [SerializeField] Dialogue dialogueAfterBattle;
    [SerializeField] GameObject exclamation;
    [SerializeField] GameObject fov;

    [SerializeField] AudioClip trainerAppears;

    bool battleLost = false;

    Character character;

    private void Awake() 
    {
        character = GetComponent<Character>();   
    }

    private void Start() 
    {
        SetFovRotation(character.Animator.DefaultDirection);
    }

    private void Update() 
    {
        character.HandleUpdate();    
    }

    public IEnumerator Interact(Transform initiator)
    {
        character.LookTowards(initiator.position);

        if (!battleLost)
        {
            AudioManager.Instance.PlayMusic(trainerAppears);
            yield return DialogueManager.Instance.ShowDialogue(dialogue);
            GameController.Instance.StartTrainerBattle(this); 
        } 
        else
        {
           yield return DialogueManager.Instance.ShowDialogue(dialogueAfterBattle);
        }         
    }

    public void BattleLost()
    {
        battleLost = true;
        fov.gameObject.SetActive(false);
    }

    public void SetFovRotation(FacingDirection dir) 
    {
        float angle = 0f;
        if (dir == FacingDirection.Right)
            angle = 90f;
        else if (dir == FacingDirection.Up)
            angle = 180f;
        else if (dir == FacingDirection.Left)            
            angle = 270f;

        fov.transform.eulerAngles = new Vector3(0f, 0f, angle);
    }

    public IEnumerator TriggerTrainerBattle(PlayerController player)
    {
        AudioManager.Instance.PlayMusic(trainerAppears);

        // acknowledge the player
        exclamation.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        exclamation.SetActive(false);

        // walk to player
        var diff = player.transform.position - transform.position;
        var moveVector = diff - diff.normalized;
        moveVector = new Vector2(Mathf.Round(moveVector.x), Mathf.Round(moveVector.y));  // moves to a tile defined as integers

        yield return character.Move(moveVector);

        // show dialog
        yield return DialogueManager.Instance.ShowDialogue(dialogue);
        GameController.Instance.StartTrainerBattle(this);
    }

    public string Name {
        get => name;
    }

    public Sprite Sprite {
        get => sprite;
    }

    public object CaptureState()
    {
        return battleLost;
    }

    public void RestoreState(object state) 
    {
        battleLost = (bool)state; 
         
        if (battleLost)
            fov.gameObject.SetActive(false); 
    }

}
