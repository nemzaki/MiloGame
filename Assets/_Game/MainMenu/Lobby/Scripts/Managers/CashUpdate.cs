using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CashUpdate : MonoBehaviour
{
    public TextMeshProUGUI cashText;
    public TextMeshProUGUI gemsText;   // assign in Inspector — optional until gem UI is placed
    public TextMeshProUGUI levelText;  // "LVL 12" — assign when XP UI is placed
    public Slider xpBar;               // 0–1 fill — assign when XP UI is placed

    public void Update()
    {
        cashText.text = SaveDataLocal.Instance.cash.ToString();

        if (gemsText != null)
            gemsText.text = SaveDataLocal.Instance.gems.ToString();

        if (levelText != null)
            levelText.text = $"LVL {SaveDataLocal.Instance.level}";

        if (xpBar != null)
        {
            int xpNeeded = XPManager.XpForLevel(SaveDataLocal.Instance.level);
            xpBar.value = xpNeeded > 0
                ? (float)SaveDataLocal.Instance.xp / xpNeeded
                : 1f;
        }
    }
}
