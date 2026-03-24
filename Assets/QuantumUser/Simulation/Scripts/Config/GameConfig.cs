using Photon.Deterministic;
using UnityEngine;

namespace Quantum
{
    public class GameConfig : AssetObject
    {
        public FP maxDistanceBetweenPlayers;

        [Header("FX")] 
        public FP avoidSlowMoTime;
        
        [Header("Ping Check")] 
        public int badPingRange;
    }
}