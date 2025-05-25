using UnityEngine;
using UnityEngine.InputSystem;
using cc.FiniteStateMachine;
using System;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Stamina))]
public class PlayerController : LivingEntity
{
    [Header("Movement Settings")]
    public MovementSettings WalkSettings;
    public MovementSettings RunSettings;

    [Header("References")]
    public Animator PlayerAnimator;
    public GameObject Hand;
    public GameObject ItemOnHand;
    public SpriteRenderer CharacterSprite;
    public Rigidbody2D Rbody2D;
    [SerializeField] private Camera _cam;

    [Header("Running")]
    public float RunStaminaDrainPerSecond = 1f;
    private bool isRunning;
    [HideInInspector] public bool IsArmed => weaponController != null;
 


    // States
    private PlayerIdleState _idleState;
    private PlayerMoveState _moveState;
 

    private StateMachineController _stateMachine;

    //inputs
    private PlayerInput _playerInput;
    private InputAction _moveAction;
    private InputAction _aimAction;
   // private InputAction _aimPointerAction;
    private InputAction _runAction;
    private InputAction _fireAction;
    private InputAction _reloadAction;

    //other components
    private Stamina stamina;
    private GunController weaponController;

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
        _aimAction = gameplay["Aim"];
       // _aimPointerAction = gameplay["AimPointer"];
        _runAction = gameplay["Run"];
        _fireAction = gameplay["Fire"];
        _reloadAction = gameplay["Reload"];


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
        weaponController = GetComponent<GunController>();
    }

    public override void Start()
    {
        base.Start();

    }

    void OnEnable()
    {
        _moveAction.Enable();
        _aimAction.Enable();
      //  _aimPointerAction.Enable();
        _runAction.Enable();
        //weapon input:
        _fireAction.canceled += weaponController.OnTriggerRelease;
        _fireAction.Enable();
        _reloadAction.Enable();
        //===

        weaponController.e_OnEquipWeapon.AddListener(UpdateItemOnHand);
    }

    void OnDisable()
    {
        _moveAction.Disable();
        _aimAction.Disable();
      //  _aimPointerAction.Disable();
        _runAction.Disable(); 
        //weapon input:
        _fireAction.canceled -= weaponController.OnTriggerRelease;
        _fireAction.Disable();
        _reloadAction.Disable();
        //===

        weaponController.e_OnEquipWeapon.RemoveListener(UpdateItemOnHand);
    }

    void Update()
    {
        // Read inputs
        Movement = _moveAction.ReadValue<Vector2>().normalized;
        //Debug.Log(Movement.sqrMagnitude);
        RunInput = _runAction.IsPressed();
        
        // Aim read
        Vector3 stick = _aimAction.ReadValue<Vector2>();
        if (stick.sqrMagnitude > 0.01f)
        {
            AimDirection = stick.normalized;
        }
        else
        {
            //Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(stick);
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

        //weapon input:
        if(_fireAction.IsPressed())
        {
            weaponController.OnTriggerHold();
        }
        if(_reloadAction.triggered)
        {
            weaponController.Reload();
        }
        //===
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
        if (_playerInput.currentControlScheme == "Keyboard&Mouse")
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            mouseWorld.z = 0f;
            Vector2 dir = ((Vector2)(mouseWorld - transform.position)).normalized;

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            Hand.transform.rotation = Quaternion.Euler(0f, 0f, angle);

            bool flip = AimDirection.x < 0f;
            CharacterSprite.flipX = flip;
            //item on hand flip:
            Vector3 scale = ItemOnHand.transform.localScale;
            scale.y = Mathf.Abs(scale.y) * (flip ? -1f : 1f);
            ItemOnHand.transform.localScale = scale;
            //---

        }
        else if(_playerInput.currentControlScheme == "Gamepad")
        {

            Vector2 aimInput = _aimAction.ReadValue<Vector2>();
            if (aimInput.sqrMagnitude < 0.01f)
                return;

            Vector2 dir = aimInput.normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            Hand.transform.rotation = Quaternion.Euler(0f, 0f, angle);

            // Optional: Debug
            Debug.Log("Angle: " + angle + " | Dir: " + dir);
        }


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
    public void UpdateItemOnHand(GameObject obj)
    {
        ItemOnHand = obj;
    }
    #endregion
}
