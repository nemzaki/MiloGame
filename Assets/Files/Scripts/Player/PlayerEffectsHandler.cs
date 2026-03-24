using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using DG.Tweening;
using Quantum;
using RootMotion.Demos;
using RootMotion.FinalIK;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;
using LayerMask = UnityEngine.LayerMask;

public unsafe class PlayerEffectsHandler : QuantumCallbacks
{
    [SerializeField] private QuantumEntityView _entityView;

    public Transform target;

    public Transform spawnPosition;
    
    private PlayerMovementHandler _playerMovementHandler;
    private PlayerStatsManager _statsManager;
    
    [Header("AIM")]
    [Space(10)]
    [SerializeField] private float aimView = 40f;
    [SerializeField] private float normalView = 60f;
    [SerializeField] private float fovTweenDuration = 0.25f;
    [SerializeField] private Ease fovEase = Ease.OutSine;
    
    private Tween _fovTween;
    private bool _wasAimingLastFrame;
    
    private void Awake()
    {
        _entityView = GetComponent<QuantumEntityView>();
        _playerMovementHandler = GetComponent<PlayerMovementHandler>();
        _statsManager = GetComponent<PlayerStatsManager>();
    }

    private void Start()
    {
        QuantumEvent.Subscribe<EventPlayerHitFX>(this, HitEvent);
        QuantumEvent.Subscribe<EventNoStamina>(this, NoStaminaEvent);
    }
    
    private void OnDisable()
    {
        QuantumEvent.UnsubscribeListener(this);
        QuantumEvent.UnsubscribeListener<EventPlayerHitFX>(this);
        QuantumEvent.UnsubscribeListener<EventNoStamina>(this);
    }

    public override void OnUpdateView(QuantumGame game)
    {
        var frame = QuantumRunner.Default.Game.Frames.Predicted;
        //var predictedFrame = QuantumRunner.Default.Game.Frames.Predicted;

        if (!frame.Exists(_entityView.EntityRef))
            return;

        var playerAttack = frame.Unsafe.GetPointer<PlayerAttack>(_entityView.EntityRef);
        
        
        if(!_playerMovementHandler.isLocalPlayer)
            return;
        
        QuantumLocalInput.Instance.spawnPosition = spawnPosition.position;
    }

    private void HitEvent(EventPlayerHitFX hitFx)
    {
        if(_entityView.EntityRef != hitFx.PlayerEntity)
            return;

        InGameUIHandler.Instance.uiFeels.PlayFeedbacks();
        
        FightFeedBackControl.Instance.PlayLightHitFeedBack();
        
        if (hitFx.HitType == nameof(HitType.Light))
        {
            var hit = LocalPoolManager.Instance.GetObjectFromPool(ObjectsInPool.Instance.hitLight);
            hit.transform.position = hitFx.HitPos.ToUnityVector3();
        }
        else if(hitFx.HitType == nameof(HitType.Mid))
        {
            var hit = LocalPoolManager.Instance.GetObjectFromPool(ObjectsInPool.Instance.hitMedium);
            hit.transform.position = hitFx.HitPos.ToUnityVector3();
        }
        else if(hitFx.HitType == nameof(HitType.Heavy))
        {
            var hit = LocalPoolManager.Instance.GetObjectFromPool(ObjectsInPool.Instance.hitHeavy);
            hit.transform.position = hitFx.HitPos.ToUnityVector3();
            
            FightFeedBackControl.Instance.PlayHardHitFeedBack();
            
            //HARD HIT
            _statsManager.ShowHardHitIndicator();
            InGameUIHandler.Instance.uiFeelsHardHit.PlayFeedbacks();
        }
    }
    
    private void NoStaminaEvent(EventNoStamina noStamina)
    {
        if(_entityView.EntityRef != noStamina.PlayerEntity)
            return;
        
        InGameUIHandler.Instance.uiFeelNoStamina.PlayFeedbacks();
    }
}
