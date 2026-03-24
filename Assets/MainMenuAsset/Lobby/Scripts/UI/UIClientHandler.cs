using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Client;
using Photon.Realtime;
using Quantum;
using UnityEngine;

public class UIClientHandler : MonoBehaviour
{
    
    public static UIClientHandler Instance { set; get; }
    
    //Quantum client
    public RealtimeClient Client { get; set; }


    [Header("Info")] 
    public int playersInRoom;
    
    [Header("State")] 
    public bool connected;
    public bool inRoom;
    public bool inLobby;
    
    [Header("Internet State")]
    public bool connectedInternet;
    
    private void Awake()
    {
        Instance = this;
    }
    
    private void Update()
    {
        Client?.Service();

        if (Client == null) 
            return;
        
        connected = Client.IsConnected;
        inRoom = Client.InRoom;
        inLobby = Client.InLobby;

        if(Client.InRoom) 
            playersInRoom = Client.CurrentRoom.PlayerCount;
        
        CheckConnection();
    }

    private void CheckConnection()
    {
        connectedInternet = Client.RealtimePeer.PeerState == PeerStateValue.Connected;
    }

    private void TryConnect()
    {
        if (Client == null)
        {
            
        }
    }
    
    private void OnDestroy()
    {
        if (Client != null && Client.IsConnected)
        {
            Client.Disconnect();
        }
    }
}
