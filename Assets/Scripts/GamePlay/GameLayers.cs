using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLayers : MonoBehaviour
{
    [SerializeField] LayerMask solidObjectsLayer;
    [SerializeField] LayerMask interactableLayer;
    [SerializeField] LayerMask grassLayer;
    [SerializeField] LayerMask playerLayer;

    public static GameLayers Instance { get; set; }

    private void Awake() 
    {
        Instance = this;    
    }
    
    public LayerMask SolidObjectsLayer {
        get => solidObjectsLayer;
    }

    public LayerMask InteractableLayer {
        get => interactableLayer;
    }

    public LayerMask GrassLayer {
        get => grassLayer;
    }

    public LayerMask PlayerLayer {
        get => playerLayer;
    }
}
