using System;
using Quantum;
using UnityEngine;

public unsafe class PlayerNetworkData : QuantumCallbacks
{
    private UpdateCharacterVisuals _updateCharacterVisuals;
    QuantumEntityView _entityView;

    private void Awake()
    {
        _entityView = GetComponent<QuantumEntityView>();
        _updateCharacterVisuals = gameObject.GetComponent<UpdateCharacterVisuals>();
    }

    private void Start()
    {
        RefreshPlayerVisuals();
    }

    private void RefreshPlayerVisuals()
    {
        var frame = QuantumRunner.Default.Game.Frames.Verified;
        
        if(!frame.Exists(_entityView.EntityRef))
            return;
        
        var localPlayer = frame.Unsafe.GetPointer<PlayerMovement>(_entityView.EntityRef);
        var playerStat = frame.Unsafe.GetPointer<PlayerStat>(_entityView.EntityRef);
        
        var playerData = frame.GetPlayerData(localPlayer->PlayerRef);

        if (localPlayer->PlayerType == EPlayerType.Player)
        {
            _updateCharacterVisuals.ChangeCharacter(playerData.currentPlayerIndex);
        }
        else if (localPlayer->PlayerType == EPlayerType.AI)
        {
            _updateCharacterVisuals.ChangeCharacter(playerStat->playerSkinIndex);
        }
    }
}
