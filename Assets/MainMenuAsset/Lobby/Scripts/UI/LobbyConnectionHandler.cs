using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

using Photon.Realtime;
using Photon.Client;
using Photon.Deterministic;
using Quantum;
using Random = UnityEngine.Random;


public class LobbyConnectionHandler : MonoBehaviour, IConnectionCallbacks, IMatchmakingCallbacks, IOnEventCallback, ILobbyCallbacks
{
    public static LobbyConnectionHandler Instance { set; get; }
    
    private MainMenuUIHandler _menuUIHandler;
    private MatchSettings _matchSettings;
    
    [Header("RUNTIME PLAYER")]
    public RuntimePlayer runtimePlayer;

    [Header("RUNTIME CONFIG")] 
    public RuntimeConfig runtimeConfig;
    
    [Header("Configs")] 
    [SerializeField] private SimulationConfig simulationConfig;
    [SerializeField] private QuantumDeterministicSessionConfigAsset quantumDeterministicSessionConfigAsset;
    [SerializeField] private SystemsConfig systemsConfig;
    
    enum PhotonEventCode : byte
    {
        StartGame = 110
    }
    
    //ROOM LIST
    private List<RoomInfo> _roomList = new List<RoomInfo>(20);
    
    [Header("AUTO MATCH MAKING")]
    [Space(10)]
    public const string StartTimeKey = "StartTime";
    public const string FastStartKey = "FastStartTime";

    public float serverElapsedTime;
    public float maxMatchMakingTime = 30;
    public float matchMakingTimer;
    public bool onStartGame;
    public int playersInRoomCount;

    
    private void Awake()
    {
        Instance = this;
        _menuUIHandler = GetComponent<MainMenuUIHandler>();
        _matchSettings = GetComponent<MatchSettings>();
    }

    private void Update()
    {
        UpdateRoomData();
        
        AutomaticMatchMaking();
    }

    private void AutomaticMatchMaking()
    {
        if(_menuUIHandler.privateRoom != "PUBLIC")
            return;
        
        // Basic validation checks
        if (UIClientHandler.Instance?.Client == null || !UIClientHandler.Instance.Client.InRoom)
            return;
        
        var client = UIClientHandler.Instance;
        playersInRoomCount = client.Client.CurrentRoom.PlayerCount;
        
        //Update game time
        _menuUIHandler.startTimeText.text = TimeFormatter.FormatTime(serverElapsedTime);
        
        // Get server time (more reliable than local deltaTime)
        serverElapsedTime = client.Client.RealtimePeer.ConnectionTime / 1000f;

        // FOR MASTER CLIENT ONLY
        if (client.Client.LocalPlayer.IsMasterClient)
        {
            // Only update timer if we haven't started the game yet
            if (!onStartGame)
            {
                // Calculate remaining time
                var remainingTime = serverElapsedTime;

                // Update room properties periodically (not every frame)
                if (Time.frameCount % 30 == 0) // Every 30 framess
                {
                    var countDownTime = new PhotonHashtable
                    {
                        { StartTimeKey, remainingTime }
                    };
                    client.Client.CurrentRoom.SetCustomProperties(countDownTime);
                }

                // Fill AI when 10 seconds remain
                if (remainingTime >= 20f && playersInRoomCount <= 1 && !_menuUIHandler.addAI)
                {
                    _menuUIHandler.addAI = true;
                    _menuUIHandler.matchmakingInfo.text = "FOUND PLAYER STARTING";
                    
                    var fillAI = new PhotonHashtable
                    {
                        { "AI", _menuUIHandler.addAI }
                    };
                    client.Client.CurrentRoom.SetCustomProperties(fillAI);
                }

                var targetTime = maxMatchMakingTime;

                if (playersInRoomCount <= 1)
                {
                    _menuUIHandler.matchmakingInfo.text = "SEARCHING FOR PLAYERS";    
                }
                
                // If 2 or more players are in the room, shorten the countdown
                if (playersInRoomCount >= 2)
                {
                    _menuUIHandler.addAI = false;

                    _menuUIHandler.matchmakingInfo.text = "FOUND PLAYER STARTING";
                    
                    //Once a real player is found remove AI
                    var fillAI = new PhotonHashtable
                    {
                        { "AI", _menuUIHandler.addAI }
                    };
                    client.Client.CurrentRoom.SetCustomProperties(fillAI);

                    // Fast start 3-second countdown logic
                    if (!client.Client.CurrentRoom.CustomProperties.ContainsKey(FastStartKey))
                    {
                        var fastStartProps = new PhotonHashtable
                        {
                            { FastStartKey, Environment.TickCount }
                        };
                        client.Client.CurrentRoom.SetCustomProperties(fastStartProps);
                    }
                }

                // Fast start timer check: if present, see if 3 seconds have elapsed
                if (client.Client.CurrentRoom.CustomProperties.TryGetValue(FastStartKey, out object fastStartObj))
                {
                    int fastStartTick = (int)fastStartObj;
                    float fastElapsed = (Environment.TickCount - fastStartTick) / 1000f;
                    if (fastElapsed >= 3f)
                    {
                        StartGame();
                        Debug.Log("Game started after 3 seconds because 2 or more players were present");
                        onStartGame = true;
                        var gameStartedProp = new PhotonHashtable { { "GameStarted", true } };
                        client.Client.CurrentRoom.SetCustomProperties(gameStartedProp);
                        return;
                    }
                }

                // Start game when time expires (regular timer)
                if (remainingTime >= targetTime)
                {
                    StartGame();
                    Debug.Log($"Game Started after {targetTime} seconds");

                    onStartGame = true;
                    var gameStartedProp = new PhotonHashtable { { "GameStarted", true } };
                    client.Client.CurrentRoom.SetCustomProperties(gameStartedProp);
                }
            }
        }
        else
        {
            // Non-master clients just track the remaining time
            matchMakingTimer = serverElapsedTime;
        }
    }
    
    
    #region Multiplayer connect and join code

    public bool ConnectToMaster()
    {
        var appSettings = new AppSettings(PhotonServerSettings.Global.AppSettings);
        
        UIClientHandler.Instance.Client = new RealtimeClient(PhotonServerSettings.Global.AppSettings.Protocol);

        //Get connection callback events etc
        UIClientHandler.Instance.Client.AddCallbackTarget(this);

        //Todo custom delete later
        UIClientHandler.Instance.Client.EventReceived += OnRoomPropertiesUpdate;
        
        if (string.IsNullOrEmpty(appSettings.AppIdQuantum.Trim()))
        {
            Utils.DebugLogError("Missing Quantum AppID");
            return false;
        }

        //If none was provided give a random one
        if (string.IsNullOrEmpty(_menuUIHandler.playerName))
        {
            Debug.LogError("ERROR PLAYER NAME IS NULL");
        }

        UIClientHandler.Instance.Client.LocalPlayer.NickName = _menuUIHandler.playerName;
        
        //Connect to the Photon Cloud
        if (!UIClientHandler.Instance.Client.ConnectUsingSettings(appSettings))
        {
            //Utils.DebugLogError("Unable to issue Connect to Master command");
            _menuUIHandler.Error("ERROR","Unable to connect");
            return false;
        }

        Utils.DebugLog($"Attempting to connect to region {appSettings.FixedRegion}");

        return true;
    }

    //START GAME PARAMETERS
    EnterRoomArgs CreateEnterRoomParams(string roomName)
    {
        //Setup room properties
        EnterRoomArgs enterRoomParams = new EnterRoomArgs();

        enterRoomParams.RoomOptions = new RoomOptions();
        
        enterRoomParams.RoomName = roomName;
   
        //CHECK ROOM IS PUBLIC OR PRIVATE
        enterRoomParams.RoomOptions.IsVisible = _menuUIHandler.privateRoom == "PUBLIC";
        
        enterRoomParams.RoomOptions.MaxPlayers = _menuUIHandler.maxPlayers;

        enterRoomParams.RoomOptions.Plugins = new string[] { "QuantumPlugin" };
        
        enterRoomParams.RoomOptions.CustomRoomProperties = new PhotonHashtable
        {
            {"MAP-GUID", _menuUIHandler.mapGuid},
            {"GameMode", _menuUIHandler.gameMode},
            {"MapIndex", _menuUIHandler.mapIndex},
            {"IsGameStarted", false},
            {"AnyTimeJoin", _menuUIHandler.anyTimeJoinRoom},
            {"MapName", _menuUIHandler.mapName},
        };

        enterRoomParams.RoomOptions.CustomRoomPropertiesForLobby = new object[] { "MAP-GUID" ,"GameMode",
            "MapIndex", "IsGameStarted", "AnyTimeJoin", "MapName"};

        enterRoomParams.RoomOptions.PlayerTtl = PhotonServerSettings.Global.PlayerTtlInSeconds * 1000;
        enterRoomParams.RoomOptions.EmptyRoomTtl = PhotonServerSettings.Global.EmptyRoomTtlInSeconds * 1000;
        
        _menuUIHandler.roomCodeTextDisplay.SetActive(_menuUIHandler.privateRoom == "PRIVATE");
        _menuUIHandler.roomInfoDisplay.SetActive(_menuUIHandler.privateRoom == "PRIVATE");
        
        _menuUIHandler.roomCodeText.text = _menuUIHandler.joinRoomHashCode;
        
        return enterRoomParams;
    }

    //AUTO JOIN OR CREATE ROOM
    public void JoinRandomOrCreateRoom()
    {
        var connectionHandler = FindAnyObjectByType<ConnectionHandler>();
        
        //Prevents disconnects during long scene loads
        connectionHandler.Client = UIClientHandler.Instance.Client;
        connectionHandler.StartFallbackSendAckThread();
        
        //Setup room properties
        var enterRoomParams = CreateEnterRoomParams(_menuUIHandler.joinRoomHashCode);
        
        //Join Random Room - BASED ON GAME MODE
        var expectedCustomRoomProperties = new PhotonHashtable { {"GameMode",_menuUIHandler.gameMode} };
        var joinRandomParams = new JoinRandomRoomArgs
        {
            ExpectedCustomRoomProperties = expectedCustomRoomProperties
        };

        //Find a random room or create one if needed.
        if (!UIClientHandler.Instance.Client.OpJoinRandomOrCreateRoom(joinRandomParams, enterRoomParams))
        {
            //Utils.DebugLogError("Unable to join random room or create room");
            _menuUIHandler.Error("ERROR","Unable to join or create random room");
            return;
        }

        //Reset match making timer
        matchMakingTimer = 0;
        serverElapsedTime = 0;
        onStartGame = false;
        
        //
        _menuUIHandler.isConnecting = false;
        Utils.DebugLog($"Joining random room or creating new room");
        Debug.Log("Enter room params "+enterRoomParams.RoomOptions.IsVisible);
        Debug.Log("Max players "+enterRoomParams.RoomOptions.MaxPlayers);
    }

    //MANUALLY CREATE ROOM
    public void CreateRoom()
    {
        var connectionHandler = FindAnyObjectByType<ConnectionHandler>();
        
        //Prevents disconnects during long scene loads
        connectionHandler.Client = UIClientHandler.Instance.Client;
        connectionHandler.StartFallbackSendAckThread();

        //Setup room properties
        var enterRoomParams = CreateEnterRoomParams(_menuUIHandler.joinRoomHashCode);

        //Find a random room or create one if needed.
        if (!UIClientHandler.Instance.Client.OpCreateRoom(enterRoomParams))
        {
            //Utils.DebugLogError("Unable to join random room or create room");
            _menuUIHandler.Error("ERROR","Unable to join or create random room");
            return;
        }

        Debug.Log("Enter room params "+enterRoomParams.RoomOptions.IsVisible);
        Utils.DebugLog($"Creating new room");
    }
    
    [ContextMenu("Join Room Test")]
    public void JoinRoom(string roomName)
    {
        var connectionHandler = FindAnyObjectByType<ConnectionHandler>();
        
        //Prevents disconnects during long scene loads
        connectionHandler.Client = UIClientHandler.Instance.Client;
        connectionHandler.StartFallbackSendAckThread();
        
        //Setup room properties
        var enterRoomParams = CreateEnterRoomParams(roomName);
        
        //CLOSE SEARCH PANEL
        _menuUIHandler.OnClosePrivateRoomClicked();
        
        //Join specific room
        if (!UIClientHandler.Instance.Client.OpJoinRoom(enterRoomParams))
        {
            //Utils.DebugLogError("Unable to join room");
            _menuUIHandler.Error("ERROR","Unable to join room");
            return;
        }
    }

    //Leave Room
    public void LeaveRoom()
    {
        if (UIClientHandler.Instance.Client == null)
            return;

        if (UIClientHandler.Instance.Client.InRoom)
        {
            UIClientHandler.Instance.Client.OpLeaveRoom(false);
            MainMenuUIHandler.Instance.isQuickPlayAutoJoinEnabled = false;
            MainMenuUIHandler.Instance.isCreatingRoom = false; // ADD THIS
            Utils.DebugLog("Leaving Room");
        }
        else
        {
            MainMenuUIHandler.Instance.Error("ERROR", "Cannot leave room because the client is not in a room");
        }
    }
    
    public void JoinLobby()
    {
        if (!UIClientHandler.Instance.Client.OpJoinLobby(TypedLobby.Default))
        {
            //Utils.DebugLogError("Unable join lobby");
            _menuUIHandler.Error("ERROR","Unable to join lobby");
        }
    }

 
   public void StartGame()
    {
        //Only master can start game
        if(!UIClientHandler.Instance.Client.LocalPlayer.IsMasterClient)
            return;
        
        if (!UIClientHandler.Instance.Client.OpRaiseEvent((byte)PhotonEventCode.StartGame, null, 
                new RaiseEventArgs{ Receivers = ReceiverGroup.All }, SendOptions.SendReliable))
        {
            //Utils.DebugLogError("Unable to start game");
            _menuUIHandler.Error("ERROR","Unable to start game");
            return;
        }

        Utils.DebugLog("Starting game");
    }

    public void ResetGameModes()
    {
        var config = runtimeConfig;
        config.battleMode = false;
    }
    private async void StartQuantumGame()
    {
        if (QuantumRunner.Default != null)
        {
            //Check for other runners. Only 1 runner is allowed
            Utils.DebugLogWarning($"Another QuantumRunner '{QuantumRunner.Default.Id}' has prevented starting the game");
            return;
        }

        var config = runtimeConfig;

        config.SimulationConfig = simulationConfig;
        config.SystemsConfig = systemsConfig;
        
        //START PARAM
        var sessionRunnerArguments = new SessionRunner.Arguments
        {
            RunnerFactory = QuantumRunnerUnityFactory.DefaultFactory,

            GameParameters = QuantumRunnerUnityFactory.CreateGameParameters,

            ClientId = Guid.NewGuid().ToString(),

            RuntimeConfig = config,

            SessionConfig = quantumDeterministicSessionConfigAsset.Config,

            GameMode = Photon.Deterministic.DeterministicGameMode.Multiplayer,
            PlayerCount = UIClientHandler.Instance.Client.CurrentRoom.PlayerCount,

            StartGameTimeoutInSeconds = 10,

            Communicator = new QuantumNetworkCommunicator(UIClientHandler.Instance.Client),
        };
            
        //MAP INDEX
        config.mapIndex = _menuUIHandler.mapIndex;
        SaveDataLocal.Instance.mapIndex = _menuUIHandler.mapIndex;
        
        //Set Game Mode
        switch (_menuUIHandler.gameMode)
        {
            case "Battle":
                config.Map.Id = _menuUIHandler.mapGuid;
                ResetGameModes();
                config.battleMode = true;
                SaveDataLocal.Instance.gameMode = "Battle";
                break;
        }
        
        //FILL WITH AI
        if (_menuUIHandler.addAI)
        {
            config.aiPlayerCount = 1;
        }
        else
        {
            config.aiPlayerCount = 0;
        }
        
        //Training
        //config.training = _menuUIHandler.training;
        config.training = false;
        
        //FREE ROAM
        config.freeMove = true;
        
        //Set custom properties for the lobby after the room is created
        var updateProperties = new Hashtable
        {
            {"IsGameStarted", true}
        };
       
        
        NetworkSceneManager.Instance.InGame();
        NetworkSceneManager.Instance.singlePlayer = false;
        NetworkSceneManager.Instance.multiplayer = true;
        
        _menuUIHandler.isJoiningGame = true;

        var runner = (QuantumRunner) await SessionRunner.StartAsync(sessionRunnerArguments);
        runner.Game.AddPlayer(runtimePlayer);
    }

    //
    public void StartSinglePlayerGame()
    {
        Debug.Log("Start single player game");
        
        var config = runtimeConfig;
        config.SimulationConfig = simulationConfig;
        config.SystemsConfig = systemsConfig;

        config.Seed = Random.Range(0, 1000);
        
        //Set Game Mode
        switch (_menuUIHandler.gameMode)
        {
            case "Battle":
                config.Map.Id = _menuUIHandler.mapGuid;
                config.mapIndex = _menuUIHandler.mapIndex;
                ResetGameModes();
                config.battleMode = true;
                SaveDataLocal.Instance.gameMode = "Battle";
                break;
        }
        
        //FILL WITH AI
        config.aiPlayerCount = 1;
        
        //Training
        config.training = _menuUIHandler.training;
        
        //Set custom properties for the lobby after the room is created
        var updateProperties = new Hashtable
        {
            {"IsGameStarted", true}
        };
        
        NetworkSceneManager.Instance.InGame();
        NetworkSceneManager.Instance.singlePlayer = true;
        NetworkSceneManager.Instance.multiplayer = false;
        
        _menuUIHandler.isJoiningGame = true;

        var runnerArgs = new SessionRunner.Arguments
        {
            RunnerFactory = QuantumRunnerUnityFactory.DefaultFactory,
            GameParameters = QuantumRunnerUnityFactory.CreateGameParameters,
            ClientId = Guid.NewGuid().ToString(),
            RuntimeConfig = config,
            SessionConfig = quantumDeterministicSessionConfigAsset.Config,
            GameMode = DeterministicGameMode.Local,
            PlayerCount = 2,
        };
        
        _ = LaunchLocalSession(runnerArgs);
    }

    private async Task LaunchLocalSession(SessionRunner.Arguments args)
    {
        var runner = (QuantumRunner) await SessionRunner.StartAsync(args);
        runner.Game.AddPlayer(runtimePlayer);
    }
    
    //CHECK IF THE GAME IS STARTED FOR ROOMS THAT KEEP OPEN
    //ToDO might come in handy later dont delete 
    private void OnRoomPropertiesUpdate(EventData photonEvent)
    {
        if (photonEvent.Code == EventCode.PropertiesChanged)
        {
            var propertiesChanged = photonEvent.Parameters[ParameterCode.Properties] as Hashtable;
            
            if (propertiesChanged != null && propertiesChanged.ContainsKey("IsGameStarted"))
            {
                var isGameStarted = (bool)propertiesChanged["IsGameStarted"];
                //Debug.Log(isGameStarted ? "Game Started" : "Game Not Started");
            }
        }
    }
    
    #endregion

    #region Connection events

    public void OnConnected()
    {
        Utils.DebugLog($"OnConnected UserId: {UIClientHandler.Instance.Client.UserId}");
    }
    
    public void OnConnectedToMaster()
    {
        Utils.DebugLog($"Connected to master server in region {UIClientHandler.Instance.Client.CurrentRegion}");

        if (_menuUIHandler.isQuickPlayAutoJoinEnabled)
        {
            JoinRandomOrCreateRoom();
        }
        else if(_menuUIHandler.isCreatingRoom)
        {
            CreateRoom();
        }
        else
        {
            JoinLobby();
        }
    }

    public void OnDisconnected(DisconnectCause cause)
    {
        Utils.DebugLog($"OnDisconnected cause {cause}");
        switch (cause)
        {
            case DisconnectCause.DisconnectByClientLogic:
                break;
            default:
                _menuUIHandler.Error("ERROR","Disconnected cause "+cause);
                break;
        }
        
        //ADDED
        QuantumRunner.ShutdownAll();
        
        //RETURN TO MAIN MENU
        NetworkSceneManager.Instance.MainMenu();
    }

    public void OnRegionListReceived(RegionHandler regionHandler)
    {
        Utils.DebugLog($"OnRegionListReceived");
    }

    public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
    {
        Utils.DebugLog($"OnCustomAuthenticationResponse");
    }

    public void OnCustomAuthenticationFailed(string debugMessage)
    {
        Utils.DebugLog($"OnCustomAuthenticationFailed");
    }

    #endregion

    #region Room events
    public void OnFriendListUpdate(List<FriendInfo> friendList)
    {
        Utils.DebugLog($"OnFriendListUpdate");
    }

    public void OnCreatedRoom()
    {
        Utils.DebugLog($"OnCreatedRoom");
    }

    public void OnCreateRoomFailed(short returnCode, string message)
    {
        Utils.DebugLog($"OnCreateRoomFailed");
        _menuUIHandler.Error("ERROR","Failed To Create Room " + message);
    }

    public void OnJoinedRoom()
    {
        Utils.DebugLog($"OnJoinedRoom");
        
        //CHECKS TO JUMP DIRECTLY IN GAME OR IN ROOM
        if (UIClientHandler.Instance.Client.CurrentRoom.CustomProperties.TryGetValue("IsGameStarted", out object checkGameStarted))
        {
            var gameStarted = (bool)checkGameStarted;
            
            // Check if automatic join is enabled and game has started
            if (_menuUIHandler.anyTimeJoinRoom && gameStarted)
            {
                //AUTO JOIN GAME 
                ClientJoinGame();
            }
            else if(!_menuUIHandler.anyTimeJoinRoom)
            {
                // GO TO ROOM - FOR RACES ETC
                _menuUIHandler.OpenRoomPanel();
            }
            else if(_menuUIHandler.anyTimeJoinRoom && !gameStarted)
            {
                //AUTO JUMP TO GAME - FOR WORLD
               ClientJoinGame();
            }
        }
        
        StartCoroutine(_menuUIHandler.UpdateRoomDetailsCo());
    }

    //JUMPS DIRECTLY TO THE GAME
    private void ClientJoinGame()
    {
        if (!UIClientHandler.Instance.Client.OpRaiseEvent((byte)PhotonEventCode.StartGame, null, 
                new RaiseEventArgs{ Receivers = ReceiverGroup.All }, SendOptions.SendReliable))
        {
            //Utils.DebugLogError("Unable to start game");
            _menuUIHandler.Error("ERROR","Unable to start game");
            return;
        }
    }
    
    public void OnJoinRoomFailed(short returnCode, string message)
    {
        //Utils.DebugLogError($"OnJoinRoomFailed return code {returnCode} message {message}");
        _menuUIHandler.Error("ERROR","OnJoinRoomFailed return code " +returnCode+ " message "+message);
    }

    public void OnJoinRandomFailed(short returnCode, string message)
    {
        //Utils.DebugLogError($"OnJoinRandomFailed return code {returnCode} message {message}");
        _menuUIHandler.Error("ERROR","OnJoinRoomFailed return code " +returnCode+ " message "+message);
    }

    public void OnLeftRoom()
    {
        Utils.DebugLog($"OnLeftRoom");
        _menuUIHandler.isJoiningGame = false;
    }

    #endregion

    #region Photon Events
    public void OnEvent(EventData photonEvent)
    {
        //Utils.DebugLog($"photonEvent received code {photonEvent.Code}. ");

        switch (photonEvent.Code)
        {
            case (byte)PhotonEventCode.StartGame:
                UIClientHandler.Instance.Client.CurrentRoom.CustomProperties.TryGetValue("MAP-GUID", out object mapGuidValue);

                if (mapGuidValue == null)
                {
                    Utils.DebugLogError("Failed to get map GUID, disconnecting");
                    UIClientHandler.Instance.Client.Disconnect();

                    return;
                }

                //Check if we are the master client
                if (UIClientHandler.Instance.Client.LocalPlayer.IsMasterClient)
                {
                    if (_menuUIHandler.gameMode == "World")
                    {
                        UIClientHandler.Instance.Client.CurrentRoom.IsVisible = true;
                        UIClientHandler.Instance.Client.CurrentRoom.IsOpen = true;
                    }
                    else 
                    {
                        UIClientHandler.Instance.Client.CurrentRoom.IsVisible = false;
                        UIClientHandler.Instance.Client.CurrentRoom.IsOpen = false;
                    }
                }
                
                StartQuantumGame();
                break;
        }
    }

    #endregion

    #region Lobby events

    public void OnJoinedLobby()
    {
        MainMenuUIHandler.Instance.startSplashScreen.SetActive(false);
        Utils.DebugLog("OnJoinedLobby");
    }

    public void OnLeftLobby()
    {
        Utils.DebugLog("OnLeftLobby");
        _menuUIHandler.isJoiningGame = false;
    }

    public void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Utils.DebugLog($"OnRoomListUpdate. Rooms {roomList.Count}");
        //Clear UI
        _menuUIHandler.ClearChildrenLayoutGroup(_menuUIHandler.sessionListLayoutGroup.transform);

        _roomList.Clear();
        
        foreach (RoomInfo roomInfo in roomList)
        {
            // Skip removed rooms
            if (roomInfo.RemovedFromList)
                continue;
            
            _roomList.Add(roomInfo);
        }
        
        //Update the ui
        UpdateRoomListUI();
    }

    void UpdateRoomListUI()
    {
        for (var i = _roomList.Count - 1; i >= 0; i--)
        {
            //Remove rooms
            if (_roomList[i].RemovedFromList)
            {
                _roomList.RemoveAt(i);
            }
            
            var sessionItem = Instantiate(_menuUIHandler.sessionListPrefab,
                _menuUIHandler.sessionListLayoutGroup.transform);

            var sessionHandlerItem = sessionItem.GetComponent<SessionItemUIHandler>();
            sessionHandlerItem.SetTexts(_roomList[i].Name,
                GetMapName(_roomList[i]), 
                $"{_roomList[i].PlayerCount}/{_roomList[i].MaxPlayers}", 
                GetGameMode(_roomList[i]));
            
            //CHECK IF ROOM FULL
            sessionHandlerItem.isFull = _roomList[i].PlayerCount >= _roomList[i].MaxPlayers;
            
            //Game Started
            sessionHandlerItem.gameStarted = GameStartedCheck(_roomList[i]);
            
            //Anytime join room
            sessionHandlerItem.anyTimeJoin = AnyTimeJoin(_roomList[i]);
        }
    }

    private void UpdateRoomData()
    {
        //Update the clients data from the master client data
        if(UIClientHandler.Instance.Client == null)
            return;
        
        if(!UIClientHandler.Instance.Client.InRoom)
            return;

        var client = UIClientHandler.Instance;
        
        //Map Index
        if (client.Client.CurrentRoom.CustomProperties.TryGetValue("MapIndex", out object mapIndexValue))
        {
            _menuUIHandler.mapIndex = (int)mapIndexValue;
        }
        
        //Game Mode
        _menuUIHandler.gameMode = GetGameMode(client.Client.CurrentRoom);
        
        //MapGUID
        if (client.Client.CurrentRoom.CustomProperties.TryGetValue("MAP-GUID", out object mapGuid))
        {
            _menuUIHandler.mapGuid = (long)mapGuid;
        }
        //Get Max Player
        _menuUIHandler.maxPlayers = client.Client.CurrentRoom.MaxPlayers;

        _menuUIHandler.mapName = GetMapName(client.Client.CurrentRoom);
        
        //Time
        serverElapsedTime = GetCountDownTime(client.Client.CurrentRoom);
    }
    
    //Get and return game mode - nick
    private static string GetGameMode(RoomInfo roomInfo)
    {
        if (!roomInfo.CustomProperties.TryGetValue("GameMode", out object gameModeObj)) 
            return null;
        
        var gameMode = (string)gameModeObj;
        return gameMode;
    }
   
    //Get Map
    private static string GetMapName(RoomInfo roomInfo)
    {
        if (!roomInfo.CustomProperties.TryGetValue("MapName", out object map)) 
            return null;
        
        var mapName = (string)map;
        return mapName;
    }
    
    
    //Anytime Join
    private static bool AnyTimeJoin(RoomInfo roomInfo)
    {
        if (!roomInfo.CustomProperties.TryGetValue("AnyTimeJoin", out object anyJoin))
            return false;

        var anyTimeJoin = (bool)anyJoin;
        return anyTimeJoin;
    }
    
    //Game started
    private bool GameStartedCheck(RoomInfo roomInfo)
    {
        if (!roomInfo.CustomProperties.TryGetValue("IsGameStarted", out object gameStarted))
            return false;

        var isGameStarted = (bool)gameStarted;
        return isGameStarted;
    }
    
    //Get Count downTime
    private static float GetCountDownTime(RoomInfo roomInfo)
    {
        if(!roomInfo.CustomProperties.TryGetValue(StartTimeKey, out object countDownTime))
            return 0;

        return (float)countDownTime;
    }
    
    public void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
    {
        Utils.DebugLog("OnLobbyStatisticsUpdate");
    }

    #endregion
}