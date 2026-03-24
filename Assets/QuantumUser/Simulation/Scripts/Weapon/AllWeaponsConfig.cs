using System;
using System.Linq;
using Photon.Deterministic;
using Quantum;
using Quantum.Scripts.Config;
using Quantum.Scripts.Weapon;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace Quantum
{
    public enum HitReactionType
    {
        HitFrontMed,
        HitHighLeftMed,
        HitHighRightMed,
        HitHighLeftWeak,
        HitHighRightWeak,
        HitMidRightWeak,
        HitMidLeftWeak,
        HitLowLeftWeak,
        HitLowRightWeak,
        HitMidFrontMed,
        HighKoPowerful,
        HitHighFrontMed,
        HitHighFrontStagger,
        HitMidFrontWeak,
        CrouchFrontWeak,
        KnockDownFront,
        HitHighUpperCutMed,
        SweepKickTrip,
        KnockDownScrew,
        HitMidTopMed,
        StylizedHitRight,
        StylizedHitLeft,
        StylizedHitFront,
    }

    public enum BlockReactionType
    {
        Duck,
        DuckLeft,
        DuckRight,
        MidBlockLeft,
        MidBlockRight,
        SlipBack,
        SlipLeft,
        SlipRight,
    }

    public enum AttackTypeName
    {
        Punch,
        PunchHard,
        Kick,
        KickHard
    }

    public enum HitBoxType
    {
        High,
        Mid,
        Low,
    }

    public enum HitType
    {
        Light,
        Mid,
        Heavy
    }
    
    public class AllWeaponsConfig : AssetObject
    {
        public AssetRef<EntityPrototype> hitBox;
        
        [Header("Grab")]
        public int grabAttackType;
        public AttackData grabData;

        [Header("Player")] 
        public int playerSkinMax = 3;
        
        [Header("Intro")] 
        public int introMax;
        public int celebrateMax;

        [Header("Weapon Assets")] 
        public AssetRef<WeaponConfig>[] weaponData;
    }
}


[Serializable]
public struct AttackData
{
    [Header("Attack")]
    [Space(10)]
    public string animationName;
    public int hardPunchType;
    public int hardKickType;
    
    [Header("Attack Timing")]
    public FP startFrame;
    public FP endFrame;
    public FP hitBoxFrame;
    public FP priorityFrame;
    
    [Header("Force")]
    [Space(10)]
    public FPVector3 actionMoveForce;
    public FPVector3 hitMoveForce;
    
    [Header("Hit Time")]
    [Space(10)]
    public FP hitTime;

    [Header("Hit Reaction")] 
    [Space(10)] 
    public FP hitCrossFadeTime;
    public HitReactionType hitReactionName;
    public bool knockDown;
    public FP knockDownRecoveryTime;
    
    [Header("Block Reaction")]
    [Space(10)]
    public BlockReactionType blockHitReactionName;

    [Header("Damage")]
    [Space(10)]
    public int damageAmount;
    public int staminaAmount;
    
    [Header("HitStop")]
    [Space(10)]
    public FP hitStopTimeAttacker;
    public FP hitStopTimeVictim;
    
    [Header("Attack Settings")] 
    [Space(10)]
    public AssetRef<EntityPrototype> hitBox;
    public AttackTypeName attackType;
    public HitBoxType hitBoxType;
    public HitType hitType;
}












