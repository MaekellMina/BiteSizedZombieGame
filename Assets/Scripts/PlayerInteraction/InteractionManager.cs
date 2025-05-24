using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// adjust for multiplayer later
/// </summary>
public class InteractionManager : MonoBehaviour
{
    public static InteractionManager instance;
    private GameObject TargetObject;
    public IInteractable currentInteractable;
    public GameObject EPrompt;
    public Transform playerTransform;
    public PlayerInput input;
    private InputAction _interactAxn;
    private void Awake()
    {
        if (instance != null)
            Destroy(this.gameObject);
        instance = this;
        _interactAxn = input.actions["Interact"];
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
        if (currentInteractable != null && Vector2.Distance(TargetObject.transform.position, playerTransform.position) <= 1f)
        {
            Debug.Log("INTERACTION ATTEMPT");
            currentInteractable.OnInteract();

        }           
        else
        {
            Debug.Log($"too far{Vector2.Distance(TargetObject.transform.position, playerTransform.position)}");
            EPrompt.gameObject.SetActive(false);
            currentInteractable = null;
            TargetObject = null;
        }

    }

    public void Unregister(IInteractable nearestInteractable)
    {
        if (currentInteractable == nearestInteractable)
        {
            EPrompt.gameObject.SetActive(false);
            currentInteractable = null;
            TargetObject = null;
        }

        if(currentInteractable ==null)
        {
            EPrompt.gameObject.SetActive(false);

        }
    }
    public void Register(IInteractable nearestInteractable)
    {
        currentInteractable = nearestInteractable;
        TargetObject = nearestInteractable.GetTargetObject();
        EPrompt.transform.position = TargetObject.transform.position + Vector3.up;
        EPrompt.gameObject.SetActive(true);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (currentInteractable != null)
            Gizmos.DrawSphere(TargetObject.transform.position, 1);
    }
}


public interface IInteractable
{
    public void OnInteract();
    public GameObject GetTargetObject();
}

