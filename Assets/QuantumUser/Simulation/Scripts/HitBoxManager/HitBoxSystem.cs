using Photon.Deterministic;
using Quantum.Scripts.Config;
using Quantum.Scripts.Weapon;
using UnityEngine;
using UnityEngine.Scripting;

namespace Quantum
{
    [Preserve]
    public unsafe class HitBoxSystem:  SystemMainThread, ISignalOnTriggerEnter3D
    {
        public override void Update(Frame frame)
        {
            foreach (var pair in frame.Unsafe.GetComponentBlockIterator<HitBox>())
            {
                var entity = pair.Entity;
                if(frame.IsCulled(entity))
                    continue;
                
                pair.Component->Update(frame, entity);
            }
        }
        
        void ISignalOnTriggerEnter3D.OnTriggerEnter3D(Frame frame, TriggerInfo3D info)
        {
            if (frame.Unsafe.TryGetPointer<HitBox>(info.Entity, out var hitBox) == false)
                return;

            var gameplay = frame.Unsafe.GetPointerSingleton<Gameplay>();
            var allWeaponConfig = frame.FindAsset<AllWeaponsConfig>(gameplay->allWeaponsConfig.Id);
            var currentWeapon = frame.FindAsset<WeaponConfig>(allWeaponConfig.weaponData[hitBox->currentWeaponID].Id);
            var crouchHit = frame.FindAsset<NormalAttackConfig>(currentWeapon.crouchHitData.Id).attackData;
            var airHit = frame.FindAsset<NormalAttackConfig>(currentWeapon.airHitData.Id).attackData;
            var transform = frame.Unsafe.GetPointer<Transform3D>(info.Entity);
            
            // Prevent double hits immediately
            if (hitBox->AlreadyHit || !hitBox->IsActive) 
                return;
            
            if (hitBox->SourceEntity == info.Other)
                return;
            
            //Get my player attack
            if(frame.Unsafe.TryGetPointer<PlayerAttack>(hitBox->SourceEntity, out var playerAttacker) == false)
                return;
            
            //Get other player, player attack
            if(frame.Unsafe.TryGetPointer<PlayerAttack>(info.Other, out var victimAttacker) == false)
                return;
            
            //Knock down should ignore block
            if (hitBox->IsBlockBox && victimAttacker->IsBlocking && !hitBox->KnockDown)
            {
                frame.Events.BlockHit(info.Other, hitBox->SourceEntity, hitBox->BlockReactionName);
                return;
            }
            
            if(hitBox->IsBlockBox)
                return;
            
            //Priority to player that start attack first
            if (playerAttacker->AttackStartFrame <= victimAttacker->AttackStartFrame) 
                return;
            
            //Check hit player
            if (frame.Unsafe.TryGetPointer<PlayerMovement>(info.Other, out var victimPlayerMovement) == false)
                return;
            
            if(frame.Unsafe.TryGetPointer<PlayerStat>(info.Other, out var victimPlayerStat) == false)
                return;
            
            var ignoreHitReaction = victimPlayerMovement->IsDodging && !frame.RuntimeConfig.freeMove;

            if (!victimPlayerMovement->IsCrouching && victimPlayerMovement->Grounded)
            {
                victimPlayerStat->TakeDamage(frame, info.Other, info.Entity,hitBox->SourceEntity,
                    hitBox->HitForce, hitBox->GotHitTime, hitBox->HitReactionName,
                    hitBox->KnockDown, hitBox->RecoveryTime, hitBox->HitCrossFadeTime,
                    ignoreHitReaction, hitBox->HitStopTimeAttacker,hitBox->HitStopTimeVictim, hitBox->DamageAmount, 
                    hitBox->StaminaAmount, hitBox->AttackTypeInfo);
            }
            else if (victimPlayerMovement->IsCrouching && victimPlayerMovement->Grounded)
            {
                victimPlayerStat->TakeDamage(frame, info.Other, info.Entity, hitBox->SourceEntity,
                    crouchHit.hitMoveForce, crouchHit.hitTime,
                    crouchHit.hitReactionName.ToString(),
                    hitBox->KnockDown, crouchHit.knockDownRecoveryTime, crouchHit.hitCrossFadeTime,
                    ignoreHitReaction, hitBox->HitStopTimeAttacker,hitBox->HitStopTimeVictim, hitBox->DamageAmount, 
                    hitBox->StaminaAmount, hitBox->AttackTypeInfo);
            }
            else if (!victimPlayerMovement->Grounded)
            {
                victimPlayerStat->TakeDamage(frame, info.Other, info.Entity, hitBox->SourceEntity,
                    airHit.hitMoveForce, airHit.hitTime, airHit.hitReactionName.ToString(),
                    hitBox->KnockDown, airHit.knockDownRecoveryTime, airHit.hitCrossFadeTime,
                    ignoreHitReaction, hitBox->HitStopTimeAttacker,hitBox->HitStopTimeVictim, hitBox->DamageAmount, 
                    hitBox->StaminaAmount, hitBox->AttackTypeInfo);
            }
            
            // Set after confirmed hit
            hitBox->AlreadyHit = true;
            hitBox->IsActive = false;
        }
        
    }
}