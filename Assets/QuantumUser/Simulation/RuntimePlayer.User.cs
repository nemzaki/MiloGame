using Photon.Deterministic;
using UnityEngine;

namespace Quantum
{
    public partial class RuntimePlayer
    {
        [HideInInspector]
        public string userID;
        public string nickname;
        public string playerPlatform;
        public int currentPlayerIndex;
        
        //player character entity
        [Header("Character")]
        public AssetRef<EntityPrototype> playerPrototype;

        [Header("Player Data")] 
        public int currentWeaponID;

        [Header("Move Type")] 
        public int idleType;
        public int moveType;
        
        [Header("Hard Punch")] 
        public int hardPunchType;
        
        [Header("Hard Kick")]
        public int hardKickType;
        
        [Header("Celebrate Type")]
        public int celebrateType;

        [Header("Fight Sequence Attack")] 
        public int attackCombo1;
        public int attackCombo2;
        public int attackCombo3;
        public int attackCombo4;
        
        [Header("Fight Sequence Kick")]
        public int kickCombo1;
        public int kickCombo2;
        public int kickCombo3;
        public int kickCombo4;
        
        partial void SerializeUserData(BitStream stream)
        {
            stream.Serialize(ref userID);

            stream.Serialize(ref idleType);
            stream.Serialize(ref currentWeaponID);
            stream.Serialize(ref nickname);
            stream.Serialize(ref playerPlatform);
            stream.Serialize(ref playerPrototype.Id);
            stream.Serialize(ref currentPlayerIndex);
            
            stream.Serialize(ref hardPunchType);
            stream.Serialize(ref hardKickType);
            stream.Serialize(ref celebrateType);
            stream.Serialize(ref moveType);
            
            stream.Serialize(ref attackCombo1);
            stream.Serialize(ref attackCombo2);
            stream.Serialize(ref attackCombo3);
            stream.Serialize(ref attackCombo4);
            
            stream.Serialize(ref kickCombo1);
            stream.Serialize(ref kickCombo2);
            stream.Serialize(ref kickCombo3);
            stream.Serialize(ref kickCombo4);
        }
    }
}