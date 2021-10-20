using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour, Interactable
{
    [SerializeField] ItemBase item;

    public bool Used { get; set; } = false;

    public IEnumerator Interact(Transform initiator)
    {
        if (!Used)
        {
            initiator.GetComponent<Inventory>().AddItem(item);

            Used = true;
            
            // instead of destroying the object, just making disabling it
            GetComponent<SpriteRenderer>().enabled = false;
            GetComponent<BoxCollider2D>().enabled = false;

            yield return DialogueManager.Instance.ShowDialogueText($"Player found {item.Name}");
        }
    }
}
