using Quantum;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerCamera : QuantumEntityViewComponent
{
    public float mouseSpeed = 3f; 
    public float rotationSmoothness = 0.1f; // Adjust this value for smoothness

    public AssetRef<AimingConfig> aimingConfig;
    public Transform pivot;
    public Transform handle;

    private QuantumLocalInput _localInput;
    private Quaternion targetRotation;

    public override unsafe void OnActivate(Quantum.Frame frame)
    {
        if (frame.Unsafe.TryGetPointer<PlayerMovement>(EntityRef, out var playerMove) == true && Game.PlayerIsLocal(playerMove->PlayerRef) == true)
        {
            _localInput = FindFirstObjectByType<QuantumLocalInput>();
        }
        else
        {
            Destroy(this);
        }
    }

    public override void OnLateUpdateView()
    {
        var frame = PredictedFrame;

        if (!frame.TryGet<KCC>(EntityRef, out var kcc)) return;
        if (!frame.TryGet<PlayerMovement>(EntityRef, out var playerMove)) return;
        if (!Game.PlayerIsLocal(playerMove.PlayerRef)) return;

        var aimingConfigAsset = QuantumUnityDB.GetGlobalAsset(aimingConfig);

        pivot.localPosition = new Vector3(0, aimingConfigAsset.Offset.Y.AsFloat, 0);

        // Get input rotation delta
        var accumulatedRotation = _localInput.GetPendingLookRotationDelta(Game);
        var newRotation = Quaternion.Euler(
            kcc.Data.LookPitch.AsFloat + accumulatedRotation.x * mouseSpeed,
            kcc.Data.LookYaw.AsFloat + accumulatedRotation.y * mouseSpeed,
            0f
        );

        // Smoothly interpolate between current and target rotation
        pivot.rotation = Quaternion.Slerp(pivot.rotation, newRotation, rotationSmoothness);

        handle.localPosition = aimingConfigAsset.Offset.XOZ.ToUnityVector3();
        handle.GetPositionAndRotation(out var cameraPosition, out var cameraRotation);

        if (Camera.main != null)
        {
            Camera.main.transform.SetPositionAndRotation(cameraPosition, cameraRotation);
        }
    }
}
