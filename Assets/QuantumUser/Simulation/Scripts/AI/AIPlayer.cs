using Photon.Deterministic;
using UnityEngine;

namespace Quantum
{
    public unsafe partial struct AIPlayer
    {
        public void Update(Frame frame, EntityRef entity)
        {
            var gameplay = frame.Unsafe.GetPointerSingleton<Gameplay>();
            
            if (!IsActive) 
                return;
            
            if(!gameplay->CanFight)
                return;

            AIConfig config;
            if (frame.RuntimeConfig.freeMove)
            {
                config = frame.FindAsset<AIConfig>(aiConfigFreeRoam.Id);
            }
            else
            {
                config = frame.FindAsset<AIConfig>(aiConfig.Id);
            }
            
            var playerMovement = frame.Unsafe.GetPointer<PlayerMovement>(entity);
            TargetEnemy = playerMovement->ClosestTarget;

            var aiPlayer = frame.Unsafe.GetPointer<AIPlayer>(entity);
            var targetEnemyAttack = frame.Unsafe.GetPointer<PlayerAttack>(TargetEnemy);
            
            CheckTargetRange(frame, entity, config);
            
            AIDecisionCheck[] decisionChecks = new AIDecisionCheck[] { };
            
            if (aiRange == AIRange.InRange && !aiPlayer->GettingSpammed && !targetEnemyAttack->InRecovery)
            {
                decisionChecks = frame.FindAsset<AIDecisionConfig>(config.inRangeDecision.Id).aiDecisions;
            }
            else if(aiRange == AIRange.OutRange && !aiPlayer->GettingSpammed && !targetEnemyAttack->InRecovery)
            {
                decisionChecks = frame.FindAsset<AIDecisionConfig>(config.outRangeDecision.Id).aiDecisions;
            }
            else if(aiPlayer->GettingSpammed&& !targetEnemyAttack->InRecovery)
            {
                decisionChecks = frame.FindAsset<AIDecisionConfig>(config.spammedDecision.Id).aiDecisions;
            }
            else if(targetEnemyAttack->InRecovery)
            {
                decisionChecks = frame.FindAsset<AIDecisionConfig>(config.playerRecoveringDecision.Id).aiDecisions;
            }
            
            if (!config.setActive)
            {
                //Reset all AI actions
                CanAttack = false;
                CanKick = false;
                IsBlocking = false;
                CanDodge = false;
                
                AiMoveDirection = FPVector3.Zero;
                aiState = AIState.Idle; //
                return;
            }
            
            CheckCoolDown(frame);
            UpdateDecisionLogic(frame, entity, decisionChecks);
            ExecuteDecision(frame, entity, config);
        }
        
        private void CheckTargetRange(Frame frame, EntityRef entity, AIConfig config)
        {
            if (TargetEnemy == EntityRef.None) return;

            var myTransform = frame.Unsafe.GetPointer<Transform3D>(entity);
            var enemyTransform = frame.Unsafe.GetPointer<Transform3D>(TargetEnemy);

            var distance = FPVector3.Distance(myTransform->Position, enemyTransform->Position);
            var newRange = (distance <= config.attackRange) ? AIRange.InRange : AIRange.OutRange;

            if (newRange != previousRange)
            {
                //Force the current action to stop
                DecisionTimer = 10;
                aiState = AIState.Idle;
                previousRange = newRange;
            }

            aiRange = newRange;
        }
        
        //Cool down between attacks
        private void CheckCoolDown(Frame frame)
        {
            if (IsCoolingDown)
            {
                CoolDownTimer += frame.DeltaTime;
                if (CoolDownTimer >= CoolDownDuration)
                {
                    IsCoolingDown = false;
                }
            }
        }
        
        private void UpdateDecisionLogic(Frame frame, EntityRef entity, AIDecisionCheck[] decisionChecks)
        {
            var playerAttack = frame.Unsafe.GetPointer<PlayerAttack>(entity);
            var playerStat = frame.Unsafe.GetPointer<PlayerStat>(entity);
            var playerMovement = frame.Unsafe.GetPointer<PlayerMovement>(entity);
            
            //Can't make decision if cooling down
            if(IsCoolingDown || playerAttack->KnockDown || playerAttack->InRecovery || playerStat->IsDead)
                return;
            
            DecisionTimer += frame.DeltaTime;
            
            if (DecisionTimer >= DecisionDuration && !playerAttack->isAttacking && !playerMovement->IsDodging)
            {
                //Update repeat tracking before selecting new decision
                CheckLastAIState();

                var decision = GetRandomDecision(frame, decisionChecks);
                
                // Only change state if it is different
                if (aiState != decision.aiState)
                {
                    // Reset attack triggers for previous state
                    var ai = frame.Unsafe.GetPointer<AIPlayer>(entity);
                    ai->CanAttack = false;
                    ai->CanKick = false;
                    ai->CanHardAttack = false;
                    ai->CanHardKick = false;
                    
                    playerAttack->ResetCombo(frame, entity);
                    aiState = decision.aiState; // switch state
                }
                
                DecisionDuration = frame.RNG->Next(decision.actionTimeMin, decision.actionTimeMax);

                //Trigger cooldown
                CoolDownDuration = decision.coolDownTime;
                DecisionTimer = 0;
                IsCoolingDown = true;
                CoolDownTimer = 0;
            }
        }
        
        private void ExecuteDecision(Frame frame, EntityRef entity, AIConfig config)
        {
            var playerAttack = frame.Unsafe.GetPointer<PlayerAttack>(entity);
            var playerStat = frame.Unsafe.GetPointer<PlayerStat>(entity);
            var playerMovement = frame.Unsafe.GetPointer<PlayerMovement>(entity);
            
            //Don't want to perform any actions if cooling down
            if (IsCoolingDown || playerAttack->KnockDown || playerAttack->InRecovery || playerStat->IsDead)
            {
                CanAttack = false;
                CanKick = false;
                CanHardAttack = false;
                CanHardKick = false;
                IsBlocking = false;
                CanDodge = false;
                AiMoveDirection = FPVector3.Zero;
                return;
            }
            
            var ai = frame.Unsafe.GetPointer<AIPlayer>(entity);
            var movement = new AIMovement();
            var attack = new AIAttack();
            
            //Check if ai getting spammed
            attack.CheckHit(frame, entity, config);
            
            switch (aiState)
            {
                case AIState.Idle:
                    AiMoveDirection = FPVector3.Zero;
                    CanAttack = false;
                    CanKick = false;
                    CanHardAttack = false;
                    CanHardKick = false;
                    ai->IsBlocking = false;
                    ai->CanDodge = false;
                    ai->DashingForward = false;
                    ai->DashingBack = false;
                    break;

                case AIState.MovingForward:
                    ai->CanDodge = false;
                    ai->DashingForward = false;
                    ai->DashingBack = false;
                    CanAttack = false;
                    CanKick = false;
                    CanHardAttack = false;
                    CanHardKick = false;
                    ai->IsBlocking = false;
                    movement.MoveForward(frame, entity);
                    break;

                case AIState.PunchAttacking:
                    ai->CanDodge = false;
                    ai->IsBlocking = false;
                    ai->DashingForward = false;
                    ai->DashingBack = false;
                    attack.PunchAttack(frame, entity, config);
                    ai->AiMoveDirection = FPVector3.Zero;
                    ai->CanAttack = true;
                    ai->CanKick = false;
                    ai->CanHardAttack = false;
                    ai->CanHardKick = false;
                    break;

                case AIState.BackPunch:
                    ai->CanDodge = false;
                    ai->IsBlocking = false;
                    ai->DashingForward = false;
                    ai->DashingBack = false;
                    attack.PunchAttack(frame, entity, config);
                    movement.MoveBack(frame, entity);
                    ai->CanAttack = true;
                    ai->CanKick = false;
                    ai->CanHardAttack = false;
                    ai->CanHardKick = false;
                    break;
                
                case AIState.BackKick:
                    ai->CanDodge = false;
                    ai->IsBlocking = false;
                    ai->DashingForward = false;
                    ai->DashingBack = false;
                    attack.KickAttack(frame, entity, config);
                    movement.MoveBack(frame, entity);
                    ai->CanAttack = false;
                    ai->CanKick = true;
                    ai->CanHardAttack = false;
                    ai->CanHardKick = false;
                    break;
                
                case AIState.KickAttacking:
                    ai->CanDodge = false;
                    CanAttack = false;
                    ai->IsBlocking = false;
                    ai->DashingForward = false;
                    ai->DashingBack = false;
                    attack.KickAttack(frame, entity, config);
                    ai->AiMoveDirection = FPVector3.Zero;
                    ai->CanKick = true;
                    ai->CanAttack = false;
                    ai->CanHardAttack = false;
                    ai->CanHardKick = false;
                    break;
                    
                case AIState.HardPunching:
                    ai->CanDodge = false;
                    CanAttack = false;
                    ai->IsBlocking = false;
                    ai->DashingForward = false;
                    ai->DashingBack = false;
                    attack.HardAttack(frame, entity, config);
                    ai->AiMoveDirection = FPVector3.Zero;
                    ai->CanHardAttack = true;
                    ai->CanAttack = false;
                    ai->CanKick = false;
                    ai->CanHardKick = false;
                    break;
                
                case AIState.HardKicking:
                    ai->CanDodge = false;
                    CanAttack = false;
                    ai->IsBlocking = false;
                    ai->DashingForward = false;
                    ai->DashingBack = false;
                    attack.HardKick(frame, entity, config);
                    ai->AiMoveDirection = FPVector3.Zero;
                    ai->CanHardKick = true;
                    ai->CanAttack = false;
                    ai->CanKick = false;
                    ai->CanHardAttack = false;
                    break;
                    
                case AIState.MovingBack:
                    ai->CanDodge = true;
                    ai->IsBlocking = false;
                    ai->DashingForward = false;
                    ai->DashingBack = false;
                    CanAttack = false;
                    CanKick = false;
                    CanHardAttack = false;
                    CanHardKick = false;
                    movement.MoveBack(frame, entity);
                    break;

                case AIState.DodgingForwardUp:
                    ai->CanDodge = true;
                    ai->IsBlocking = false;
                    CanAttack = false;
                    CanKick = false;
                    CanHardAttack = false;
                    CanHardKick = false;
                    movement.DodgeForwardUp(frame, entity);
                    break;

                case AIState.DodgeBackUp:
                    ai->CanDodge = true;
                    ai->IsBlocking = false;
                    ai->DashingForward = false;
                    ai->DashingBack = false;
                    CanAttack = false;
                    CanKick = false;
                    CanHardAttack = false;
                    CanHardKick = false;
                    movement.DodgeBackUp(frame, entity);
                    break;
                
                case AIState.DodgingForwardDown:
                    ai->CanDodge = true;
                    ai->IsBlocking = false;
                    ai->DashingForward = false;
                    ai->DashingBack = false;
                    CanAttack = false;
                    CanKick = false;
                    CanHardAttack = false;
                    CanHardKick = false;
                    movement.DodgeForwardDown(frame, entity);
                    break;
                
                case AIState.DodgingBackDown:
                    ai->CanDodge = true;
                    ai->IsBlocking = false;
                    ai->DashingForward = false;
                    ai->DashingBack = false;
                    CanAttack = false;
                    CanKick = false;
                    CanHardAttack = false;
                    CanHardKick = false;
                    movement.DodgeBackDown(frame, entity);
                    break;
                
                case AIState.DashForward:
                    ai->CanDodge = true;
                    ai->IsBlocking = false;
                    CanAttack = false;
                    CanKick = false;
                    CanHardAttack = false;
                    CanHardKick = false;
                    ai->DashingForward = true;
                    ai->DashingBack = false;
                    break;
                
                case AIState.DashBack:
                    ai->CanDodge = true;
                    ai->IsBlocking = false;
                    CanAttack = false;
                    CanKick = false;
                    CanHardAttack = false;
                    CanHardKick = false;
                    ai->DashingBack = true;
                    ai->DashingForward = false;
                    break;
                
                case AIState.Blocking:
                    ai->IsBlocking = true;
                    CanAttack = false;
                    CanKick = false;
                    CanHardAttack = false;
                    CanHardKick = false;
                    movement.StopMove(frame, entity);
                    break;
                
                case AIState.RoamDodgeForward:
                    ai->AIDodge = true;
                    CanAttack = false;
                    CanKick = false;
                    CanHardAttack = false;
                    CanHardKick = false;
                    playerMovement->CardinalDirection = "Forward";
                    break;
                
                case AIState.RoamDodgeBack:
                    ai->AIDodge = true;
                    CanAttack = false;
                    CanKick = false;
                    CanHardAttack = false;
                    CanHardKick = false;
                    playerMovement->CardinalDirection = "Back";
                    break;
                
                case AIState.RoamDodgeLeft:
                    ai->AIDodge = true;
                    CanAttack = false;
                    CanKick = false;
                    CanHardAttack = false;
                    CanHardKick = false;
                    playerMovement->CardinalDirection = "Left";
                    break;
                
                case AIState.RoamDodgeRight:
                    ai->AIDodge = true;
                    CanAttack = false;
                    CanKick = false;
                    CanHardAttack = false;
                    CanHardKick = false;
                    playerMovement->CardinalDirection = "Right";
                    break;
                
                default:
                    CanAttack = false;
                    CanKick = false;
                    CanHardAttack = false;
                    CanHardKick = false;
                    ai->CanDodge = false;
                    ai->IsBlocking = true;
                    break;
            }
        }

        private AIDecisionCheck GetRandomDecision(Frame frame, AIDecisionCheck[] decisionChecks)
        {
            if (decisionChecks == null || decisionChecks.Length == 0)
                return null;

            FP totalChance = 0;
            foreach (var decisionCheck in decisionChecks)
                totalChance += decisionCheck.chance;

            var maxAttempts = decisionChecks.Length;
            var attempts = 0;
            
            while (attempts < maxAttempts)
            {
                var randomValue = frame.RNG->Next(0, totalChance);

                FP cumulative = 0;
                AIDecisionCheck selectedDecision = null;
                foreach (var decisionCheck in decisionChecks)
                {
                    cumulative += decisionCheck.chance;
                    if (randomValue < cumulative)
                    {
                        selectedDecision = decisionCheck;
                        break;
                    }
                }
                
                if (selectedDecision == null)
                    selectedDecision = decisionChecks[decisionChecks.Length - 1];

                // Prevent selecting the same action if it has reached or exceeded its max repeat amount
                if (!(selectedDecision.aiState == LastActionState && ActionRepeatCount >= selectedDecision.repeatAmount))
                {
                    return selectedDecision;
                }
                
                attempts++;
            }

            // If all attempts fail, return the last selected decision anyway
            return decisionChecks[decisionChecks.Length - 1];
        }

        //CHECK REPEAT STATE
        private void CheckLastAIState()
        {
            if (aiState == LastActionState)
            {
                // If the current AI state is the same as the last one, increase the repeat counter
                ActionRepeatCount++;
            }
            else
            {
                // If the AI state has changed, reset the repeat counter to 1
                ActionRepeatCount = 1;
                // Update the last action state to the current one
                LastActionState = aiState;
            }
        }
    }
}
