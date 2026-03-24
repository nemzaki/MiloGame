using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using TMPro;

public class MatchSettings : MonoBehaviour
{
    public static MatchSettings Instance { set; get; }
    
    [Header("Data")]
    public MatchSettingsSO mapData;

    public List<SelectMapObj> mapObjs = new List<SelectMapObj>();
    
    [Header("UI")] 
    public GameObject mapSelectionObj;
    public RectTransform mapTypesParent;
    
    public TextMeshProUGUI mapName;
    public TextMeshProUGUI mapDescription;
    
    
    private void Awake()
    {
        Instance = this;
    }


    public void SelectMap(int index)
    {
        MainMenuUIHandler.Instance.mapIndex = index;
        
        switch (MainMenuUIHandler.Instance.gameMode)
        {
            case "Battle":
                PlayerPrefs.SetInt("LastSelectedMap", index);
                mapName.text = mapData.gameMaps[index].mapName;
                mapDescription.text = mapData.gameMaps[index].mapDescription;
                MainMenuUIHandler.Instance.mapGuid = mapData.gameMaps[index].mapAsset.Guid.Value;
                break;
        }

        MainMenuUIHandler.Instance.mapName = mapName.text;
        OutlineSelectedMap(MainMenuUIHandler.Instance.mapIndex);
    }

    private void OutlineSelectedMap(int index)
    {
        for (var i = 0; i < mapObjs.Count; i++)
        {
            mapObjs[i].outline.SetActive(index == i);
        }
    }
    
    public void SpawnMapTypes()
    {
        ClearMapSelect();

        var mapDataIndex = 0;
        
        switch (MainMenuUIHandler.Instance.gameMode)
        {
            //Spawn Race Map Select Obj
            case "Battle":
            {
                for (var i = 0; i < mapData.gameMaps.Length; i++)
                {
                    var mapObj = Instantiate(mapSelectionObj, mapTypesParent);
                    var mapSelect = mapObj.GetComponent<SelectMapObj>();
                    mapSelect.index = i;
                    mapObjs.Add(mapSelect);
                }

                //Get map data
                //mapDataIndex = PlayerPrefs.GetInt("LastSelectedMap", 0);
                OutlineSelectedMap(mapDataIndex);
                SelectMap(mapDataIndex);
                
                break;
            }
        }
        
        SelectMap(mapDataIndex);
    }
    
    private void ClearMapSelect()
    {
        mapObjs.Clear();
        
        foreach (Transform child in mapTypesParent)
        {
            Destroy(child.gameObject);
        }
    }
}
