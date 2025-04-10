using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class PlayerIdleState : cc.FiniteStateMachine.IPlayerState
{
    Rigidbody2D _rbody2d;
    float speed = 5f;
    UnityEvent OnEnterEvent = new UnityEvent();
    UnityEvent OnExitEvent = new UnityEvent();
    public PlayerIdleState(Rigidbody2D rbody2d, float speed, UnityAction onEnter = null, UnityAction onExit = null)
    {
      //  _moveAction = moveAction;
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
       
    }


}


