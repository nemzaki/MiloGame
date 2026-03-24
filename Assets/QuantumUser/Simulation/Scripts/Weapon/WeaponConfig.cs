using Photon.Deterministic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Quantum.Scripts.Weapon
{
    public class WeaponConfig : AssetObject
    {

        [Header("Weapon Type")] 
        public int weaponID;
        public bool useIK;

        [Header("Item")] 
        public bool melee;

        [Header("Moving")] 
        public FP moveStateAiming;
        public FP moveStateRelax;

        [Header("Melee")] 
        public int meleeType;

        [Header("Weapon Settings")] 
        public FP impulseForce = FP._0_03;
        
        public AssetRef<NormalAttackConfig>[] attacks;
        public AssetRef<NormalAttackConfig>[] kicks;
        
        [Header("Jump Attack Datasets")]
        public AssetRef<NormalAttackConfig>[] jumpPunchAttacks;
        public AssetRef<NormalAttackConfig>[] jumpKickAttacks;
        
        [Header("Hard Attack Datasets")]
        public AssetRef<NormalAttackConfig>[] hardAttacks;
        
        [Header("Forward Up Attack")]
        public AssetRef<NormalAttackConfig>[] forwardUpAttacks;

        [Header("Forward Up Kick")]
        public AssetRef<NormalAttackConfig>[] forwardUpKicks;
        
        [Header("Hard Kick Datasets")]
        public AssetRef<NormalAttackConfig>[] hardKickAttacks;

        [Header("Low Punch Datasets")] 
        public AssetRef<NormalAttackConfig>[] lowPunchAttack;
        
        [Header("Low Kick Datasets")] 
        public AssetRef<NormalAttackConfig>[] lowKickAttack;
        
        [Header("Punch Back Datasets")]
        public AssetRef<NormalAttackConfig>[] punchBackAttacks;
        
        [Header("Kick Back Datasets")]
        public AssetRef<NormalAttackConfig>[] kickBackAttacks;
        
        [Header("Special Hits")] 
        public AssetRef<NormalAttackConfig> crouchHitData;
        public AssetRef<NormalAttackConfig> airHitData;

        [Header("Sweep Kick Datasets")] 
        public AssetRef<NormalAttackConfig> sweepKickData;
    }
}