using System;
using TMPro;
using UnityEngine;

public class PlayerMatchStats : MonoBehaviour
{
    public TextMeshProUGUI totalMatchesText;
    public TextMeshProUGUI totalWinText;
    public TextMeshProUGUI totalLoseText;

    private void Update()
    {
        totalMatchesText.text = SaveDataLocal.Instance.totalMatches.ToString();
        totalWinText.text = SaveDataLocal.Instance.totalWins.ToString();
        totalLoseText.text = SaveDataLocal.Instance.totalLoses.ToString();
    }
}
