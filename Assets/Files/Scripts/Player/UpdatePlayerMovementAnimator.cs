using Quantum;
using RootMotion.FinalIK;
using UnityEngine;


public class UpdatePlayerMovementAnimator : QuantumCallbacks
{
    private QuantumEntityView _entityView;

    public Animator _anim;
    
    private PlayerMovementHandler _mHandler;
    private PlayerEffectsHandler _effectsHandler;
    public FullBodyBipedIK bipedIK;
    
    [Header("Weapon")] 
    public int weaponType;
    
    [Header("Physics")] 
    public Transform rootBone;
    
    public float horizontalDirection;
    public float verticalDirection;
    public float lookXDirection;
    public float moveState;
    public float dodgeRunning;
    public float runningSpeed;
    
    public bool isGrounded;
    public bool isMoving;
    public bool isSprinting;
    public bool isAiming;
    public bool isCrouching;
    public bool isRagdoll;

    [Header("Fight Stance")]
    public float fightStanceRange;
    public bool inFightStance;
    [SerializeField] 
    private float fightStanceLerpSpeed = 8f;
    
    [Header("Attacking")] 
    public bool freeRoam;
    public bool isAttacking;
    
    [Header("Player Properties")]
    public float moveType;
    public float idleType;
    
    [Header("Dodging")] 
    public bool isDodging;
    public bool isDashing;
    
    public bool dashForward;
    public bool dashBack;
    
    public bool dodgeForward;
    public bool dodgeBack;
    public bool dodgeLeft;
    public bool dodgeRight;
    public bool dodgeBackDown;
    public bool dodgeBackUp;
    public bool dodgeForwardUp;
    public bool dodgeForwardDown;

    [Header("Hit")] 
    public bool gotHit;
    public bool isDead;
    public bool hitWall;
    
    [Header("Celebration")] 
    public bool showIntro;
    public bool showCelebration;
    public int celebrationType;
    
    [Header("Block")] 
    public bool blocking;
    
    [Header("Running")] 
    public float lerpDuration = 2;
    private float _currentValue;
    private float _elapsedTime;
    private bool _isLerping;
    public float smoothedDodgeValue = 0f;
    
    private static readonly int Grounded = Animator.StringToHash("Grounded");
    private static readonly int Horizontal = Animator.StringToHash("Horizontal");
    private static readonly int Vertical = Animator.StringToHash("Vertical");
    private static readonly int IsMoving = Animator.StringToHash("IsMoving");
    private static readonly int Sprint = Animator.StringToHash("Sprint");
    private static readonly int MoveState = Animator.StringToHash("MoveState");
    private static readonly int LookX = Animator.StringToHash("LookX");
    private static readonly int Aim = Animator.StringToHash("Aim");
    private static readonly int Dodge = Animator.StringToHash("Dodge");
    private static readonly int DodgeLeft = Animator.StringToHash("DodgeLeft");
    private static readonly int DodgeRight = Animator.StringToHash("DodgeRight");
    private static readonly int DodgeForward = Animator.StringToHash("DodgeForward");
    private static readonly int DodgeBack = Animator.StringToHash("DodgeBack");
    private static readonly int GotHit = Animator.StringToHash("GotHit");
    private static readonly int Blocking = Animator.StringToHash("Blocking");
    private static readonly int IsDodging = Animator.StringToHash("IsDodging");
    private static readonly int IsCrouching = Animator.StringToHash("IsCrouching");
    private static readonly int IsAttacking = Animator.StringToHash("IsAttacking");
    private static readonly int DodgeBackDown = Animator.StringToHash("DodgeBackDown");
    private static readonly int DodgeBackUp = Animator.StringToHash("DodgeBackUp");
    private static readonly int DodgeForwardUp = Animator.StringToHash("DodgeForwardUp");
    private static readonly int DodgeForwardDown = Animator.StringToHash("DodgeForwardDown");
    private static readonly int Dead = Animator.StringToHash("Dead");
    private static readonly int IsDead = Animator.StringToHash("IsDead");
    private static readonly int ShowCelebration = Animator.StringToHash("ShowCelebration");
    private static readonly int DodgeRunning = Animator.StringToHash("DodgeRun");
    private static readonly int ShowIntro = Animator.StringToHash("ShowIntro");
    private static readonly int IdleType = Animator.StringToHash("IdleType");
    private static readonly int CelebrationType = Animator.StringToHash("CelebrateType");
    private static readonly int DashForward = Animator.StringToHash("DashForward");
    private static readonly int DashBack = Animator.StringToHash("DashBack");
    private static readonly int IsDashing = Animator.StringToHash("IsDashing");
    private static readonly int MoveType = Animator.StringToHash("MoveType");
    private static readonly int FreeRoam = Animator.StringToHash("FreeRoam");
    private static readonly int FightStanceRange = Animator.StringToHash("FightStanceRange");
    private static readonly int RunningSpeed = Animator.StringToHash("RunningSpeed");

    //Hit
    private bool _gotHit;
    private float gotHitTimer;
    
    
    private void Awake()
    {
        _entityView = GetComponent<QuantumEntityView>();
        _mHandler = GetComponent<PlayerMovementHandler>();
        _effectsHandler = GetComponent<PlayerEffectsHandler>();
    }

    private void Start()
    {
        QuantumEvent.Subscribe<EventMeleeAttack>(this, MeleeAttack);
        QuantumEvent.Subscribe<EventKickMeleeAttack>(this, KickMeleeAttack);
        QuantumEvent.Subscribe<EventDodge>(this, DodgeEvent);
        QuantumEvent.Subscribe<EventBlockHit>(this, BlockHit);
        QuantumEvent.Subscribe<EventPlayerHit>(this, PlayerHitEvent);
        QuantumEvent.Subscribe<EventGetRootMotionPos>(this, GetRootPosEvent);
        QuantumEvent.Subscribe<EventDidFinish>(this, FinisherAttackerEvent);
        QuantumEvent.Subscribe<EventGetsFinished>(this, FinisherVictimEvent);
        QuantumEvent.Subscribe<EventDead>(this, DeadEvent);
        QuantumEvent.Subscribe<EventPlayerLeave>(this, PlayerLeaveEvent);
        QuantumEvent.Subscribe<EventPlayerDodge>(this, PlayerDodgeEvent);
        QuantumEvent.Subscribe<EventHitWall>(this, HitWallEvent);
        QuantumEvent.Subscribe<EventAttackName>(this, AttackEvent);
    }

    private void OnDestroy()
    {
        QuantumEvent.UnsubscribeListener<EventMeleeAttack>(this);
        QuantumEvent.UnsubscribeListener<EventKickMeleeAttack>(this);
        QuantumEvent.UnsubscribeListener<EventDodge>(this);
        QuantumEvent.UnsubscribeListener<EventBlockHit>(this);
        QuantumEvent.UnsubscribeListener<EventPlayerHit>(this);
        QuantumEvent.UnsubscribeListener<EventGetRootMotionPos>(this);
        QuantumEvent.UnsubscribeListener<EventGetsFinished>(this);
        QuantumEvent.UnsubscribeListener<EventDidFinish>(this);
        QuantumEvent.UnsubscribeListener<EventDead>(this);
        QuantumEvent.UnsubscribeListener<EventPlayerLeave>(this);
        QuantumEvent.UnsubscribeListener<EventPlayerDodge>(this);
        QuantumEvent.UnsubscribeListener<EventHitWall>(this);
        QuantumEvent.UnsubscribeListener<EventAttackName>(this);
    }
    
    
    public override void OnUpdateView(QuantumGame game)
    {
        var frame = QuantumRunner.Default.Game.Frames.Predicted;
        //var predictedFrame = QuantumRunner.Default.Game.Frames.Predicted;
        
        if(!frame.Exists(_entityView.EntityRef))
            return;
        
        var playerMovement = frame.Get<PlayerMovement>(_entityView.EntityRef);
        var playerAttack = frame.Get<PlayerAttack>(_entityView.EntityRef);
        var playerStat = frame.Get<PlayerStat>(_entityView.EntityRef);
        var gameplay = frame.GetSingleton<Gameplay>();
        
        freeRoam = frame.RuntimeConfig.freeMove;

        weaponType = playerStat.currentWeaponID;
        
        moveType = playerStat.MoveType;
        idleType = playerAttack.IdleType.AsFloat;
        isAiming = playerStat.IsAiming;
        isGrounded = playerMovement.Grounded;
        
        
        moveState = playerMovement.MoveState.AsFloat;
        dodgeRunning = playerMovement.DodgeRunning.AsFloat;
        runningSpeed = playerMovement.RunningSpeed.AsFloat;
        
        horizontalDirection = playerMovement.MoveDirection.X.AsFloat;
        verticalDirection = playerMovement.MoveDirection.Z.AsFloat;
        lookXDirection = playerMovement.InputDesires.DeltaYaw.AsFloat;
        
        isCrouching = playerMovement.IsCrouching;
        
        isAttacking = playerAttack.isAttacking;
        inFightStance = playerMovement.InFightStance;
        
        isDead = playerStat.IsDead;
        showCelebration = playerStat.ShowCelebration;

        showIntro = playerStat.ShowedIntro;
        celebrationType = playerStat.CelebrateType;
        
        isRagdoll = playerMovement.CanRagdoll;
        hitWall = playerAttack.HitWall;
        
        
        if (Mathf.Abs(horizontalDirection) < 0.2f && verticalDirection > 0)
        {
            isSprinting = playerMovement.Sprinting;
        }
        else
        {
            isSprinting = false;
        }
        
        //
        if (dodgeBack || dodgeForward || dodgeLeft || dodgeRight)
        {
            isDodging = true;
        }
        else
        {
            isDodging = false;
        }

        isDashing = playerMovement.IsDashing;
        
        dashForward = playerMovement.DashForward;
        dashBack = playerMovement.DashBack;

        dodgeForward = playerMovement.CurrentDodgeDir == DodgeDir.Forward;
        dodgeBack = playerMovement.CurrentDodgeDir == DodgeDir.Back;
        dodgeLeft = playerMovement.CurrentDodgeDir == DodgeDir.Left;
        dodgeRight = playerMovement.CurrentDodgeDir == DodgeDir.Right;
        dodgeForwardUp = playerMovement.CurrentDodgeDir == DodgeDir.ForwardUp;
        dodgeForwardDown = playerMovement.CurrentDodgeDir == DodgeDir.ForwardDown;
        dodgeBackDown = playerMovement.CurrentDodgeDir == DodgeDir.BackDown;
        dodgeBackUp = playerMovement.CurrentDodgeDir == DodgeDir.BackUp;
        
        //
        gotHit = playerAttack.GotHit;
        
        //
        blocking = playerAttack.IsBlocking;
        
        CheckMoving(frame, horizontalDirection, verticalDirection);
        ConvertMoveInput();
        //ForTpsMovement();
        UpdateAnim();
        GotHitManage();
        HitStopSloMo(gameplay,playerAttack);
        SmoothLerpRunning();
        UpdateFightStance();
    }

    private void HitStopSloMo(Gameplay gameplay, PlayerAttack playerAttack)
    {
        // HIT STOP ALWAYS WINS
        if (playerAttack.HitStop)
        {
            _anim.speed = 0f;
            return;
        }

        // SLOW-MO (avoid / perfect dodge)
        /*if (gameplay.AvoidSlowMo)
        {
            _anim.speed = 0.5f;
            return;
        }*/

        // NORMAL PLAY
        _anim.speed = 1f;
    }

    private void Ragdoll()
    {
        _anim.enabled = !isRagdoll;    
    }
    
    //To Play correct strafe for topdown type movement
    private void ConvertMoveInput()
    {
        var moveInput = new Vector3(horizontalDirection,0,verticalDirection);
        
        var localMove = transform.InverseTransformDirection(moveInput);
        var turnAmount = localMove.x;
        var forwardAmount = localMove.z;
        
        var currentHorizontal = _anim.GetFloat(Horizontal);
        var currentVertical = _anim.GetFloat(Vertical);

        var newHorizontal = Mathf.Lerp(currentHorizontal, turnAmount, Time.deltaTime * 5);
        var newVertical = Mathf.Lerp(currentVertical, forwardAmount, Time.deltaTime * 5);

        _anim.SetFloat(Horizontal, newHorizontal);
        _anim.SetFloat(Vertical, newVertical);
    }

    private void ForTpsMovement()
    {
        var currentHorizontal = _anim.GetFloat(Horizontal);
        var currentVertical = _anim.GetFloat(Vertical);

        var newHorizontal = Mathf.Lerp(currentHorizontal, horizontalDirection, Time.deltaTime * 5);
        var newVertical = Mathf.Lerp(currentVertical, verticalDirection, Time.deltaTime * 5);

        _anim.SetFloat(Horizontal, newHorizontal);
        _anim.SetFloat(Vertical, newVertical);
    }

    private void UpdateFightStance()
    {
        var target = inFightStance ? 0 : 1f;
        fightStanceRange = Mathf.Lerp(fightStanceRange, target, Time.deltaTime * fightStanceLerpSpeed);
    }
    
    //To Play correct animations
    void UpdateAnim()
    {
        if(_anim == null)
            return;
        
        _anim.SetBool(Grounded, isGrounded);
        _anim.SetBool(IsMoving, isMoving);
        
        _anim.SetFloat(MoveState, moveState);
        
        var currentLookYaw = _anim.GetFloat(LookX);
        var newLookYaw = Mathf.Lerp(currentLookYaw, lookXDirection, Time.deltaTime * 5);
        
        _anim.SetFloat(LookX, newLookYaw);
        _anim.SetFloat(FightStanceRange, fightStanceRange);
        _anim.SetFloat(MoveType, moveType);
        _anim.SetFloat(IdleType, idleType);
        _anim.SetBool(Sprint, isSprinting);
        
        _anim.SetBool(Aim, isAiming);

        _anim.SetBool(DashForward, dashForward);
        _anim.SetBool(DashBack, dashBack);
        
        _anim.SetBool(DodgeLeft, dodgeLeft);
        _anim.SetBool(DodgeRight, dodgeRight);
        _anim.SetBool(DodgeForward, dodgeForward);
        _anim.SetBool(DodgeBack, dodgeBack);
        _anim.SetBool(DodgeBackDown, dodgeBackDown);
        _anim.SetBool(DodgeBackUp, dodgeBackUp);
        _anim.SetBool(DodgeForwardUp, dodgeForwardUp);
        _anim.SetBool(DodgeForwardDown, dodgeForwardDown);
        
        _anim.SetBool(IsDodging, isDodging);
        _anim.SetBool(IsDashing, isDashing);
        //
        _anim.SetBool(GotHit, gotHit);
        
        //
        _anim.SetBool(Blocking, blocking);
        
        _anim.SetBool(IsCrouching, isCrouching);
        
        _anim.SetBool(IsAttacking,isAttacking);
        
        _anim.SetBool(IsDead, isDead);
        _anim.SetBool(ShowCelebration, showCelebration);
        
        _anim.SetFloat(DodgeRunning, smoothedDodgeValue);

        _anim.SetBool(ShowIntro, showIntro);
        _anim.SetInteger(CelebrationType, celebrationType);
        _anim.SetBool(FreeRoam, freeRoam);
        
        _anim.SetFloat(RunningSpeed, runningSpeed);
        
        _anim.SetBool("HitWall", hitWall);
    }

    private void CheckMoving(Frame frame, float horizontal, float vertical)
    {
        if (!frame.RuntimeConfig.freeMove)
        {
            if (dodgeForward || dodgeBack || dodgeForwardUp || dodgeBackUp)
            {
                isMoving = true;
            }
            else
            {
                isMoving = false;
            }
        }
        else if(frame.RuntimeConfig.freeMove)
        {
            if (horizontal != 0 || vertical != 0)
            {
                isMoving = true;
            }
            else
            {
                isMoving = false;
            }
        }
    }

    private void SmoothLerpRunning()
    {
        var targetValue = dodgeRunning >= 0.5f ? 1f : 0f;

        _elapsedTime += Time.deltaTime;
        var t = Mathf.Clamp01(_elapsedTime / lerpDuration);

        smoothedDodgeValue = Mathf.Lerp(smoothedDodgeValue, targetValue, t);

        // Optional: Reset elapsed time when target changes significantly
        if (Mathf.Abs(smoothedDodgeValue - targetValue) < 0.01f)
        {
            _elapsedTime = 0f;
        }
    }
    
    //FUNCTIONS
    
    //Locally prevent the hit trigger from getting called twice
    private void GotHitManage()
    {
        if(!_gotHit)
            return;
        
        gotHitTimer += Time.deltaTime;
        if (gotHitTimer >= 0.2f)
        {
            _gotHit = false;
            gotHitTimer = 0;
        }
    }
    
    //QUANTUM EVENT
    private void MeleeAttack(EventMeleeAttack meleeAttack)
    {
        if(meleeAttack.PlayerEntity != _entityView.EntityRef)
            return;
        
        //_anim.SetTrigger(Attack);
        //_anim.SetInteger(MeleeComboType, meleeAttack.attackType);
    }

    private void KickMeleeAttack(EventKickMeleeAttack kickMeleeAttack)
    {
        if(kickMeleeAttack.PlayerEntity != _entityView.EntityRef)
            return;
        
        //_anim.SetTrigger(Kick);
        //_anim.SetInteger(MeleeComboType,kickMeleeAttack.attackType);
    }
    
    private void PlayerHitEvent(EventPlayerHit playerHit)
    {
        if(playerHit.PlayerEntity != _entityView.EntityRef)
            return;
        
        //if(_gotHit)
            //return;
        
        //Use for camera shake a such
        
        //_anim.CrossFade(playerHit.HitReactionName, playerHit.crossFadeTime.AsFloat, 2, 0);
        _anim.Play(playerHit.HitReactionName, 2,0);
        _gotHit = true;
        gotHitTimer = 0;
    }
    
    private void DodgeEvent(EventDodge dodge)
    {
        if(dodge.PlayerEntity != _entityView.EntityRef)
            return;
        
        _anim.SetTrigger(Dodge);
    }

    private void PlayerDodgeEvent(EventPlayerDodge playerDodge)
    {
        if(playerDodge.Entity != _entityView.EntityRef)
            return;
        
        _anim.SetTrigger(playerDodge.Direction);    
    }
    
    private void BlockHit(EventBlockHit blockHit)
    {
        if(blockHit.PlayerEntity != _entityView.EntityRef)
            return;
        
        _anim.Play(blockHit.BlockReactionName, 2,0);
    }

    private void FinisherAttackerEvent(EventDidFinish didFinish)
    {
        if(didFinish.PlayerEntity != _entityView.EntityRef)
            return;
        
        _anim.Play(didFinish.FinisherName);
    }
    
    private void FinisherVictimEvent(EventGetsFinished getsFinished)
    {
        if(getsFinished.PlayerEntity != _entityView.EntityRef)
            return;
        
        _anim.Play(getsFinished.FinisherName);
    }
    
    private void GetRootPosEvent(EventGetRootMotionPos eventRootMotionPos)
    {
        if (eventRootMotionPos.AttackerEntity == _entityView.EntityRef)
        {
            QuantumLocalInput.Instance.rootPositionAttacker = rootBone.position;
            Debug.Log("Att Root Pos"+ QuantumLocalInput.Instance.rootPositionAttacker);
        }
        else if(eventRootMotionPos.VictimEntity == _entityView.EntityRef)
        {
            QuantumLocalInput.Instance.rootPositionVictim = rootBone.position;
        }
    }

    private void HitWallEvent(EventHitWall hitWall)
    {
        if(hitWall.PlayerEntity != _entityView.EntityRef)
            return;
        
        _anim.Play("HitWall");
    }
    
    private void DeadEvent(EventDead eventDead)
    {
        if(eventDead.PlayerEntity != _entityView.EntityRef)
            return;
        
        _anim.SetTrigger(Dead);
    }

    private void PlayerLeaveEvent(EventPlayerLeave eventPlayerLeave)
    {
        if(eventPlayerLeave.entity != _entityView.EntityRef)
            return;
        
        _anim.SetTrigger("Disconnected");
    }

    private void AttackEvent(EventAttackName attackName)
    {
        if(attackName.PlayerEntity != _entityView.EntityRef)
            return;
        
        _anim.Play(attackName.ActionName);
    }
}








