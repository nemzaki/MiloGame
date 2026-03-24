using System;
using Photon.Deterministic;
using Quantum.Scripts.Weapon;
using UnityEngine;

namespace Quantum
{
    public unsafe partial struct PlayerAttack
    {
        //
        public void OnInit(Frame frame, EntityRef entity)
        {
            //Initialize combo entry
            QueuedComboEntry = ComboEntryType.Normal;
        }
        
        #region MeleeAttack
        
        //Melee Attack
        
        //STARTS THE ATTACK
        private void StartAttackInput(Frame frame, EntityRef entity, WeaponConfig currentWeapon, 
            bool attackPressed, bool kickPressed, bool attackHardPressed, bool kickHardPressed)
        { 
            var movement = frame.Unsafe.GetPointer<PlayerMovement>(entity);
            var aiPlayer = frame.Unsafe.GetPointer<AIPlayer>(entity);
            
            didAttack = attackPressed || kickPressed || attackHardPressed || kickHardPressed;

            //CHAIN COMMANDS - NOTE SHOULD BE QUEUED
            if (movement->InputDesires.SweepKick)
            {
                QueuedComboEntry = ComboEntryType.SweepKick;
            }
            
            //DECIDE FORWARD UP OR BACK OR SPECIAL ATTACKS BEFORE
            if (!isAttacking && didAttack)
            {
                ForwardUpAttackAtStart = movement->CurrentDodgeDir == DodgeDir.ForwardUp 
                                         || (movement->CurrentDodgeDir == DodgeDir.Forward && frame.RuntimeConfig.freeMove);
                BackAttackAtStart = movement->CurrentDodgeDir == DodgeDir.Back;
            }
            
            var isNewCombo =
                (attackPressed && CurrentComboType != ComboType.Punch) ||
                (kickPressed && CurrentComboType != ComboType.Kick) ||
                (attackHardPressed && CurrentComboType != ComboType.PunchHard) ||
                (kickHardPressed && CurrentComboType != ComboType.KickHard);

            if (isNewCombo)
            {
                //queue the new attack if the attack changed
                QueueAttackInput(attackPressed, kickPressed, attackHardPressed, kickHardPressed);
                ResetComboRequest = true;
                currentComboLength = 0;
            }

            //Check if the attack type changes while attacking
            //like switching from punch -> kick
            if (ResetComboRequest && !isAttacking)
            {
                ResetCombo(frame, entity);
                ResetComboRequest = false;
            }

            //Check did attack while attacking
            //If chain combo prevent queuing attacks
            if (isAttacking && didAttack && isNewCombo && !IsSweepKicking)
            {
                QueueComboChange = true;
            }
            
            //When attack type is switch then queue the new attack
            //Like switching from punching to kicking
            if (!isAttacking && QueueComboChange)
            {
                getAttackData = false;
                CurrentComboType = QueuedComboType;
                HardAttack = QueueHardAttack;
                didAttack = true;
                QueueComboChange = false;
            }
            
            // Early return: Prevent starting new attacks if already started one
            if (isAttacking)
                return;
            
            // Apply attack immediately
            ApplyAttackInput(attackPressed, kickPressed, attackHardPressed, kickHardPressed);
            
            aiPlayer->CanAttack = false;
        }
        
        private void StartAttack(EntityRef entity,Frame frame, bool punchAttack, bool kickAttack, 
            bool punchHardAttack, bool kickHardAttack)
        {
            var attackComboList = frame.ResolveList(AttackComboList);
            var kickComboList = frame.ResolveList(KickComboList);
            
            //
            ComboResetTimer = 0;
            
            DidMeleeAttack = true;
            isAttacking = true;
            comboQueued = false;
            currentFrame = 0;
            hitBoxTriggered = false;
            blockBoxTriggered = false;
            AttackStartFrame = frame.Number;

            IsForwardUpAttacking = ForwardUpAttackAtStart;
            IsBackAttacking = BackAttackAtStart;
            IsSweepKicking = QueuedComboEntry == ComboEntryType.SweepKick;
            QueuedComboEntry = ComboEntryType.Normal;
            
            if (punchAttack)
            {
                CurrentComboType = ComboType.Punch;
                     
                currentComboLength += 1;
                
                currentComboIndex = attackComboList[currentComboIncrement];
            }
            else if (kickAttack)
            {
                CurrentComboType = ComboType.Kick;
                
                currentComboLength += 1;

                currentComboIndex = kickComboList[currentComboIncrement];
            }
            else if (punchHardAttack)
            {
                CurrentComboType = ComboType.PunchHard;

                //RESET COOLDOWN
                IsCoolingDown = false;
                IsCoolingDownTimer = 0;

                currentComboIndex = 0;
            }
            else if (kickHardAttack)
            {
                CurrentComboType = ComboType.KickHard;
                
                //RESET COOLDOWN
                IsCoolingDown = false;
                IsCoolingDownTimer = 0;

                currentComboIndex = 0;
            }
            
            if (punchAttack || punchHardAttack)
            {
                frame.Events.MeleeAttack(entity, currentComboIndex);
            }
            else if (kickAttack || kickHardAttack)
            {
                frame.Events.KickMeleeAttack(entity, currentComboIndex);
            }
        }
        
        //GETS THE CURRENT ATTACK TYPE
        private void ApplyAttackInput(bool attackPressed, bool kickPressed, bool attackHardPressed, bool kickHardPressed)
        {
            if (attackPressed)
            {
                CurrentComboType = ComboType.Punch;
                HardAttack = false;
            }
            else if (kickPressed)
            {
                CurrentComboType = ComboType.Kick;
                HardAttack = false;
            }
            else if (attackHardPressed)
            {
                CurrentComboType = ComboType.PunchHard;
                HardAttack = true;
            }
            else if (kickHardPressed)
            {
                CurrentComboType = ComboType.KickHard;
                HardAttack = true;
            }

            getAttackData = false;
        }
        
        //QUEUE PENDING ATTACK INPUTS
        private void QueueAttackInput(bool attackPressed, bool kickPressed, bool attackHardPressed, bool kickHardPressed)
        {
            if (attackPressed)
            {
                QueuedComboType = ComboType.Punch;
                QueueHardAttack = false;
            }
            else if (kickPressed)
            {
                QueuedComboType = ComboType.Kick;
                QueueHardAttack = false;
            }
            else if (attackHardPressed)
            {
                QueuedComboType = ComboType.PunchHard;
                QueueHardAttack = true;
                
                //RESET COOLDOWN
                IsCoolingDown = false;
                IsCoolingDownTimer = 0;
            }
            else if (kickHardPressed)
            {
                QueuedComboType = ComboType.KickHard;
                QueueHardAttack = true;
                
                //RESET COOLDOWN
                IsCoolingDown = false;
                IsCoolingDownTimer = 0;
            }

            //getAttackData = false;
        }
        
        //SETS ATTACK DATA CONTROLS CURRENT CURRENT ATTACK
        private void InAttack(Frame frame, EntityRef entity)
        { 
            var playerMovement = frame.Unsafe.GetPointer<PlayerMovement>(entity);
            var playerStats = frame.Unsafe.GetPointer<PlayerStat>(entity);  
            
            //Reset attack rate
            //Use to prevent registering multiple attacks
            //Each frame
            if (DidMeleeAttack)
            {
                MeleeAttackTimer += frame.DeltaTime;
                if (MeleeAttackTimer >= FP._0_10)
                {
                    DidMeleeAttack = false;
                    MeleeAttackTimer = 0;
                }
            }

            if (isAttacking)
            {
                currentFrame++;

                if (!getAttackData)
                {
                    //Play swing clip audio
                    frame.Events.PlayerSwingClip(entity);
                    
                    SetAttackData(frame, entity);

                    //Convert FP time to frame 
                    //frame = time * framerate // How to get start and end frames

                    var attackData = frame.FindAsset(NormalAttack).attackData;
                    
                    startFrame = attackData.startFrame * 60;
                    endFrame = attackData.endFrame * 60;
                    hitBoxFrame = attackData.hitBoxFrame * 60;
                    PriorityFrame = attackData.priorityFrame * 60;
                    
                    //ENEMY CHECK CAN AVOID ATTACK ------
                    var enemyMovement = frame.Unsafe.GetPointer<PlayerMovement>(playerMovement->ClosestTarget);
                    enemyMovement->CheckCanDodgeAvoidHit();
                    //-----------------------------------
                    
                    getAttackData = true;
                }

                //Queue Attack
                if (didAttack && !DidMeleeAttack && currentFrame >= startFrame && currentFrame <= endFrame
                    && !playerMovement->IsCrouching && !HardAttack && !IsBackAttacking && !IsForwardUpAttacking
                    && !IsSweepKicking && !ResetComboRequest)
                {
                    comboQueued = true;
                }

                //UNINTERRUPTIBLE ATTACK
                if (currentFrame >= PriorityFrame && PriorityFrame != 0)
                {
                    NoDamageAttack = true;
                }
                
                //TRIGGER HIT BOX
                if (!hitBoxTriggered && currentFrame >= hitBoxFrame)
                {
                    var attackData = frame.FindAsset(NormalAttack).attackData;
                    
                    ShowHitBox(frame, entity, attackData.hitMoveForce,
                        frame.FindAsset(NormalAttack).attackData.hitTime,
                        false, attackData.hitReactionName.ToString(),
                        attackData.blockHitReactionName.ToString(), attackData.knockDown,
                        attackData.knockDownRecoveryTime, attackData.hitCrossFadeTime,
                        attackData.hitStopTimeAttacker, attackData.hitStopTimeVictim, attackData.hitBox,
                        attackData.damageAmount, attackData.staminaAmount,
                        attackData.attackType.ToString(), attackData.hitBoxType,
                        attackData.hitType);
                    
                    //REDUCE STAMINA
                    playerStats->TakeStamina(frame, frame.FindAsset(NormalAttack).attackData.staminaAmount);
                    
                    hitBoxTriggered = true;
                }

                //TRIGGER BLOCK BOX
                if (!blockBoxTriggered && currentFrame >= startFrame)
                {
                    var attackData = frame.FindAsset(NormalAttack).attackData;
                    
                    ShowHitBox(frame, entity, attackData.hitMoveForce,
                        attackData.hitTime,
                        true, attackData.hitReactionName.ToString(),
                        attackData.blockHitReactionName.ToString(),
                        attackData.knockDown, attackData.knockDownRecoveryTime,
                        attackData.hitCrossFadeTime,attackData.hitStopTimeAttacker, attackData.hitStopTimeVictim,
                        attackData.hitBox, attackData.damageAmount,
                        attackData.staminaAmount, attackData.attackType.ToString(),
                        attackData.hitBoxType, attackData.hitType);
                    
                    blockBoxTriggered = true;
                }
                
                //End of attack
                if (currentFrame > endFrame)
                {
                    IncrementComboIndex(frame);
                    getAttackData = false;

                    IsBackAttacking = false;
                    IsForwardUpAttacking = false;
                    IsSweepKicking = false;
                    
                    NoDamageAttack = false;
                    
                    if (QueuedComboEntry == ComboEntryType.SweepKick)
                    {
                        CurrentComboType = ComboType.SweepKick;
                        
                        //Stops all current attacks
                        didAttack = false;
                        comboQueued = false;
                        QueueComboChange = false;
                        ResetComboRequest = false;
                        
                        StartAttack(entity, frame,
                            punchAttack: false,
                            kickAttack: false,
                            punchHardAttack: false,
                            kickHardAttack: false);
                    }
                    else if (comboQueued)
                    {
                        StartAttack(entity, frame, CurrentComboType == ComboType.Punch,
                            CurrentComboType == ComboType.Kick,
                            CurrentComboType == ComboType.PunchHard,
                            CurrentComboType == ComboType.KickHard);
                    }
                    else
                    {
                        DidCrouchAttack = false;
                        isAttacking = false; // Let combo index persist for possible continuation
                        currentFrame = 0;
                        comboQueued = false;
                        hitBoxTriggered = false;
                        blockBoxTriggered = false;
                    }
                }
            }
            else
            {
                if (didAttack && !DidMeleeAttack)
                {
                    StartAttack(entity, frame, CurrentComboType == ComboType.Punch,
                        CurrentComboType == ComboType.Kick,
                        CurrentComboType == ComboType.PunchHard, CurrentComboType == ComboType.KickHard);

                    ComboResetTimer = 0; // Reset timer since player continued combo
                }
            }


            // Combo timeout logic
            if (!isAttacking && currentComboIncrement > 0)
            {
                ComboResetTimer += frame.DeltaTime;
                if (ComboResetTimer >= 1)
                {
                    ResetCombo(frame, entity);
                    ComboResetTimer = 0;
                }
            }
        }
        
        private void IncrementComboIndex(Frame frame)
        {
            //Chain Combos should not increment combo index
            if (CurrentComboType == ComboType.SweepKick)
            {
                currentComboIncrement = 0;
                return;
            }
            
            var attackComboList = frame.ResolveList(AttackComboList);
            var kickComboList = frame.ResolveList(KickComboList);
            
            // Advance combo index if possible
            if (currentComboIncrement < 3)
            {
                currentComboIncrement++;
            }
            else
            {
                currentComboIncrement = 0;
                
                if (CurrentComboType == ComboType.Punch)
                {
                    currentComboIndex = attackComboList[0];
                }
                
                if(CurrentComboType == ComboType.Kick)
                {
                    currentComboIndex = kickComboList[0];
                }
            }
        }

        //SETS ATTACK DATA FIGHT STYLE, ATTACK TYPE
        private void SetAttackData(Frame frame, EntityRef entity)
        {
            var gameplay = frame.Unsafe.GetPointerSingleton<Gameplay>();
            var playerStat = frame.Unsafe.GetPointer<PlayerStat>(entity);
            
            var allWeaponsConfig = frame.FindAsset<AllWeaponsConfig>(gameplay->allWeaponsConfig.Id);
            var currentWeapon = frame.FindAsset<WeaponConfig>(allWeaponsConfig.weaponData[playerStat->currentWeaponID].Id);
            var playerMovement = frame.Unsafe.GetPointer<PlayerMovement>(entity);
            var playerData = frame.GetPlayerData(playerMovement->PlayerRef);

            var aiPlayer = frame.Unsafe.GetPointer<AIPlayer>(entity);
            var playerStats = frame.Unsafe.GetPointer<PlayerStat>(entity);
            
            
            //GET FIGHT DATA
            var hardPunchType = 0;
            var hardKickType = 0;
            
            switch (playerMovement->PlayerType)
            {
                case EPlayerType.Player:
                    hardPunchType = playerData.hardPunchType;
                    hardKickType = playerData.hardKickType;
                    break;
                case EPlayerType.AI:
                    hardPunchType = aiPlayer->AIHardPunchType;
                    hardKickType = aiPlayer->AIHardKickType;
                    break;
                case EPlayerType.None:  
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            //
            if (playerMovement->Grounded)
            {
                switch (CurrentComboType)
                {
                    case ComboType.Punch:
                        HandleNormalAttack(frame, entity, playerMovement, currentWeapon, playerData, isPunch: true);
                        break;

                    case ComboType.Kick:
                        HandleNormalAttack(frame, entity, playerMovement, currentWeapon, playerData, isPunch: false);
                        break;

                    case ComboType.PunchHard:
                        NormalAttack = frame.FindAsset<NormalAttackConfig>(currentWeapon.hardAttacks[hardPunchType].Id);
                     
                        HardPunchType = frame.FindAsset(NormalAttack).attackData.hardPunchType;
                        
                        frame.Events.AttackName(entity,
                            frame.FindAsset<NormalAttackConfig>(NormalAttack.Id).attackData.animationName);
                        
                        GetAttackMoveForce(frame, entity);
                        break;

                    case ComboType.KickHard:
                        NormalAttack =
                            frame.FindAsset<NormalAttackConfig>(currentWeapon.hardKickAttacks[hardKickType].Id);
                        HardKickType = frame.FindAsset(NormalAttack).attackData.hardKickType;
                        
                        frame.Events.AttackName(entity,
                            frame.FindAsset<NormalAttackConfig>(NormalAttack.Id).attackData.animationName);
                        
                        GetAttackMoveForce(frame, entity);
                        break;
                    
                    case ComboType.SweepKick:
                        NormalAttack = frame.FindAsset<NormalAttackConfig>(currentWeapon.sweepKickData.Id);
                        
                        frame.Events.AttackName(entity,
                            frame.FindAsset<NormalAttackConfig>(NormalAttack.Id).attackData.animationName);
                        
                        GetAttackMoveForce(frame, entity);
                        break;
                }
                
                //CHECK STAMINA
                if (frame.FindAsset(NormalAttack).attackData.staminaAmount > playerStats->PlayerStamina)
                {
                    NoStamina = true;
                    frame.Events.NoStamina(entity);
                }
            }
            else
            {
                //attackData = frame.FindAsset<NormalAttackConfig>(allWeaponsConfig.jumpAttacks[0].Id).attackData;
                if (CurrentComboType is ComboType.Punch or ComboType.KickHard)
                {
                    NormalAttack = frame.FindAsset<NormalAttackConfig>(currentWeapon.jumpPunchAttacks[0].Id);
                }
                else if(CurrentComboType is ComboType.Kick or ComboType.KickHard)
                {
                    NormalAttack = frame.FindAsset<NormalAttackConfig>(currentWeapon.jumpKickAttacks[0].Id);
                }

                AttackMoveForce = FPVector3.Zero;
            }
        }
        
        //GETS THE ATTACK DATA ASSET
        private void HandleNormalAttack(Frame frame, EntityRef entity, PlayerMovement* playerMovement, 
            WeaponConfig weaponConfig, RuntimePlayer playerData, bool isPunch)
        {
            NormalAttackConfig attackConfig;
            
            if (playerMovement->IsCrouching)//Crouch attacks
            {
                attackConfig = isPunch
                    ? frame.FindAsset<NormalAttackConfig>(weaponConfig.lowPunchAttack[0].Id)
                    : frame.FindAsset<NormalAttackConfig>(weaponConfig.lowKickAttack[0].Id);
                
                DidCrouchAttack = true;
            }
            else if (BackAttackAtStart)//Back attacks
            {
                attackConfig = isPunch
                    ? frame.FindAsset<NormalAttackConfig>(weaponConfig.punchBackAttacks[0].Id)
                    : frame.FindAsset<NormalAttackConfig>(weaponConfig.kickBackAttacks[0].Id);

                BackAttackAtStart = false;
            }
            else if(ForwardUpAttackAtStart)//Forward-Up attacks
            {
                attackConfig = isPunch
                    ? frame.FindAsset<NormalAttackConfig>(weaponConfig.forwardUpAttacks[0].Id)
                    : frame.FindAsset<NormalAttackConfig>(weaponConfig.forwardUpKicks[0].Id);

                ForwardUpAttackAtStart = false;
                
                //Stop dodging
                playerMovement->ResetDodge();
            }
            else //Normal ground attacks
            {
                if (isPunch)
                {
                    attackConfig = frame.FindAsset<NormalAttackConfig>(weaponConfig.attacks[currentComboIndex].Id);
                }
                else
                {
                    attackConfig = frame.FindAsset<NormalAttackConfig>(weaponConfig.kicks[currentComboIndex].Id);
                }
                
                
            }

            //attackData = attackConfig.attackData;
            NormalAttack = attackConfig;
            
            frame.Events.AttackName(entity,
                frame.FindAsset<NormalAttackConfig>(NormalAttack.Id).attackData.animationName);
            
            GetAttackMoveForce(frame, entity);
        }
        
        
        //RESET STAMINA
        //NEEDS MORE WORK
        private void ResetStamina(Frame frame, EntityRef entity)
        {
            var playerStats = frame.Unsafe.GetPointer<PlayerStat>(entity);
            
            if(!NoStamina)
                return;

            if (frame.FindAsset(NormalAttack).attackData.staminaAmount < playerStats->PlayerStamina)
            {
                NoStamina = false;
            }
        }
        
        private void GetAttackMoveForce(Frame frame, EntityRef entity)
        {
            var playerMovement = frame.Unsafe.GetPointer<PlayerMovement>(entity);
            var kcc = frame.Unsafe.GetPointer<KCC>(entity);
            
            if(!playerMovement->Grounded)
                return;
            
            var kccForward = kcc->Data.LookRotation * FPVector3.Forward;
            var kccUp = FPVector3.Up * frame.FindAsset(NormalAttack).attackData.actionMoveForce.Y;
            AttackMoveForce = (kccForward + kccUp) * frame.FindAsset(NormalAttack).attackData.actionMoveForce.Z;
        }
        
        //Slight move force when attacking
        private void AttackForce(Frame frame, EntityRef entity)
        {
            var playerMovement = frame.Unsafe.GetPointer<PlayerMovement>(entity);
            var playerTransform = frame.Unsafe.GetPointer<Transform3D>(entity);
            var enemyTransform = frame.Unsafe.GetPointer<Transform3D>(playerMovement->ClosestTarget);
            
            var kcc = frame.Unsafe.GetPointer<KCC>(entity);
            var config = frame.FindAsset<PlayerConfig>(playerConfig.Id);
            
            //DISTANCE
            var toEnemy = enemyTransform->Position - playerTransform->Position;
            toEnemy.Y = 0;
            var distance = toEnemy.Magnitude;
            
            if(!isAttacking || !playerMovement->Grounded)
                return;

            if (!frame.RuntimeConfig.freeMove || distance > config.freeFlowRange)
            {
                if (isAttacking && playerMovement->Grounded)
                {
                    kcc->AddExternalForce(AttackMoveForce);
                }
            }
            else
            {
                //FREE FLOW COMBAT
                if (hitBoxTriggered)
                    return;
                
                if (distance <= 1)
                    return;

                var direction = toEnemy.Normalized;
                var attractionStrength = config.attractionStrength;

                var attractionForce = direction * attractionStrength;
                kcc->AddExternalForce(attractionForce);
            }
        }

        private void HitForce(Frame frame, EntityRef entity)
        {
            var kcc = frame.Unsafe.GetPointer<KCC>(entity);
            var playerStat = frame.Unsafe.GetPointer<PlayerStat>(entity);
            var playerMovement = frame.Unsafe.GetPointer<PlayerMovement>(entity);
            
            if (GotHit && !playerStat->IsDead)
            {
                //Stop Current Dodge
                playerMovement->ResetDodge();
                
                var kccHitDirection = kcc->Data.LookRotation * -FPVector3.Forward * HitMoveForce.Z;
                var kccHitUp = FPVector3.Up * HitMoveForce.Y;

                var hitForce = (kccHitDirection + kccHitUp);
                
                //Force should be in the opposite direction box
                kcc->AddExternalForce(hitForce);
                //kcc->AddExternalForce(-transform->Forward * HitMoveForce.Z);
            }
        }

        private void CheckHitStop(Frame frame, EntityRef entity)
        {
            if (!HitStop) 
                return;
            
            var kcc = frame.Unsafe.GetPointer<KCC>(entity);
            kcc->Data.DynamicVelocity = FPVector3.Zero;
            
            HitStopTimer += frame.DeltaTime;
            if (HitStopTimer >= HitStopTime)
            {
                HitStop = false;
                HitStopTimer = 0;
            }
        }

        public void ResetAllAttackState(Frame frame, EntityRef entity)
        {
            ResetCombo(frame, entity);
            didAttack = false;
            QueueComboChange = false;
            blockBoxTriggered = false;
            IsBackAttacking = false;
            hitBoxTriggered = false;
            IsForwardUpAttacking = false;
            ForwardUpAttackAtStart = false;
            BackAttackAtStart = false;
            QueuedComboEntry = ComboEntryType.Normal;
            IsSweepKicking = false;
            HardAttack = false;
        }
        
        public void ResetCombo(Frame frame, EntityRef entity)
        {
            var playerMovement = frame.Unsafe.GetPointer<PlayerMovement>(entity);

            var attackComboList = frame.ResolveList(AttackComboList);
            var kickComboList = frame.ResolveList(KickComboList);
            
            if (CurrentComboType == ComboType.Punch)
            {
                currentComboIndex = attackComboList[0];
            }
            else if(CurrentComboType == ComboType.Kick)
            {
                currentComboIndex = kickComboList[0];
            }
            
            currentComboIncrement = 0;
            
            isAttacking = false;
            currentFrame = 0;
            comboQueued = false;
            hitBoxTriggered = false;
            CurrentComboType = ComboType.None;
            DidCrouchAttack = false;
            QueuedComboEntry = ComboEntryType.Normal;
            
            playerMovement->ResetDashing();

            if (frame.RuntimeConfig.freeMove && playerMovement->IsDodging)
            {
                playerMovement->ResetDodge();
            }
        }
        
        private void ResetAIState(Frame frame, EntityRef entity)
        {
            var playerMovement = frame.Unsafe.GetPointer<PlayerMovement>(entity);
            
            if (KnockDown || InRecovery)
            {
                //aiState = AIState.Idle; // reset to neutral
                playerMovement->ResetDodge();
                isAttacking = false;
                ResetCombo(frame, entity);
                IsBackAttacking = false;
            }
        }

        //COMBO COOLDOWN
        private void ComboCooldown(Frame frame, EntityRef entity, PlayerConfig playerConfig)
        {
            if (currentComboLength >= 3)
            {
                IsCoolingDown = true;
                
                //Reset attack-related flags so nothing auto-triggers
                didAttack = false;
                comboQueued = false;
                QueueComboChange = false;
            }

            if (!IsCoolingDown) 
                return;
            
            IsCoolingDownTimer += frame.DeltaTime;
            if (IsCoolingDownTimer >= playerConfig.comboCooldownDuration)
            {
                currentComboLength = 0;
                IsCoolingDown = false;
                IsCoolingDownTimer = 0;
            }
        }
        
        
        //Hit box to do damage
        private void ShowHitBox(Frame frame, EntityRef entity, FPVector3 hitForce, FP hitTime, 
            bool isBlockBox, string hitReactionName, string blockReactionName, bool knockDown, FP recoveryTime,
            FP hitCrossFadeTime, FP hitStopTimeAttacker, FP hitStopTimeVictim, AssetRef<EntityPrototype> hitBoxAsset,
            int damageAmount, int staminaAmount, string attackType, HitBoxType hitBoxType,
            HitType hitType)
        {
            var playerTransform = frame.Unsafe.GetPointer<Transform3D>(entity);
            var hitBoxEntity = frame.Create(hitBoxAsset);
            var hitBox = frame.Unsafe.GetPointer<HitBox>(hitBoxEntity);
            var playerMovement = frame.Unsafe.GetPointer<PlayerMovement>(entity);
            var playerStat = frame.Unsafe.GetPointer<PlayerStat>(entity);
            
            hitBox->IsActive = true;
            hitBox->SourceEntity = entity;
            hitBox->GotHitTime = hitTime;
            hitBox->HitForce = hitForce;
            hitBox->IsHitBox = !isBlockBox;
            hitBox->IsBlockBox = isBlockBox;
            hitBox->HitReactionName = hitReactionName;
            hitBox->BlockReactionName = blockReactionName;
            hitBox->KnockDown = knockDown;
            hitBox->RecoveryTime = recoveryTime;
            hitBox->HitCrossFadeTime = hitCrossFadeTime;
            hitBox->HitStopTimeAttacker = hitStopTimeAttacker;
            hitBox->HitStopTimeVictim = hitStopTimeVictim;
            hitBox->DamageAmount = damageAmount;
            hitBox->StaminaAmount = staminaAmount;
            hitBox->AttackTypeInfo = attackType;
            hitBox->currentWeaponID = playerStat->currentWeaponID; 
            hitBox->HitType = hitType.ToString();
            
            var hitBoxTransform = frame.Unsafe.GetPointer<Transform3D>(hitBoxEntity);
            var config = frame.FindAsset<PlayerConfig>(playerConfig.Id);
            
            // Calculate forward offset position
            var forwardOffset = playerTransform->Rotation * 
                                (FPVector3.Forward * GetHitBoxType(config, hitBoxType).hitBoxForwardOffset) * GetHitBoxType(config, hitBoxType).hitBoxOffset;

            // Final spawn position = player's current position + forward offset
            if (!playerMovement->IsCrouching)
            {
                hitBoxTransform->Position = playerTransform->Position + forwardOffset + GetHitBoxType(config, hitBoxType).hitBoxSpawnPos;
            }
            else
            {
                hitBoxTransform->Position = playerTransform->Position + forwardOffset + config.crouchHitBox.hitBoxSpawnPos;
            }
            
            // Optional: Match player rotation
            hitBoxTransform->Rotation = playerTransform->Rotation;
        }

        private HitBoxSettings GetHitBoxType(PlayerConfig config, HitBoxType hitBoxType)
        {
            if (hitBoxType == HitBoxType.High)
            {
                return config.highHitBox;
            }
            
            if(hitBoxType == HitBoxType.Mid)
            {
                return config.midHitBox;
            }
            
            if (hitBoxType == HitBoxType.Low)
            {
                return config.lowHitBox;
            }

            //Default
            return config.highHitBox;
        }
        #endregion
        
        private bool CanAttackAndDodge(Frame frame, EntityRef entity)
        {
            var playerMovement = frame.Unsafe.GetPointer<PlayerMovement>(entity);

            if (!frame.RuntimeConfig.freeMove)
            {
                // Only prevent attacks if a "real" dodge is active
                if (playerMovement->IsDodging && playerMovement->CurrentDodgeDir != DodgeDir.ForwardUp)
                    return false;
            }
    
            if (frame.RuntimeConfig.freeMove)
            {
                // Allow forward and forward-up dodges
                if (playerMovement->CurrentDodgeDir == DodgeDir.Forward|| playerMovement->CurrentDodgeDir != DodgeDir.ForwardUp)
                    return true;

                // Any other dodge blocks attacks
                if (playerMovement->IsDodging)
                    return false;
            }

            return true; // Otherwise allow attacking
        }
        
        #region Block Systemisd

        private void Block(Frame frame, EntityRef entity)
        {
            var playerMovement = frame.Unsafe.GetPointer<PlayerMovement>(entity);
            var input = playerMovement->InputDesires;
            var aiPlayer = frame.Unsafe.GetPointer<AIPlayer>(entity);

            if (((playerMovement->PlayerType == EPlayerType.Player && input.PlayerBlock) ||
                 (playerMovement->PlayerType == EPlayerType.AI && aiPlayer->IsBlocking)) &&
                !KnockDown && !InRecovery && !isAttacking)
            {
                IsBlocking = true;
            }
            else
            {
                IsBlocking = false;
            }
        }
        
        #endregion
        //USE WEAPON AND OTHER ITEMS
        private void UseItemAction(Frame frame, EntityRef entity, PlayerConfig config, InputDesires inputDesires, PlayerMovement* playerMovement, WeaponConfig currentWeapon)
        {
            var kcc = frame.Unsafe.GetPointer<KCC>(entity);
            var aiPlayer = frame.Unsafe.GetPointer<AIPlayer>(entity);
            var playerStat = frame.Unsafe.GetPointer<PlayerStat>(entity);
            
            var aimingDirection = AimingUtility.GetTargetHitPosition(frame, playerMovement->InputDesires.CameraPosition, kcc);
            var gameplay = frame.Unsafe.GetPointerSingleton<Gameplay>();
            
            if (currentWeapon.melee && !GotHit && !KnockDown && !InRecovery && !gameplay->ResettingRound
                && gameplay->CanFight && CanAttackAndDodge(frame, entity) && !IsCoolingDown && !playerStat->IsDead
                && !playerMovement->IsDashing && !NoStamina && !HitWall)
            {
                if (playerMovement->PlayerType == EPlayerType.Player)
                {
                    StartAttackInput(frame, entity, currentWeapon, inputDesires.PlayerAttack, inputDesires.PlayerKick, 
                        inputDesires.PlayerAttackHard, inputDesires.PlayerKickHard);
                }
                else if(playerMovement->PlayerType == EPlayerType.AI)
                {
                    StartAttackInput(frame, entity, currentWeapon, aiPlayer->CanAttack, aiPlayer->CanKick, 
                        aiPlayer->CanHardAttack, aiPlayer->CanHardKick);
                }
            }
            
            Draw.Line(aimingDirection.Position, aimingDirection.Forward * 500);
        }
        
        
        public void Update(Frame frame, EntityRef entity)
        {
            var config = frame.FindAsset<PlayerConfig>(playerConfig.Id);
            var playerMovement = frame.Unsafe.GetPointer<PlayerMovement>(entity);
            var inputDesires = playerMovement->InputDesires;
            
            var gameplay = frame.Unsafe.GetPointerSingleton<Gameplay>();
            
            var playerStat = frame.Unsafe.GetPointer<PlayerStat>(entity);
            var allWeaponsConfig = frame.FindAsset<AllWeaponsConfig>(gameplay->allWeaponsConfig.Id);
            var currentWeapon = frame.FindAsset<WeaponConfig>(allWeaponsConfig.weaponData[playerStat->currentWeaponID].Id);
            
            playerStat->Update(frame, entity);
            
            UseItemAction(frame, entity, config, inputDesires, playerMovement, currentWeapon);
            InAttack(frame, entity);
            Block(frame, entity);
            AttackForce(frame, entity);
            HitForce(frame, entity);
            
            CheckHitStop(frame, entity);
            ResetAIState(frame, entity);
            ComboCooldown(frame, entity, config);
            ResetStamina(frame, entity);
        }
    }
}
