using UnityEngine;

namespace Quantum
{
    public class DynamicPropConfig : AssetObject
    {
        [Header("Physic")] 
        public int impulseForceMin;
        public int impulseForceMax;
    }
}