using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using cc.FiniteStateMachine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class PlayerWalkState : cc.FiniteStateMachine.IPlayerState
{
    InputAction _moveAction;
    Rigidbody2D _rbody2d;
    float speed = 5f;
    UnityEvent OnEnterEvent = new UnityEvent();
    UnityEvent OnExitEvent = new UnityEvent();
 
    public PlayerWalkState(InputAction moveAction, Rigidbody2D rbody2d, float speed, UnityAction onEnter = null, UnityAction onExit = null)
    {
        _moveAction = moveAction;
        _rbody2d = rbody2d;
        this.speed = speed;

        if (onEnter != null)
            OnEnterEvent.AddListener(onEnter);
        if (onExit != null)
            OnExitEvent.AddListener(onExit);
    }


    public void OnEnter()
    {
   
        OnEnterEvent?.Invoke();

    }

    public void OnExit()
    {     
        _rbody2d.velocity = Vector2.zero;
        OnExitEvent?.Invoke();
    }

    public void Tic()
    {

    }

    public void OnPhysicsTic()
    {
       _rbody2d.MovePosition(_rbody2d.position + _moveAction.ReadValue<Vector2>().normalized * speed * Time.fixedDeltaTime);
    }


}
