using System;
using Photon.Deterministic;
using UnityEngine;

namespace Quantum
{
    public class PlayerConfig : AssetObject
    {
        [Header("Aim Pos")] 
        public FPVector3 aimPosStart;

        [Header("Attack")] 
        public FP comboCooldownDuration = 1;

        public HitBoxSettings crouchHitBox;
        public HitBoxSettings highHitBox;
        public HitBoxSettings midHitBox;
        public HitBoxSettings lowHitBox;
        public HitBoxSettings blockHitBox;
        
        [Header("Dodge")]
        public int dodgeForce;
        public FP dodgeDuration;
        public FP curveAmount;
        
        [Header("Roam Dodge")]
        public int roamDodgeForce;
        public FP dodgeDurationRoam;
        public FP canDodgeAvoidHitTime = FP._0_50; //Window where player cant get damaged
        
        [Header("Dash")] 
        public int dashForce;
        public FP dashDuration;
        
        [Header("Block")] 
        public AssetRef<EntityPrototype> blockBox;
        
        [Header("Health")]
        public int startingHealth = 100;
        public int maxHealth = 150;
        
        [Header("Stamina")] 
        public int maxStamina = 100;
        public FP staminaRecoveryRate = FP._0_05;

        [Header("Range")] 
        public FP autoSprintRange;
        public int freeFlowRange = 5;
        
        [Header("Movement")] 
        public int rotationSpeed = 5;
        public int normalSpeed = 15;
        public int sprintSpeed = 25;
        public FP jumpForce = 10;
        public FP attractionStrength = FP._0_05;
        
        [Header("Physics")] 
        public FP groundAcceleration = 55;
        public FP groundDeceleration = 25;
        public FP airAcceleration    = 25;
        public FP airDeceleration    = FP._1 + FP._0_20 + FP._0_10;

        [Header("Hit")] 
        public FP hitWallTime = 1;
        public FP hitForce = 25;
        public FP timeToKnockDownRecover = FP._0_10;
        public FP timeToGroundRecover = FP._0_10;

        [Header("Attacking")] 
        public FP isStrafingRange = 5;
        
        [Header("Crouch Settings")] 
        public FP capsuleHeightStanding;
        public FP capsuleCenterStanding;
        public FP capsuleHeightCrouching;
        public FP capsuleCenterCrouching;
        
        [Header("Cam")] 
        public FP maxCamDistance = 20;
        public FP minRotValue;
        public FP maxRotValue;
    }
}

[Serializable]
public class HitBoxSettings
{
    public FPVector3 hitBoxSpawnPos;
    public FP hitBoxOffset;
    public FP hitBoxForwardOffset;
}












