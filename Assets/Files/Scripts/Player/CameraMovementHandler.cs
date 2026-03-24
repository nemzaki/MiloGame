using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Photon.Deterministic;
using Quantum;
using RootMotion.FinalIK;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;
using Input = UnityEngine.Input;

public class CameraMovementHandler : QuantumCallbacks
{
    public QuantumEntityView entityView;
    
    
    private UpdatePlayerMovementAnimator _playerMovementAnimator;
    private PlayerMovementHandler _playerMovementHandler;

    [Header("Target")] 
    public CinemachineTargetGroup.Target target;
    public Transform cameraTarget;
    private float _smoothPitch;
    private float _smoothPitchVelocity;
    public float camRotPos;

    [Header("Camera")] 
    public bool useDynamicCamera;
    public CinemachineCamera playerCamera;
    public CinemachineThirdPersonFollow cameraFollow;
    public DynamicCameraPos camSideRight;
    public DynamicCameraPos camSideLeft;

    public float cameraPitch;
    public bool movingRight;
    public bool movingLeft;
    private float _smoothedH;
    private float _hVelocity;
    private float _cameraSideVelocity;
    private float _cameraYawVelocity;

    [SerializeField] float inputSmoothTime = 0.15f;
    [SerializeField] float cameraSideSmoothTime = 0.2f;
    [SerializeField] float cameraYawSmoothTime = 0.12f;
    [SerializeField] float cameraYawSmoothTimeReset = 1;
    
    [SerializeField] float deadZone = 0.1f;
    
    private float _sideCommitTimer;
    [SerializeField] float sideCommitTime = 0.25f;

    [Header("Cam Dodge")] 
    [SerializeField] private float dodgeLeftXPos;
    [SerializeField] private float dodgeRightXPos;
    [SerializeField] private float dodgeSmoothSpeed;
    
    private void Awake()
    {
        entityView = GetComponent<QuantumEntityView>();
        _playerMovementAnimator = GetComponent<UpdatePlayerMovementAnimator>();
        _playerMovementHandler = GetComponent<PlayerMovementHandler>();
    }
    
    private void Start()
    {
        //Add to camera group
        GameManager.Instance.cameraTargetGroup.Targets.Add(target);
    }

    public override void OnUpdateView(QuantumGame game)
    {
        var frame = QuantumRunner.Default.Game.Frames.Predicted;
        
        if(!frame.Exists(entityView.EntityRef))
            return;
        
        //
        if(!_playerMovementHandler.isLocalPlayer)
            return;
        
        FreeMoveDynamicCamera(frame);
    }

    private void FreeMoveDynamicCamera(Frame frame)
    {
        var gameplay = frame.GetSingleton<Gameplay>();
        var playerMovement = frame.Get<PlayerMovement>(entityView.EntityRef);
        
        if (frame.RuntimeConfig.freeMove && useDynamicCamera)
        {
            // --- CAMERA SIDE DECISION (SIDE COMMIT + DODGE OVERRIDE) ---
            var isDodgingLeft  = _playerMovementAnimator.dodgeLeft;
            var isDodgingRight = _playerMovementAnimator.dodgeRight;
            
            //Smooth movement
            var rawH = QuantumLocalInput.Instance.h;

            //DODGE CAM ------------
            //when dodging the camera damp should move left and right
           if (isDodgingLeft)
           {
               cameraTarget.transform.DOLocalMoveX(dodgeLeftXPos, dodgeSmoothSpeed);
           }
           else if(isDodgingRight)
           {
               cameraTarget.transform.DOLocalMoveX(dodgeRightXPos, dodgeSmoothSpeed);
           }
           else
           {
               cameraTarget.transform.DOLocalMoveX(0, dodgeSmoothSpeed);
           }
            //------------------------
            
            // 1) Dodge overrides immediately BUT does NOT kill side commit system
            if (isDodgingLeft)
            {
                movingLeft = true;
                movingRight = false;
            }
            else if (isDodgingRight)
            {
                movingRight = true;
                movingLeft = false;
            }
            else
            {
                // 2) Normal side commit logic resumes when NOT dodging
                if (Mathf.Abs(rawH) > 0.7f)
                {
                    _sideCommitTimer += Time.deltaTime;

                    if (_sideCommitTimer >= sideCommitTime)
                    {
                        movingRight = rawH > 0;
                        movingLeft  = rawH < 0;
                    }
                }
                else
                {
                    _sideCommitTimer = 0f;
                }
            }
            
            playerCamera.enabled = true;
            
            // Normalize camRotPos to 0–1 range using inverse lerp over expected angle range (-180 to 180)
            if (movingLeft)
            {
                camRotPos = playerMovement.CamViewRotPos.AsFloat;

                float smoothYaw;

                if (gameplay.InCloseCombat)
                {
                    smoothYaw = Mathf.SmoothDampAngle(
                        cameraTarget.localEulerAngles.y,
                        camRotPos,
                        ref _cameraYawVelocity,
                        cameraYawSmoothTime
                    );
                }
                else
                {
                    smoothYaw = Mathf.SmoothDampAngle(
                        cameraTarget.localEulerAngles.y,
                        0f,
                        ref _cameraYawVelocity,
                        cameraYawSmoothTimeReset
                    );
                }


                cameraTarget.localEulerAngles = new Vector3(cameraPitch, smoothYaw, 0);

                var normalizedRot = Mathf.InverseLerp(-180f, 180f, camRotPos);
                var targetCameraSide =
                    Mathf.Lerp(camSideRight.camSideMin, camSideRight.camSideMax, normalizedRot);

                cameraFollow.CameraSide = Mathf.SmoothDamp(cameraFollow.CameraSide,
                    targetCameraSide, ref _cameraSideVelocity, cameraSideSmoothTime);
            }
            else if(movingRight)
            {
                camRotPos = playerMovement.CamViewRotPos.AsFloat;

                float smoothYaw;

                if (gameplay.InCloseCombat)
                {
                    smoothYaw = Mathf.SmoothDampAngle(
                        cameraTarget.localEulerAngles.y,
                        -camRotPos,
                        ref _cameraYawVelocity,
                        cameraYawSmoothTime
                    );
                }
                else
                {
                    smoothYaw = Mathf.SmoothDampAngle(
                        cameraTarget.localEulerAngles.y,
                        0f,
                        ref _cameraYawVelocity,
                        cameraYawSmoothTimeReset
                    );
                }

                cameraTarget.localEulerAngles = new Vector3(cameraPitch, smoothYaw, 0);

                var normalizedRot = Mathf.InverseLerp(-180f, 180f, camRotPos);
                var targetCameraSide =
                    Mathf.Lerp(camSideLeft.camSideMin, camSideLeft.camSideMax, normalizedRot);

                cameraFollow.CameraSide = Mathf.SmoothDamp(cameraFollow.CameraSide,
                    targetCameraSide, ref _cameraSideVelocity, cameraSideSmoothTime
                );
            }
        }

    }
    
}

[Serializable]
public class DynamicCameraPos
{
    public float camSideMin;
    public float camSideMax;
}









