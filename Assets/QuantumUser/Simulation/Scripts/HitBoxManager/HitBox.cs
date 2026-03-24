using Photon.Deterministic;

namespace Quantum
{
    public unsafe partial struct HitBox
    {
        
        public void Update(Frame frame, EntityRef entity)
        {
            destroyTimer += frame.DeltaTime;

            var hitBox = frame.Unsafe.GetPointer<HitBox>(entity);
            
            //Set Not Active
            if (destroyTimer >= FP._0_10)
            {
                hitBox->IsActive = false;
            }
            
            //Destroy Hit Box
            if (destroyTimer >= 1)
            {
                frame.Destroy(entity);
            }
            
            //Check if player got hit
            if(!frame.Exists(SourceEntity))
                return;
            
            var playerAttack = frame.Unsafe.GetPointer<PlayerAttack>(SourceEntity);
            if (playerAttack->GotHit)
            {
                frame.Destroy(entity);
            }
        }
    }
}