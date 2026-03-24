using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Deterministic;
using Quantum;
using RootMotion.FinalIK;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;
using Input = UnityEngine.Input;

public class PlayerMovementHandler : QuantumCallbacks
{
    public QuantumEntityView entityView;
    
    [HideInInspector]
    public BodyTilt bodyTilt;
    
    public bool isLocalPlayer;
    public float currentHealth;
    public bool isPlayerActive;
    
    [Header("Visuals")] 
    public GameObject playerSkin;
    
    public static int LocalPlayerID { get; private set; } = -1;
    
    
    private void Awake()
    {
        entityView = GetComponent<QuantumEntityView>();
    }
    
    private void Start()
    {
        var frame = QuantumRunner.Default.Game.Frames.Verified;
        var playerMovement = frame.Get<PlayerMovement>(entityView.EntityRef);
        
        if (QuantumRunner.Default.Session.IsLocalPlayer(playerMovement.PlayerRef))
        {
            isLocalPlayer = true;
        }

        //Set interpolotion mode
        if (isLocalPlayer)
        {
            entityView.InterpolationMode = QuantumEntityViewInterpolationMode.Prediction;
        }
        else
        {
            //entityView.InterpolationMode = QuantumEntityViewInterpolationMode.SnapshotInterpolation;
        }
    }

    public override void OnUpdateView(QuantumGame game)
    {
        var frame = QuantumRunner.Default.Game.Frames.Predicted;
        //var predictedFrame = QuantumRunner.Default.Game.Frames.Predicted;
        
        if(!frame.Exists(entityView.EntityRef))
            return;


        if (LocalPlayerID < 0 && isLocalPlayer)
        {
            var localPlayers = game.GetLocalPlayers();
            if (localPlayers.Count > 0)
            {
                SetLocalPlayer(localPlayers[0]);
            }
        }
        
        //
        //Experimental
        /*var kcc = frame.Get<KCC>(entityView.EntityRef);

        var lookPitch = kcc.Data.LookPitch.AsFloat;

        if (frame.TryGet<KCC>(entityView.EntityRef, out KCC verifiedKCC))
        {
            _smoothPitch = Mathf.SmoothDamp(_smoothPitch, verifiedKCC.Data.LookPitch.AsFloat,
                ref _smoothPitchVelocity, 0.1f);
            lookPitch = _smoothPitch;
        }

        lookPitch = Mathf.Clamp(lookPitch, lookPitchMin, lookPitchMax);

        cameraHandle.TrackingTarget.localRotation = Quaternion.Euler(lookPitch, 0.0f, 0.0f);*/

        //
        if(!isLocalPlayer)
            return;
        
        InGameUIHandler.Instance.isPlayerActive = isPlayerActive;
        
        GameManager.Instance.localPlayerEntity = entityView.EntityRef;
        GameManager.Instance.localPlayer = gameObject;
        GameManager.Instance.isLocalPlayer = isLocalPlayer;
    }
    
    
    //Get the local player index
    private void SetLocalPlayer(int index)
    {
        if (LocalPlayerID == index)
            return;

        LocalPlayerID = index;
    }
}













