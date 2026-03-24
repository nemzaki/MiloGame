using Photon.Deterministic;
using Quantum.Scripts.Weapon;
using UnityEngine;

namespace Quantum
{
    public unsafe partial struct PlayerStat
    {
       
        public void TakeDamageAmount(int damage)
        {
            PlayerHealth -= damage;
            HitCounter = 0;
        }

        public void TakeStamina(Frame frame, int stamina)
        {
            if(PlayerStamina < 0 || frame.RuntimeConfig.freeMove)
                return;
            
            PlayerStamina -= stamina;
        }

        private void CheckStamina(Frame frame, EntityRef entity)
        {
            var playerAttack = frame.Unsafe.GetPointer<PlayerAttack>(entity);
            var config = frame.FindAsset<PlayerConfig>(playerAttack->playerConfig.Id);

            if (PlayerStamina < config.maxStamina && !playerAttack->isAttacking)
            {
                PlayerStamina += frame.DeltaTime * config.staminaRecoveryRate;
            }
        }

        private void ResetHitCounter(Frame frame)
        {
            if (HitCounter > 0)
            {
                HitCounterTimer += frame.DeltaTime;
                if (HitCounterTimer >= 2)
                {
                    HitCounter = 0;
                    HitCounterTimer = 0;
                }
            }
        }
        
        
        #region HANDLECONNECTION
        
        //BAD CONNECTION DEFEAT
        private void CheckConnectionDefeat(Frame frame, EntityRef entity)
        {
            var playerMovement = frame.Unsafe.GetPointer<PlayerMovement>(entity);
            var input = playerMovement->InputDesires;
            
            if (input.ConnectionBad)
            {
                HandleDisconnection(frame, entity);
                Debug.Log("Quantum Technical defeat bad connection");
            }
        }
        
        //WHEN PLAYER QUITS
        public void HandleDisconnection(Frame frame, EntityRef entity)
        {
            var gameplay = frame.Unsafe.GetPointerSingleton<Gameplay>();
        
            frame.Events.PlayerLeave(entity);
            Disconnected = true;
            
            gameplay->DisconnectedEntity = entity;
        }
        
        
        #endregion
        
        public void Update(Frame frame, EntityRef entity)
        {
            //GetAIData(frame, entity);
            Intro(frame, entity);
            ResetHealthTraining(frame, entity);
            CheckStamina(frame, entity);
            ResetHitCounter(frame);
            CheckPing(frame, entity);
            
            CheckConnectionDefeat(frame, entity);
            
            TakingHit(frame, entity);
            Recovery(frame, entity);
            ResetShowCelebration(frame, entity);
            Dead(frame, entity);
        }

        //--------------------HEALTH----------------------------
        
        private void Dead(Frame frame, EntityRef entity)
        {
            var playerStats = frame.Unsafe.GetPointer<PlayerStat>(entity);
            var playerAttack = frame.Unsafe.GetPointer<PlayerAttack>(entity);
            
            if (playerStats->IsDead)
            {
                playerAttack->ResetAllAttackState(frame, entity);
            }
        }
        
        #region TakeDamage
        public void TakeDamage(Frame frame, EntityRef entity, EntityRef hitBoxEntity, EntityRef sourceEntity, FPVector3 hitForce, 
            FP hitTime, string hitReactionName, bool knockDown, FP knockDownRecoveryTime, FP hitCrossFadeTime,
            bool ignoreHitReaction, FP hitStopTimeAttacker, FP hitStopTimeVictim, int damageAmount, int staminaAmount, string attackType)
        {
            var playerAttack = frame.Unsafe.GetPointer<PlayerAttack>(entity);
            var playerMovement = frame.Unsafe.GetPointer<PlayerMovement>(entity);
            var sourcePlayerAttack = frame.Unsafe.GetPointer<PlayerAttack>(sourceEntity);
            var playerStat = frame.Unsafe.GetPointer<PlayerStat>(entity);
            var enemyPlayerStat = frame.Unsafe.GetPointer<PlayerStat>(sourceEntity);
            
            // Skip if player is in recovery or already knocked down
            if (playerAttack->InRecovery || playerAttack->KnockDown || 
                playerAttack->NoDamageAttack || playerStat->IsDead)
            {
                return;
            }
            
            //CHECK AVOID HIT
            if(playerMovement->CheckSuccessfullyAvoidedHit())
                return;
            
            // Skip if player is blocking AND it's not a knockdown
            if (playerAttack->IsBlocking && !knockDown)
            {
                frame.Events.PlayerBlockClip(entity);
                return;
            }
            
            //DEAD
            if (playerStat->PlayerHealth <= 0)
            {
                playerStat->IsDead = true;
            }
            
            //HIT COUNTER
            enemyPlayerStat->HitCounter++;
            enemyPlayerStat->HitCounterTimer = 0;
            
            // -- DAMAGE APPLIES BELOW --
            
            //HIT FX ------------------------->
            if (frame.Unsafe.TryGetPointer<HitBox>(hitBoxEntity, out var hitBox))
            {
                var hitBoxTransform = frame.Unsafe.GetPointer<Transform3D>(hitBoxEntity);
                
                if (!hitBox->ShowHitFX)
                {
                    frame.Events.PlayerHitFX(hitBox->SourceEntity, hitBoxTransform->Position, hitBox->HitType);
                    hitBox->ShowHitFX = true;
                }
                
                //----------------FEELS------------------
            
                //HARD HITS
                if (hitBox->HitType == "Heavy")
                {
                    foreach (var dynamicProp in frame.Unsafe.GetComponentBlockIterator<DynamicProp>())
                    {
                        dynamicProp.Component->FeedBackForce(frame, dynamicProp.Entity);
                    }
                }
                //---------------------------------------
            }
            
            //Cancel all current attack
            playerAttack->ResetAllAttackState(frame, entity);
            
            //Player hit sound fx
            frame.Events.PlayerHitClip(entity);
            
            //Update attack type counter
            UpdateAttackTypeInfo(enemyPlayerStat, attackType);
            
            //REDUCE HEALTH
            var playerStats = frame.Unsafe.GetPointer<PlayerStat>(entity);
            
            //Take Damage
            playerStats->TakeDamageAmount(damageAmount);
            
            //Reset hit timer if got hit again
            if (playerAttack->GotHit)
            {
                playerAttack->GotHitTimer = 0;
                
                //HIT STOP
                playerAttack->HitStopTimer = 0;
                sourcePlayerAttack->HitStopTimer = 0;
            }

            //HIT STOP
            playerAttack->HitStopTime = hitStopTimeVictim;
            sourcePlayerAttack->HitStopTime = hitStopTimeAttacker;
            
            sourcePlayerAttack->HitStop = true;
            playerAttack->HitStop = true;
            //
            
            playerAttack->isAttacking = false;
            playerAttack->GotHit = true;
            playerAttack->HitMoveForce = hitForce;
            
            playerAttack->KnockDown = knockDown;
            playerAttack->RecoveryTime = knockDownRecoveryTime;
            
            if (!ignoreHitReaction)
            {
                //Normal Hit
                frame.Events.PlayerHit(entity, sourceEntity, hitReactionName, hitCrossFadeTime);
            }
            
            //Hit Reaction
            gotHitTime = hitTime;

            //Do this for now
            IsAiming = true;
        }

        //Get combo type info
        private void UpdateAttackTypeInfo(PlayerStat* playerStat, string attackType)
        {
            switch (attackType)
            {
                case nameof(AttackTypeName.Punch):
                    playerStat->PunchAmount += 1;
                    break;
                case nameof(AttackTypeName.Kick):
                    playerStat->KickAmount += 1;
                    break;
                case nameof(AttackTypeName.PunchHard):
                    playerStat->PunchHardAmount += 1;
                    break;
                case nameof(AttackTypeName.KickHard):
                    playerStat->KickHardAmount += 1;
                    break;
            }
        }
        
        private void ResetShowCelebration(Frame frame, EntityRef entity)
        {
            var playerStats = frame.Unsafe.GetPointer<PlayerStat>(entity);
            
            if(!playerStats->ShowCelebration)
                return;
            
            playerStats->ShowCelebrateTimer += frame.DeltaTime;

            if (playerStats->ShowCelebrateTimer >= 1)
            {
                playerStats->ShowCelebration = false;
                playerStats->ShowCelebrateTimer = 0;
            }
        }
        
        private void TakingHit(Frame frame, EntityRef entity)
        {
            var playerAttack = frame.Unsafe.GetPointer<PlayerAttack>(entity);

            if (playerAttack->GotHit)
            {
                playerAttack->GotHitTimer += frame.DeltaTime;

                if (playerAttack->GotHitTimer >= gotHitTime)
                {
                    if (playerAttack->KnockDown)
                    {
                        playerAttack->InRecovery = true;
                    }
                    
                    playerAttack->GotHit = false;
                    playerAttack->GotHitTimer = 0;
                }
            }
        }

        private void Recovery(Frame frame, EntityRef entity)
        {
            var playerAttack = frame.Unsafe.GetPointer<PlayerAttack>(entity);

            if (playerAttack->InRecovery)
            {
                playerAttack->RecoveryTimer += frame.DeltaTime;

                if (playerAttack->RecoveryTimer >= playerAttack->RecoveryTime)
                {
                    playerAttack->InRecovery = false;
                    playerAttack->KnockDown = false;
                    playerAttack->RecoveryTimer = 0;
                }
            }
        }
        
        #endregion
        
        #region ROUNDS
        //--------------------ROUNDS-----------------------------
        
        //TIME OUT
        public void CheckRoundEndTimeOut(Frame frame, EntityRef entity)
        {
            var gameplay = frame.Unsafe.GetPointerSingleton<Gameplay>();
            if (gameplay->SuddenDeath)
                return;

            var playerMovement = frame.Unsafe.GetPointer<PlayerMovement>(entity);
            if (playerMovement->ClosestTarget == EntityRef.None)
                return;

            var enemyEntity = playerMovement->ClosestTarget;
            var enemyStat = frame.Unsafe.GetPointer<PlayerStat>(enemyEntity);

            // SAME HEALTH → Sudden Death (handled elsewhere)
            if (PlayerHealth == enemyStat->PlayerHealth)
                return;

            // ONLY the winner executes logic
            if (PlayerHealth > enemyStat->PlayerHealth)
            {
                ShowCelebration = true;
                RoundsWon += 1;

                enemyStat->IsDead = true;
                frame.Events.Dead(enemyEntity);
                Debug.Log(entity + " WON");
            }
        }
        
        //KO
        public void CheckRoundEndKo(Frame frame, EntityRef entity)
        {
            var gameplay = frame.Unsafe.GetPointerSingleton<Gameplay>();

            // Sudden Death already resolved elsewhere
            if (gameplay->SuddenDeath)
                return;

            if (!IsDead)
            {
                ShowCelebration = true;
                RoundsWon += 1;
            }
        }
        
        #endregion

        #region STATS
        //--------------------STATS-----------------------------
        private void CheckPing(Frame frame, EntityRef entity)
        {
            var playerMovement = frame.Unsafe.GetPointer<PlayerMovement>(entity);
            var playerInput = playerMovement->InputDesires;
            var gameplay = frame.Unsafe.GetPointerSingleton<Gameplay>();
            var gameConfig = frame.FindAsset<GameConfig>(gameplay->gameConfig.Id);

            BadPing = playerInput.PlayerPing >= gameConfig.badPingRange;
        }
        
        private void ResetHealthTraining(Frame frame, EntityRef entity)
        {
            var playerAttack = frame.Unsafe.GetPointer<PlayerAttack>(entity);

            var config = frame.FindAsset<PlayerConfig>(playerAttack->playerConfig.Id);

            if (PlayerHealth <= 50 && frame.RuntimeConfig.training)
                PlayerHealth = config.maxHealth;
        }
        #endregion
        
        private void Intro(Frame frame, EntityRef entity)
        {
            var gameplay = frame.Unsafe.GetPointerSingleton<Gameplay>();
            
            if (!gameplay->InitiateFight) 
                return;
            
            ShowedIntro = true;

            if (ShowedIntro)
            {
                ShowIntroTimer += frame.DeltaTime;

                if (ShowIntroTimer >= 1)
                {
                    ShowedIntro = false;
                }
            }
        }

        public void SetData(Frame frame, EntityRef entity)
        {
            var gameplay = frame.Unsafe.GetPointerSingleton<Gameplay>();
            var playerMovement = frame.Unsafe.GetPointer<PlayerMovement>(entity);
            var playerAttack = frame.Unsafe.GetPointer<PlayerAttack>(entity);
            
            playerAttack->AttackComboList = frame.AllocateList<int>(4);
            playerAttack->KickComboList = frame.AllocateList<int>(4);
            
            if (playerMovement->PlayerType == EPlayerType.Player)
            {
                var playerData = frame.GetPlayerData(playerMovement->PlayerRef);
                
                currentWeaponID = playerData.currentWeaponID;
                MoveType = playerData.moveType;
                playerSkinIndex = playerData.currentPlayerIndex;
                CelebrateType = playerData.celebrateType;
                
                playerAttack->IdleType = playerData.idleType;
                PlayerName = playerData.nickname;
                
                //PUNCH
                var attackComboList = frame.ResolveList(playerAttack->AttackComboList);
                attackComboList.Add(playerData.attackCombo1);
                attackComboList.Add(playerData.attackCombo2);
                attackComboList.Add(playerData.attackCombo3);
                attackComboList.Add(playerData.attackCombo4);

                //KICK
                var kickComboList = frame.ResolveList(playerAttack->KickComboList);
                kickComboList.Add(playerData.kickCombo1);
                kickComboList.Add(playerData.kickCombo2);
                kickComboList.Add(playerData.kickCombo3);
                kickComboList.Add(playerData.kickCombo4);
            }
            else if(playerMovement->PlayerType == EPlayerType.AI)
            {
                var allWeaponConfig = frame.FindAsset<AllWeaponsConfig>(gameplay->allWeaponsConfig.Id);
                var currentWeapon = frame.FindAsset<WeaponConfig>(allWeaponConfig.weaponData[currentWeaponID].Id);
                
                var aiAttack = frame.Unsafe.GetPointer<PlayerAttack>(entity);
                var aiPlayer = frame.Unsafe.GetPointer<AIPlayer>(entity);
                
                //SET AI DATA
                //MoveType = playerData.moveType;
					
                var allWeaponsConfig = frame.FindAsset<AllWeaponsConfig>(gameplay->allWeaponsConfig.Id);
                playerSkinIndex = frame.RNG->Next(0, allWeaponsConfig.playerSkinMax);
                aiPlayer->AIIdleType = frame.RNG->Next(0, 0);
                aiPlayer->AIHardPunchType = frame.RNG->Next(0, currentWeapon.hardAttacks.Length);
                aiPlayer->AIHardKickType = frame.RNG->Next(0, currentWeapon.hardKickAttacks.Length);

                aiPlayer->AIName = gameplay->AINameGenerator(frame);
                PlayerName = aiPlayer->AIName;
                aiAttack->IdleType = aiPlayer->AIIdleType;
                
                //PUNCH
                var attackComboList = frame.ResolveList(playerAttack->AttackComboList);
                attackComboList.Add(0);
                attackComboList.Add(1);
                attackComboList.Add(2);
                attackComboList.Add(3);

                //KICK
                var kickComboList = frame.ResolveList(playerAttack->KickComboList);
                kickComboList.Add(0);
                kickComboList.Add(1);
                kickComboList.Add(2);
                kickComboList.Add(3);
            }
        }
    }
}