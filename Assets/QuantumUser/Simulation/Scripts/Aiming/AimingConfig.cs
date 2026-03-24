namespace Quantum
{
    using Photon.Deterministic;

    /// <summary>
    /// Determines the distance used for the aiming-related features, such as raycasting to find projectiles destination or to position the camera.
    /// </summary>
    public class AimingConfig : AssetObject
    {
        public FPVector3 Offset;
    }
}