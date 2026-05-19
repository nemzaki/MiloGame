using System;
using Quantum;
using TMPro;
using UnityEngine;

public class PlayerMatchRewards : MonoBehaviour
{

    public RewardItem punchReward;
    public RewardItem kickReward;
    public RewardItem punchHardReward;
    public RewardItem kickHardReward;
    
    public GameObject winReward;
    public TextMeshProUGUI winRewardText;
    
    public int rewardAmount = 100;
    public int hardRewardAmount = 150;
    public int winRewardAmount = 1000;
    
    public int totalRewardAmount = 0;
    public bool claimedReward = false;
    public TextMeshProUGUI totalRewardText;
    
    public GameObject claimRewardButton;

    private bool _registerMatchStat;
    
    void Update()
    {
        var localPlayer = GameManager.Instance.localPlayer;
        
        if(localPlayer == null)
            return;
        
        var entityView = localPlayer.GetComponent<QuantumEntityView>();
        
        if (QuantumRunner.Default == null || !QuantumRunner.Default.IsRunning)
            return;

        var game = QuantumRunner.Default.Game;
        if (game == null)
            return;

        var frame = game.Frames.Verified;
        if (frame == null)
            return;
        
        if(!frame.Exists(entityView.EntityRef))
            return;
        
        var playerStat = frame.Get<PlayerStat>(entityView.EntityRef);
        
        if(!GameManager.Instance.gameFinished)
            return;

        punchReward.comboAmount.text = "X" + playerStat.PunchAmount;
        kickReward.comboAmount.text = "X" + playerStat.KickAmount;
        punchHardReward.comboAmount.text = "X" + playerStat.PunchHardAmount;
        kickHardReward.comboAmount.text = "X" + playerStat.KickHardAmount;

        punchReward.rewardAmount = playerStat.PunchAmount * rewardAmount;
        kickReward.rewardAmount = playerStat.KickAmount * rewardAmount;
        punchHardReward.rewardAmount = playerStat.PunchHardAmount * hardRewardAmount;
        kickHardReward.rewardAmount = playerStat.KickHardAmount * hardRewardAmount;

        punchReward.rewardAmountText.text = punchReward.rewardAmount.ToString();
        kickReward.rewardAmountText.text = kickReward.rewardAmount.ToString();
        punchHardReward.rewardAmountText.text = punchHardReward.rewardAmount.ToString();
        kickHardReward.rewardAmountText.text = kickHardReward.rewardAmount.ToString();

        var addWinReward = 0;
        if (playerStat.Won)
        {
            winReward.SetActive(true);
            winRewardText.text = winRewardAmount.ToString();
            addWinReward = winRewardAmount;

            if (!_registerMatchStat)
            {
                //Register Win Stat
                if (!frame.RuntimeConfig.training)
                {
                    SaveDataLocal.Instance.totalWins += 1;
                    SaveDataLocal.Instance.SaveGame();
                    Debug.Log("Register new win "+SaveDataLocal.Instance.totalWins);
                }
                
                _registerMatchStat = true;
            }
        }
        else
        {
            winReward.SetActive(false);
            addWinReward = 0;
            
            if (!_registerMatchStat)
            {
                //Register Loss Stat
                if (!frame.RuntimeConfig.training)
                {
                    SaveDataLocal.Instance.totalLoses += 1;
                    SaveDataLocal.Instance.SaveGame();
                    Debug.Log("Register new Lost "+SaveDataLocal.Instance.totalWins);
                }
                
                _registerMatchStat = true;
            }
        }
        
        totalRewardAmount = punchReward.rewardAmount + punchHardReward.rewardAmount + kickHardReward.rewardAmount + kickHardReward.rewardAmount + addWinReward;
        totalRewardText.text = totalRewardAmount.ToString();
    }

    public void ClaimReward()
    {
        if (claimedReward) return;
        SaveDataLocal.Instance.cash += totalRewardAmount;
        SaveDataLocal.Instance.SaveGame();
        claimRewardButton.SetActive(false);
        claimedReward = true;
    }
}

[Serializable]
public class RewardItem
{
    public int rewardAmount;
    public TextMeshProUGUI comboAmount;
    public TextMeshProUGUI rewardAmountText;
}