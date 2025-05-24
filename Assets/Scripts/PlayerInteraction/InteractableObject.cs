using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InteractableObject : MonoBehaviour,IInteractable
{
    public UnityEvent E_OnInteract = new UnityEvent();
    void Start()
    {
        
    }
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            InteractionManager.instance.Register(this);
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            InteractionManager.instance.Unregister(this);
        }
    }

    public void OnInteract()
    {
        E_OnInteract?.Invoke();
    }

    public GameObject GetTargetObject()
    {
        return gameObject;
    }
}
