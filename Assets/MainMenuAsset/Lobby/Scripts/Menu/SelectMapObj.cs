using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectMapObj : MonoBehaviour
{
    public int index;

    private MatchSettings _matchSettings;

    public GameObject outline;

    private int _savedMapData;
    
    private void Start()
    {
        _matchSettings = MatchSettings.Instance;
    }
    
    
    public void SelectMap()
    {
        _matchSettings.SelectMap(index);
    }
}
