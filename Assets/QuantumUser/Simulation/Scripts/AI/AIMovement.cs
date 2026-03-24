using System.Net.NetworkInformation;
using Photon.Deterministic;
using UnityEngine;

namespace Quantum
{
    public unsafe class AIMovement
    {
        private static bool _canStartMoving;
        private static FP _canStartMovingTimer;
        
        //Slight delay when going to move
        //Prevents glitchy movement
        private bool CheckStartMoving(Frame frame)
        {
            _canStartMovingTimer += frame.DeltaTime;
            if (_canStartMovingTimer >= FP._0_20)
            {
                _canStartMoving = true;
                _canStartMovingTimer = 0;
                return true;
            }   
            return false;
        }

        public void TargetDirection(Frame frame, EntityRef entity)
        {
            var aiPlayer = frame.Unsafe.GetPointer<AIPlayer>(entity);
            
            var targetPos = frame.Unsafe.GetPointer<Transform3D>(aiPlayer->TargetEnemy);
            var myTransform = frame.Unsafe.GetPointer<Transform3D>(entity);

            var rawDirection = targetPos->Position - myTransform->Position;
            aiPlayer->AiMoveDirection = rawDirection.Normalized;
        }

        public void MoveBack(Frame frame, EntityRef entity)
        {
            var aiPlayer = frame.Unsafe.GetPointer<AIPlayer>(entity);

            var targetPos = frame.Unsafe.GetPointer<Transform3D>(aiPlayer->TargetEnemy);
            var myTransform = frame.Unsafe.GetPointer<Transform3D>(entity);

            var rawDirection = myTransform->Position - targetPos->Position; // Reverse
            aiPlayer->AiMoveDirection = rawDirection.Normalized;
        }
        
        public void MoveForward(Frame frame, EntityRef entity)
        {
            var aiPlayer = frame.Unsafe.GetPointer<AIPlayer>(entity);
            
            if(aiPlayer->TargetEnemy == EntityRef.None)return;

            if (aiPlayer->aiRange == AIRange.InRange)
            {
                //Stop Move
                aiPlayer->AiMoveDirection = FPVector3.Zero;
                
                _canStartMoving = false;
                _canStartMovingTimer = 0;
            }
            else
            {
                if(!CheckStartMoving(frame))
                    return;
                
                TargetDirection(frame, entity);
            }
        }

        #region  FightDodge
        
        public void DodgeBack(Frame frame, EntityRef entity)
        {
            var playerMovement = frame.Unsafe.GetPointer<PlayerMovement>(entity);
            var aiPlayer = frame.Unsafe.GetPointer<AIPlayer>(entity);
            
            if(aiPlayer->TargetEnemy == EntityRef.None)return;
            
            FPVector3 back;
            
            if (playerMovement->isFacingRight)
            {
                back = FPVector3.Right;
            }
            else
            {
                back = FPVector3.Left;
            }

            aiPlayer->AiMoveDirection = back;
        }
        
        public void DodgeForwardDown(Frame frame, EntityRef entity)
        {
            var playerMovement = frame.Unsafe.GetPointer<PlayerMovement>(entity);
            var aiPlayer = frame.Unsafe.GetPointer<AIPlayer>(entity);
            
            if(aiPlayer->TargetEnemy == EntityRef.None)return;
            
            FPVector3 forwardDown;
            
            if (playerMovement->isFacingRight)
            {
                forwardDown = FPVector3.Right + FPVector3.Down;
            }
            else
            {
                forwardDown = FPVector3.Left + FPVector3.Down;
            }

            aiPlayer->AiMoveDirection = forwardDown;
        }
           
        public void DodgeBackDown(Frame frame, EntityRef entity)
        {
            var playerMovement = frame.Unsafe.GetPointer<PlayerMovement>(entity);
            var aiPlayer = frame.Unsafe.GetPointer<AIPlayer>(entity);
            
            if(aiPlayer->TargetEnemy == EntityRef.None)return;
            
            FPVector3 backDown;
            
            if (playerMovement->isFacingRight)
            {
                backDown = FPVector3.Left + FPVector3.Down;
            }
            else
            {
                backDown = FPVector3.Right + FPVector3.Down;
            }

            aiPlayer->AiMoveDirection = backDown;
        }
        
        public void DodgeForwardUp(Frame frame, EntityRef entity)
        {
            var playerMovement = frame.Unsafe.GetPointer<PlayerMovement>(entity);
            var aiPlayer = frame.Unsafe.GetPointer<AIPlayer>(entity);
            
            if(aiPlayer->TargetEnemy == EntityRef.None)return;

            FPVector3 forwardUp;
            
            if (playerMovement->isFacingRight)
            {
                forwardUp = FPVector3.Right + FPVector3.Up;
            }
            else
            {
                forwardUp = FPVector3.Left + FPVector3.Up;
            }

            aiPlayer->AiMoveDirection = forwardUp;
        }
        
        public void DodgeBackUp(Frame frame, EntityRef entity)
        {
            var playerMovement = frame.Unsafe.GetPointer<PlayerMovement>(entity);
            var aiPlayer = frame.Unsafe.GetPointer<AIPlayer>(entity);
            
            if(aiPlayer->TargetEnemy == EntityRef.None)return;

            FPVector3 backUp;
            
            if (playerMovement->isFacingRight)
            {
                backUp = FPVector3.Left + FPVector3.Up;
            }
            else
            {
                backUp = FPVector3.Right + FPVector3.Up;
            }

            aiPlayer->AiMoveDirection = backUp;
        }
        
        #endregion
        
        
        public void StopMove(Frame frame, EntityRef entity)
        {
            var aiPlayer = frame.Unsafe.GetPointer<AIPlayer>(entity);
            aiPlayer->AiMoveDirection = FPVector3.Zero;
        }
    }
}