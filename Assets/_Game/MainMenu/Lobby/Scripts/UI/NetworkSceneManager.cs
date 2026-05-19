using System;
using System.Collections;
using System.Collections.Generic;
using Quantum;
using Quantum.Demo;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkSceneManager : QuantumCallbacks
{
    public static NetworkSceneManager Instance { set; get; }

    [Header("Loading")] 
    public GameObject loadingPanel;
    
    [Header("Game State")] 
    public bool singlePlayer;
    public bool multiplayer;

    [Header("CURRENT ROOM")] [Space(10)] 
    public int totalPlayersInRoom;

    [Header("Scene Data")] 
    public int currentMapIndex;
    public AssetGuid mapId;

    [Header("Screens")] 
    public GameObject technicalDefeatScreen;
    public GameObject badConnectionPanel;
    
    void Awake()
    {
        if (Instance != this && Instance != null)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
        DontDestroyOnLoad(this);
    }

    #region Scenes
    public void MainMenu()
    {
        if(GameSceneNavHandler.Instance == null)
            return;
        
        //Destroy the current runner
        if (singlePlayer)
        {
            QuantumRunner.Default.Shutdown();
        }
        
        GameSceneNavHandler.Instance.mainMenuScene.SetActive(true);
        GameSceneNavHandler.Instance.mainMenuCanvas.gameObject.SetActive(true);
        GameSceneNavHandler.Instance.menuManager.SetActive(true);
    }

    public void InGame()
    {
        GameSceneNavHandler.Instance.mainMenuScene.SetActive(false);
        GameSceneNavHandler.Instance.mainMenuCanvas.gameObject.SetActive(false);
        GameSceneNavHandler.Instance.menuManager.SetActive(false);
    }
    #endregion


    #region UI
    private void HideAllPanels()
    {
        loadingPanel.SetActive(false);
    }
    
    #endregion

    public void LoadScene(string scene)
    {
        StartCoroutine(LoadGameScene(scene));
    }
    
    private IEnumerator LoadGameScene(string scene)
    {
        var asyncLoad = SceneManager.LoadSceneAsync(scene);

        while (asyncLoad != null && !asyncLoad.isDone)
        {
            Debug.Log($"Loading progress: {asyncLoad.progress}");
            yield return null;
        }
    }
    
    public void UnloadScene(string scene)
    {
        SceneManager.UnloadSceneAsync(scene);
    }
    
    public override void OnUnitySceneLoadBegin(QuantumGame game)
    {
        HideAllPanels();
        loadingPanel.SetActive(true);
    }

    public override void OnUnitySceneLoadDone(QuantumGame game)
    {
        HideAllPanels();
    }

    private void UpdateRoomInfo()
    {
        if(UIClientHandler.Instance.Client == null)
            return;
        
        if(!UIClientHandler.Instance.Client.InRoom)
            return;

        totalPlayersInRoom = UIClientHandler.Instance.Client.CurrentRoom.PlayerCount;
    }

    public void CloseLoadingPanel()
    {
        if(loadingPanel.activeSelf)
            loadingPanel.SetActive(false);
    }

    public void OpenTechnicalDefeatScreen()
    {
        if(!multiplayer)
            return;
        
        technicalDefeatScreen.SetActive(true);
    }

    public void OpenRewardDefeat()
    {
        InGameUIHandler.Instance.OpenRewardScreen();
    }
    
    private void Update()
    {
        //UpdateRoomInfo();
    }
}















