using UnityEngine.Scripting;

namespace Quantum
{
    [Preserve]
    public unsafe class PlayerMovementSystem :  SystemMainThread, ISignalOnPlayerDisconnected, ISignalOnComponentAdded<PlayerMovement>
    {
     
        public void OnAdded(Frame frame, EntityRef entity, PlayerMovement* component)
        {
            component->OnInit(frame, entity);
        }
        
        public override void Update(Frame frame)
        {
            foreach (var pair in frame.Unsafe.GetComponentBlockIterator<PlayerMovement>())
            {
                var entity = pair.Entity;
                if(frame.IsCulled(entity))
                    continue;
                
                pair.Component->Update(frame, entity);
            }
        }
        
        public void OnPlayerDisconnected(Frame frame, PlayerRef playerRef)
        {
            foreach (var player in frame.GetComponentIterator<PlayerMovement>())
            {
                if (player.Component.PlayerRef != playerRef)
                    continue;
                
                var playerStats = frame.Unsafe.GetPointer<PlayerStat>(player.Entity);
                playerStats->HandleDisconnection(frame, player.Entity);
            }
        }

        
    }
}