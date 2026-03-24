using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Quantum;
using UnityEngine.Serialization;

[Serializable]
[CreateAssetMenu(fileName = "MatchSettings", menuName = "Settings/Match Settings")]
public class MatchSettingsSO : ScriptableObject
{
    public MapSetup[] gameMaps;
}

[Serializable]
public sealed class MapSetup
{
    public Map mapAsset;
    public string mapName;
    public string mapDescription;
}
