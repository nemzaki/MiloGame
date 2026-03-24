using Photon.Deterministic;

namespace Quantum
{
    public unsafe class AIAttack
    {

        public void CheckHit(Frame frame, EntityRef entity, AIConfig config)
        {
            var aiPlayer = frame.Unsafe.GetPointer<AIPlayer>(entity);
            var playerAttack = frame.Unsafe.GetPointer<PlayerAttack>(entity);
            
            if (playerAttack->GotHit)
            {
                aiPlayer->GettingHitTimer += frame.DeltaTime;
            }
            else
            {
                if(aiPlayer->GettingHitTimer <= 0)
                    return;
                
                aiPlayer->GettingHitTimer -= frame.DeltaTime;
            }
            
            //Check Spammed
            if (aiPlayer->GettingHitTimer >= config.gettingSpammedMax)
            {
                aiPlayer->GettingSpammed = true;
            }

            if (aiPlayer->GettingSpammed)
            {
                aiPlayer->HyperAwareTimer += frame.DeltaTime;
                if (aiPlayer->HyperAwareTimer >= config.hyperAwareTime)
                {
                    aiPlayer->HyperAwareTimer = 0;
                    aiPlayer->GettingHitTimer = 0;
                    aiPlayer->GettingSpammed = false;
                }
            }
        }
        
        public void PunchAttack(Frame frame, EntityRef entity, AIConfig config)
        {
            var aiPlayer = frame.Unsafe.GetPointer<AIPlayer>(entity);
            
            aiPlayer->PunchAttackTimer += frame.DeltaTime;

            if (aiPlayer->PunchAttackTimer >= config.attackRate)
            {
                aiPlayer->CanAttack = true;
                aiPlayer->PunchAttackTimer = 0;
            }
            else
            {
                aiPlayer->CanAttack = false;
            }
        }

        public void KickAttack(Frame frame, EntityRef entity, AIConfig config)
        {
            var aiPlayer = frame.Unsafe.GetPointer<AIPlayer>(entity);

            aiPlayer->KickAttackTimer += frame.DeltaTime;

            if (aiPlayer->KickAttackTimer >= config.kickRate)
            {
                aiPlayer->CanKick = true;
                aiPlayer->KickAttackTimer = 0;
            }
            else
            {
                aiPlayer->CanKick = false;
            }
        }

        public void HardAttack(Frame frame, EntityRef entity, AIConfig config)
        {
            var aiPlayer = frame.Unsafe.GetPointer<AIPlayer>(entity);
            
            aiPlayer->HardPunchAttackTimer += frame.DeltaTime;

            if (aiPlayer->HardPunchAttackTimer >= 1)
            {
                aiPlayer->CanHardAttack = true;
                aiPlayer->HardPunchAttackTimer = 0;
            }
            else
            {
                aiPlayer->CanHardAttack = false;
            }
        }

        public void HardKick(Frame frame, EntityRef entity, AIConfig config)
        {
            var aiPlayer = frame.Unsafe.GetPointer<AIPlayer>(entity);
            
            aiPlayer->HardKickAttackTimer += frame.DeltaTime;

            if (aiPlayer->HardKickAttackTimer >= 1)
            {
                aiPlayer->CanHardKick = true;
                aiPlayer->HardKickAttackTimer = 0;
            }
            else
            {
                aiPlayer->CanHardKick = false;
            }
        }
    }
}










