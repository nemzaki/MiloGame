using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Deterministic;
using Quantum;
using UnityEngine;
using UnityEngine.InputSystem;
using ControlFreak2;
using Quantum.Collections;
using Unity.Cinemachine;
using UnityEngine.Serialization;
using Input = UnityEngine.Input;

public class QuantumLocalInput : MonoBehaviour {

      public static QuantumLocalInput Instance { get; private set; }

      [SerializeField]
      private QuantumEntityViewUpdater _entityViewUpdater;
      
      [HideInInspector] public int localPlayer;

      private ChainComboSystem _chainComboSystem;
      
      public static Camera cam;

      public float h;
      public float v;

      public Transform player;
      
      
      [Header("Look")]
      public float mouseSpeed = 3f;
      public float deltaPitch;
      public float deltaYaw;
      
      [Header("Player")]
      public Vector3 spawnPosition;
      public bool isRunning;
      public Vector3 rootPositionAttacker;
      public Vector3 rootPositionVictim;

      [Header("Local Inputs")] 
      public float holdThreshold = 0.2f;
      private float _keyDownTime;
      public bool isHolding;

      private float _kickKeyDownTime;
      public bool isHoldingKick;
      
      public bool aiming;

      [Header("Double Tap")] 
      public bool _dodgeForwardQueued;
      public bool _dodgeBackQueued;
      private float _lastForwardTapTime;
      private float _lastBackTapTime;
      public float doubleTapTime = 0.3f;
      
      [Header("Double Drag")]
      public float dragThreshold = 0.5f;     // How far joystick must move
      public float doubleDragTime = 0.3f;    // Time window for double drag
      public float resetThreshold = 0.2f;    // Joystick must return near 0 before next drag

      private float lastDragTime = -1f;
      private int dragCount = 0;
      private bool dragRegistered = false;

      public bool doubleDragLeft;
      public bool doubleDragRight;

      [Header("Stats / Connection")] 
      public int ping;
      private const int Window = 20;
    
      public List<int> pingSamples = new List<int>();
      private float _averagePing() => (float)pingSamples.Average();
      private float _jitter() => pingSamples.Max() - pingSamples.Min();

      public int warningConnectionTime = 3;
      public int badConnectionCheckTime = 10;
      private float _badConnectionCheckTimer;
      public bool isConnectionBad;
      public bool constantBadConnection;
      public bool warningBadConnection;
      
      [Header("Finisher")] 
      public int finisher;
      
      //
      private bool _attackPressed;
      private bool _hardAttackPressed;
      private bool _kickPressed;
      private bool _hardKickPressed;
      
      private bool _dodgePressed;
      private bool _blockPressed;
      private bool _finisherPressed;
      
      //COMBO INPUTS
      public bool _sweepKickQueued;
      
      private struct PolledInput
      {
        public int Frame;
        public Quantum.Input Input;
      }

      private PolledInput[] _polledInputs = new PolledInput[20];

      private void Awake() {
          Instance = this;
      }

      private void Start()
      {
          QuantumCallback.Subscribe(this, (CallbackPollInput callback) => PollInput(callback));
          _chainComboSystem = GetComponent<ChainComboSystem>();
      }
      
      //STABLE CONNECTION CHECK
      private void UpdatePing(int newPing)
      {
          if(pingSamples.Count >= Window) pingSamples.RemoveAt(0);
          pingSamples.Add(newPing);
      }

      private bool IsBadConnection()
      {
          UpdatePing(ping);
          
          var avgPing = _averagePing();
          var avgJitter = _jitter();

          var highPing = avgPing > 190;
          var unstable = avgJitter > 30;
          
          return highPing || unstable;
      }

      private void CheckConnection()
      {
          if (IsBadConnection())
          {
              _badConnectionCheckTimer += Time.deltaTime;
          }
          else
          {
              _badConnectionCheckTimer = 0;

              warningBadConnection = false;
              constantBadConnection = false;
          }

          //WARN PLAYER
          if (_badConnectionCheckTimer >= warningConnectionTime)
          {
              warningBadConnection = true;
          }
          
          //BAD CONNECTION DISCONNECT
          if (_badConnectionCheckTimer >= badConnectionCheckTime)
          {
              //Hand Technical defeat here
              constantBadConnection = true;
          }
      }
      
    
      private void GetPunchInput()
      {
          if (CF2Input.GetKeyDown(KeyCode.P))
          {
              _keyDownTime = Time.time;
              isHolding = false;
          }

          //Key Still held down
          if (CF2Input.GetKey(KeyCode.P))
          {
              var heldTime = Time.time - _keyDownTime;

              if (!isHolding && heldTime > holdThreshold)
              {
                  isHolding = true;
                  _hardAttackPressed = true;
              }
          }

          if (!CF2Input.GetKey(KeyCode.P) && !isHolding && _keyDownTime > 0)
          {
              var heldTime = Time.time - _keyDownTime;
              if (heldTime < holdThreshold)
              {
                  //Combo chain
                  _chainComboSystem.comboInputBuffer.AddInput(ChainComboSystem.AttackInput.Punch);

                  if (_chainComboSystem.TryExecuteCombo(_chainComboSystem.comboInputBuffer.GetSequence()))
                  {
                      ClearAttackInputs();
                      return;
                  }

                  //Normal Attack
                  _attackPressed = true;
              }

              _keyDownTime = 0;
          }
      }
      
      private void GetKickInput()
      {
          if (CF2Input.GetKeyDown(KeyCode.K))
          {
              _kickKeyDownTime = Time.time;
              isHoldingKick = false;
          }

          //Key Still held down
          if (CF2Input.GetKey(KeyCode.K))
          {
              var heldTime = Time.time - _kickKeyDownTime;

              if (!isHoldingKick && heldTime > holdThreshold)
              {
                  isHoldingKick = true;
                  _hardKickPressed = true;
              }
          }

          if (!CF2Input.GetKey(KeyCode.K) && !isHoldingKick && _kickKeyDownTime > 0)
          {
              var heldTime = Time.time - _kickKeyDownTime;
              if (heldTime < holdThreshold)
              {
                  //Combo chain
                  _chainComboSystem.comboInputBuffer.AddInput(ChainComboSystem.AttackInput.Kick);

                  if (_chainComboSystem.TryExecuteCombo(_chainComboSystem.comboInputBuffer.GetSequence()))
                  {
                      ClearAttackInputs();
                      return;
                  }
                  
                  //Normal Kick
                  _kickPressed = true;
              }

              _kickKeyDownTime = 0;
          }
      }

      private void ClearAttackInputs()
      {
          _attackPressed = false;
          _kickPressed = false;
          _hardAttackPressed = false;
          _hardKickPressed = false;
      }

      public void QueueSweepKick()
      {
        _sweepKickQueued = true;    
      }
      
      private void DoubleDrag()
      {
          // Detect forward (positive) or back (negative) drag
          if (Mathf.Abs(h) > dragThreshold && !dragRegistered)
          {
              // Register a new drag
              if (Time.time - lastDragTime <= doubleDragTime)
              {
                  dragCount++;

                  if (dragCount == 2)
                  {
                      Debug.Log("Double Drag Detected!");
                      OnDoubleDrag(h);
                      dragCount = 0;
                  }
              }
              else
              {
                  dragCount = 1; // start new sequence
              }

              lastDragTime = Time.time;
              dragRegistered = true; // block until released
          }

          // Reset dragRegistered when joystick returns to center
          if (Mathf.Abs(h) < resetThreshold)
          {
              dragRegistered = false;
          }
      }
      
      void OnDoubleDrag(float direction)
      {
          // Do something with the input
          if (direction > 0)
          {
              doubleDragRight = true;
          }
          else
          {
              doubleDragLeft = true;
          }
      }
      
      // Called from Unity Update (collect inputs)
      private void Update() 
      {
          if (QuantumRunner.Default == null || !QuantumRunner.Default.IsRunning)
              return;
          
          ping = QuantumRunner.Default.Game.Session.Stats.Ping;
          
          if (CF2Input.GetKeyDown(KeyCode.D)) {
              if (Time.time - _lastForwardTapTime < doubleTapTime) {
                  _dodgeForwardQueued = true; // queue until PollInput consumes it
              }
              _lastForwardTapTime = Time.time;
          }

          if (CF2Input.GetKeyDown(KeyCode.A)) {
              if (Time.time - _lastBackTapTime < doubleTapTime) {
                  _dodgeBackQueued = true;
              }
              _lastBackTapTime = Time.time;
          }
          
          DoubleDrag();
          
          isConnectionBad = IsBadConnection();
          CheckConnection();
      }
      
      private void LateUpdate()
      {
          CursorManage();
          
          deltaPitch -= CF2Input.GetAxis("Mouse Y") * mouseSpeed;
          deltaYaw += CF2Input.GetAxis("Mouse X") * mouseSpeed;
          
          if (CF2Input.GetKeyDown(KeyCode.I))
          {
              aiming = !aiming;
          }

          if (CF2Input.GetKeyDown(KeyCode.G))
          {
              _dodgePressed = true;
          }
          
          GetPunchInput();
          
          GetKickInput();
          
          if (CF2Input.GetKeyDown(KeyCode.F))
          {
              _finisherPressed = true;
          }

          _blockPressed = CF2Input.GetKey(KeyCode.B);
      }
      
      private void CursorManage() 
      {
          // Enter key is used for locking/unlocking cursor in game view.
          Keyboard keyboard = Keyboard.current;
          if (keyboard != null && (keyboard.escapeKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame))
          {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
              Cursor.lockState = CursorLockMode.None;
              Cursor.visible = true;
            }
            else
            {
              Cursor.lockState = CursorLockMode.Locked;
              Cursor.visible = false;
            }
          }
      }
      
      private void PollInput(CallbackPollInput callback)
      {
          Quantum.Input input = new Quantum.Input();

          h = CF2Input.GetAxis("Joystick Move X");
          v = CF2Input.GetAxis("Joystick Move Y");
          
          input.RootPosition = rootPositionAttacker.ToFPVector3();
          input.RootPositionVictim = rootPositionVictim.ToFPVector3();
          
          var dir = ProcessInput(h, v);
          input.MovementDirection = dir.ToFPVector2();

          //input.VerticalAxis = _v.ToFP();
          
          /*input.PlayerDodgeForward = _dodgeForwardQueued;
          input.PlayerDodgeBack = _dodgeBackQueued;

          // reset so it only lasts one frame
          _dodgeForwardQueued = false;
          _dodgeBackQueued = false;*/

          input.PlayerPing = ping;
          input.PlayerDodgeForward = doubleDragRight;
          input.PlayerDodgeBack = doubleDragLeft;
          
          doubleDragRight = false;
          doubleDragLeft = false;
          
          input.PlayerJump = CF2Input.GetButton("Jump");

          input.PlayerAttack = _attackPressed;
          _attackPressed = false;

          input.PlayerAttackHard = _hardAttackPressed;
          _hardAttackPressed = false;
          
          input.PlayerKick = _kickPressed;
          _kickPressed = false;
          
          input.PlayerKickHard = _hardKickPressed;
          _hardKickPressed = false;
          
          //CHAIN COMBO INPUTS
          input.SweepKick = _sweepKickQueued;
          
          _sweepKickQueued = false;
          
          // Use the stored value
          input.PlayerDodge = _dodgePressed;
          _dodgePressed = false; // Reset so it's only true for one tick
          
          input.PlayerBlock = _blockPressed;
          
          input.FinisherAttack = _finisherPressed;
          _finisherPressed = false;
          
          input.PlayerSprint = isRunning;
          
          input.CameraPosition = cam.transform.position.ToFPVector3();
          
          //LOOK
          input.DeltaPitch = FP.FromFloat_UNSAFE(deltaPitch);
          deltaPitch = 0;

          input.DeltaYaw = FP.FromFloat_UNSAFE(deltaYaw);
          deltaYaw = 0;
          
          _polledInputs[callback.Frame % _polledInputs.Length] = new PolledInput() { Frame = callback.Frame, Input = input };
              
          input.InterpolationOffset = (byte)Mathf.Clamp(callback.Frame - _entityViewUpdater.SnapshotInterpolation.CurrentFrom, 0, 255);
          input.InterpolationAlpha  = _entityViewUpdater.SnapshotInterpolation.Alpha.ToFP();
          
          //CONNECTION
          input.ConnectionWarning = warningBadConnection;
          input.ConnectionBad = constantBadConnection;
          
          callback.SetInput(input, DeterministicInputFlags.Repeatable);
      }

      private static Vector3 ProcessInput(float x, float y)
      {
          cam = Camera.main;

          if (cam == null)
              return Vector3.zero;
      
          Vector3 forward = Vector3.Normalize(Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up));
          Vector3 right   = Vector3.Normalize(Vector3.ProjectOnPlane(cam.transform.right, Vector3.up));

          var dir = forward * y;
          dir += right * x;
          dir.Normalize();
          dir.y = 0;
          
          
          return dir;
      }
      
    
      //Look
      public Vector2 GetPendingLookRotationDelta(QuantumGame game)
      {
          var pendingLookRotationDelta = default(Vector2);

          for (int i = 0; i < game.Session.LocalInputOffset; ++i)
          {
              // To get responsive look rotation, we need to apply all pending inputs ahead of predicted tick => these can be delayed by local input offset.
              // For example if the LocalInputOffset == 2, PredictedFrame.Number == 100, inputs for ticks 101 and 102 are already polled and we should apply them as well.
              Quantum.Input polledInput = GetInputForFrame(game.Frames.Predicted.Number + i + 1);
              pendingLookRotationDelta.x += polledInput.DeltaPitch.AsFloat;
              pendingLookRotationDelta.y += polledInput.DeltaYaw.AsFloat;
          }

          return new Vector2(deltaPitch + pendingLookRotationDelta.x, deltaYaw + pendingLookRotationDelta.y);
      }
      
      private Quantum.Input GetInputForFrame(int frame)
      {
          if (frame <= 0)
              return default;

          PolledInput polledInput = _polledInputs[frame % _polledInputs.Length];
          if (polledInput.Frame == frame)
              return polledInput.Input;

          return default;
      }
}






