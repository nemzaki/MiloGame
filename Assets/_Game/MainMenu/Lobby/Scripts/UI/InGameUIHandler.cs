using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using MoreMountains.Feedbacks;
using Quantum;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class InGameUIHandler : MonoBehaviour
{

    public static InGameUIHandler Instance { get; private set; }
    
    [Header("PLAYER STATE")]
    public bool isPlayerActive;
    
    [Header("CONTROLS")] 
    public GameObject walkingControls;
    
    [Header("SCREENS")]
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject gameOverMenu;
    [SerializeField] private GameObject inGameMenu;
    [SerializeField] private GameObject rewardsMenu;
    [SerializeField] private GameObject technicalVictoryScreen;
    
    public GameObject fadeScreen;

    [Header("CONNECTION")] 
    public GameObject warningBadConnectionPanel;
    public GameObject connectionBadPanel;
    
    [Header("PANELS")] 
    public GameObject roundPanel;
    public TextMeshProUGUI currentRoundText;
    public TextMeshProUGUI fightText;
    public GameObject switchRoundScreen;
    
    public TextMeshProUGUI wonText;
    public TextMeshProUGUI winInfoText;
    
    public GameObject suddenDeathPanel;
    
    [Header("Buttons")] 
    public GameObject reloadButton;

    [Header("Player Stat")] 
    [Space(10)] 
    public Color roundWonNormalColor;
    public Color roundWonActiveColor;
    public PlayerHealth[] playerHealth;
    public bool checkPlayerWon;

    public string winnerName;
    public string disconnectedPlayerName;
    
    [Header("AI")] 
    public GameObject aiIcon;

    [Header("Feels")] 
    public MMF_Player uiFeels;
    public MMF_Player uiFeelsHardHit;
    public MMF_Player uiFeelNoStamina;
    
    private bool _registerTechnicalDefeat;
    
    private void Start()
    {
        Instance = this;
        
        QuantumEvent.Subscribe<EventRoundOver>(this, RoundOverEvent);
        QuantumEvent.Subscribe<EventPrepareRound>(this, PrepareRoundEvent);
        QuantumEvent.Subscribe<EventStartNewRound>(this, StartRoundEvent);
    }

    private void OnDisable()
    {
        QuantumEvent.UnsubscribeListener<EventRoundOver>(this);
        QuantumEvent.UnsubscribeListener<EventPrepareRound>(this);
        QuantumEvent.UnsubscribeListener<EventStartNewRound>(this);
        
        QuantumCallback.UnsubscribeListener(this);
        UIClientHandler.Instance.Client?.RemoveCallbackTarget(this);
        
        UIClientHandler.Instance.Client?.AddCallbackTarget(this);
        QuantumCallback.Subscribe(this, (CallbackPluginDisconnect c) => OnCallbackPluginDisconnect(c.Reason));
    }
    
    private void Update()
    {
        if(!QuantumRunner.Default)
            return;

        var frame = QuantumRunner.Default.Game.Frames.Predicted;
        if(frame == null)return;

        var gameplay = frame.GetSingleton<Gameplay>();

        if (gameplay.WinnerEntity != EntityRef.None)
        {
            var playerStat = frame.Get<PlayerStat>(gameplay.WinnerEntity);
            winnerName = playerStat.PlayerName;
        }

        if (gameplay.DisconnectedEntity != EntityRef.None)
        {
            var playerStat = frame.Get<PlayerStat>(gameplay.DisconnectedEntity);
            disconnectedPlayerName = playerStat.PlayerName;
        }
        
        aiIcon.SetActive(frame.RuntimeConfig.training);
        suddenDeathPanel.SetActive(gameplay.SuddenDeath);
    }
    
    private void HandeAllPanel()
    {
        pauseMenu.SetActive(false);
        gameOverMenu.SetActive(false);
        inGameMenu.SetActive(false);
        rewardsMenu.SetActive(false);
        technicalVictoryScreen.SetActive(false);
    }

    public void PauseGame()
    {
        HandeAllPanel();
        pauseMenu.SetActive(true);
    }

    public void ResumeGame()
    { 
        HandeAllPanel();
        inGameMenu.SetActive(true);
    }

    public void GameOver()
    {
        HandeAllPanel();
        gameOverMenu.SetActive(true);

        wonText.text = winnerName + " won!";
    }

    public void TechnicalVictory()
    {
        HandeAllPanel();
        technicalVictoryScreen.SetActive(true);
        
        winInfoText.text = disconnectedPlayerName + " disconnected!";
    }
    
    
    public void OpenRewardScreen()
    {
        HandeAllPanel();
        rewardsMenu.SetActive(true);
    }
    
    //NETWORK
    private void OnCallbackPluginDisconnect(string reason) {
        Debug.Log("NETWORK: DISCONNECTED "+reason);
        UIClientHandler.Instance.Client.Disconnect();
    }

    public void OnLeaveClicked()
    {
        if (NetworkSceneManager.Instance.multiplayer)
        {
            if(!UIClientHandler.Instance.connected)
                return;
        
            UIClientHandler.Instance.Client.Disconnect();
            gameObject.SetActive(false);
        }
        else if(NetworkSceneManager.Instance.singlePlayer)
        {
            // Unload the game scene if it's loaded additively
            //SceneManager.UnloadSceneAsync("RaceTrack");
            
            //Unload any active scene except the first one
            for (var i = 1; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.isLoaded)
                {
                    SceneManager.UnloadSceneAsync(scene);
                }
            }
        }
      
        NetworkSceneManager.Instance.MainMenu();
    }

    public void SurrenderScreen()
    {
        if (!_registerTechnicalDefeat)
        {
            var frame = QuantumRunner.Default?.Game?.Frames.Verified;
            if(frame == null)
                return;
            
            //Register Lose Stat
            if (!frame.RuntimeConfig.training)
            {
                SaveDataLocal.Instance.totalLoses += 1;
                SaveDataLocal.Instance.SaveGame();
            }
            
            NetworkSceneManager.Instance.OpenTechnicalDefeatScreen();

            //Show reason for disconnect - Bad connection locally
            NetworkSceneManager.Instance.badConnectionPanel.SetActive(QuantumLocalInput.Instance.constantBadConnection);

            _registerTechnicalDefeat = true;
        }
    }
    
    //QUANTUM EVENTS
    private void RoundOverEvent(EventRoundOver eventRoundOver)
    {
        inGameMenu.SetActive(false);
    }

    private void PrepareRoundEvent(EventPrepareRound prepareRound)
    {
        switchRoundScreen.SetActive(true);
    }

    private void StartRoundEvent(EventStartNewRound startRound)
    {
        inGameMenu.SetActive(true);
        StartCoroutine(RoundInfo(startRound.CurrentRound));
    }

    IEnumerator RoundInfo(int currentRound)
    {
        roundPanel.SetActive(true);
        currentRoundText.text = currentRound.ToString();
        yield return new WaitForSeconds(0.5f);
        switchRoundScreen.SetActive(false);
        yield return new WaitForSeconds(2);
        roundPanel.SetActive(false);
        fightText.gameObject.SetActive(true);
    }
}

[Serializable]
public class PlayerHealth
{
    public TextMeshProUGUI playerName;
    public ProgressBarPro playerHealthSlider;
    public ProgressBarPro playerStaminaSlider;
    public Image round1Won;
    public Image round2Won;
    public GameObject hitCounter;
    public TextMeshProUGUI hitCounterText;
    public GameObject hardHitIndicator;
    public GameObject noStaminaIndicator;
    public GameObject badPingIndicator;
}















