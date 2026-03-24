using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Unity.Services.CloudSave;
using UnityEngine;
using UnityEngine.Serialization;

public class SavePlayerDataLocal : MonoBehaviour
{
     public static SavePlayerDataLocal Instance { set; get; }

    public string fileName;

    public int playerIndex;

    private string _folderPath;
    public SaveDataLocal saveDataLocal;
    
    [Header("Player Current Data")] 
    public int currentPlayerType;
    public int currentPlayerHat;
    

    public List<PlayerInputEntry> playerEntries;
    
    private bool _loadData;
    private float _loadTimer;
    
    public ResourceManager resourceManager;

    
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        UpdateLoadData();
    }

    public string GetFilePath(string filename)
    {
        var folderPath = Application.persistentDataPath;
        
        if (!Directory.Exists(folderPath)) {
            Directory.CreateDirectory(folderPath);
        }
        
        return folderPath + "/" + filename;
    }
    
    public void UpdateLoadData()
    {
        playerIndex = SaveDataLocal.Instance.currentPlayerIndex;
        LoadPlayerData();
        SetStartDefaultData();
    }
    
    //Player Data
    [ContextMenu("Save Player Data")]
    public void SavePlayerData()
    {
        PlayerInputEntry existingEntry = playerEntries.Find(entry => entry.index == currentPlayerType);
        if (existingEntry != null)
        {
            existingEntry.currentPlayerType = currentPlayerType;
            existingEntry.currentHatType = currentPlayerHat;
        }
        
        // Save the updated list to JSON file
        SaveToBinary<PlayerInputEntry>(playerEntries, fileName);
    }
    
    private void SetStartDefaultData()
    {
        if (saveDataLocal.startDefaultDataPlayer == 0)
        {
            playerEntries.Clear();

            for (var i = 0; i < resourceManager.playerData.player.Length; i++)
            {
                playerIndex = i;
                playerEntries.Add(new PlayerInputEntry(playerIndex, playerIndex,  0));
            }
            saveDataLocal.startDefaultDataPlayer = 1;
        }
        else
        {
            // // Check if there is additional player data and add only new entries
            if (resourceManager.playerData.player.Length > playerEntries.Count)
            {
                for (var i = playerEntries.Count; i < resourceManager.playerData.player.Length; i++)
                {
                    playerIndex = i;
                    playerEntries.Add(new PlayerInputEntry(playerIndex, playerIndex,0));
                }
            }
        }
    }
    
    public void LoadPlayerData()
    {
        playerEntries = LoadListFromBinary<PlayerInputEntry>(fileName);
        
        if(playerEntries == null)
            return;
        
        if(playerIndex > playerEntries.Count)
            return;
        
        foreach (var t in playerEntries)
        {
            currentPlayerType = playerEntries[playerIndex].currentPlayerType;
            currentPlayerHat = playerEntries[playerIndex].currentHatType;
        }
    }
    
    public void SaveToBinary<T> (List<T> toSave, string filename) {
        string filePath = GetFilePath(filename);
        BinaryFormatter formatter = new BinaryFormatter();
        using (FileStream stream = new FileStream(filePath, FileMode.Create))
        {
            formatter.Serialize(stream, toSave);
        }
    }
    
    public List<T> LoadListFromBinary<T>(string filename)
    {
        string filePath = GetFilePath(filename);
        if (File.Exists(filePath))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream stream = new FileStream(filePath, FileMode.Open))
            {
                return formatter.Deserialize(stream) as List<T>;
            }
        }
        else
        {
            //DEFAULT DATA
            currentPlayerType = 0;
            currentPlayerHat = 0;
            
            return new List<T>();
        }
    }
}

[Serializable]
public class PlayerInputEntry
{
    public int index;
    
    //Player
    public int currentPlayerType;
    public int currentHatType;
    
    public PlayerInputEntry(int index, int currentPlayerType, int currentHatType)
    {
        this.index = index;

        this.currentPlayerType = currentPlayerType;
        this.currentHatType = currentHatType;
    }
}
