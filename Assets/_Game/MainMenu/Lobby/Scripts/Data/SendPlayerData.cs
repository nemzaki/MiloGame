using UnityEngine;

public class SendPlayerData : MonoBehaviour
{
    public void SendData()
    {
        var lobbyConnectionHandler = LobbyConnectionHandler.Instance;

        lobbyConnectionHandler.runtimeConfig.Seed = Random.Range(0, 100);

        lobbyConnectionHandler.runtimePlayer.nickname = SaveDataLocal.Instance.playerName;
        lobbyConnectionHandler.runtimePlayer.currentPlayerIndex = SaveDataLocal.Instance.currentPlayerIndex;
        lobbyConnectionHandler.runtimePlayer.moveType = SaveDataLocal.Instance.currentMovementType;
        lobbyConnectionHandler.runtimePlayer.idleType = SaveDataLocal.Instance.currentIdleType;
        lobbyConnectionHandler.runtimePlayer.hardPunchType = SaveDataLocal.Instance.currentHardPunchType;
        lobbyConnectionHandler.runtimePlayer.hardKickType = SaveDataLocal.Instance.currentHardKickType;
        lobbyConnectionHandler.runtimePlayer.celebrateType = SaveDataLocal.Instance.currentCelebrationType;
    }
}
