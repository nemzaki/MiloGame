using System;
using Unity.Cinemachine;
using UnityEngine;
using DG.Tweening;
using Quantum;

public class DynamicCameraFOV : MonoBehaviour
{
    [Header("Cam State")]
    public bool inCloseCombat;
    
    [Header("Presets")]
    public CamPreset normalCameraPreset;
    public CamPreset closeCombatCameraPreset;

    private CinemachineThirdPersonFollow _thirdPersonFollow;
    private bool _lastInCloseCombat;

    private void Awake()
    {
        _thirdPersonFollow = GetComponent<CinemachineThirdPersonFollow>();
        _lastInCloseCombat = inCloseCombat;
    }

    private void ChangeFOV()
    {
        if (_lastInCloseCombat == inCloseCombat)
            return;

        _lastInCloseCombat = inCloseCombat;
        
        DOTween.Kill(_thirdPersonFollow);
        
        if (inCloseCombat)
        {
            _thirdPersonFollow
                .DoShoulderOffset(closeCombatCameraPreset.shoulderOffset,
                    closeCombatCameraPreset.duration)
                .SetEase(Ease.OutSine);
        }
        else
        {
            _thirdPersonFollow
                .DoShoulderOffset(normalCameraPreset.shoulderOffset,
                    normalCameraPreset.duration)
                .SetEase(Ease.OutSine);
        }
    }
    
    private void Update()
    {
        var frame = QuantumRunner.Default.Game.Frames.Predicted;
        
        var gameplay = frame.GetSingleton<Gameplay>();
        inCloseCombat = gameplay.InCloseCombat;
        
        ChangeFOV();
    }
}

[Serializable]
public class CamPreset
{
    public float fov;
    public Vector3 shoulderOffset;
    public float duration;
}

public static class CinemachineTweenExtensions
{
    public static Tween DoShoulderOffset(
        this CinemachineThirdPersonFollow follow,
        Vector3 target,
        float duration)
    {
        return DOTween.To(
            () => follow.ShoulderOffset,
            x => follow.ShoulderOffset = x,
            target,
            duration
        );
    }
}
