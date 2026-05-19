using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SessionItemUIHandler : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI sessionNameText;
    
    [SerializeField]
    TextMeshProUGUI sessionMapText;

    [SerializeField]
    TextMeshProUGUI numberOfPlayersText;

    [SerializeField] 
    private TextMeshProUGUI gameModeText;
    
    public bool isFull;
    public bool canClick = true;

    [Header("Room Info")] 
    public string gameMode;
    public bool anyTimeJoin;
    public bool gameStarted;
    public int maxPlayers;
    
    public void SetTexts(string sessionName, string mapName, string numberOfPlayers, string gameMode)
    {
        sessionNameText.text = sessionName;
        sessionMapText.text = mapName;
        numberOfPlayersText.text = numberOfPlayers;
        gameModeText.text = gameMode;
    }

    public void OnJoinButtonClicked()
    {
        if(isFull)
            return;
        
        if(!canClick)
            return;
        
        if(!UIClientHandler.Instance.Client.IsConnected)
            return;
        
        MainMenuUIHandler.Instance.OnRoomJoinedClicked(sessionNameText.text);

        MainMenuUIHandler.Instance.anyTimeJoinRoom = anyTimeJoin;
        
        StartCoroutine(CanClick());
    }

    IEnumerator CanClick()
    {
        canClick = false;
        yield return new WaitForSeconds(5);
        canClick = true;
    }

}
