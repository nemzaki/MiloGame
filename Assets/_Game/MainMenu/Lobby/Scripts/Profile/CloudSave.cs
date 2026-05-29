using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models;
using UnityEngine;

public class CloudSave : MonoBehaviour
{
    public static CloudSave Instance { set; get; }

    // ── Core player data keys ─────────────────────────────────────────────────
    public const string KeyPlayerName        = "player_name";
    public const string KeyCash              = "cash";
    public const string KeyTotalMatches      = "total_matches";
    public const string KeyTotalWins         = "total_wins";
    public const string KeyTotalLoses        = "total_loses";
    public const string KeyPlayerIndex       = "player_index";
    public const string KeyHatIndex          = "hat_index";
    public const string KeyMovementType      = "movement_type";
    public const string KeyIdleType          = "idle_type";
    public const string KeyHardPunchType     = "hard_punch_type";
    public const string KeyHardKickType      = "hard_kick_type";
    public const string KeyCelebrationType   = "celebration_type";
    public const string KeyShopData          = "shop_data";

    // ── Phase 2 keys — stubs, populated by their respective tasks ─────────────
    public const string KeyXp                = "xp";
    public const string KeyLevel             = "level";
    public const string KeyGems              = "gems";
    public const string KeyChallengeProgress = "challenge_progress";
    public const string KeyOwnedCosmetics    = "owned_cosmetics";
    public const string KeyRoomData          = "room_data";
    public const string KeyOwnedMusic        = "owned_music";
    public const string KeySeasonalItems     = "seasonal_items";

    private static readonly HashSet<string> AllKeys = new HashSet<string>
    {
        KeyPlayerName, KeyCash,
        KeyTotalMatches, KeyTotalWins, KeyTotalLoses,
        KeyPlayerIndex, KeyHatIndex, KeyMovementType,
        KeyIdleType, KeyHardPunchType, KeyHardKickType, KeyCelebrationType,
        KeyShopData,
        KeyXp, KeyLevel, KeyGems,
        KeyChallengeProgress, KeyOwnedCosmetics, KeyRoomData, KeyOwnedMusic, KeySeasonalItems
    };

    private void Awake()
    {
        Instance = this;
    }

    private bool IsSignedIn =>
        AuthenticationService.Instance != null && AuthenticationService.Instance.IsSignedIn;

    // ── Public API ────────────────────────────────────────────────────────────

    // Called by AuthManager.SignInConfirmAsync() after every successful sign-in.
    // Downloads all cloud keys and merges them into the live SaveDataLocal state.
    public async Task InitializeAsync()
    {
        if (!IsSignedIn) return;

        try
        {
            var cloud = await CloudSaveService.Instance.Data.Player.LoadAsync(AllKeys);
            MergeCoreData(cloud);
            MergeShopData(cloud);
        }
        catch (CloudSaveException e)
        {
            // Non-fatal — player continues with local data.
            Debug.LogWarning($"[CloudSave] InitializeAsync: {e.Reason} — {e.Message}");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[CloudSave] InitializeAsync unexpected error: {e.Message}");
        }
    }

    // Fire-and-forget from SaveDataLocal.SaveGame().
    // Pushes all syncable fields to the cloud key-value store.
    public async Task SaveCoreDataAsync()
    {
        if (!IsSignedIn) return;

        try
        {
            var save = SaveDataLocal.Instance;

            var data = new Dictionary<string, object>
            {
                { KeyPlayerName,      save.playerName ?? string.Empty },
                { KeyCash,            save.cash },
                { KeyGems,            save.gems },
                { KeyXp,              save.xp },
                { KeyLevel,           save.level },
                { KeyTotalMatches,    save.totalMatches },
                { KeyTotalWins,       save.totalWins },
                { KeyTotalLoses,      save.totalLoses },
                { KeyPlayerIndex,     save.currentPlayerIndex },
                { KeyHatIndex,        save.currentHatIndex },
                { KeyMovementType,    save.currentMovementType },
                { KeyIdleType,        save.currentIdleType },
                { KeyHardPunchType,   save.currentHardPunchType },
                { KeyHardKickType,    save.currentHardKickType },
                { KeyCelebrationType, save.currentCelebrationType },
            };

            if (AllFightShopData.Instance != null)
            {
                var shopSnapshot = new FightShopData
                {
                    introItems     = AllFightShopData.Instance.introItems,
                    hardPunchItems = AllFightShopData.Instance.hardPunchItems,
                    hardKickItems  = AllFightShopData.Instance.hardKickItems,
                    celebrateItems = AllFightShopData.Instance.celebrateItems,
                };
                data[KeyShopData] = JsonUtility.ToJson(shopSnapshot);
            }

            data[KeyOwnedCosmetics] = BuildSkinSnapshot();

            await CloudSaveService.Instance.Data.Player.SaveAsync(data);
        }
        catch (CloudSaveException e)
        {
            Debug.LogWarning($"[CloudSave] SaveCoreDataAsync: {e.Reason} — {e.Message}");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[CloudSave] SaveCoreDataAsync unexpected error: {e.Message}");
        }
    }

    // Generic getter for Phase 2 systems (XP, gems, room, music, etc.).
    public async Task<T> GetValueAsync<T>(string key, T defaultValue)
    {
        if (!IsSignedIn) return defaultValue;

        try
        {
            var result = await CloudSaveService.Instance.Data.Player.LoadAsync(
                new HashSet<string> { key });

            if (result.TryGetValue(key, out var item))
                return item.Value.GetAs<T>();
        }
        catch (CloudSaveException e)
        {
            Debug.LogWarning($"[CloudSave] GetValueAsync({key}): {e.Message}");
        }

        return defaultValue;
    }

    // Generic setter for Phase 2 systems.
    public async Task SetValueAsync(string key, object value)
    {
        if (!IsSignedIn) return;

        try
        {
            await CloudSaveService.Instance.Data.Player.SaveAsync(
                new Dictionary<string, object> { { key, value } });
        }
        catch (CloudSaveException e)
        {
            Debug.LogWarning($"[CloudSave] SetValueAsync({key}): {e.Message}");
        }
    }

    // Force-refreshes gem balance from cloud — call after IAP purchase resolves in Task 1.4.
    public async Task GetGemBalance()
    {
        if (!IsSignedIn) return;
        try
        {
            var result = await CloudSaveService.Instance.Data.Player.LoadAsync(
                new HashSet<string> { KeyGems });
            if (TryGet<int>(result, KeyGems, out var balance))
            {
                SaveDataLocal.Instance.gems = balance;
                SaveDataLocal.Instance.SaveGame();
            }
        }
        catch (CloudSaveException e)
        {
            Debug.LogWarning($"[CloudSave] GetGemBalance: {e.Message}");
        }
    }

    // Client-side gem deduction for spending (cosmetics, boosts). Never used for awarding.
    public bool SpendGems(int amount)
    {
        if (SaveDataLocal.Instance.gems < amount) return false;
        SaveDataLocal.Instance.gems -= amount;
        SaveDataLocal.Instance.SaveGame();
        return true;
    }

    // ── Skin ownership snapshot helpers ──────────────────────────────────────

    private static string BuildSkinSnapshot()
    {
        var snapshot = new SkinOwnershipData();
        var allPlayerData = UnityEngine.Object.FindObjectOfType<AllPlayerData>();
        if (allPlayerData != null)
        {
            for (int ci = 0; ci < allPlayerData.player.Length; ci++)
            {
                var slot = new CharacterSkinSlot { characterIndex = ci };
                var skins = allPlayerData.player[ci].skins;
                if (skins != null)
                {
                    for (int si = 0; si < skins.Length; si++)
                    {
                        if (si == 0 || skins[si].currentStatus == "owned")
                            slot.ownedSkins.Add(si);
                    }
                }
                snapshot.characters.Add(slot);
            }
        }
        return JsonUtility.ToJson(snapshot);
    }

    private static void MergeOwnedSkins(Dictionary<string, Item> cloud)
    {
        if (!TryGet<string>(cloud, KeyOwnedCosmetics, out var json) || string.IsNullOrEmpty(json))
            return;

        SkinOwnershipData cloudData;
        try { cloudData = JsonUtility.FromJson<SkinOwnershipData>(json); }
        catch { return; }
        if (cloudData?.characters == null) return;

        var allPlayerData = UnityEngine.Object.FindObjectOfType<AllPlayerData>();
        if (allPlayerData == null) return;

        foreach (var slot in cloudData.characters)
        {
            if (slot.characterIndex < 0 || slot.characterIndex >= allPlayerData.player.Length) continue;
            var skins = allPlayerData.player[slot.characterIndex].skins;
            if (skins == null) continue;
            foreach (var si in slot.ownedSkins)
            {
                if (si > 0 && si < skins.Length && skins[si].currentStatus != "owned")
                    skins[si].currentStatus = "owned";
            }
        }
    }

    // ── Merge helpers ─────────────────────────────────────────────────────────

    private static void MergeCoreData(Dictionary<string, Item> cloud)
    {
        var save = SaveDataLocal.Instance;

        // player_name: cloud wins — auth service is the source of truth for name
        if (TryGet<string>(cloud, KeyPlayerName, out var cloudName) && !string.IsNullOrEmpty(cloudName))
            save.playerName = cloudName;

        // xp/level: take higher — progression only goes up
        if (TryGet<int>(cloud, KeyXp, out var cloudXp))
            save.xp = TakeHigher(save.xp, cloudXp);
        if (TryGet<int>(cloud, KeyLevel, out var cloudLevel))
            save.level = TakeHigher(save.level, cloudLevel);

        // gems: cloud always wins — prevents client-side inflation of premium currency
        if (TryGet<int>(cloud, KeyGems, out var cloudGems))
            save.gems = cloudGems;

        // stats & currency: take higher — these counters only go up
        if (TryGet<int>(cloud, KeyCash, out var cloudCash))
            save.cash = TakeHigher(save.cash, cloudCash);
        if (TryGet<int>(cloud, KeyTotalMatches, out var cloudMatches))
            save.totalMatches = TakeHigher(save.totalMatches, cloudMatches);
        if (TryGet<int>(cloud, KeyTotalWins, out var cloudWins))
            save.totalWins = TakeHigher(save.totalWins, cloudWins);
        if (TryGet<int>(cloud, KeyTotalLoses, out var cloudLoses))
            save.totalLoses = TakeHigher(save.totalLoses, cloudLoses);

        // skin ownership: owned never reverts
        MergeOwnedSkins(cloud);

        // cosmetic selections: local wins unless local is still at zero (fresh install on this device)
        if (save.currentPlayerIndex == 0 && TryGet<int>(cloud, KeyPlayerIndex, out var ci))
            save.currentPlayerIndex = ci;
        if (save.currentHatIndex == 0 && TryGet<int>(cloud, KeyHatIndex, out var hi))
            save.currentHatIndex = hi;
        if (save.currentMovementType == 0 && TryGet<int>(cloud, KeyMovementType, out var mt))
            save.currentMovementType = mt;
        if (save.currentIdleType == 0 && TryGet<int>(cloud, KeyIdleType, out var it))
            save.currentIdleType = it;
        if (save.currentHardPunchType == 0 && TryGet<int>(cloud, KeyHardPunchType, out var hp))
            save.currentHardPunchType = hp;
        if (save.currentHardKickType == 0 && TryGet<int>(cloud, KeyHardKickType, out var hk))
            save.currentHardKickType = hk;
        if (save.currentCelebrationType == 0 && TryGet<int>(cloud, KeyCelebrationType, out var ct))
            save.currentCelebrationType = ct;
    }

    private static void MergeShopData(Dictionary<string, Item> cloud)
    {
        if (AllFightShopData.Instance == null) return;
        if (!TryGet<string>(cloud, KeyShopData, out var cloudJson) || string.IsNullOrEmpty(cloudJson)) return;

        try
        {
            var cloudShop = JsonUtility.FromJson<FightShopData>(cloudJson);
            if (cloudShop == null) return;

            // For each array: if cloud says an item is "owned", it stays owned — never reverts.
            MergeOwnedArray(AllFightShopData.Instance.introItems,     cloudShop.introItems);
            MergeOwnedArray(AllFightShopData.Instance.hardPunchItems, cloudShop.hardPunchItems);
            MergeOwnedArray(AllFightShopData.Instance.hardKickItems,  cloudShop.hardKickItems);
            MergeOwnedArray(AllFightShopData.Instance.celebrateItems, cloudShop.celebrateItems);

            // Persist merged ownership to disk
            AllFightShopData.Instance.SaveData();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[CloudSave] MergeShopData parse error: {e.Message}");
        }
    }

    // Applies "owned never reverts" rule across a parallel array pair.
    // Uses reflection so the same method works for all four item types.
    private static void MergeOwnedArray<T>(T[] local, T[] cloud) where T : class
    {
        if (local == null || cloud == null) return;

        FieldInfo field = typeof(T).GetField("currentStatus");
        if (field == null) return;

        int len = Math.Min(local.Length, cloud.Length);
        for (int i = 0; i < len; i++)
        {
            if ((string)field.GetValue(cloud[i]) == "owned" &&
                (string)field.GetValue(local[i]) != "owned")
            {
                field.SetValue(local[i], "owned");
            }
        }
    }

    private static int TakeHigher(int local, int cloud) => Math.Max(local, cloud);

    private static bool TryGet<T>(Dictionary<string, Item> dict, string key, out T value)
    {
        if (dict.TryGetValue(key, out var item))
        {
            try
            {
                value = item.Value.GetAs<T>();
                return true;
            }
            catch { }
        }
        value = default;
        return false;
    }
}

[Serializable]
class SkinOwnershipData
{
    public List<CharacterSkinSlot> characters = new List<CharacterSkinSlot>();
}

[Serializable]
class CharacterSkinSlot
{
    public int characterIndex;
    public List<int> ownedSkins = new List<int> { 0 };
}
