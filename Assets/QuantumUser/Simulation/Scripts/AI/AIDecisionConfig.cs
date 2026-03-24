using System;
using Photon.Deterministic;
using Quantum;
using UnityEngine;

namespace Quantum
{
    public class AIDecisionConfig : AssetObject
    {
        public AIDecisionCheck[] aiDecisions;
    }
}

[Serializable]
public class AIDecisionCheck
{
    public AIState aiState;
    public FP chance;
    public FP actionTimeMin;
    public FP actionTimeMax;
    public FP coolDownTime;
    public int repeatAmount;
}