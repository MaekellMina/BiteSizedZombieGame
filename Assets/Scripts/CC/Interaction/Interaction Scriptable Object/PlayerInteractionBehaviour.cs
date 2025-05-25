using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using cc.Interaction.SO;
using UnityEngine.InputSystem;

public class PlayerInteractionBehaviour : MonoBehaviour
{
    public PlayerInput input;
    private InputAction _interactAxn;
    [SerializeField]
    InteractionSignalSO interactionSignal;
    [Space]
    public GameObject InteractionPrompt;
    void Awake()
    {
        _interactAxn = input.actions["Interact"];
        interactionSignal.AssignPlayer(this.gameObject, InteractionPrompt);
    }

    private void OnEnable()
    {
        _interactAxn.started += _interactAxn_started;
        _interactAxn.Enable();

    }
    private void OnDisable()
    {
        _interactAxn.started -= _interactAxn_started;
        _interactAxn.Disable();
    }

    private void _interactAxn_started(InputAction.CallbackContext obj)
    {
        Debug.Log("INTERACT");    
        if(interactionSignal.currentInteractable!=null)
            interactionSignal?.Raise(interactionSignal.currentInteractable);
       
    }

  
}
