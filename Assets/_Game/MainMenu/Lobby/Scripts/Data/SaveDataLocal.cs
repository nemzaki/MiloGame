using UnityEngine;
using System.IO;
using System;
using System.Collections;
using System.Threading.Tasks;
using Unity.Services.CloudSave;
using UnityEngine.Serialization;

public class SaveDataLocal : MonoBehaviour
{
    public static SaveDataLocal Instance { set; get; }
    
    public string dataFile = "GameData.json";
    private string _folderPath;
    
    [Header("Authentication")] 
    public string logInStatus;
    public string previousAccount;
    public string guessLogin;
    public string appleIDToken;
    public string googleIDToken;

    [Header("Player")] 
    public string playerName;
    public int currentMovementType;
    public int currentPlayerIndex;
    public int currentHatIndex;
    public int startDefaultDataPlayer;

    [Header("Fight")] 
    public int currentIdleType;
    public int currentHardPunchType;
    public int currentHardKickType;
    public int currentCelebrationType;
    
    [Header("Currency")]
    public int cash;
    public int gems;
    
    [Header("Game")] 
    public string gameMode;
    public int mapIndex;
    public int graphics;

    [Header("Settings")] 
    public string language;
    public float soundFXVolume = 100;
    public float musicFXVolume = 100;
    public string haptics = "on";
    public string showNames = "on";
    public int currentRegionIndex;

    [Header("Progression")]
    public int xp;
    public int level;

    [Header("Stats")]
    public int totalMatches;
    public int totalWins;
    public int totalLoses;
    
    private void Awake()
    {
        Instance = this;
        LoadGame();
    }

    public string GetFilePath(string filename)
    {
        var folderPath = Application.persistentDataPath;
        
        if (!Directory.Exists(folderPath)) {
            Directory.CreateDirectory(folderPath);
        }
        
        return folderPath + "/" + filename;
    }
    
    public void SaveGame()
    {
        DataSave data = new DataSave();
        data.cash = cash;
        data.gems = gems;
        
        data.playerName = playerName;
        data.currentPlayerIndex = currentPlayerIndex;
        data.currentMovementType = currentMovementType;
        data.currentHatIndex = currentHatIndex;
        data.startDefaultDataPlayer = startDefaultDataPlayer;
        data.logInStatus = logInStatus;

        data.language = language;
        data.graphics = graphics;
        data.musicFXVolume = musicFXVolume;
        data.soundFXVolume = soundFXVolume;
        data.haptics = haptics;
        data.currentRegionIndex = currentRegionIndex;
        data.gameMode = gameMode;
        data.mapIndex = mapIndex;
        
        //Authentication
        data.guessLogin = guessLogin;
        data.previousAccount = previousAccount;
        // appleIDToken stores "linked" or "" only — never the actual Apple identity token
        data.appleIDToken = appleIDToken;
        
        data.currentIdleType = currentIdleType;
        data.currentHardPunchType = currentHardPunchType;
        data.currentHardKickType = currentHardKickType;
        data.currentCelebrationType = currentCelebrationType;
        
        data.xp = xp;
        data.level = level;

        data.totalMatches = totalMatches;
        data.totalWins = totalWins;
        data.totalLoses = totalLoses;
        
        File.WriteAllText(GetFilePath(dataFile), JsonUtility.ToJson(data));

        // Non-blocking cloud sync — never delays the local save.
        if (CloudSave.Instance != null)
            _ = CloudSave.Instance.SaveCoreDataAsync();
    }
    
    [ContextMenu("Load Data")]
    public void LoadGame()
    {
        if (File.Exists(GetFilePath(dataFile)))
        {
            try
            {
                DataSave data = JsonUtility.FromJson<DataSave>(File.ReadAllText(GetFilePath(dataFile)));

                cash = data.cash;
                gems = data.gems;
                
                playerName = data.playerName;
                currentPlayerIndex = data.currentPlayerIndex;
                currentMovementType = data.currentMovementType;
                currentHatIndex = data.currentHatIndex;
                startDefaultDataPlayer = data.startDefaultDataPlayer;
                logInStatus = data.logInStatus;
                
                language = data.language;
                graphics = data.graphics;
                musicFXVolume = data.musicFXVolume;
                soundFXVolume = data.soundFXVolume;
                haptics = data.haptics;

                currentRegionIndex = data.currentRegionIndex;
                gameMode = data.gameMode;
                mapIndex = data.mapIndex;
                
                //Authentication
                guessLogin = data.guessLogin;
                appleIDToken = data.appleIDToken;
                previousAccount = data.previousAccount;
                
                currentIdleType = data.currentIdleType;
                currentHardPunchType = data.currentHardPunchType;
                currentHardKickType = data.currentHardKickType;
                currentCelebrationType = data.currentCelebrationType;
                
                xp = data.xp;
                level = data.level;

                totalMatches = data.totalMatches;
                totalWins = data.totalWins;
                totalLoses = data.totalLoses;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Save file corrupted or incompatible, resetting to defaults: {e.Message}");
                SetDefaults();
            }
        }
        else
        {
            SetDefaults();
        }
    }

    private void SetDefaults()
    {
        cash = 20000;
        gems = 0;
        xp = 0;
        level = 1;
        playerName = null;
        currentRegionIndex = -1;
        startDefaultDataPlayer = 0;
        language = "English";
        graphics = 1;
        musicFXVolume = 1;
        soundFXVolume = 1;
        haptics = "on";
        showNames = "on";
    }
    
    #region Cloud
    public void AnonymousLoginReset()
    {
        if (previousAccount == "real")
        {
            StartCoroutine(RemovePreviousAccountData());
        }
    }
    
    //If a real account was logged in reset that data
    //The data will be in the cloud anyway
    IEnumerator RemovePreviousAccountData()
    {
        //Delete the current files
        DeleteLocalData();

        yield return new WaitForSeconds(0.5f);
        
        LoadGame();
        
        //Save login Status
        logInStatus = "in";
        previousAccount = "guess";
        guessLogin = "guess";
        
        //Get default name from database if none is created
        AuthManager.Instance.SignInConfirmAsync();
        
        SaveGame();
    }
    
    public void UploadDataToCloud()
    {
        _folderPath = Application.persistentDataPath;
        
        if (Directory.Exists(_folderPath))
        {
            string[] files = Directory.GetFiles(_folderPath);

            foreach (string filePath in files)
            {
                string fileName = Path.GetFileName(filePath);
                CloudSaveFile.Instance.SavePlayerFile(filePath, fileName);
            }
        }
        else
        {
            Debug.LogError("Folder not found!");
        }
    }
    
    public async Task DeleteCloudData()
    {
        //Get all files for player
        var fileData = await CloudSaveService.Instance.Files.Player.ListAllAsync();

        foreach (var file in fileData)
        {
            await CloudSaveFile.Instance.DeletePlayerFile(file.Key);
        }
    }
    
    public async Task LoadCloudData()
    {
        //Delete the current files
        DeleteLocalData();
        
        //Get all files for player
        var fileData = await CloudSaveService.Instance.Files.Player.ListAllAsync();

        foreach (var file in fileData)
        {
            //Get cloud data
            await CloudSaveFile.Instance.GetPlayerFileAsStream(file.Key);
        }
        
        LoadGame();
        
        //If there's data in the cloud 
        //then the player is real :)
        logInStatus = "in";
        
        //Get default name from database if none is created
        AuthManager.Instance.SignInConfirmAsync();
        
        SaveGame();
    }
    
    private void DeleteLocalData()
    {
        var path = Application.persistentDataPath;
        
        if (Directory.Exists(path))
        {
            // Get all files in the directory
            var files = Directory.GetFiles(path);

            // Loop through each file and delete it
            foreach (var file in files)
            {
                try
                {
                    File.Delete(file);
                }
                catch (IOException e)
                {
                    Debug.LogError($"Error deleting file {file}: {e.Message}");
                }
            }
        }
        else
        {
            Debug.LogError("Persistent data path not found.");
        }
    }
    
    #endregion
}

[Serializable]
class DataSave
{
    //Authentication
    public string logInStatus;
    public string guessLogin;
    public string previousAccount;
    // Persisted as "linked" or "" only — the actual Apple/Google identity tokens are NEVER stored on disk
    public string appleIDToken;

    //Fight
    public int currentIdleType;
    public int currentHardPunchType;
    public int currentHardKickType;
    public int currentCelebrationType;
    
    //Currency
    public int cash;
    public int gems;
    
    //Player
    public string playerName;
    public int currentMovementType;
    public int currentPlayerIndex;
    public int currentHatIndex;
    public int startDefaultDataPlayer;
    //
    public string language;
    public int graphics;
    public float soundFXVolume;
    public float musicFXVolume;
    public string haptics;
    
    public int currentRegionIndex;
    public string gameMode;
    public int mapIndex;
    
    public int xp;
    public int level;

    public int totalMatches;
    public int totalWins;
    public int totalLoses;

}
