using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class AllPlayerData : MonoBehaviour
{
    public PlayerItem[] player;

    private void Start()
    {
        foreach (var character in player)
        {
            character.Start();
        }
    }
}
[Serializable]
public class PlayerItem
{
    [Header("Player")]
    public string playerName;
    public int playerIndex;
    public int playerMovementType;

    [Header("Default Data")]
    public string defaultStatus;

    public GameObject playerObj;
    public int cost;
    public string status;

    [Header("Skins")]
    public SkinItem[] skins;

    //DATA
    [Header("Load Data")]
    private List<PlayerItemData> items = new List<PlayerItemData>();
    public List<PlayerItemData> loadedItemPlayer = new List<PlayerItemData>();

    [HideInInspector]
    public string saveFilePath;
    public string saveFileName;


    public void Start()
    {
        saveFileName = "Player_" + playerName + "_Data.dat";
        LoadItem();
    }

    //
    public void SaveStartData()
    {
        saveFilePath = SavePlayerDataLocal.Instance.GetFilePath(saveFileName);

        SavePlayerItemData();
    }

    // Save all items to file
    private void SaveData()
    {
        string json = JsonUtility.ToJson(new PlayerItemDataList(items), true);
        File.WriteAllText(saveFilePath, json);
    }

    // Load all items from file
    private void LoadData()
    {
        //GET FILE PATH
        saveFilePath = SavePlayerDataLocal.Instance.GetFilePath(saveFileName);

        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            PlayerItemDataList itemList = JsonUtility.FromJson<PlayerItemDataList>(json);
            items = itemList.items;

            //Get Car Item
            loadedItemPlayer = GetPlayerItems();

            //Get status
            status = loadedItemPlayer[0].itemStatus;
        }
        else
        {
            //NO FIle found
            status = defaultStatus;
            loadedItemPlayer.Clear();

            SaveStartData();
        }
    }

    // Add a new item
    private void AddItem(PlayerItemData item)
    {
        items.Add(item);
        SaveData();
    }

    public void SavePlayerItemData()
    {
        //Reset list
        items.Clear();

        SaveItemData("Player", status);
    }

    private void SaveItemData(string itemName, string itemStatus)
    {
        PlayerItemData newItem = new PlayerItemData
        {
            itemName = itemName,
            itemStatus = itemStatus
        };

        // Add the new item to the save manager
        AddItem(newItem);
    }

    // Get Player Items
    private List<PlayerItemData> GetPlayerItems()
    {
        return items.Where(items => items.itemName == "Player").ToList();
    }

    [ContextMenu("Load Data")]
    public void LoadItem()
    {
        LoadData();
    }
}

[Serializable]
public class SkinItem
{
    public string displayName;
    public GameObject skinPrefab;
    public string defaultStatus = "buy";
    public string currentStatus = "buy";
    public int itemCost;
}

[System.Serializable]
public class PlayerItemData
{
    public string itemName;
    public string itemStatus;
}

[System.Serializable]
public class PlayerItemDataList
{
    public List<PlayerItemData> items;

    public PlayerItemDataList(List<PlayerItemData> items)
    {
        this.items = items;
    }
}
