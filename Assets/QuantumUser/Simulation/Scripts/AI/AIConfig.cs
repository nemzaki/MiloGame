using System;
using Photon.Deterministic;
using Quantum;
using UnityEngine;

namespace Quantum
{
    
    public class AIConfig : AssetObject
    {
        [Header("AI DEBUG")] 
        public bool setActive;

        [Header("RANGE")] 
        public FP attackRange;
        
        [Header("STATE")] 
        public bool attack;
        public bool block;
        public bool jump;
        
        [Header("Attack")]
        public FP attackRate = FP._0_25;

        [Header("Kick")] 
        public FP kickRate = FP._0_50;
        
        [Header("Jump")] 
        public FP jumpRate = 1;

        [Header("Mind")] 
        public FP gettingSpammedMax = 5;
        public FP hyperAwareTime = 5;
        
        [Header("AI DECISION IN RANGE")]
        [Space(10)]
        public AssetRef<AIDecisionConfig> inRangeDecision;
        
        [Header("AI DECISION OUT RANGE")] 
        [Space(10)]
        public AssetRef<AIDecisionConfig> outRangeDecision;
        
        [Header("AI DECISION GETTING SPAMMED")]
        [Space(10)]
        public AssetRef<AIDecisionConfig> spammedDecision;

        [Header("AI DECISION GETTING SPAMMED")] 
        [Space(10)]
        public AssetRef<AIDecisionConfig> playerRecoveringDecision;
    }
}

