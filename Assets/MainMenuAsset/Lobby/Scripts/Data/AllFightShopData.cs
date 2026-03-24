using System;
using System.IO;
using UnityEngine;

public class AllFightShopData : MonoBehaviour
{
    public static AllFightShopData Instance;
    
    public IntroItem[] introItems;
    public HardPunchItem[] hardPunchItems;
    public HardKickItem[] hardKickItems;
    public CelebrateItem[] celebrateItems;
    
    public string saveFilePath;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            saveFilePath = Path.Combine(Application.persistentDataPath, "fightshopdata.json");
            LoadData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [ContextMenu("Save Shop Data")]
    public void SaveData()
    {
        FightShopData data = new FightShopData
        {
            introItems = introItems,
            hardPunchItems = hardPunchItems,
            hardKickItems = hardKickItems,
            celebrateItems = celebrateItems,
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(saveFilePath, json);

        Debug.Log("Shop data saved to file:\n" + saveFilePath);
    }

    [ContextMenu("Load Shop Data")]
    public void LoadData()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            FightShopData savedData = JsonUtility.FromJson<FightShopData>(json);

            // Merge for each type
            introItems = MergeArrays(introItems, savedData.introItems);
            hardPunchItems = MergeArrays(hardPunchItems, savedData.hardPunchItems);
            hardKickItems = MergeArrays(hardKickItems, savedData.hardKickItems);
            celebrateItems = MergeArrays(celebrateItems, savedData.celebrateItems);

            Debug.Log("Shop data loaded from file.");
        }
        else
        {
            Debug.Log("No saved data file found. Using default shop data.");
        }
    }

// Generic merge helper
    private T[] MergeArrays<T>(T[] currentArray, T[] savedArray) where T : class, new()
    {
        if (savedArray == null || savedArray.Length == 0)
            return currentArray;

        T[] result = new T[currentArray.Length];

        for (int i = 0; i < currentArray.Length; i++)
        {
            if (i < savedArray.Length)
            {
                // Use saved data if it exists
                result[i] = savedArray[i];
            }
            else
            {
                // Keep new default data (wasn't saved before)
                result[i] = currentArray[i];
            }
        }

        return result;
    }

}

[Serializable]
public class FightShopData
{
    public IntroItem[] introItems;
    public HardPunchItem[] hardPunchItems;
    public HardKickItem[] hardKickItems;
    public CelebrateItem[] celebrateItems;
}

[Serializable]
public class IntroItem
{
    public string defaultStatus = "buy";
    public string currentStatus = "buy";
    public int itemCost;
}

[Serializable]
public class CelebrateItem
{
    public string defaultStatus = "buy";
    public string currentStatus = "buy";
    public int itemCost;
}

[Serializable]
public class HardPunchItem
{
    public string defaultStatus = "buy";
    public string currentStatus = "buy";
    public int itemCost;
}

[Serializable]
public class HardKickItem
{
    public string defaultStatus = "buy";
    public string currentStatus = "buy";
    public int itemCost;
}
