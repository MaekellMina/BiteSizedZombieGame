using UnityEngine;
using UnityEngine.InputSystem;
using cc.FiniteStateMachine;
using System;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Stamina))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public MovementSettings WalkSettings;
    public MovementSettings RunSettings;

    [Header("References")]
    public Animator PlayerAnimator;
    public GunController Gun;
    public GameObject Hand;
    public SpriteRenderer ItemOnHand;
    public SpriteRenderer CharacterSprite;
    public Rigidbody2D Rbody2D;
    [SerializeField] private Camera _cam;

    [Header("Running")]
    public float RunStaminaDrainPerSecond = 1f;
    private bool isRunning;
    [HideInInspector] public bool IsArmed => Gun != null;
 


    // States
    private PlayerIdleState _idleState;
    private PlayerMoveState _moveState;
 

    private StateMachineController _stateMachine;

    //inputs
    private PlayerInput _playerInput;
    private InputAction _moveAction;
    private InputAction _aimStickAction;
    private InputAction _aimPointerAction;
    private InputAction _runAction;

    //other components
    private Stamina stamina;

    public Vector2 Movement { get; private set; }
    public Vector2 AimDirection { get; private set; }
    public bool RunInput { get; private set; }


    #region Monobehaviour stuff
    void Awake()
    {
     
        _cam = Camera.main;

        // Setup Input
        _playerInput = GetComponent<PlayerInput>();
        var gameplay = _playerInput.actions;
        _moveAction = gameplay["Move"];
        _aimStickAction = gameplay["AimStick"];
        _aimPointerAction = gameplay["AimPointer"];
        _runAction = gameplay["Run"];


        //fsm stufff
        _stateMachine = new StateMachineController();
        initializeStates();

        //add transitions

        _stateMachine.AddTransition(_idleState, _moveState, startWalk());
        _stateMachine.AddTransition(_moveState, _idleState, endWalk());


        //initialize statemachine
        _stateMachine.SetState(_idleState);

        //define transitions
        Func<bool> startWalk() => () => Movement.sqrMagnitude > 0;
        Func<bool> endWalk() => () => Movement.sqrMagnitude <=  0;

        //other components
        stamina = GetComponent<Stamina>();
    }

    void Start()
    {

    }

    void OnEnable()
    {
        _moveAction.Enable();
        _aimStickAction.Enable();
        _aimPointerAction.Enable();
        _runAction.Enable();
    }

    void OnDisable()
    {
        _moveAction.Disable();
        _aimStickAction.Disable();
        _aimPointerAction.Disable();
        _runAction.Disable();
    }

    void Update()
    {
        // Read inputs
        Movement = _moveAction.ReadValue<Vector2>().normalized;
        //Debug.Log(Movement.sqrMagnitude);
        RunInput = _runAction.IsPressed();
        // Aim read
        Vector2 stick = _aimStickAction.ReadValue<Vector2>();
        if (stick.sqrMagnitude > 0.01f)
        {
            AimDirection = stick.normalized;
        }
        else
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            mouseWorld.z = 0f; // force flat plane
            AimDirection = ((Vector2)(mouseWorld - transform.position)).normalized;        

        }

        // State updates
        _stateMachine.Tic();

        // Running
        bool wantsToRun = RunInput && Movement != Vector2.zero;
        if (wantsToRun)
        {
            if (!isRunning && stamina.CanStartRun)
            {
                isRunning = true;
            }
        }
        else
        {
            isRunning = false;
        }

        if (isRunning)
        {
            Run();

            if (stamina.CurrentStamina <= 0f)
            {
                isRunning = false;
                _moveState.SetSpeed(WalkSettings);
            }
        }
        else
        {
            if(Movement != Vector2.zero)
                _moveState.SetSpeed(WalkSettings);
        }

    }

    void FixedUpdate()
    {
        _stateMachine.PhysicsTic();
    }
    void LateUpdate()
    {
        rotateTowardsAim();
    }

    #endregion

    void rotateTowardsAim()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mouseWorld.z = 0f;
        Vector2 dir = ((Vector2)(mouseWorld - transform.position)).normalized;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Hand.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        bool flip = AimDirection.x < 0f;
        CharacterSprite.flipX = flip;
        ItemOnHand.flipY = flip;
    }

    /// <summary>
    /// create states 
    /// </summary>
    void initializeStates()
    {
        _moveState = new PlayerMoveState(_moveAction, Rbody2D, WalkSettings.speed,
            ()=> { PlayerAnimator.Play(WalkSettings.animName); Debug.Log("BEGIN WOK"); });
        _moveState.playerAnim = PlayerAnimator;
        _idleState = new PlayerIdleState( Rbody2D, WalkSettings.speed,
          () => { PlayerAnimator.Play("Idle"); Debug.Log("IDLE"); });
    }

    private void Run()
    {
        _moveState.SetSpeed(RunSettings);
        stamina.DrainStamina(RunStaminaDrainPerSecond * Time.deltaTime);
    }

    #region Public Methods

    #endregion
}
