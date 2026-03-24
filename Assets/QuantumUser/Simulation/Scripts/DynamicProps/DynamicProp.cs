using Photon.Deterministic;

namespace Quantum
{
    public unsafe partial struct DynamicProp
    {
        
        public void FeedBackForce(Frame frame, EntityRef entity)
        {
            var rb = frame.Unsafe.GetPointer<PhysicsBody3D>(entity);
            var config = frame.FindAsset<DynamicPropConfig>(dynamicPropConfig.Id);

            var randomTilt = FPVector3.Forward * frame.RNG->Next(-FP._0_33, FP._0_33) 
                             + FPVector3.Right * frame.RNG->Next(-FP._0_33, FP._0_33);

            var forceDir = (FPVector3.Up + randomTilt).Normalized;
            
            var impactForce = frame.RNG->Next(config.impulseForceMin, config.impulseForceMax);
            
            var force = forceDir * impactForce;
            rb->AddLinearImpulse(force);
        }
    }
}