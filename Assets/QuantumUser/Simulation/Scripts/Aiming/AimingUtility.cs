using UnityEngine;

namespace Quantum
{
  using Photon.Deterministic;

  /// <summary>
  /// Utility class built to help 
  /// </summary>
  public static unsafe class AimingUtility
  {
    /// <summary>
    /// The total distance used for the raycast to try and find a target.
    /// </summary>
    private static readonly FP RaycastDistance = FP._10 * FP._3;

    /// <summary>
    /// Performs a raycast in order to try and find a target geometry (characters, static) and returns the direction from a projectile position towards such raycast hit (if any).
    /// </summary>
   public static Transform3D GetTargetHitPosition(Frame frame, FPVector3 camPosition, KCC* kcc)
    {
        var aimingConfig = frame.FindAsset<AimingConfig>(frame.RuntimeConfig.AimingConfig.Id);

        // Start creating aim position based on the character position and an offset which simulates a shoulder position.
        //var aimPosition = characterTransform->Position + characterTransform->TransformDirection(aimingConfig.Offset.XYO);
        var aimPosition = camPosition;
        
        // Based on the character current Pitch and Yaw, create a new quaternion so we can create a new Transform for the aim based on the position and rotation
        var aimRotation = FPQuaternion.Euler(new FPVector3(kcc->Data.LookPitch, kcc->Data.LookYaw, 0));

        // The transform is used here just to use its utilities (TransformDirection, Forward)
        var aimTransform = new Transform3D()
        {
          Position = aimPosition,
          Rotation = aimRotation
        };
        
        return aimTransform;
    }
    
    public static FPVector3 GetTargetDirection(Frame frame, Transform3D* characterTransform, KCC* kcc)
    {
        var aimingConfig = frame.FindAsset<AimingConfig>(frame.RuntimeConfig.AimingConfig.Id);

        // Start creating aim position based on the character position and an offset which simulates a shoulder position.
        var aimPosition = characterTransform->Position + characterTransform->TransformDirection(aimingConfig.Offset.XYO);

        // Based on the character current Pitch and Yaw, create a new quaternion so we can create a new Transform for the aim based on the position and rotation
        var aimRotation = FPQuaternion.Euler(new FPVector3(kcc->Data.LookPitch, kcc->Data.LookYaw, 0));

        // The transform is used here just to use its utilities (TransformDirection, Forward)
        var aimTransform = new Transform3D()
        {
          Position = aimPosition,
          Rotation = aimRotation
        };

        // Add depth
        aimTransform.Position += aimTransform.TransformDirection(new FPVector3(0, 0, aimingConfig.Offset.Z));

        // Perform a raycast based on the aim transform we just created
        var aimingHitPosition = AimingRaycast(frame, aimTransform);

        // Debug the ray and the hit
        //Draw.Line(aimTransform.Position, aimTransform.Position + aimTransform.Forward * FP._10 * 3);
        //Draw.Sphere(aimingHitPosition, FP._0_50);
        // Find the desired direction for the projectile based on its position and the raycast hit
        var projectileDir = aimingHitPosition;

        // If the angle between the aim transform and the corrected projectile direction is too big, maybe the raycast hit a nearby wall
        // As we don't want the projectile to move backwards or sideways, we just the end position of the raycast instead
        if (FPVector3.Dot(aimTransform.Forward, projectileDir) > 0)
        {
          return projectileDir;
        }
        else
        {
          return aimTransform.Position + aimTransform.Forward * RaycastDistance;
        }
    }
    
    
  /// <summary>
    /// Returns the position based on the result of a raycast from the aim position and its direction, hitting only relevant layers.
    /// </summary>
    private static FPVector3 AimingRaycast(Frame frame, Transform3D aimTransform)
    {
      var queryOptions = QueryOptions.HitStatics | QueryOptions.HitKinematics | QueryOptions.ComputeDetailedInfo;
      var hit = frame.Physics3D.Raycast(aimTransform.Position, aimTransform.Forward, RaycastDistance, frame.Layers.GetLayerMask("Static", "Character", "Target"), queryOptions);

      if (hit != null)
      {
        return hit.Value.Point;
      }
      else
      {
        return aimTransform.Position + aimTransform.Forward * RaycastDistance;
      }
    }
  }
}