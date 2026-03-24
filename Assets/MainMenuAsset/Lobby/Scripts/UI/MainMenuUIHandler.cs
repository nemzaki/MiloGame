  using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Client;
using UnityEngine;

using UnityEngine.UI;
using TMPro;
using Photon.Realtime;
using Quantum;
using Quantum.Demo;
using Quantum.Demo.Legacy;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;


public class MainMenuUIHandler : MonoBehaviour
{
 
    public static MainMenuUIHandler Instance { set; get; }
    
    //Connection
    public LobbyConnectionHandler _connectionHandler;
    
    [SerializeField] private CheckInternetConnection checkInternetConnection;
    
    [Header("PLAYER DETAILS")] 
    [Space(10)] 
    public TextMeshProUGUI playerNameText;
    public string playerName = "";
    
    #region ScreensAndPanels

    [Header("MAIN SCREENS")] 
    [Space(10)] public GameObject startSplashScreen;
    
    public GameObject mainScreen;
    public GameObject gameShopScreen;
    
    [SerializeField] CanvasGroup mainPanel;
    
    [SerializeField] CanvasGroup playerPanel;

    [SerializeField] CanvasGroup sessionPanel;

    [SerializeField] private CanvasGroup roomPanel;

    [SerializeField] private CanvasGroup roomCreatePanel;

    [SerializeField] private CanvasGroup connectingPanel;

    [SerializeField] private CanvasGroup settingsPanel;
    
    [SerializeField] private GameObject errorPanel;

    [SerializeField] private CanvasGroup privateRoomPanel;

    [SerializeField] private CanvasGroup gameModePanel;
    
    [SerializeField] private CanvasGroup portalPanel;
    #endregion
    
    #region Room
    [Header("ROOM DETAILS")]
    [Space(10)]
    [SerializeField]
    VerticalLayoutGroup roomMemberListLayoutGroup;

    [SerializeField]
    GameObject roomMemberNamePrefab;

    [Header("SESSION DETAILS")]
    [Space(10)]
    public VerticalLayoutGroup sessionListLayoutGroup;
    public GameObject sessionListPrefab;
    
    [Header("GAME SETTINGS")] 
    [Space(10)]
    //public PhotonRegions selectableRegions;
    public string gameMode;
    public int mapIndex;
    public string mapName;
    public long mapGuid;
    public int maxPlayers;
    public string privateRoom;
    public bool anyTimeJoinRoom;
    public string isGameStarted;
    public string joinRoomHashCode;
    public bool singlePlayer;
    public bool multiPlayer;
    public bool training;
    
    //Modes
    public bool isQuickPlayAutoJoinEnabled = false;
    public bool isCreatingRoom = false;

    [Header("ROOM CREATE")] 
    [Space(10)]
    public TextMeshProUGUI currentGameModeText;
    public TextMeshProUGUI roomCodeText;
    public GameObject roomCodeTextDisplay;
    public GameObject roomInfoDisplay;
    public TMP_InputField joinCustomRoomField;
    public TextMeshProUGUI currentPlayersInRoom;
    public TextMeshProUGUI maxPlayersInRoom;
    
    [Header("Region")] 
    [Space(10)]
    public Regions[] region;
    public int currentRegionIndex;
    public TextMeshProUGUI regionText;
    
    [Header("Max Players")] 
    [Space(10)]
    public GameObject setMaxPlayersObj;
    public int currentMaxPlayersIndex;
    public TextMeshProUGUI maxPlayersText;
    public int[] maxPlayersValues;
    public bool addAI;
    
    [Header("Private Room")] 
    public bool isPrivateRoom;
    
    [Header("Auto Match Making")] 
    [Space(10)]
    public TextMeshProUGUI startTimeText;
    public GameObject autoMatchMakingObj;
    
    #endregion

    #region ErrorHandling
    [Header("ERROR")] 
    [Space(10)]
    public TextMeshProUGUI errorCodeText;
    public TextMeshProUGUI errorText;
    #endregion

    #region Connection
    [Header("CONNECTION")] 
    [Space(10)]
    public TextMeshProUGUI connectingInfoText;
    public GameObject connectionScreen;
    public TextMeshProUGUI connectionStatus;
    private float _connectingTimer;
    public float retryConnectionTime = 3;
    private float _retryConnectionTimer;
    public TextMeshProUGUI retryConnectionText;
    private bool _retryingConnection;
    #endregion
    
    [Header("Loading Panel")] 
    public GameObject checkActionStatus;
    public TextMeshProUGUI actionMessageText;
    
    [Header("UI Elements")] 
    public GameObject startGameButton;

    [Header("Menu State")]
    public bool inLobby;
    public bool inRoomCreate;
    public bool isConnecting;
    public bool isJoiningGame;

    [Header("MatchMaking")] 
    public TextMeshProUGUI matchmakingInfo;
    
    private void Awake()
    {
        Instance = this;
        
        _connectionHandler = GetComponent<LobbyConnectionHandler>();
        isPrivateRoom = false;
    }
    
    private void Start()
    {
        //Name
        GetPlayerName(SaveDataLocal.Instance.playerName);
        
        var appSettings = PhotonServerSettings.Global.AppSettings;
        currentRegionIndex = SaveDataLocal.Instance.currentRegionIndex;
        appSettings.FixedRegion = region[currentRegionIndex].region;
        
        regionText.text = region[currentRegionIndex].name;
        
        ConnectedToServer();
    }

    private void OnEnable()
    {
        HideAllPanels();
        mainScreen.SetActive(true);
        mainPanel.gameObject.SetActive(true);
    }

    private async void ConnectedToServer()
    {
        try
        {
            startSplashScreen.SetActive(true);
            var isConnected = await checkInternetConnection.CheckHasConnection();
            if (isConnected)
            {
                //Connected
            }
            else
            {
                CloseActionStatus("Error Connecting", 2);
                connectionScreen.SetActive(true);
                return;
            }
            
            _connectionHandler.ConnectToMaster();
        }
        catch (Exception e)
        {
            throw; // TODO handle exception
        }

        isQuickPlayAutoJoinEnabled = false;
    }
    
    private void Update()
    {
        currentGameModeText.text = gameMode;
        
        CheckConnectingScreen();
        RetryConnection();

        maxPlayersInRoom.text = maxPlayers.ToString();
        
        if (UIClientHandler.Instance)
        {
            var aiCount = addAI ? 1 : 0;
            var playerCount = UIClientHandler.Instance.playersInRoom;
            var totalPlayer = aiCount + playerCount;
            currentPlayersInRoom.text = totalPlayer.ToString();
        }
    }
    
    private void CheckConnectingScreen()
    {
        if (connectionScreen.activeSelf)
        {
            _connectingTimer += Time.deltaTime;
            if (_connectingTimer >= 10)
            {
                connectionStatus.gameObject.SetActive(true);   
            }
        }
    }

    private void RetryConnection()
    {
        //RETRY CONNECTION
        if (!_retryingConnection) 
            return;
        
        if (_retryConnectionTimer <= 0)
        {
            _retryingConnection = false;
            _retryConnectionTimer = 0;
            retryConnectionText.gameObject.SetActive(false);
        }
            
        _retryConnectionTimer -= Time.deltaTime;
        retryConnectionText.text = _retryConnectionTimer.ToString("F0");
    }
    
    public async void OnRetryConnection()
    {
        try
        {
            if(_retryingConnection)
                return;

            var isConnected = await checkInternetConnection.CheckHasConnection();
            if (isConnected)
            {
                connectionScreen.SetActive(false);
                
                //Retry Connection to master
                if (!UIClientHandler.Instance.connected)
                {
                    _connectionHandler.ConnectToMaster();
                }
            }
            
            _retryingConnection = true;
            retryConnectionText.gameObject.SetActive(true);
            _retryConnectionTimer = retryConnectionTime;
        }
        catch (Exception e)
        {
            throw; // TODO handle exception
        }
    }
    
    public void GetPlayerName(string getName)
    {
        if (string.IsNullOrEmpty(getName))
        {
            SaveDataLocal.Instance.playerName = "Player#"+Random.Range(1000, 9999);
            SaveDataLocal.Instance.SaveGame();
            playerName = SaveDataLocal.Instance.playerName;
            playerNameText.text = playerName;
        }
        else
        {
            playerName = getName;
            playerNameText.text = getName;
        }
    }
    
    #region MiniPanels 
    public void OpenActionStatus(string message)
    {
        checkActionStatus.SetActive(true);
        actionMessageText.text = message;
    }
    
    IEnumerator CloseStatus(float delay)
    {
        yield return new WaitForSeconds(1);
        checkActionStatus.SetActive(false);
    }
    
    public void CloseActionStatus(string message, float delayTime)
    {
        StartCoroutine(CloseStatus(delayTime));
        actionMessageText.text = message;
    }
    
    
    #endregion --------------->
    
    #region UI Code --------------->
    
    public void HideAllPanels()
    {
        mainPanel.gameObject.SetActive(false);
        roomPanel.gameObject.SetActive(false);
        playerPanel.gameObject.SetActive(false);
        sessionPanel.gameObject.SetActive(false);
        roomCreatePanel.gameObject.SetActive(false);
        connectingPanel.gameObject.SetActive(false);
        settingsPanel.gameObject.SetActive(false);
        gameModePanel.gameObject.SetActive(false);
        portalPanel.gameObject.SetActive(false);
    }

    public void OpenPortal()
    {
        HideAllPanels();
        portalPanel.gameObject.SetActive(true);
    }
    public void OpenGameShopScreen()
    {
        mainScreen.SetActive(false);
        gameShopScreen.SetActive(true);
    }

    public void CloseGameShopScreen()
    {
        mainScreen.SetActive(true);
        gameShopScreen.SetActive(false); 
    }
    
    public void OpenGameModePanel()
    {
        HideAllPanels();
        gameModePanel.gameObject.SetActive(true);
    }
    
    public void OpenRoomPanel()
    {
        HideAllPanels();
        roomPanel.gameObject.SetActive(true);
    }

    public void PlayOnlineRandomGame()
    {
        SelectGameModeQuickPlay("Battle");
        //Set random map later
        //MatchSettings.Instance.SelectMap(Random.Range(0,4));
        MatchSettings.Instance.SelectMap(4);
    }
    
    private void ConnectingPanel(string message)
    {
        isConnecting = true;
        connectingInfoText.text = message;
        connectingPanel.gameObject.SetActive(true);
    }

    public void Error(string errorCode,string message)
    {
        errorText.text = message;
        errorPanel.gameObject.SetActive(true);
        
        //Close Automatically if not connected
        if (inLobby)
        {
            OnExitSessionClicked();
            inLobby = false;
        }
        else if(inRoomCreate)
        {
            OnLeaveRoomCreateClicked();
            inRoomCreate = false;
        }
        else if(isConnecting)
        {
            OnMainPanelClicked();
            isConnecting = false;
        }
        else if(connectingPanel.isActiveAndEnabled)
        {
            OnMainPanelClicked();
        }
        NetworkSceneManager.Instance.CloseLoadingPanel();
    }
    
    void UpdateRoomDetails()
    {
        if(isJoiningGame)
            return;
        
        if (!UIClientHandler.Instance.Client.InRoom)
        {
            Error("ERROR","Client no longer in room, cannot update room details");
            ClearChildrenLayoutGroup(roomMemberListLayoutGroup.transform);
            
            //Return to main screen
            OnMainPanelClicked();
            return;
        }

        ClearChildrenLayoutGroup(roomMemberListLayoutGroup.transform);

        //Loop through the list of players and add them to the UI 
        foreach (KeyValuePair<int, Player> player in UIClientHandler.Instance.Client.CurrentRoom.Players)
        {
            var instantiatedObject = Instantiate(roomMemberNamePrefab, roomMemberListLayoutGroup.transform);
            var roomMemberHandler = instantiatedObject.GetComponent<RoomMemberItemUIHandler>();
            roomMemberHandler.playerNameText.text = player.Value.NickName;
            roomMemberHandler.masterClientIndicator.enabled = player.Value.IsMasterClient;
        }
        
        startGameButton.SetActive(UIClientHandler.Instance.Client.LocalPlayer.IsMasterClient);
    }

    public void ClearChildrenLayoutGroup(Transform layoutGroup)
    {
        //Clear the old UI in reversed order
        for (int i = layoutGroup.childCount - 1; i >= 0; i--)
        {
            Destroy(layoutGroup.GetChild(i).gameObject);
        }
    }

    public IEnumerator UpdateRoomDetailsCo()
    {
        while (roomPanel.gameObject.activeInHierarchy)
        {
            UpdateRoomDetails();
            yield return new WaitForSeconds(1);
        }
    }

    #endregion

    #region UI Events --------------->
    
    private void OnConnectSettings()
    {
    }
    
    public void OnPlayerProfileClicked()
    {
        HideAllPanels();
        playerPanel.gameObject.SetActive(true);
    }
    
    [ContextMenu("Quick Play test")]
    public async void OnQuickPlayClicked()
    {
        try
        {
            //OpenActionStatus("SEARCHING...");
            var isConnected = await checkInternetConnection.CheckHasConnection();
            if (isConnected)
            {
                //CloseActionStatus("SEARCHING", 0);
                HideAllPanels();
                ConnectingPanel("Looking For Match");
                
                //isCreatingRoom = true;

                isQuickPlayAutoJoinEnabled = true;
                
                if (UIClientHandler.Instance.Client != null && UIClientHandler.Instance.Client.IsConnected)
                {
                    _connectionHandler.JoinRandomOrCreateRoom();
                }
                else
                {
                    _connectionHandler.ConnectToMaster();
                }
            
                OnConnectSettings();
            }
            else
            {
                CloseActionStatus("Error Connecting", 2);
                connectionScreen.SetActive(true);
                return;
            }
        }
        catch (Exception e)
        {
            throw; // TODO handle exception
        }
    }
    
    public void OnStartGameClicked()
    {
        if (multiPlayer)
        {
            _connectionHandler.StartGame();
        }
        else if(singlePlayer)
        {
            _connectionHandler.StartSinglePlayerGame();
        }
    }
    
    public async void OnSessionBrowserClicked()
    {
        try
        {
            OpenActionStatus("Loading Lobby...");
            var isConnected = await checkInternetConnection.CheckHasConnection();
            if (isConnected)
            {
                CloseActionStatus("Lobby loaded", 2);
                Debug.Log("Has Connection....");
            }
            else
            {
                CloseActionStatus("Error loading lobby", 2);
                connectionScreen.SetActive(true);
                return;
            }
        
            isQuickPlayAutoJoinEnabled = false;
            isCreatingRoom = false;

            inLobby = true;
        
            HideAllPanels();
            sessionPanel.gameObject.SetActive(true);
            privateRoomPanel.gameObject.SetActive(false);    
        
            if (UIClientHandler.Instance.Client != null && UIClientHandler.Instance.Client.IsConnected)
            {
                _connectionHandler.JoinLobby();
            }
            else
            {
                _connectionHandler.ConnectToMaster();
            }
        }
        catch (Exception e)
        {
            throw; // TODO handle exception
        }
    }

    public void OnRefreshLobbyClicked()
    {
        if(!UIClientHandler.Instance.Client.InLobby)
            return;
        
        if (UIClientHandler.Instance.Client != null && UIClientHandler.Instance.Client.IsConnected)
        {
            _connectionHandler.JoinLobby();
        }
        else
        {
            _connectionHandler.ConnectToMaster();
        }
    }

    public void OnExitSessionClicked()
    {
        inLobby = false;
        HideAllPanels();
        mainPanel.gameObject.SetActive(true);
    }
    
    public void OnLeaveRoomCreateClicked()
    {
        if (UIClientHandler.Instance.inRoom)
        {
           
            LobbyConnectionHandler.Instance.LeaveRoom();
        }
        
        inRoomCreate = false;
        OpenPortal();
    }
    
    public void OnOpenRoomCreateMenuClicked()
    {
        inRoomCreate = true;
        HideAllPanels();
        roomCreatePanel.gameObject.SetActive(true);
    }

    public void OnMainPanelClicked()
    {
        HideAllPanels();
        mainPanel.gameObject.SetActive(true);
    }
   
    public void OnSettingPanelClicked()
    {
        HideAllPanels();
        settingsPanel.gameObject.SetActive(true);
    }

    public void GenerateRandomRoomCode()
    {
        //Generate room code
        joinRoomHashCode = RandomRoomCode.GenerateRandomCode(6);
    }
    
    #endregion
    
    #region Room --------------->
    public async void OnCreateRoomClicked()
    {
        try
        {
            OpenActionStatus("Trying to create room...");
            var isConnected = await checkInternetConnection.CheckHasConnection();
            if (isConnected)
            {
                CloseActionStatus("Room created", 2);
                HideAllPanels();
                ConnectingPanel("Creating Room");

                //isQuickPlayAutoJoinEnabled = false;
                isCreatingRoom = true;

                if (UIClientHandler.Instance.Client != null && UIClientHandler.Instance.Client.IsConnected)
                {
                    _connectionHandler.CreateRoom();
                }
                else
                {
                    _connectionHandler.ConnectToMaster();
                }
            
                OnConnectSettings();
            }
            else
            {
                CloseActionStatus("Error Connecting", 2);
                connectionScreen.SetActive(true);
                return;
            }
        }
        catch (Exception e)
        {
            throw; // TODO handle exception
        }
    }

    public void OnAutoJoinRoomClicked()
    {
        if (UIClientHandler.Instance.Client != null && UIClientHandler.Instance.Client.IsConnected)
        {
            _connectionHandler.JoinRandomOrCreateRoom();
        }
        else
        {
            _connectionHandler.ConnectToMaster();
        }
    }
    
    public async void OnOpenPrivateRoomClicked()
    {
        try
        {
            OpenActionStatus("Connecting...");
            var isConnected = await checkInternetConnection.CheckHasConnection();
            if (!isConnected)
            {
                CloseActionStatus("Error loading lobby", 0);
                OnMainPanelClicked();
                connectionScreen.SetActive(true);
                return;
            }

            CloseActionStatus("Connected", 1);

            privateRoomPanel.gameObject.SetActive(true);
            
            if (UIClientHandler.Instance.Client != null && UIClientHandler.Instance.Client.IsConnected)
            {
                _connectionHandler.JoinLobby();
            }
            else
            {
                _connectionHandler.ConnectToMaster();
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }

    public void OnClosePrivateRoomClicked()
    {
        privateRoomPanel.gameObject.SetActive(false);  
    }
    
    public async void OnRoomJoinedClicked(string roomName)
    {
        _connectionHandler.JoinRoom(roomName);
    }

    public void OnRoomPrivateJoinedClicked()
    {
        if (string.IsNullOrEmpty(joinCustomRoomField.text))
        {
            CloseActionStatus("ENTER ROOM NAME", 3);
            return;
        }
        
        _connectionHandler.JoinRoom(joinCustomRoomField.text);
    }
    
    public void OnLeaveRoomClicked()
    {
        if (UIClientHandler.Instance.inRoom)
        {
            _connectionHandler.LeaveRoom();
        }

        HideAllPanels();
        mainPanel.gameObject.SetActive(true);
    }
    

    #endregion
    
    #region DropDown --------------->
    
    //Region
    public void ChangeRegionNext()
    {
        var appSettings = PhotonServerSettings.Global.AppSettings;
        
        if(currentRegionIndex == region.Length - 1)
            return;
        
        currentRegionIndex += 1;
        regionText.text = region[currentRegionIndex].name;
        appSettings.FixedRegion = region[currentRegionIndex].region;
        SaveDataLocal.Instance.currentRegionIndex = currentRegionIndex;
        SaveDataLocal.Instance.SaveGame();
    }
    
    public void ChangeRegionPrevious()
    {
        var appSettings = PhotonServerSettings.Global.AppSettings;

        if(currentRegionIndex == 0)
            return;
        
        currentRegionIndex -= 1;
        regionText.text = region[currentRegionIndex].name;
        appSettings.FixedRegion = region[currentRegionIndex].region;
        SaveDataLocal.Instance.currentRegionIndex = currentRegionIndex;
        SaveDataLocal.Instance.SaveGame();
    }
    
    //Max players
    public void ChangeMaxPlayersNext()
    {
        if(currentMaxPlayersIndex == maxPlayersValues.Length -1)
            return;

        currentMaxPlayersIndex += 1;
        maxPlayersText.text = maxPlayersValues[currentMaxPlayersIndex].ToString();
        maxPlayers = maxPlayersValues[currentMaxPlayersIndex];
    }

    public void ChangeMaxPlayersPrevious()
    {
        if(currentMaxPlayersIndex == 0)
            return;

        currentMaxPlayersIndex -= 1;
        maxPlayersText.text = maxPlayersValues[currentMaxPlayersIndex].ToString();
        maxPlayers = maxPlayersValues[currentMaxPlayersIndex];
    }
    
    #endregion

    //When working with lobbies
    public async void SelectGameMode(string currentGameMode)
    {
        try
        {
            gameMode = currentGameMode;
            CurrentGameMode();
        
            OnOpenRoomCreateMenuClicked();

            singlePlayer = false;
            multiPlayer = true;
        
            NetworkSceneManager.Instance.singlePlayer = false;
            NetworkSceneManager.Instance.multiplayer = true;
        }
        catch (Exception e)
        {
            throw; // TODO handle exception
        }
    }
    
    
    //When doing a quick match
    public void OpenPrivateRoom()
    {
        privateRoom = "PRIVATE";
        isPrivateRoom = true;
        autoMatchMakingObj.SetActive(false);
        addAI = false;
    }

    public void OpenPublicRoom()
    {
        privateRoom = "PUBLIC";
        isPrivateRoom = false;
        autoMatchMakingObj.SetActive(true);
        addAI = false;
    }
    
    public void SelectGameModeQuickPlay(string currentGameMode)
    {
        gameMode = currentGameMode;
        anyTimeJoinRoom = false;

        if (multiPlayer)
        {
            if (isPrivateRoom)
            {
                OnCreateRoomClicked();
            }
            else
            {
                OnQuickPlayClicked();
            }
        }
        else if(singlePlayer)
        {
            StartCoroutine(AutoStartSinglePlayer());
        }

        GenerateRandomRoomCode();
    }

    IEnumerator AutoStartSinglePlayer()
    {
        yield return new WaitForSeconds(1);
        OnStartGameClicked();
    }
    
    private void CurrentGameMode()
    {
        MatchSettings.Instance.SpawnMapTypes();

        switch (gameMode)
        {
            case "Battle":
                isQuickPlayAutoJoinEnabled = false;
                setMaxPlayersObj.SetActive(true);
                
                //Check Room That don't close
                anyTimeJoinRoom = false;
                break;
        }
        
        //Display Current values
        maxPlayersText.text = maxPlayersValues[currentMaxPlayersIndex].ToString();
        
        GenerateRandomRoomCode();
    }

    public void SinglePlayer()
    {
        singlePlayer = true;   
        multiPlayer = false;
    }

    public void Multiplayer()
    {
        multiPlayer = true;
        singlePlayer = false;
        training = false;
    }

    public void StartTraining()
    {
        training = true;    
    }
    
    public void SaveGameData()
    {
        SaveDataLocal.Instance.SaveGame();
    }
}
[Serializable]
public class Regions
{
    public string name;
    public string region;
}