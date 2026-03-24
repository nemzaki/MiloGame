using UnityEngine;
using System.Collections.Generic;
using Unity.Cinemachine;
using Unity.Cinemachine.Samples;

[ExecuteAlways]
public class AimCamera : CinemachineCameraManagerBase
{
    CinemachineVirtualCameraBase AimingCamera;
    CinemachineVirtualCameraBase FreeCamera;

    public bool isAiming;
    
    protected override void Start()
    {
        base.Start();

        // Find the player and the aiming camera.
        // We expect to have one camera with a CinemachineThirdPersonAim component
        // whose Follow target is a player with a SimplePlayerAimController child.
        for (int i = 0; i < ChildCameras.Count; ++i)
        {
            var cam = ChildCameras[i];
            if (!cam.isActiveAndEnabled)
                continue;
            if (AimingCamera == null
                && cam.TryGetComponent<CinemachineThirdPersonAim>(out var aim)
                && aim.NoiseCancellation)
            {
                AimingCamera = cam;
            }
            else if (FreeCamera == null)
                FreeCamera = cam;
        }

        if (AimingCamera == null)
            Debug.LogError("AimCameraRig: no valid CinemachineThirdPersonAim camera found among children");
        if (FreeCamera == null)
            Debug.LogError("AimCameraRig: no valid non-aiming camera found among children");
    }

    protected override CinemachineVirtualCameraBase ChooseCurrentCamera(Vector3 worldUp, float deltaTime)
    {
        var newCam = isAiming ? AimingCamera : FreeCamera;
        return newCam;
    }
}