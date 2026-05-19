using System;
using ControlFreak2;
using Photon.Client.StructWrapping;
using Quantum;
using UnityEngine;

public class CameraFollowTarget : MonoBehaviour
{
    private QuantumEntityView _entityView;
    
    [SerializeField] private Transform followTarget;

    [SerializeField] private float rotationSpeed = 30;
    [SerializeField] private float topClamp = 70;
    [SerializeField] private float bottomClamp = -40f;

    private float cinemachineYaw;
    private  float cinemachinePitch;

    private void Awake()
    {
        _entityView = GetComponent<QuantumEntityView>();
    }

    private void LateUpdate()
    {
        CameraLogic();
    }
    
    private void CameraLogic()
    {
        var frame = QuantumRunner.Default.Game.Frames.Predicted;
        if(frame == null)return;

        var kcc = frame.Get<KCC>(_entityView.EntityRef);
        
        var lookYaw   = CF2Input.GetAxis("Mouse X");
        var lookPitch = CF2Input.GetAxis("Mouse Y");

        cinemachinePitch = UpdateRotation(cinemachinePitch, lookPitch, bottomClamp, topClamp, true);
        cinemachineYaw = UpdateRotation(cinemachineYaw, lookYaw, float.MinValue, float.MaxValue, false);
        
        ApplyRotation(cinemachinePitch, cinemachineYaw);
    }

    private void ApplyRotation(float pitch, float yaw)
    {
        followTarget.rotation = Quaternion.Euler(pitch,yaw, followTarget.eulerAngles.z);
    }
    
    private float UpdateRotation(float currentRotation, float input, float min, float max, bool isXAxis)
    {
        currentRotation += isXAxis ? -input: input;
        return Mathf.Clamp(currentRotation, min, max);
        
    }
}
