using System;
using System.Collections.Generic;
using Quantum;
using TMPro;
using UnityEngine;

public unsafe class GameMatchInfo : MonoBehaviour
{
    public static GameMatchInfo Instance { get; private set; }
    
    [Header("Game Info")]
    public TextMeshProUGUI gameTimeText;
    public GameObject trainingIcon;

    public bool showedGameOverScreen;
    
    private void Awake()
    {
        Instance = this;
    }
    
    private void OnDisable()
    {
        QuantumEvent.UnsubscribeListener(this);
    }
    
    private void Update()
    {
        var frame = QuantumRunner.Default?.Game?.Frames.Verified;
        if(frame == null)
            return;
        
        GetInfo(frame);
        GameOver(frame);
    }

    private void GetInfo(Frame frame)
    {
        if(!GameManager.Instance.gameInProgress)
            return;
        
        var gameplay = frame.Unsafe.GetPointerSingleton<Quantum.Gameplay>();

        var currentGameTime = frame.RuntimeConfig.gameTime - gameplay->InGameMatchTime;
        gameTimeText.text = currentGameTime.ToString("F0");
        
        gameTimeText.gameObject.SetActive(!frame.RuntimeConfig.training);
        trainingIcon.SetActive(frame.RuntimeConfig.training);
    }

    private void GameOver(Frame frame)
    {
        var playerEntity = GameManager.Instance.localPlayerEntity;
        
        if(!frame.Exists(playerEntity))
            return;
        
        var playerStat = frame.Get<PlayerStat>(playerEntity);
        
        if (showedGameOverScreen) return;
        
        if (GameManager.Instance.gameFinished && !playerStat.TechnicalVictory && !playerStat.TechnicalDefeat)
        {
            InGameUIHandler.Instance.GameOver();
            showedGameOverScreen = true;
        }
        
        if (GameManager.Instance.gameFinished && playerStat.TechnicalVictory)
        {
            //Register Win Stat
            if (!frame.RuntimeConfig.training)
            {
                SaveDataLocal.Instance.totalWins += 1;
                SaveDataLocal.Instance.SaveGame();
                Debug.Log("Register new win "+SaveDataLocal.Instance.totalWins);
            }
            
            InGameUIHandler.Instance.TechnicalVictory();
            showedGameOverScreen = true;
        }

        if (GameManager.Instance.gameFinished && playerStat.TechnicalDefeat)
        {
            InGameUIHandler.Instance.SurrenderScreen();
        }
    }
}

