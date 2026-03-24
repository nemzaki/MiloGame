using Quantum.Core;
using Quantum.Physics3D;
using Quantum.Prototypes;
using Quantum.Scripts.Weapon;
using UnityEngine;
using UnityEngine.Scripting;

namespace Quantum
{
    [Preserve]
    public unsafe class PlayerAttackSystem :  SystemMainThread, ISignalOnComponentAdded<PlayerAttack>, ISignalOnTrigger3D, 
        ISignalOnTriggerExit3D, ISignalOnCollisionEnter3D, ISignalOnPlayerDisconnected
    {
     
        public void OnAdded(Frame frame, EntityRef entity, PlayerAttack* component)
        {
            component->OnInit(frame, entity);
        }
        
        public override void Update(Frame frame)
        {
            foreach (var pair in frame.Unsafe.GetComponentBlockIterator<PlayerAttack>())
            {
                var entity = pair.Entity;
                if(frame.IsCulled(entity))
                    continue;
                
                pair.Component->Update(frame, entity);
                
                var aiPlayer = frame.Unsafe.GetPointer<AIPlayer>(entity);
                aiPlayer->Update(frame, entity);
            }
        }


        public void OnTrigger3D(Frame frame, TriggerInfo3D info)
        {
            if (frame.Unsafe.TryGetPointer<PlayerAttack>(info.Entity, out var playerAttack) == false)
                return;

            if (frame.Unsafe.TryGetPointer<PlayerMovement>(info.Entity, out var playerMovement) == false)
                return;
        }
        
        public void OnTriggerExit3D(Frame frame, ExitInfo3D info)
        {
            if (frame.Unsafe.TryGetPointer<PlayerAttack>(info.Entity, out var playerAttack) == false)
                return;
        }
        
        void ISignalOnCollisionEnter3D.OnCollisionEnter3D(Frame frame, CollisionInfo3D info)
        {
            //ONLY PLAYERS COLLISION 
            if (frame.Unsafe.TryGetPointer<PlayerAttack>(info.Entity, out var playerAttack) == false)
                return;
            
        }

        
        public void OnPlayerDisconnected(Frame frame, PlayerRef player)
        {
            //Player lost match if disconnected
        }
        
    }
}