using System;
using Photon.Deterministic;
using UnityEngine;

namespace Quantum
{
    public partial class RuntimeConfig
    {
	    [Header("Mode")] 
	    public bool battleMode;
	    public bool training;
	    
	    [Header("Game Type")]
	    public bool freeMove;
	    public bool hasRounds;
	    
        [Header("Player")]
        public byte playerCount;
        
        public FP waitingTime;
        public FP startTime;
        public FP gameTime;
        
        public AssetRef<AimingConfig> AimingConfig;

        [Header("Map Index")]
        public Int32 mapIndex;
        
        [Header("AI")]
        public byte aiPlayerCount;
        
        partial void SerializeUserData(BitStream stream)
        {
	        stream.Serialize(ref battleMode);
	        stream.Serialize(ref training);
	        
	        //Player
	        stream.Serialize(ref playerCount);
	        stream.Serialize(ref waitingTime);
	        stream.Serialize(ref startTime);
	        stream.Serialize(ref gameTime);
	        
	        //Map
	        stream.Serialize(ref mapIndex);
	        
	        stream.Serialize(ref AimingConfig);
	        
	        stream.Serialize(ref aiPlayerCount);
	        
	        stream.Serialize(ref freeMove);
        }
    }
}