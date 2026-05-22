using UnityEngine;
using UnityEngine.Events;

public class XPManager : MonoBehaviour
{
    public static XPManager Instance { get; private set; }

    // Subscribe to receive the new level when the player levels up.
    public UnityEvent<int> OnLevelUp;

    private const int MaxLevel = 100;

    private void Awake()
    {
        Instance = this;
    }

    // XP required to complete a given level (level 1 needs 200, level 5 needs 1000).
    public static int XpForLevel(int level) => level * 200;

    public void AwardXP(int amount, string source = "")
    {
        var save = SaveDataLocal.Instance;
        if (save.level >= MaxLevel) return;

        save.xp += amount;
        Debug.Log($"[XPManager] +{amount} XP ({source}) | Total: {save.xp} / {XpForLevel(save.level)} to level {save.level + 1}");

        // Handle multi-level-up in one call (e.g. a big challenge completion).
        while (save.level < MaxLevel && save.xp >= XpForLevel(save.level))
        {
            save.xp -= XpForLevel(save.level);
            save.level++;
            OnLevelUp?.Invoke(save.level);
            Debug.Log($"[XPManager] Level up → {save.level}");
        }

        save.SaveGame();
    }
}
