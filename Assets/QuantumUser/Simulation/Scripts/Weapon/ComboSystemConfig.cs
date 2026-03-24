using UnityEngine;
using UnityEngine.Serialization;

namespace Quantum
{
    public class ComboSystemConfig : AssetObject
    {
        [Header("Fight Style")] 
        public int fightStyle;
        
        [Header("Punch")]
        public AssetRef<NormalAttackConfig>[] punchComboSystem;
        
        [Header("Kick")]
        public AssetRef<NormalAttackConfig>[] kickComboSystem;

    }
}