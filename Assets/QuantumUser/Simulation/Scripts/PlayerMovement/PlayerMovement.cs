using System;
using Photon.Deterministic;
using Quantum.Prototypes;
using Quantum.Scripts.Config;
using Quantum.Scripts.Weapon;
using UnityEngine;

namespace Quantum
{
    public unsafe partial struct PlayerMovement
    {

        private static bool InDanger;
        
        public void OnInit(Frame frame, EntityRef entity)
        {
            
        }
        
        
        public void Teleport(Frame frame, EntityRef entity, Transform3D* position)
        {
            var myTransform = frame.Unsafe.GetPointer<Transform3D>(entity);
            myTransform->Position = position->Position;
            myTransform->Rotation = position->Rotation;
        }
        
        private void ProcessInput(Frame frame, EntityRef entity)
        {
            InputDesires.Clear();

            if (PlayerType == EPlayerType.AI)
            {
                //Do nothing with ai for now
            }
            else if (PlayerRef.IsValid)
            {
                InputDesires.CopyFromInput(frame.GetPlayerInput(PlayerRef));
                
                var inputCommand = frame.GetPlayerCommand(PlayerRef) as IInputCommand;
                if (inputCommand != null)
                {
                    inputCommand.Process(frame, entity, ref InputDesires);
                }
            }
        }
        
        public void Update(Frame frame, EntityRef entity)
        {
            //GET COMPONENTS
            var playerAttack = frame.Unsafe.GetPointer<PlayerAttack>(entity);
            var config = frame.FindAsset<PlayerConfig>(playerAttack->playerConfig.Id);
            var transform = frame.Unsafe.GetPointer<Transform3D>(entity);
            var kcc = frame.Unsafe.GetPointer<KCC>(entity);
            var aiPlayer = frame.Unsafe.GetPointer<AIPlayer>(entity);
           
            
            CheckLockAxis(frame, transform);
            ProcessInput(frame, entity);
            Sprint(frame, entity, config);
            AutoSprint(frame, entity, config);
            LockOnTarget(frame, entity, config);
            
            CheckCanMove(frame, entity);
            CheckCrouch(frame, entity);
            Strafe(frame, entity);
            GetMovementDirection(frame, entity);
            DistanceToEnemy(frame, entity);
            CamViewRot(frame, entity, config);
            Dodge(frame, entity);
            SetMovementType(frame, aiPlayer);
            HandleRotation(frame, entity, playerAttack, transform, kcc, config);
            CheckFightStance(frame, entity);
            CheckHitWall(frame, entity);
            RecoverFromKnockDown(frame, entity);
            HandleDodgeAvoidHit(frame, entity);
            
            //JUMP
            if (CurrentDodgeDir == DodgeDir.Up && kcc->IsGrounded && !StopMove && !IsDodging
                && !playerAttack->isAttacking)
            {
                //kcc->Jump(FPVector3.Up * config.jumpForce);
            }
            
            //Grounded
            Grounded = kcc->IsGrounded;
            
            //
            TargetAimDir = AimingUtility.GetTargetDirection(frame, transform, kcc);
        }

        private void CheckLockAxis(Frame frame, Transform3D* transform)
        {
            if (!frame.RuntimeConfig.freeMove)
            {
                // Keep player locked on Z=0
                var position = transform->Position;
                position.Z = 0;
                transform->Position = position;
            }

        }

        private void CheckCanMove(Frame frame, EntityRef entity)
        {
            var playerAttack = frame.Unsafe.GetPointer<PlayerAttack>(entity);
            var gameplay = frame.Unsafe.GetPointerSingleton<Gameplay>();
            var config = frame.FindAsset<PlayerConfig>(playerAttack->playerConfig.Id);
            var kcc = frame.Unsafe.GetPointer<KCC>(entity);
            
            if (!StopMove && !playerAttack->isAttacking && !IsDodging && !playerAttack->GotHit && !playerAttack->IsBlocking
                && !playerAttack->KnockDown && !playerAttack->InRecovery && !playerAttack->HitWall
                && CurrentDodgeDir != DodgeDir.Up && CurrentDodgeDir != DodgeDir.Down && !gameplay->ResettingRound && gameplay->CanFight
                && !playerAttack->HitStop)
            {
                Move(frame, MoveDirection, kcc, config);
            }
        }
        
        //SET MOVEMENT TYPE
        private void SetMovementType(Frame frame, AIPlayer* aiPlayer)
        {
            //Street Fight movement
            if (PlayerType == EPlayerType.Player)
            {
                if (!frame.RuntimeConfig.freeMove)
                {
                    MoveDirection = new FPVector3(InputDesires.MoveDirection.X, 0, 0);
                }
                else
                {
                    MoveDirection = InputDesires.MoveDirection.XOY;
                }
            }
            else if(PlayerType == EPlayerType.AI)
            {
                MoveDirection = aiPlayer->AiMoveDirection.Normalized;
            }
        }
        
        //ROTATION
        private void HandleRotation(Frame frame, EntityRef entity, PlayerAttack* playerAttack, 
            Transform3D* transform, KCC* kcc, PlayerConfig config)
        {
            //GTA TYPE ROTATION
            var playerStat = frame.Unsafe.GetPointer<PlayerStat>(entity);
            
            if (MoveDirection != default && !playerStat->IsAiming && !playerAttack->GotHit && !playerAttack->IsBlocking 
                && !playerAttack->InRecovery && !playerAttack->KnockDown && !playerAttack->HitWall && !playerAttack->HitStop) 
            {
                FPVector3 faceDirection = default;
                
                if (!frame.RuntimeConfig.freeMove)
                {
                    faceDirection = new FPVector3(MoveDirection.X, 0, 0); // purely left/right
                }
                else if(frame.RuntimeConfig.freeMove)
                {
                    faceDirection = new FPVector3(MoveDirection.X, 0, MoveDirection.Z);
                }
                
                // Prevent zero-length vector
                if (faceDirection.SqrMagnitude > 0)
                {
                    var toRotation = FPQuaternion.LookRotation(faceDirection.Normalized, FPVector3.Up);
                    var lookDir = FPQuaternion.Slerp(transform->Rotation, toRotation, frame.DeltaTime * config.rotationSpeed);
                    kcc->Data.SetLookRotation(lookDir);
                }
            }

        }
        
        //
        private void Sprint(Frame frame, EntityRef entity, PlayerConfig config)
        {
            if(frame.RuntimeConfig.freeMove)
                return;
            
            if (InputDesires.PlayerSprint || CurrentDodgeDir == DodgeDir.ForwardUp
                || CurrentDodgeDir == DodgeDir.BackUp)
            {
                Speed = config.sprintSpeed;
            }
            else
            {
                Speed = config.normalSpeed;
            }
            
            Sprinting = InputDesires.PlayerSprint;
        }

        private void AutoSprint(Frame frame, EntityRef entity, PlayerConfig config)
        {
            if(!frame.RuntimeConfig.freeMove)
                return;

            if(ClosestTarget == EntityRef.None)
                return;
            
            var transform = frame.Unsafe.GetPointer<Transform3D>(entity);
            var enemyTransform = frame.Unsafe.GetPointer<Transform3D>(ClosestTarget);

            var distanceBetweenTargets = FPVector3.Distance(transform->Position, enemyTransform->Position);

            if (distanceBetweenTargets > config.autoSprintRange)
            {
                Speed = config.sprintSpeed;
                RunningSpeed = 1;
            }
            else
            {
                Speed = config.normalSpeed;
                RunningSpeed = 0;
            }
        }
        
        private void Strafe(Frame frame, EntityRef entity)
        {
            if(ClosestTarget == EntityRef.None)
                return;

            var playerStat = frame.Unsafe.GetPointer<PlayerStat>(entity);
            var playerAttack = frame.Unsafe.GetPointer<PlayerAttack>(entity);
            var config = frame.FindAsset<PlayerConfig>(playerAttack->playerConfig.Id);
            var transform = frame.Unsafe.GetPointer<Transform3D>(entity);
            var enemyTransform = frame.Unsafe.GetPointer<Transform3D>(ClosestTarget);

            var dist = FPVector3.Distance(transform->Position, enemyTransform->Position);

            if (dist <= config.isStrafingRange)
            {
                playerStat->IsAiming = true;
            }
            else
            {
                playerStat->IsAiming = false;
            }
        }
        
        private void Move(Frame frame, FPVector3 moveDirection, KCC* kcc, PlayerConfig config)
        {
            FP acceleration;

            //Prevent movement if stop movement is true
            if (!frame.RuntimeConfig.freeMove)
            {
                if (StopMovement)
                {
                    if (isFacingRight && moveDirection.X < 0)
                    {
                        moveDirection.X = 0; // Block backward when facing right
                    }
                    else if (!isFacingRight && moveDirection.X > 0)
                    {
                        moveDirection.X = 0; // Block backward when facing left
                    }
                }
            }

            if (moveDirection == FPVector3.Zero)
            {
                // No desired move velocity - we are stopping.
                acceleration = kcc->IsGrounded ? config.groundDeceleration : config.airDeceleration;
            }
            else
            {
                acceleration = kcc->IsGrounded ? config.groundAcceleration : config.airAcceleration;
            }

            var velocity = moveDirection * acceleration * (Speed * frame.DeltaTime);

            if (!frame.RuntimeConfig.freeMove)
            {
                // Lock Z-axis
                velocity.Z = 0;
            }

            kcc->Data.KinematicVelocity = velocity;
        }

        private void CheckCrouch(Frame frame, EntityRef entity)
        {
            var col = frame.Unsafe.GetPointer<PhysicsCollider3D>(entity);
            var playerAttack = frame.Unsafe.GetPointer<PlayerAttack>(entity);
            var config = frame.FindAsset<PlayerConfig>(playerAttack->playerConfig.Id);
            
            if(frame.RuntimeConfig.freeMove)
                return;
            
            if (CurrentDodgeDir == DodgeDir.Down)
            {
                IsCrouching = true;
                MoveState = 2;

                //For quantum capsule height is calculated differently
                col->Shape.Capsule.Extent = config.capsuleHeightCrouching;
                col->Shape.Centroid = new FPVector3(0, config.capsuleCenterCrouching, 0);
            }
            else
            {
                IsCrouching = false;
                MoveState = 1;

                col->Shape.Capsule.Extent = config.capsuleHeightStanding;
                col->Shape.Centroid = new FPVector3(0, config.capsuleCenterStanding, 0);
            }
        }
        
        private void LockOnTarget(Frame frame, EntityRef entity, PlayerConfig config)
        {
            var kcc = frame.Unsafe.GetPointer<KCC>(entity);
            var playerAttack = frame.Unsafe.GetPointer<PlayerAttack>(entity);
            var myTransform = frame.Unsafe.GetPointer<Transform3D>(entity);
            var closestDistance = FP.MaxValue;
            
            //Find the closest target
            foreach (var playerPair in frame.Unsafe.GetComponentBlockIterator<PlayerMovement>())
            {
                if (playerPair.Entity == entity)
                    continue;//Skip self, but continue checking others

                var targetTransform = frame.Unsafe.GetPointer<Transform3D>(playerPair.Entity);
                var directionToTarget = targetTransform->Position - myTransform->Position;
                var distanceToTarget = directionToTarget.Magnitude;

                if (distanceToTarget <= 70 && distanceToTarget < closestDistance)
                {
                    closestDistance = distanceToTarget;
                    ClosestTarget = playerPair.Entity;
                }
            }
            
            if(ClosestTarget == EntityRef.None)
                return;
            
            // Look at the closest target
            var targetPos = frame.Unsafe.GetPointer<Transform3D>(ClosestTarget);
            var direction = (targetPos->Position - myTransform->Position).Normalized;
            var targetRotation = FPQuaternion.LookRotation(direction, FPVector3.Up);
            
            var lookRotation = FPQuaternion.Slerp(myTransform->Rotation, targetRotation,
                frame.DeltaTime * config.rotationSpeed);
            kcc->Data.SetLookRotation(lookRotation);
        }
        
        private void GetMovementDirection(Frame frame, EntityRef entity)
        {
            if(frame.RuntimeConfig.freeMove)
                return;
            
            var transform = frame.Unsafe.GetPointer<Transform3D>(entity);
            
            isFacingRight = FPVector3.Dot(transform->Forward, FPVector3.Right) > 0;
        }

        private void DistanceToEnemy(Frame frame, EntityRef entity)
        {
            if (ClosestTarget == EntityRef.None)
                return;

            if(frame.RuntimeConfig.freeMove)
                return;
            
            var gameplay = frame.Unsafe.GetPointerSingleton<Gameplay>();
            var gameplayConfig = frame.FindAsset<GameConfig>(gameplay->gameConfig.Id);
            
            var transform = frame.Unsafe.GetPointer<Transform3D>(entity);
            var enemyTransform = frame.Unsafe.GetPointer<Transform3D>(ClosestTarget);

            var distance = FPVector3.Distance(transform->Position, enemyTransform->Position);

            if (distance >= gameplayConfig.maxDistanceBetweenPlayers)
            {
                StopMovement = true;
            }
            else
            {
                StopMovement = false;
            }
        }
        
        #region Dodge
        private void Dodge(Frame frame, EntityRef entity)
        {
            var playerAttack = frame.Unsafe.GetPointer<PlayerAttack>(entity);

            if (playerAttack->KnockDown) 
                return;
            
            if (!frame.RuntimeConfig.freeMove)
            {
                NormalDodgeSystem(frame, entity);
            }
            else if (frame.RuntimeConfig.freeMove)
            {
                FreeRoamDodgeSystem(frame, entity);
            }
        }
        
        private void NormalDodgeSystem(Frame frame, EntityRef entity)
        {
            var transform = frame.Unsafe.GetPointer<Transform3D>(entity);
            var kcc = frame.Unsafe.GetPointer<KCC>(entity);
            var playerAttack = frame.Unsafe.GetPointer<PlayerAttack>(entity);
            var config = frame.FindAsset<PlayerConfig>(playerAttack->playerConfig.Id);
            var ai = frame.Unsafe.GetPointer<AIPlayer>(entity);
            var playerStat = frame.Unsafe.GetPointer<PlayerStat>(entity);

            var input = InputDesires;
            
            var gameplay = frame.Unsafe.GetPointerSingleton<Gameplay>();
            
            if (PlayerType == EPlayerType.Player)
            {
                GetMoveDirection(frame, entity, InputDesires.MoveDirection.X,
                    InputDesires.MoveDirection.Y);
            }
            else if (PlayerType == EPlayerType.AI)
            {
                GetMoveDirection(frame, entity, MoveDirection.X, MoveDirection.Y);
            }
            
            if (Grounded && !IsDodging && !gameplay->ResettingRound
                && gameplay->CanFight && !playerAttack->GotHit && !playerAttack->InRecovery && !IsDashing
                && !playerAttack->HitWall)
            {
                //Has to be set this way so dodge stops with the timer
                //Dash is different from dodging
                //DASH -------------------------->
                if (input.PlayerDodgeForward || ai->DashingForward)
                {
                    DashForward = true;
                    IsDashing = true;
                    DodgeDirection = transform->Forward * config.dashForce;
                }
                else if (input.PlayerDodgeBack || ai->DashingBack)
                {
                    DashBack = true;
                    IsDashing = true;
                    DodgeDirection = transform->Back * config.dashForce;
                }

                //DODGE ------------------------->
                if (CurrentDodgeDir == DodgeDir.BackDown)
                {
                    DodgeDirection = transform->Back * config.dodgeForce;
                    IsDodging = true;
                }
                else if (CurrentDodgeDir == DodgeDir.ForwardDown)
                {
                    DodgeDirection = transform->Forward * config.dodgeForce;
                    IsDodging = true;
                }
                else if (CurrentDodgeDir == DodgeDir.BackUp)
                {
                    MoveDirection = transform->Back;
                    DodgeRunning = 1;
                }
                else if (CurrentDodgeDir == DodgeDir.ForwardUp)
                {
                    MoveDirection = transform->Forward;

                    //Only sprint forward for now in free move mode
                    DodgeRunning = 1;
                }
                else
                {
                    DodgeRunning = 0;
                }
            }

            //DODGE
            if (IsDodging && Grounded && !playerStat->IsDead)
            {
                IsDodgingTimer += frame.DeltaTime;

                kcc->AddExternalForce(DodgeDirection);

                DodgeDuration = config.dodgeDuration;

                if (IsDodgingTimer >= DodgeDuration)
                {
                    ResetDodge();
                }
            }

            //DASH
            if (IsDashing && Grounded && !playerStat->IsDead && !playerAttack->GotHit && !playerAttack->HitWall)
            {
                IsDashingTimer += frame.DeltaTime;

                kcc->AddExternalForce(DodgeDirection);

                DodgeDuration = config.dashDuration;

                if (IsDashingTimer >= DodgeDuration)
                {
                    ResetDodge();
                }
            }
        }
        
        private void FreeRoamDodgeSystem(Frame frame, EntityRef entity)
        {
            var transform = frame.Unsafe.GetPointer<Transform3D>(entity);
            var kcc = frame.Unsafe.GetPointer<KCC>(entity);
            var playerAttack = frame.Unsafe.GetPointer<PlayerAttack>(entity);
            var config = frame.FindAsset<PlayerConfig>(playerAttack->playerConfig.Id);
            var ai = frame.Unsafe.GetPointer<AIPlayer>(entity);

            var input = InputDesires;
            
            //todo
            //create a short window when new enemy attack starts
            //Within that window the player can dodge //cancels current hit
            //Pass that window if the hit lands player cant dodge
            
            if ((input.PlayerDodge || ai->AIDodge) 
                && !IsDodging && !playerAttack->InRecovery && !playerAttack->HitWall
                && !playerAttack->GotHit)
            {
                playerAttack->isAttacking = false;

                //if there's a current dodge stop it
                //TODO update this later
                if (IsDodging)
                {
                    ResetDodge();
                }

                IsDodging = true;
                
                //Get Cardinal Direction Only For Player
                FPVector3 inputDirectionVec3 = default;
                if (PlayerType == EPlayerType.Player)
                {
                    inputDirectionVec3 = new FPVector3(input.MoveDirection.X, 0, input.MoveDirection.Y);
                    PlayerRelativeDirection(frame, entity, inputDirectionVec3);
                }

                if (CardinalDirection == "Forward")
                {
                    DodgeDirection = transform->Forward;
                    CurrentDodgeDir = DodgeDir.Forward;
                }
                else if (CardinalDirection == "Back")
                {
                    DodgeDirection = transform->Back;
                    CurrentDodgeDir = DodgeDir.Back;
                }
                else if (CardinalDirection == "Left" || CardinalDirection == "Right")
                {
                    if (inputDirectionVec3.Magnitude > FP._0_01)
                    {
                        DodgeDirection = inputDirectionVec3;
                    }
                    else
                    {
                        // fallback if input is zero
                        DodgeDirection = (CardinalDirection == "Left") ? transform->Left : transform->Right;
                    }
                    
                    if (CardinalDirection == "Left")
                    {
                        CurrentDodgeDir = DodgeDir.Left;
                    }

                    if (CardinalDirection == "Right")
                    {
                        CurrentDodgeDir = DodgeDir.Right;
                    }
                }
                
                frame.Events.PlayerDodge(entity, CardinalDirection);
            }
            
            //CHECK DODGING
            if (IsDodging && !playerAttack->HitWall)
            {
                if(ClosestTarget == EntityRef.None)
                    return;
                    
                var target = frame.Unsafe.GetPointer<Transform3D>(ClosestTarget);
                
                IsDodgingTimer += frame.DeltaTime;
                
                FPVector3 dodgeDirection;
                
                if (CurrentDodgeDir == DodgeDir.Left || CurrentDodgeDir == DodgeDir.Right)
                {
                    // Direction to target
                    var toTarget = (target->Position - transform->Position).Normalized;

                    // Orbit direction (right if DodgeRight, left if DodgeLeft)
                    var tangent = FPVector3.Cross(FPVector3.Up, toTarget);
                    if (CurrentDodgeDir == DodgeDir.Left)
                        tangent = -tangent;

                    // Add curvature
                    var curveAmount = config.curveAmount; // e.g. 0.25
                    var radial = toTarget;
                    var curvedDirection = (tangent + radial * curveAmount).Normalized;
                    
                    dodgeDirection = curvedDirection.Normalized * config.roamDodgeForce;
                }
                else if (CurrentDodgeDir == DodgeDir.Forward)
                {
                    dodgeDirection = transform->Forward * config.roamDodgeForce;
                }
                else // DodgeBack
                {
                    dodgeDirection = transform->Back * config.roamDodgeForce;
                }

                kcc->AddExternalForce(dodgeDirection);

                if (IsDodgingTimer >= config.dodgeDurationRoam)
                {
                    ai->AIDodge = false;
                    ResetDodge();
                }
            }
        }
        
        private void GetMoveDirection(Frame frame, EntityRef entity, FP horizontal, FP vertical)
        {
            var playerAttack = frame.Unsafe.GetPointer<PlayerAttack>(entity);

            if (playerAttack->KnockDown)
            {
                CurrentDodgeDir = DodgeDir.None;
                return;
            }
               
            
            var threshold = FP._0_10;

            bool forward, back;
            if (isFacingRight)
            {
                forward = horizontal > threshold;
                back = horizontal < -threshold;
            }
            else
            {
                back = horizontal > threshold;
                forward = horizontal < -threshold;
            }

            var up = vertical > threshold;
            var down = vertical < -threshold;

            // NEW: reduce diagonal sensitivity
            if ((forward || back) && (up || down))
            {
                if (FPMath.Abs(horizontal) > FPMath.Abs(vertical) * FP._1_50) // make horizontals dominate
                {
                    up = false;
                    down = false;
                }
                else if (FPMath.Abs(vertical) > FPMath.Abs(horizontal) * FP._1_50) // make verticals dominate
                {
                    forward = false;
                    back = false;
                }
            }
            
            if (forward && up && !playerAttack->DidCrouchAttack)
            {
                CurrentDodgeDir = DodgeDir.ForwardUp;
            }

            if (forward && down && Grounded && !playerAttack->isAttacking && !playerAttack->DidCrouchAttack)
            {
                CurrentDodgeDir = DodgeDir.ForwardDown;
            }

            if (back && up && !playerAttack->DidCrouchAttack)
            {
                CurrentDodgeDir = DodgeDir.BackUp;
            }

            if (back && down && Grounded && !playerAttack->isAttacking && !playerAttack->DidCrouchAttack
                && !StopMovement)
            {
                CurrentDodgeDir = DodgeDir.BackDown;
            }

            if (forward && !up && !down && !playerAttack->DidCrouchAttack)
            {
                CurrentDodgeDir = DodgeDir.Forward;
            }

            if (back && !up && !down && !playerAttack->DidCrouchAttack)
            {
                CurrentDodgeDir = DodgeDir.Back;
            }

            if (!up && !down && !forward && !back && !playerAttack->DidCrouchAttack)
            {
                CurrentDodgeDir = DodgeDir.None;
            }

            if (up && !forward && !back && !playerAttack->DidCrouchAttack && !StopMovement)
            {
                CurrentDodgeDir = DodgeDir.Up;
            }

            if (down && !forward && !back && !playerAttack->isAttacking)
            {
                CurrentDodgeDir = DodgeDir.Down;
            }
            
            //Force crouch attack to finish
            if (playerAttack->DidCrouchAttack)
            {
                CurrentDodgeDir = DodgeDir.Down;
            }
        }

        //DODGE AVOID HIT
        public void CheckCanDodgeAvoidHit()
        {
            CanDodgeAvoidHit = true;
            CanDodgeAvoidHitTimer = 0;
        }

        private void HandleDodgeAvoidHit(Frame frame, EntityRef entity)
        {
            var playerAttack = frame.Unsafe.GetPointer<PlayerAttack>(entity);
            var playerConfig = frame.FindAsset<PlayerConfig>(playerAttack->playerConfig.Id);

            if (!CanDodgeAvoidHit) 
                return;
            
            CanDodgeAvoidHitTimer += frame.DeltaTime;

            if (CanDodgeAvoidHitTimer >= playerConfig.canDodgeAvoidHitTime)
            {
                CanDodgeAvoidHit = false;
                CanDodgeAvoidHitTimer = 0;
            }
        }

        public bool CheckSuccessfullyAvoidedHit()
        {
            //CHECK AVOIDED DODGE
            if (IsDodging && CanDodgeAvoidHit)
            {
                // SUCCESSFUL AVOID
                CanDodgeAvoidHit = false;
                
                return true;
            }
            
            return false;
        }
        
        //DASHING DIFFERENT FROM DODGING
        //DASHING CAN TAKE HIT
        public void ResetDashing()
        {
            IsDashing = false;
            IsDashingTimer = 0;
            DashForward = false;
            DashBack = false;
        }
        
        public void ResetDodge()
        {
            IsDodging = false;
            IsDashing = false;
            IsDodgingTimer = 0;
            IsDashingTimer = 0;
            
            CurrentDodgeDir = DodgeDir.None;
        }

        #region  FreeMoveDodge
        private void PlayerRelativeDirection(Frame frame, EntityRef entity, FPVector3 dir)
        {
            var transform = frame.Unsafe.GetPointer<Transform3D>(entity);
            
            var forward = FPVector3.Normalize(FPVector3.ProjectOnPlane(transform->Forward, FPVector3.Up));
            var right   = FPVector3.Normalize(FPVector3.ProjectOnPlane(transform->Right, FPVector3.Up));
          
            CardinalDirection = GetCardinalDirection(dir, forward, right);
        }
        
        private string GetCardinalDirection(FPVector3 inputDir, FPVector3 playerForward, FPVector3 playerRight)
        {
            var forwardDot = FPVector3.Dot(inputDir, playerForward);
            var rightDot = FPVector3.Dot(inputDir, playerRight);
            
            // Use thresholds to determine direction
            if (FPMath.Abs(forwardDot) < FPMath.Abs(rightDot))
            {
                if (rightDot > FP._0_50)
                {
                    return "Right";
                }
                else
                {
                    return "Left";
                   
                }
            }
            else
            {
                if (forwardDot > FP._0_50)
                {
                    return "Forward";
                }
                else
                {
                    return "Back";
                }
            }
        }

        private void RecoverFromKnockDown(Frame frame, EntityRef entity)
        {
            var playerAttack = frame.Unsafe.GetPointer<PlayerAttack>(entity);
            var playerConfig = frame.FindAsset<PlayerConfig>(playerAttack->playerConfig.Id);
            
            if (!playerAttack->KnockDown || playerAttack->HitWall)
                return;

            // Dodge input is now recovery input
            if (InputDesires.PlayerDodge)
            {
                if (!playerAttack->InRecovery && playerAttack->GotHitTimer >= playerConfig.timeToKnockDownRecover)
                {
                    playerAttack->KnockDown = false;
                    playerAttack->InRecovery = true;

                    //HARD reset movement & dodge state
                    ResetDodge();

                    //Shorter recovery time when recovered from knockdown
                    playerAttack->RecoveryTimer = 1;
                    frame.Events.AttackName(entity, "KnockDownRecover");
                }
                else if(playerAttack->InRecovery && playerAttack->RecoveryTimer < playerConfig.timeToGroundRecover)
                {
                    playerAttack->KnockDown = false;
                    playerAttack->InRecovery = true;

                    //HARD reset movement & dodge state
                    ResetDodge();

                    //Shorter recovery time when recovered from knockdown
                    playerAttack->RecoveryTimer = 1;
                    
                    frame.Events.AttackName(entity, "GroundRecover");
                }
                
                //Todo 
                //Check if lying on face or back
            }
        }
        
        #endregion

        #endregion
        
        private void CamViewRot(Frame frame, EntityRef entity, PlayerConfig config)
        {
            if(!frame.RuntimeConfig.freeMove)
                return;
            
            var transform = frame.Unsafe.GetPointer<Transform3D>(entity);
            var enemyTransform = frame.Unsafe.GetPointer<Transform3D>(ClosestTarget);

            var distance = FPVector3.Distance(transform->Position, enemyTransform->Position);
            
            //Normalise the value because lerp only goes from 0 to 1
            var t = FPMath.Clamp(distance / config.maxCamDistance, 0, FP._10);
            
            CamViewRotPos = FPMath.Lerp(config.minRotValue, config.maxRotValue, t);
        }
        
        //FIGHT STANCE 
        private void CheckFightStance(Frame frame, EntityRef entity)
        {
            var playerAttack = frame.Unsafe.GetPointer<PlayerAttack>(entity);
            //Check attacking to update fight stance
            if (playerAttack->isAttacking || playerAttack->GotHit)
            {
                FightStanceTimer = 0;
                InFightStance = true;
            }
            
            if (InFightStance)
            {
                FightStanceTimer += frame.DeltaTime;

                if (FightStanceTimer >= 2)
                {
                    InFightStance = false;
                    FightStanceTimer = 0;
                }
            }
        }
        
        private void CheckHitWall(Frame frame, EntityRef entity)
        {
            var playerAttack = frame.Unsafe.GetPointer<PlayerAttack>(entity);
            var playerStat = frame.Unsafe.GetPointer<PlayerStat>(entity);
            var transform = frame.Unsafe.GetPointer<Transform3D>(entity);
            var col = frame.Unsafe.GetPointer<PhysicsCollider3D>(entity);
            var playerConfig = frame.FindAsset<PlayerConfig>(playerAttack->playerConfig.Id);
            var kcc = frame.Unsafe.GetPointer<KCC>(entity);
            
            var queryOptions = QueryOptions.HitStatics | QueryOptions.HitDynamics | QueryOptions.HitKinematics | QueryOptions.ComputeDetailedInfo;

            var startPos = new FPVector3(transform->Position.X, transform->Position.Y + col->Shape.Capsule.Height / 2,
                transform->Position.Z);

            var backward = -transform->Forward;
            
            var hit = frame.Physics3D.Raycast(startPos, backward, 1, layerMask:-1, queryOptions);
            
            Draw.Ray(startPos, backward * 1, ColorRGBA.Red);
            
            if (hit.HasValue && hit.Value.Entity != entity && !playerAttack->HitWall)
            {
                if (playerAttack->KnockDown && !playerAttack->InRecovery)
                {
                    //RESET HIT
                    kcc->SetDynamicVelocity(FPVector3.Zero);
                    
                    //HIT WALL 
                    playerAttack->HitWall = true;
                    playerStat->TakeDamageAmount(5);
                    frame.Events.HitWall(entity);
                }
            }
            
            //RESET HIT WALL
            if (playerAttack->HitWall)
            {
                playerAttack->HitWallTimer += frame.DeltaTime;
                if (playerAttack->HitWallTimer >= playerConfig.hitWallTime)
                {
                    playerAttack->HitWall = false;
                    playerAttack->HitWallTimer = 0;
                }
            }
        }
        
    }
}













